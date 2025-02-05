/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

namespace CdAudioLib.MsAdpcm
{
    internal struct adpcm_state
    {
        internal short coeff1;
        internal short coeff2;
        internal int delta;
        internal short sample1;
        internal short sample2;
    }

    public abstract class AdpcmBase
    {
        internal const int BLOCK_HEADER_LEN = 6;

        internal short[][] coefficient;

        internal static readonly int[] ADAPTATION_TABLE = new int[]
        {
                230, 230, 230, 230, 307, 409, 512, 614,
                768, 614, 512, 409, 307, 230, 230, 230,
        };

        internal static short Clamp(int val, int min, int max)
        {
            if (val < min) return (short)min;
            else if (val > max) return (short)max;
            else return (short)val;
        }
    }




}
