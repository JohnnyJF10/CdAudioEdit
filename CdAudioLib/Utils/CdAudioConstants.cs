/*
   Copyright 2025 Jonas Nebel

   Author:  Jonas Nebel
   Created: 02.01.2025

   License: MIT
*/

namespace CdAudioLib.Utils
{
    public static class CdAudioConstants
    {
        public const int OPTIMAL_BUFFER_SIZE = 0xfff;

        public const int FIRST_WAV_POS = 34840;
        public const int CD_AUDIO_CAPACITY = 130;
        public const int CD_AUDIO_NAME_LEN = 260;
        public const int CD_AUDIO_ENTRY_LEN = 268;

        public const uint PCM_USUAL_FMT_SIZE = 16;
        public const ushort PCM_FORMAT = 1;

        public const uint TR_AUDIO_HEADER_SIZE = 82;
        public const uint TR_AUDIO_FMT_SIZE = 50;
        public const ushort TR_AUDIO_FORMAT = 2;
        public const ushort TR_AUDIO_NUM_CHANNELS = 2;
        public const uint TR_AUDIO_SAMPLE_RATE = 44100;
        public const uint TR_AUDIO_BYTE_RATE = 44359;
        public const ushort TR_AUDIO_BLOCK_ALIGNMENT = 2048;
        public const ushort TR_AUDIO_BITS_PER_SAMPLE = 4;

        public const ushort TR_AUDIO_FMT_EXTRA_LENGTH = 32;
        public const ushort TR_AUDIO_SAMPLES_PER_BLOCK = 2036;
        public const ushort TR_AUDIO_COEFFICIENT_COUNT = 7;
        public const int TR_AUDIO_DATA_START_POS = 0x5a;

        public static readonly short[] TR_AUDIO_COEFFICIENT_1 = { 256, 512, 0, 192, 240, 460, 392 };
        public static readonly short[] TR_AUDIO_COEFFICIENT_2 = { 0, -256, 0, 64, 0, -208, -232 };

        public const uint TR_AUDIO_FACT_SIZE = 4;

        public const int TR_AUDIO_NUM_OF_REQ_PCM_SHORTS_PER_BLOCK = 4072;
        public const int TR_AUDIO_NUM_OF_ADPCM_BYTES_PER_BLOCK = 2048;

        public const string RIFF_WORD = "RIFF"; //        
        public static readonly char[] RIFF_CHARS = { 'R', 'I', 'F', 'F' };

        public const string WAVE_WORD = "WAVE";
        public static readonly char[] WAVE_CHARS = { 'W', 'A', 'V', 'E' };


        public const string FMT_WORD = "fmt ";
        public static readonly char[] FMT_CHARS = { 'f', 'm', 't', ' ' };

        public const string DATA_WORD = "data";
        public static readonly char[] DATA_CHARS = { 'd', 'a', 't', 'a' };

        public const string FACT_WORD = "fact";
        public static readonly char[] FACT_CHARS = { 'f', 'a', 'c', 't' };

    }
}
