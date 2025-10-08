namespace Voxel_Engine.SVO
{
    public static class IndexHelper
    {
        /// <summary>
        /// Interleaves a given number with two zero-bits after each input bit
        /// The first insertion occurs between the least significant bit and the next higher bit
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ulong Interleave_Two0(uint input)
        {
            ulong result = 0;
            int bitIndex = 0;

            while (input != 0)
            {
                if ((input & 1) != 0)
                {
                    result |= 1UL << (bitIndex * 3);
                }

                input >>= 1;
                bitIndex++;
            }

            return result;
        }
        
        /// <summary>
        /// Interleaves three coordinates into an octal digit
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static OctalNumber Interleave3(uint x, uint y, uint z)
        {
            return new OctalNumber()
            {
                Value = (Interleave_Two0(x) << 2) | (Interleave_Two0(y) << 1) | Interleave_Two0(z)
            };
        }

        public static int GetMostSignificantBit(ulong num)
        {
            if (num == 0)
                return 0; // Kein gesetztes Bit

            var position = 0;
            while (num != 0)
            {
                num >>= 1;
                position++;
            }
            
            return position - 1;
        }
    }

    public struct OctalNumber
    {
        public ulong Value;

        /// <summary>
        /// Returns the 3 bits belonging to the octal digit according to depth.
        /// Depth 0 returns the octal digit with most significance (first non 0 entry)
        /// </summary>
        /// <param name="depth"></param>
        /// <returns>A value in [0,7]</returns>
        public int GetOctalDigit(int depth)
        {
            var msb = IndexHelper.GetMostSignificantBit(Value);
            var shift = ((msb / 3) * 3) - 3 * depth;
            var shiftedValue = (Value >> shift);
            return (int)(shiftedValue & 0b111);
        }
    }
}