using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoundToColorApplication
{
    public static class FrequencyAnalyzer
    {
        public static void Analyze(short[] amplitudes, int samplingRate, 
            out double[] frequencyAmplitudes, out double[] idx2Frequency)
        {
            idx2Frequency = Fourier.FrequencyScale(amplitudes.Length, samplingRate);

            var fft = amplitudes.Select(a => new Complex(a, 0)).ToArray();
            Fourier.Forward(fft);
            frequencyAmplitudes = fft.Select(c => Math.Abs(c.Real)).ToArray();
        }
    }
}
