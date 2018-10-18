using PolyToDJ.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PolyToDJ.DOMJudge
{
    public static class DOMJudgeWriter
    {
        public static void Write(Problem problem, string path)
        {
            var ini = new StringBuilder();
            ini.Append($"name='{problem.Name}'\n");
            ini.Append($"timelimit={problem.Limits.Time.ToString(CultureInfo.InvariantCulture)}\n");
            ini.Append($"probid='{problem.Id}'\n");
            ini.Append($"color='{problem.Color}'\n");

            var yaml = new StringBuilder();
            yaml.Append("validation: custom\n");
            yaml.Append("limits:\n");
            yaml.Append($"  memory: {problem.Limits.Memory / (1024 * 1024)}\n");

            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);

            // ini
            File.WriteAllText(Path.Combine(dir, "domjudge-problem.ini"), ini.ToString());

            // yaml
            File.WriteAllText(Path.Combine(dir, "problem.yaml"), yaml.ToString());

            // Statement
            File.WriteAllBytes(Path.Combine(dir, "problem.pdf"), problem.Statement.Content.ToArray());

            // tests
            var dataDir = Path.Combine(dir, "data");
            Directory.CreateDirectory(dataDir);
            var sampleTests = problem.TestCases.Where(t => t.IsSample).ToArray();
            if (sampleTests.Length > 0)
            {
                var samplesDir = Path.Combine(dataDir, "sample");
                Directory.CreateDirectory(samplesDir);
                for (int n = 0; n < sampleTests.Length; n++)
                {
                    File.WriteAllText(Path.Combine(samplesDir, $"{n}.in"), sampleTests[n].Input);
                    File.WriteAllText(Path.Combine(samplesDir, $"{n}.ans"), sampleTests[n].Output);
                }
            }
            var realTests = problem.TestCases.Where(t => !t.IsSample).ToArray();
            if (realTests.Length > 0)
            {
                var realDir = Path.Combine(dataDir, "secret");
                Directory.CreateDirectory(realDir);
                for (int n = 0; n < realTests.Length; n++)
                {
                    File.WriteAllText(Path.Combine(realDir, $"{n}.in"), realTests[n].Input);
                    File.WriteAllText(Path.Combine(realDir, $"{n}.ans"), realTests[n].Output);
                }
            }

            // checker
            var checkerDir = Path.Combine(dir,"output_validators");
            Directory.CreateDirectory(checkerDir);
            var checkerLang = GetLangArg(problem.Checker.Language);
            foreach(var file in problem.Checker.Files)
            {
                File.WriteAllBytes(Path.Combine(checkerDir, file.Name), file.Content.ToArray());
            }
            File.WriteAllText(Path.Combine(checkerDir, "run"), ReadResource("DOMJudge.run"));
            File.WriteAllText(Path.Combine(checkerDir, "build"), $"#!/bin/sh\ng++ -std={checkerLang} -O2 {problem.Checker.MainFile.Name} -o checker");

            ZipFile.CreateFromDirectory(dir, path);
        }


        private static string GetLangArg(ProblemProgrammingLanguage lang)
        {
            switch(lang)
            {
                case ProblemProgrammingLanguage.Cpp11:
                    return "c++11";
                case ProblemProgrammingLanguage.Cpp14:
                    return "c++14";
                case ProblemProgrammingLanguage.Cpp17:
                    return "c++17";
                default:
                    throw new Exception("Wut?");
            }
        }

        private static string ReadResource(string resourceName)
        {
            using (var stream = typeof(DOMJudgeWriter).Assembly.GetManifestResourceStream("PolyToDJ." + resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}