

namespace CdAudioLib.MsAdpcm
{
    public class Decoder : AdpcmBase
    {
        public Decoder()
        {
            coefficient = new short[][]
            {
                new short[] { 256, 512, 0, 192, 240, 460, 392 },
                new short[] { 0, -256, 0, 64, 0, -208, -232 }
            };
        }

        public Decoder(short[][] coefficientTable)
            => coefficient = coefficientTable;

        private void DecodeBlock(byte[] adpcmInputBuf, ref short[] pcmResultBuf)
        {
            int outputLength = (adpcmInputBuf.Length - 12) * 2;

            if (pcmResultBuf.Length != outputLength)
                throw new Exception("Error, wrong target array size, must be " +
                    $"{outputLength}, but is {pcmResultBuf.Length}.");

            adpcm_state[] state = new[]
            {
                new adpcm_state(),
                new adpcm_state()
            };

            int offset = 0, predicator;

            // Read MS-ADPCM header

            predicator = Clamp(adpcmInputBuf[offset++], 0, 6);

            state[0].coeff1 = coefficient[0][predicator];
            state[0].coeff2 = coefficient[1][predicator];

            predicator = Clamp(adpcmInputBuf[offset++], 0, 6);

            state[1].coeff1 = coefficient[0][predicator];
            state[1].coeff2 = coefficient[1][predicator];

            state[0].delta = (BitConverter.ToInt16(adpcmInputBuf, offset)); offset += 2;
            state[1].delta = (BitConverter.ToInt16(adpcmInputBuf, offset)); offset += 2;

            state[0].sample1 = (BitConverter.ToInt16(adpcmInputBuf, offset)); offset += 2;
            state[1].sample1 = (BitConverter.ToInt16(adpcmInputBuf, offset)); offset += 2;

            state[0].sample2 = (BitConverter.ToInt16(adpcmInputBuf, offset)); offset += 2;
            state[1].sample2 = (BitConverter.ToInt16(adpcmInputBuf, offset)); offset += 2;

            // Decode

            pcmResultBuf[0] = state[0].sample2;
            pcmResultBuf[1] = state[1].sample2;

            pcmResultBuf[2] = state[0].sample1;
            pcmResultBuf[3] = state[1].sample1;

            int byteValue;
            for (int i = 4; i < outputLength;)
            {
                byteValue = adpcmInputBuf[offset++];

                pcmResultBuf[i++] = (DecodeSample(byteValue >> 4, ref state[0]));
                pcmResultBuf[i++] = (DecodeSample(byteValue & 0xf, ref state[1]));
            }
        }

        private short DecodeSample(int nibble, ref adpcm_state state)
        {
            int signed = nibble >= 8 ? nibble - 16 : nibble;

            int predicator = ((state.sample1 * state.coeff1 +
                              state.sample2 * state.coeff2) >> 8) +
                              (signed * state.delta);

            predicator = Clamp(predicator, -0x8000, 0x7fff);

            state.sample2 = state.sample1;
            state.sample1 = (short)predicator;

            state.delta = (int)Math.Floor((float)(ADAPTATION_TABLE[nibble] * state.delta) / 256f);
            if (state.delta < 16) state.delta = 16;

            return (short)predicator;
        }


    }
}

