using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Utilities;

namespace SoundToColorApplication
{
    public class SoundVisualizerVM
    {
        private IValueHolderReadOnly<int> _samplingRate;
        private IValueHolderReadOnly<short[]> _samples;

        public IValueHolderReadOnly<Color> Color { get { return _color; } }
        private ValueHolder<Color> _color;

        public IValueHolderReadOnly<IReadOnlyList<KeyValuePair<Frequency, double>>> Frequencies { get { return _frequencies; } }
        private IValueHolder<IReadOnlyList<KeyValuePair<Frequency, double>>> _frequencies;

        public IValueHolderReadOnly<IReadOnlyList<KeyValuePair<int, double>>> Amplitudes { get { return _amplitudes; } }
        private IValueHolder<IReadOnlyList<KeyValuePair<int, double>>> _amplitudes;

        public List<ISound2ColorMapping> Sound2ColorMappings { get; set; }

        private const double IntensityLimit = 350000;
        private const double ColorChangingSpeed = 0.1;

        public SoundVisualizerVM(IValueHolderReadOnly<short[]> samples, IValueHolderReadOnly<int> samplingRate)
        {
            _color = new ValueHolder<Color>();
            _samplingRate = samplingRate;
            _samples = samples;
            _frequencies = new ValueHolder<IReadOnlyList<KeyValuePair<Frequency, double>>>();
            _amplitudes = new ValueHolder<IReadOnlyList<KeyValuePair<int, double>>>();

            Sound2ColorMappings = new List<ISound2ColorMapping>{ 
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(0,0,255),
                    IntensityMultiplier = 1,
                    SoundFrequencyMidpoint = new Frequency(280),
                    SoundFrequencySpanWidth = new Frequency(300)},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(0,255,0),
                    IntensityMultiplier = 1,
                    SoundFrequencyMidpoint = new Frequency(600),
                    SoundFrequencySpanWidth = new Frequency(500)},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(255,0,0),
                    IntensityMultiplier = 1,
                    SoundFrequencyMidpoint = new Frequency(1600),
                    SoundFrequencySpanWidth = new Frequency(1000)}};

            _samples.PropertyChanged += HandleAmplitudesChanged;
            _samplingRate.PropertyChanged += HandleSamplingRateChanged;
        }

        private void HandleSamplingRateChanged(object sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        private void HandleAmplitudesChanged(object sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            double[] frequencyIntensities;
            Frequency[] idx2Freq;

            FrequencyAnalyzer.Analyze(_samples.Value, _samplingRate.Value, out frequencyIntensities, out idx2Freq);

            var aggregatedColorIntensities = new double[Sound2ColorMappings.Count];

            var amplitudes = new List<KeyValuePair<int,double>>();
            var frequencies = new List<KeyValuePair<Frequency,double>>();

            for (int x = 0; x < _samples.Value.Length; x++)
            {
                if (x < _samples.Value.Length / 2)
                {
                    int c = 0;
                    foreach (var mapping in Sound2ColorMappings)
                    {
                        var intensity = frequencyIntensities[x] * mapping.IntensityMultiplier *
                            mapping.GetIntensityFromSoundFrequency(idx2Freq[x]);
                        aggregatedColorIntensities[c++] += intensity;
                    }
                }
 
                amplitudes.Add(new KeyValuePair<int,double>(x, _samples.Value[x]));
                if (x <= _samples.Value.Length / 2)
                    frequencies.Add(new KeyValuePair<Frequency, double>(idx2Freq[x], frequencyIntensities[x]));
            }

            double redIntensity = 0, blueIntensity = 0, greenIntensity = 0;
            for (int i = 0; i < Sound2ColorMappings.Count; i++)
            {
                redIntensity += aggregatedColorIntensities[i] * Sound2ColorMappings[i].Color.R;
                greenIntensity += aggregatedColorIntensities[i] * Sound2ColorMappings[i].Color.G;
                blueIntensity += aggregatedColorIntensities[i] * Sound2ColorMappings[i].Color.B;
            }

            var totalIntensity = redIntensity + blueIntensity + greenIntensity;
            var redPart = redIntensity / totalIntensity;
            var bluePart = blueIntensity / totalIntensity;
            var greenPart = greenIntensity / totalIntensity;

            var scaling = totalIntensity / IntensityLimit;

            var red = redPart * scaling;
            var blue = bluePart * scaling;
            var green = greenPart * scaling;

                if (_color.Value == null)
                    _color.Value = System.Windows.Media.Color.FromRgb((byte)Math.Min(255, red), (byte)Math.Min(255, green), (byte)Math.Min(255, blue));
                else
                    _color.Value = System.Windows.Media.Color.FromRgb(
                        (byte)(Math.Min(255, red) * ColorChangingSpeed + _color.Value.R * (1-ColorChangingSpeed)),
                        (byte)(Math.Min(255, green) * ColorChangingSpeed + _color.Value.G * (1-ColorChangingSpeed)),
                        (byte)(Math.Min(255, blue) * ColorChangingSpeed + _color.Value.B * (1-ColorChangingSpeed)));

            _amplitudes.Value = amplitudes; 
            _frequencies.Value = frequencies;
        }
    }
}
