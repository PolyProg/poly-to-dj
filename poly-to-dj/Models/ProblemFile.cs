using System;
using System.Collections.Immutable;

namespace PolyToDJ.Models
{
    public sealed class ProblemFile
    {
        public string Name { get; }
        public ImmutableArray<byte> Content { get; }

        public ProblemFile(string name, ImmutableArray<byte> content)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Content = content;
        }
    }
}