using PolyToDJ.DOMJudge;
using PolyToDJ.Polygon;
using System;
using System.Collections.Generic;
using System.IO;

namespace PolyToDJ
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            // dependencies: texlive-full

            foreach (var file in Directory.EnumerateFiles("D:\\HC2"))
            {
                var fff = new FileInfo(file);
                Console.WriteLine(fff.Name);
                var name = fff.Name.Replace("hc2-2019-", "").Replace("$linux.zip", "");
                name = name.Substring(0, name.LastIndexOf('-')); // remove the dash then 
                var letter = (new Dictionary<string, char> {
                    {"hashpoly-easy", 'X'},
                    {"mst-easy", 'A'},
                    {"mst-medium", 'A'},
                    {"mst-hard", 'A'},
                })[name];
                var color = (new Dictionary<string, string> {
                    {"hashpoly-easy", "#FF0000"},
                    {"mst-easy", "#FF0000"},
                    {"mst-medium", "#FF0000"},
                    {"mst-hard", "#FF0000"},
                })[name];
                var problem = PolygonParser.Parse(fff.FullName, letter, color);
                var outf = $"D:\\HC2\\out\\{letter}.zip";
                if (File.Exists(outf))
                {
                    File.Delete(outf);
                }
                DOMJudgeWriter.Write(problem, outf);
            }

            Console.WriteLine("Hello World!");
        }
    }
}
