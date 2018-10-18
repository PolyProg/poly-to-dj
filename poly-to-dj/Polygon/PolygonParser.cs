using PolyToDJ.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PolyToDJ.Polygon
{
    public static class PolygonParser
    {
        public static Problem Parse(string filePath, char problemLetter, string problemColor)
        {
            var zip = ZipFile.OpenRead(filePath);
            var zipEntries = zip.Entries.ToArray();

            var xmlEntry = zip.GetEntry("problem.xml") ?? throw new Exception("problem.xml not found");
            var problemXml = XDocument.Load(xmlEntry.Open());

            // Problem element
            var problemElem = problemXml.Root;
            AssertNoOtherAttributes(problemElem, "url", "short-name", "revision");
            AssertNoOtherChildren(problemElem, "names", "statements", "tutorials", "judging", "files", "assets", "properties", "stresses", "documents", "tags");
            var problemId = problemElem.Attribute("short-name").Value;

            // Problem names collection
            var problemNamesElem = SingleChild(problemElem, "names");
            AssertNoAttributes(problemNamesElem);
            AssertNoOtherChildren(problemNamesElem, "name");
            // Problem names children
            var problemNamesElems = problemNamesElem.Elements("name");
            var problemNameElem = problemNamesElems.SingleOrDefault(e =>
            {
                AssertNoChildren(e);
                AssertNoOtherAttributes(e, "language", "value");
                return e.Attribute("language").Value == "english";
            }) ?? throw new Exception("Problem name in English not found.");
            var problemName = problemNameElem.Attribute("value").Value;

            // We ignore tutorials

            // Judging info
            var judgingElem = SingleChild(problemElem, "judging");
            AssertNoOtherAttributes(judgingElem, "cpu-name", "cpu-speed", "input-file", "output-file");
            //AssertAttributeIs(judgingElem, "cpu-speed", "3600");
            AssertAttributeIs(judgingElem, "input-file", "");
            AssertAttributeIs(judgingElem, "output-file", "");
            AssertNoOtherChildren(judgingElem, "testset");
            var problemTests = judgingElem.Elements("testset").Select(testSetsElem =>
            {
                AssertNoOtherAttributes(testSetsElem, "name");
                AssertNoOtherChildren(testSetsElem, "time-limit", "memory-limit", "test-count", "input-path-pattern", "answer-path-pattern", "tests");
                AssertDirectChildrenHaveNoAttributes(testSetsElem);
                var testsElem = SingleChild(testSetsElem, "tests");
                AssertNoOtherChildren(testsElem, "test");
                AssertChildrenCountIs(testsElem, int.Parse(testSetsElem.Element("test-count").Value));
                return new
                {
                    TimeLimit = int.Parse(testSetsElem.Element("time-limit").Value),
                    MemoryLimit = int.Parse(testSetsElem.Element("memory-limit").Value),
                    Tests = testsElem.Elements("test").Select((testElem, idx) =>
                    {
                        AssertNoOtherAttributes(testElem, "method", "from-file", "cmd", "sample");
                        int realIdx = idx + 1; // polygon is 1-based...
                        return new ProblemTestCase(
                            isSample: testElem.Attribute("sample")?.Value == "true",
                            input: ReadZipEntryText(zip.GetEntry(Printf(testSetsElem.Element("input-path-pattern").Value, realIdx))),
                            output: ReadZipEntryText(zip.GetEntry(Printf(testSetsElem.Element("answer-path-pattern").Value, realIdx)))
                        );
                    }).ToArray() // NOTE: always use eager eval to avoid later errors
                };
            }).Aggregate((a, b) =>
            {
                if (a.TimeLimit != b.TimeLimit)
                {
                    throw new Exception("Different time limits per test set are not supported.");
                }
                if (a.MemoryLimit != b.MemoryLimit)
                {
                    throw new Exception("Different memory limits per test set are not supported.");
                }

                return new
                {
                    TimeLimit = a.TimeLimit,
                    MemoryLimit = b.MemoryLimit,
                    Tests = a.Tests.Concat(b.Tests).ToArray()
                };
            });

            // Files
            var filesElem = SingleChild(problemElem, "files");
            AssertNoAttributes(filesElem);
            AssertNoOtherChildren(filesElem, "resources", "executables"); // we ignore executables
            var resourcesElem = SingleChild(filesElem, "resources");
            AssertNoAttributes(resourcesElem);
            AssertNoOtherChildren(resourcesElem, "file");
            var allFiles = resourcesElem.Elements("file").Select(fileElem =>
            {
                AssertNoOtherAttributes(fileElem, "type", "path");
                AssertNoChildren(fileElem);
                var path = fileElem.Attribute("path").Value;
                AssertFileHasType(path, "h", "cpp", "sty", "tex", "ftl");
                return path;
            });
            var cppAndHFiles = allFiles.Where(s => s.EndsWith(".h") || s.EndsWith(".cpp")).Select(path =>
                ReadZipEntry(zip.GetEntry(path))
            );

            // Assets
            var assetsElem = SingleChild(problemElem, "assets");
            AssertNoAttributes(assetsElem);
            AssertNoOtherChildren(assetsElem, "checker", "validators", "solutions");
            var checkerElem = SingleChild(assetsElem, "checker");
            AssertNoOtherAttributes(checkerElem, "name", "type");
            AssertAttributeIs(checkerElem, "type", "testlib");
            AssertNoOtherChildren(checkerElem, "source", "binary", "copy", "testset");
            // We ignore copy and testset
            var checkerSourceElem = SingleChild(checkerElem, "source");
            AssertNoOtherAttributes(checkerSourceElem, "type", "path");
            AssertNoChildren(checkerSourceElem);
            var checkerSourceEntry = zip.GetEntry(checkerSourceElem.Attribute("path").Value);
            var checkerFile = ReadZipEntry(checkerSourceEntry);
            var problemChecker = new ProblemChecker(
                language: ParseProgrammingLanguage(checkerSourceElem.Attribute("type").Value),
                files: cppAndHFiles.Append(checkerFile).ToImmutableArray(),
                mainFile: checkerFile
            );

            // Statement
            var statementsElem = SingleChild(problemElem, "statements");
            AssertNoAttributes(statementsElem);
            AssertNoOtherChildren(statementsElem, "statement");
            var statementPath = statementsElem.Elements("statement").Single(statementElem =>
            {
                AssertNoOtherAttributes(statementElem, "charset", "language", "path", "type", "mathjax");
                AssertNoChildren(statementElem);
                return statementElem.Attribute("type").Value == "application/x-tex";
            }).Attribute("path").Value;
            // Magic latex stuff
            /*var statementTex = ReadZipEntryText(zip.GetEntry(statementPath));
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var header = ReadResource("Polygon.header.tex");
            var footer = ReadResource("Polygon.footer.tex");
            File.WriteAllText(Path.Combine(tempDir, "statement.tex"),
                header +
                // TODO make that work
                //@"\addtocounter{problem}{" + (problemLetter - 'A') + "}\n" + // fix to have the proper letter
                statementTex +
                footer);
            // also we need olymp.sty for some reason
            File.WriteAllText(Path.Combine(tempDir, "olymp.sty"), ReadZipEntryText(zip.GetEntry(allFiles.Single(f => f.EndsWith("olymp.sty")))));
             var process = new Process
             {
                 StartInfo = new ProcessStartInfo
                 {
                     FileName = "pdflatex",
                     Arguments = "statement.tex",
                     WorkingDirectory = tempDir
                 }
             };
             process.Start();
             process.WaitForExit();
             Console.WriteLine(tempDir);
            var statementPdf = Path.Combine(tempDir, "statement.pdf");*/

            // TODO support solutions

            // We ignore properties, stresses and documents
            return new Problem(
                id: problemLetter.ToString(), //problemId,
                name: problemName,
                color: problemColor,
                statement: new ProblemFile("statement.pdf",  ReadResourceBytes("Polygon.problem.pdf") /*File.ReadAllBytes(statementPdf).ToImmutableArray()*/),
                checker: problemChecker,
                limits: new ProblemLimits(problemTests.TimeLimit / 1000.0M, problemTests.MemoryLimit),
                testCases: problemTests.Tests.ToImmutableArray()
            );
        }

        private static void AssertAttributeIs(XElement element, string name, string expectedValue)
        {
            if (element.Attribute(name).Value != expectedValue)
            {
                throw new Exception($"{element.Name}.{name} was expected to be {expectedValue}");
            }
        }

        private static void AssertNoAttributes(XElement element)
        {
            if (element.HasAttributes)
            {
                throw new Exception($"Element {element.Name} was not expected to have attributes");
            }
        }

        private static void AssertNoOtherAttributes(XElement element, params string[] names)
        {
            foreach (var attribute in element.Attributes())
            {
                if (!names.Contains(attribute.Name.ToString()))
                {
                    throw new Exception($"Unknown attribute {attribute.Name} in element {element.Name}");
                }
            }
        }

        private static void AssertNoChildren(XElement element)
        {
            if (element.HasElements)
            {
                throw new Exception($"Element {element.Name} was not expected to have children");
            }
        }

        private static void AssertNoOtherChildren(XElement element, params string[] names)
        {
            foreach (var child in element.Elements())
            {
                if (!names.Contains(child.Name.ToString()))
                {
                    throw new Exception($"Unknown element {child.Name} in element {element.Name}");
                }
            }
        }

        private static void AssertChildrenCountIs(XElement element, int expectedCount)
        {
            if (element.Elements().Count() != expectedCount)
            {
                throw new Exception($"Expected {element.Name} to have {expectedCount} elements, but it has {element.Elements().Count()}");
            }
        }

        private static void AssertDirectChildrenHaveNoAttributes(XElement element)
        {
            foreach (var child in element.Elements())
            {
                AssertNoAttributes(child);
            }
        }

        private static void AssertFileHasType(string fileName, params string[] types)
        {
            var fileType = fileName.Substring(fileName.LastIndexOf('.') + 1);
            if (!types.Contains(fileType))
            {
                throw new Exception("Found a file of unknown type: " + fileName);
            }
        }

        private static XElement SingleChild(XElement parent, string name)
        {
            var children = parent.Elements(name);
            if (!children.Any())
            {
                throw new Exception($"Element {parent.Name} was expected to have a child {name} but didn't");
            }
            if (children.Count() > 1)
            {
                throw new Exception($"Element {parent.Name} was expected to have only one child {name}");
            }

            return children.First();
        }


        private static ProblemProgrammingLanguage ParseProgrammingLanguage(string type)
        {
            switch (type)
            {
                case "cpp.g++":
                case "cpp.g++11":
                    return ProblemProgrammingLanguage.Cpp11;

                case "cpp.g++14":
                    return ProblemProgrammingLanguage.Cpp14;

                case "cpp.g++17":
                    return ProblemProgrammingLanguage.Cpp17;

                default:
                    throw new Exception("Unexpected programming language: " + type);
            }
        }

        private static string Printf(string format, int arg)
        {
            if (format.Count(c => c == '%') != 1)
            {
                throw new Exception("Didn't expect this format string: " + format);
            }

            var result = format.Replace("%02d", arg.ToString("D2"));
            if (result.Contains("%"))
            {
                throw new Exception("Didn't expect this format string: " + format);
            }

            return result;
        }


        private static string ReadZipEntryText(ZipArchiveEntry entry)
        {
            using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static ProblemFile ReadZipEntry(ZipArchiveEntry entry)
        {
            var memoryStream = new MemoryStream();
            using (var stream = entry.Open())
            {
                stream.CopyTo(memoryStream);

                return new ProblemFile(
                    name: entry.Name,
                    content: memoryStream.ToArray().ToImmutableArray()
                );
            }
        }


        private static string ReadResource(string resourceName)
        {
            using (var stream = typeof(PolygonParser).Assembly.GetManifestResourceStream("PolyToDJ." + resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static ImmutableArray<byte> ReadResourceBytes(string resourceName)
        {
            foreach (var name in typeof(PolygonParser).Assembly.GetManifestResourceNames())
            {
                Console.WriteLine(name);
            }
            var memoryStream = new MemoryStream();
            using (var stream = typeof(PolygonParser).Assembly.GetManifestResourceStream("PolyToDJ." + resourceName))
            {
                stream.CopyTo(memoryStream);

                return memoryStream.ToArray().ToImmutableArray();
            }
        }
    }
}