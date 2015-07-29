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

        public IValueHolderReadOnly<double> AverageIntensity { get { return _averageIntensity; } }
        private IValueHolder<double> _averageIntensity;

        public double MinIntensity = 10000000;
        public double MaxIntensity = 200000000;
        private const double ColorChangingSpeed = 0.1;

        public SoundVisualizerVM(IValueHolderReadOnly<short[]> samples, IValueHolderReadOnly<int> samplingRate)
        {
            _color = new ValueHolder<Color>();
            _samplingRate = samplingRate;
            _samples = samples;
            _frequencies = new ValueHolder<IReadOnlyList<KeyValuePair<Frequency, double>>>();
            _amplitudes = new ValueHolder<IReadOnlyList<KeyValuePair<int, double>>>();
            _averageIntensity = new ValueHolder<double>();

            Sound2ColorMappings = new List<ISound2ColorMapping>{ 
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(0,0,255),
                    IntensityMultiplier = 0.8,
                    SoundFrequencyMidpoint = new Frequency(250),
                    SoundFrequencySpanWidth = new Frequency(300)},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(0,255,0),
                    IntensityMultiplier = 1.1,
                    SoundFrequencyMidpoint = new Frequency(700),
                    SoundFrequencySpanWidth = new Frequency(450)},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(255,0,0),
                    IntensityMultiplier = 0.9,
                    SoundFrequencyMidpoint = new Frequency(1500),
                    SoundFrequencySpanWidth = new Frequency(1000)},
            new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(255,0,0),
                    IntensityMultiplier = 0.4,
                    SoundFrequencyMidpoint = new Frequency(2600),
                    SoundFrequencySpanWidth = new Frequency(1400)}};

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

            double redPart, greenPart, bluePart;

            GetColorFromIntensities(redIntensity, greenIntensity, blueIntensity,
                out redPart, out greenPart, out bluePart);

            if (_color.Value == null)
                _color.Value = System.Windows.Media.Color.FromRgb(
                    (byte)(255 * redPart),
                    (byte)(255 * greenPart),
                    (byte)(255 * bluePart));
            else
                _color.Value = System.Windows.Media.Color.FromRgb(
                    (byte)((255 * redPart) * ColorChangingSpeed + _color.Value.R * (1 - ColorChangingSpeed)),
                    (byte)((255 * greenPart) * ColorChangingSpeed + _color.Value.G * (1 - ColorChangingSpeed)),
                    (byte)((255 * bluePart) * ColorChangingSpeed + _color.Value.B * (1 - ColorChangingSpeed)));

            _amplitudes.Value = amplitudes; 
            _frequencies.Value = frequencies;
        }

        // Update this function later so that it doesn't depend on and updates local averageintensities, it
        // should basically be a static function.
        private void GetColorFromIntensities(double redIntensity, double greenIntensity, double blueIntensity,
        out double redPart, out double greenPart, out double bluePart)
        {
            var totalIntensity = redIntensity + blueIntensity + greenIntensity;
            redPart = totalIntensity == 0 ? 0 : redIntensity / totalIntensity;
            bluePart = totalIntensity == 0 ? 0 : blueIntensity / totalIntensity;
            greenPart = totalIntensity == 0 ? 0 : greenIntensity / totalIntensity;

            var newAverageIntensity = _averageIntensity.Value * (1 - ColorChangingSpeed) + totalIntensity * ColorChangingSpeed;
            _averageIntensity.Value = Math.Min(Math.Max(newAverageIntensity, MinIntensity), MaxIntensity);

            Func<double, double> intensify = (d) =>
            {
                if (d >= 1.0 / 3)
                {
                    d = Math.Sqrt((d - 1.0 / 3) * 3.0 / 2) * 2.0 / 3 + 1.0 / 3;
                }
                else
                {
                    d = -Math.Sqrt(Math.Abs(d - 1.0 / 3) * 3.0) * 1.0 / 3 + 1.0 / 3;
                }
                return d;
            };


            var scaling = totalIntensity / _averageIntensity.Value;

            redPart = Math.Min(Math.Max(redPart * scaling, 0), 1);
            bluePart = Math.Min(Math.Max(bluePart * scaling, 0), 1);
            greenPart = Math.Min(Math.Max(greenPart * scaling, 0), 1);


            redPart = intensify(redPart);
            bluePart = intensify(bluePart);
            greenPart = intensify(greenPart);

            var normalizingFactor = (redPart + bluePart + greenPart) / scaling;

            redPart /= normalizingFactor;
            bluePart /= normalizingFactor;
            greenPart /= normalizingFactor;
        }
    }
}
