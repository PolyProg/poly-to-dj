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
                name = name.Substring(0, name.LastIndexOf('-')); // remove the dash then working copy number
                var letter = (new Dictionary<string, char> {
                    {"hashpoly-easy", 'A'},
                    {"hashpoly-medium", 'B'},
                    {"hashpoly-hard", 'C'},
                    {"spaceships-easy", 'D'},
                    {"spaceships-medium", 'E'},
                    {"spaceships-hard", 'F'},
                    {"rings-easy", 'G'},
                    {"rings-medium", 'H'},
                    {"rings-hard", 'I'},
                    {"cannon-field-easy", 'J'},
                    {"cannon-field-hard", 'K'},
                    {"expectations-easy", 'L'},
                    {"expectations-hard", 'M'},
                    {"mst-easy", 'N'},
                    {"mst-medium", 'O'},
                    {"mst-hard", 'P'},
                })[name];
                var color = (new Dictionary<string, string> {
                    {"hashpoly-easy", "#FF0000"},
                    {"hashpoly-medium", "#DD0000"},
                    {"hashpoly-hard", "#BB0000"},
                    {"spaceships-easy", "#0000FF"},
                    {"spaceships-medium", "#0000DD"},
                    {"spaceships-hard", "#0000BB"},
                    {"rings-easy", "#FFFFFF"},
                    {"rings-medium", "#DDDDDD"},
                    {"rings-hard", "#BBBBBB"},
                    {"cannon-field-easy", "#FF00FF"},
                    {"cannon-field-hard", "#DD00DD"},
                    {"expectations-easy", "#FFFF00"},
                    {"expectations-hard", "#DDDD00"},
                    {"mst-easy", "#00FF00"},
                    {"mst-medium", "#00DD00"},
                    {"mst-hard", "#00BB00"},
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
