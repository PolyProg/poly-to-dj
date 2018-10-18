using System;
using System.Collections.Immutable;

namespace PolyToDJ.Models
{
    public sealed class Problem
    {
        public string Id { get; }
        public string Name { get; }
        // TODO proper 'Color' type with R/G/B
        public string Color { get; }
        public ProblemFile Statement { get; } // TODO do we just enforce this to be a pdf?
        public ProblemLimits Limits { get; }
        public ImmutableArray<ProblemTestCase> TestCases { get; }

        /// <summary>
        /// May be null.
        /// </summary>
        public ProblemChecker Checker { get; }

        public Problem(string id, string name, string color, ProblemFile statement, ProblemLimits limits, ImmutableArray<ProblemTestCase> testCases, ProblemChecker checker)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Color = color ?? throw new ArgumentNullException(nameof(color));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
            Limits = limits ?? throw new ArgumentNullException(nameof(limits));
            TestCases = testCases;
            Checker = checker;
        }
    }
}