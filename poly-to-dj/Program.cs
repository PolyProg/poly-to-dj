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

            foreach (var file in Directory.EnumerateFiles("D:\\MARTIAN\\contest"))
            {
                var fff = new FileInfo(file);
                Console.WriteLine(fff.Name);
                var name = fff.Name.Replace("hc2-2019-", "").Replace("$linux.zip", "");
                //name = name.Substring(0, name.LastIndexOf('-')); // remove the dash then working copy number
                var letter = name[0];/*(new Dictionary<string, char> {
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
                })[name];*/
                var color = (new Dictionary<char, string> {
                    {'A', "#FF0000"},
                    {'B', "#00FF00"},
                    {'C', "#0000FF"},
                    {'D', "#FFFF00"},
                    {'E', "#FF00FF"},
                    {'F', "#00FFFF"},
                    {'G', "#FFFFFF"},
                })[letter];
                var problem = PolygonParser.Parse(fff.FullName, letter, color);
                var outf = $"D:\\MARTIAN\\contest\\out\\{letter}.zip";
                Directory.CreateDirectory(outf.Replace($"{letter}.zip", ""));
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
