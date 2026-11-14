namespace RenkYolu.Campaign
{
    // Keep the board sequence identical on Mono and IL2CPP, even if their Random implementations change.
    internal sealed class RoadRandom
    {
        private uint state;

        internal RoadRandom(int seed, int level)
        {
            state = unchecked((uint)seed + (uint)level * 747796405u);
        }

        internal int Next(int maximum)
        {
            unchecked
            {
                state += 0x9E3779B9u;
                uint value = (state ^ (state >> 16)) * 0x85EBCA6Bu;
                value = (value ^ (value >> 13)) * 0xC2B2AE35u;
                return (int)((value ^ (value >> 16)) % (uint)maximum);
            }
        }

        internal int Next(int minimum, int maximum) => minimum + Next(maximum - minimum);
    }
}
