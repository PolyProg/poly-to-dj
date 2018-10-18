using System;
using System.Collections.Immutable;

namespace PolyToDJ.Models
{
    public sealed class ProblemChecker
    {
        public ProblemProgrammingLanguage Language { get; }
        public ImmutableArray<ProblemFile> Files { get; }
        public ProblemFile MainFile { get; }

        public ProblemChecker(ProblemProgrammingLanguage language, ImmutableArray<ProblemFile> files, ProblemFile mainFile)
        {
            if (!files.Contains(mainFile))
            {
                throw new ArgumentException("Files should contain main files.");
            }

            Language = language;
            Files = files;
            MainFile = mainFile;
        }
    }
}