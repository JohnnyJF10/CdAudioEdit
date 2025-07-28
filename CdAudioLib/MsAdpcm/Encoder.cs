

using CdAudioLib.Utils;

namespace CdAudioLib.MsAdpcm
{
    public class Encoder : AdpcmBase
    {
        public int numOfAdpcmTestSamples = 42;

        public Encoder()
        {
            coefficient = new short[][]
            {
                new short[] { 256, 512, 0, 192, 240, 460, 392 },
                new short[] { 0, -256, 0, 64, 0, -208, -232 }
            };
        }

        public Encoder(short[][] coefficientTable)
            => coefficient = coefficientTable;

        public void EncodeBlock(short[] pcmInputBuf, ref byte[] adpcmResultBuf)
        {
            int nSamplesToFindBestPred = numOfAdpcmTestSamples;

            if (adpcmResultBuf.Length != CdAudioConstants.TR_AUDIO_NUM_OF_ADPCM_BYTES_PER_BLOCK)
                throw new Exception("Error, wrong target array size, must be " +
                    $"{CdAudioConstants.TR_AUDIO_NUM_OF_ADPCM_BYTES_PER_BLOCK}, but is {adpcmResultBuf.Length}.");

            short[] testSamples0 = new short[nSamplesToFindBestPred];
            short[] testSamples1 = new short[nSamplesToFindBestPred];

            for (int i = 0; i < nSamplesToFindBestPred; i++)
            {
                testSamples0[i] = pcmInputBuf[2 * i];
                testSamples1[i] = pcmInputBuf[2 * i + 1];
            }

            adpcm_state[] state = new[]
            {
                new adpcm_state(),
                new adpcm_state()
            };

            int bestPredictorIndex0 = GetBestPredictorIndex(testSamples0, ref state[0]);
            int bestPredictorIndex1 = GetBestPredictorIndex(testSamples1, ref state[1]);

            state[0].coeff1 = coefficient[0][bestPredictorIndex0];
            state[0].coeff2 = coefficient[1][bestPredictorIndex0];

            state[1].coeff1 = coefficient[0][bestPredictorIndex1];
            state[1].coeff2 = coefficient[1][bestPredictorIndex1];

            int outPos = 0;
            int inPos = 0;

            // Encode predictor.
            adpcmResultBuf[outPos++] = (byte)bestPredictorIndex0;
            adpcmResultBuf[outPos++] = (byte)bestPredictorIndex1;

            // Encode delta.
            adpcmResultBuf[outPos++] = (byte)(state[0].delta & 0xff);
            adpcmResultBuf[outPos++] = (byte)(state[0].delta >> 8);

            adpcmResultBuf[outPos++] = (byte)(state[1].delta & 0xff);
            adpcmResultBuf[outPos++] = (byte)(state[1].delta >> 8);


            // Encode first two samples.
            state[0].sample2 = pcmInputBuf[inPos++];
            state[1].sample2 = pcmInputBuf[inPos++];

            state[0].sample1 = pcmInputBuf[inPos++];
            state[1].sample1 = pcmInputBuf[inPos++];

            adpcmResultBuf[outPos++] = (byte)(state[0].sample1 & 0xff);
            adpcmResultBuf[outPos++] = (byte)(state[0].sample1 >> 8);

            adpcmResultBuf[outPos++] = (byte)(state[1].sample1 & 0xff);
            adpcmResultBuf[outPos++] = (byte)(state[1].sample1 >> 8);

            adpcmResultBuf[outPos++] = (byte)(state[0].sample2 & 0xff);
            adpcmResultBuf[outPos++] = (byte)(state[0].sample2 >> 8);

            adpcmResultBuf[outPos++] = (byte)(state[1].sample2 & 0xff);
            adpcmResultBuf[outPos++] = (byte)(state[1].sample2 >> 8);

            for (; outPos < adpcmResultBuf.Length;)
            {
                byte code1 = EncodeSample(pcmInputBuf[inPos++], ref state[0]);
                byte code2 = EncodeSample(pcmInputBuf[inPos++], ref state[1]);

                adpcmResultBuf[outPos++] = (byte)((code1 << 4) | code2);
            }
        }


        private byte EncodeSample(short sample, ref adpcm_state state)
        {
            int predictor = (state.sample1 * state.coeff1 + state.sample2 * state.coeff2) >> 8;
            int code = sample - predictor;
            int bias = state.delta / 2;
            if (code < 0)
                bias = -bias;
            code = (code + bias) / state.delta;
            code = Clamp(code, -8, 7) & 0xf;

            predictor += ((code & 0x8) != 0) ? (code - 0x10) * state.delta : code * state.delta;

            state.sample2 = state.sample1;
            state.sample1 = Clamp(predictor, -0x8000, 0x7fff);
            state.delta = (ADAPTATION_TABLE[code] * state.delta) >> 8;
            if (state.delta < 16)
                state.delta = 16;
            return (byte)code;
        }

        private int GetBestPredictorIndex(short[] inputSamples, ref adpcm_state state)
        {
            int inputCount = inputSamples.Length;

            int m_numCoefficients = coefficient[0].Length;

            int bestPredictorIndex = 0;
            int bestPredictorError = Int32.MaxValue;
            for (int k = 0; k < m_numCoefficients; k++)
            {
                int a0 = coefficient[0][k];
                int a1 = coefficient[1][k];

                int currentPredictorError = 0;
                for (int i = 2; i < inputCount; i++)
                {
                    int error = Math.Abs(inputSamples[i] -
                        ((a0 * inputSamples[i - 1] +
                        a1 * inputSamples[i - 2]) >> 8));
                    currentPredictorError += error;
                }
                currentPredictorError /= 4 * inputCount;

                if (currentPredictorError < bestPredictorError)
                {
                    bestPredictorError = currentPredictorError;
                    bestPredictorIndex = k;
                }

                if (currentPredictorError == 0) break;
            }
            if (bestPredictorError < 16) bestPredictorError = 16;

            state.delta = bestPredictorError;
            return bestPredictorIndex;
        }

    };
}


