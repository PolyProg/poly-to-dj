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
            // TO READ: https://gist.github.com/iamarcel/8047384bfbe9941e52817cf14a79dc34
            // dependencies: texlive-full

            foreach (var file in Directory.EnumerateFiles("D:\\ICPC"))
            {
                var fff = new FileInfo(file);
                Console.WriteLine(fff.Name);
                var letter = (new Dictionary<string, char> {
                    {"apple", 'A'},
                    {"biased-benchmarking", 'B'},
                    {"commutative-concatenation", 'C'},
                    {"debts", 'D'},
                    {"equal-subsequences", 'E'},
                    {"modulo", 'F'},
                    {"oriental", 'G'},
                    {"highways", 'H'},
                    {"peak-isolation", 'I'},
                    {"pirate", 'J'},
                    {"timogehrov", 'K'},
                })[fff.Name.Replace(".zip","")];
                var color = (new Dictionary<string, string> {
                    {"apple", "#FF0000"},
                    {"biased-benchmarking", "#ff8b1e"},
                    {"commutative-concatenation", "#ffd70c"},
                    {"debts", "#00ff00"},
                    {"equal-subsequences", "#187d01"},
                    {"modulo", "#ff3df5"},
                    {"oriental", "#0000ff"},
                    {"highways", "#25ffff"},
                    {"peak-isolation", "#ffffff"},
                    {"pirate", "#000000"},
                    {"timogehrov", "#783711"},
                })[fff.Name.Replace(".zip", "")];
                var problem = PolygonParser.Parse(fff.FullName, letter, color);
                var outf = $"D:\\ICPC\\out\\{letter}.zip";
                if (File.Exists(outf))
                {
                    File.Delete(outf);
                }
                DOMJudgeWriter.Write(problem, outf);
            }
            //var problem  = PolygonParser.Parse(@"D:\Projects\polygon2domjudge\dryrun\easy.zip", 'X', "#f0f000");

            Console.WriteLine("Hello World!");
        }
    }
}
