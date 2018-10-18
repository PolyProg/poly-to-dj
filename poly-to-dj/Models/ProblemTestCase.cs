using System;

namespace PolyToDJ.Models
{
    public sealed class ProblemTestCase
    {
        public bool IsSample { get; }
        public string Input { get; }
        public string Output { get; }

        public ProblemTestCase(bool isSample, string input, string output)
        {
            IsSample = isSample;
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Output = output ?? throw new ArgumentNullException(nameof(output));
        }
    }
}