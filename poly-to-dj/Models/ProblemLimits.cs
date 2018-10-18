namespace PolyToDJ.Models
{
    public sealed class ProblemLimits
    {
        /// <summary>
        /// Time limit, in seconds.
        /// </summary>
        public decimal Time { get; }

        /// <summary>
        /// Memory limit, in bytes.
        /// </summary>
        public int Memory { get; }


        public ProblemLimits(decimal time, int memory)
        {
            Time = time;
            Memory = memory;
        }
    }
}