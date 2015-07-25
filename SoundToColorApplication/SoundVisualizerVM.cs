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
        private const double factor = 100.0 / Int16.MaxValue;
        private const double translation = 300;
        private Color _oldColor;
        private IValueHolderReadOnly<int> _samplingRate;
        private IValueHolderReadOnly<short[]> _samples;
        private ValueHolder<Color> _color;

        public IValueHolderReadOnly<Color> Color { get { return _color; } }

        public IValueHolderReadOnly<IReadOnlyList<KeyValuePair<Frequency, double>>> Frequencies { get { return _frequencies; } }
        private IValueHolder<IReadOnlyList<KeyValuePair<Frequency, double>>> _frequencies;

        public IValueHolderReadOnly<IReadOnlyList<KeyValuePair<int, double>>> Amplitudes { get { return _amplitudes; } }
        private IValueHolder<IReadOnlyList<KeyValuePair<int, double>>> _amplitudes;

        public List<ISound2ColorMapping> Sound2ColorMappings { get; set; }

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
                    IntensityMultiplier = 0.9,
                    SoundFrequencyMidpoint = new Frequency(600),
                    SoundFrequencySpanWidth = new Frequency(500)},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(255,0,0),
                    IntensityMultiplier = 0.8,
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
            double[] y2, idx2Freq;

            FrequencyAnalyzer.Analyze(_samples.Value, _samplingRate.Value, out y2, out idx2Freq);

            var red = 0.0;
            var green = 0.0;
            var blue = 0.0;

            var amplitudes = new List<KeyValuePair<int,double>>();
            var frequencies = new List<KeyValuePair<Frequency,double>>();

            for (int x = 0; x < _samples.Value.Length; x++)
            {
                var r = Red(idx2Freq[x]);
                var g = Green(idx2Freq[x]);
                var b = Blue(idx2Freq[x]);

                if (x < _samples.Value.Length / 2)
                {
                    red += y2[x] * r * 0.002;
                    blue += y2[x] * b * 0.002;
                    green += y2[x] * g * 0.002;
                }

                amplitudes.Add(new KeyValuePair<int,double>(x, _samples.Value[x]));
                if (x <= _samples.Value.Length / 2)
                    frequencies.Add(new KeyValuePair<Frequency, double>(new Frequency(idx2Freq[x]), y2[x]));
            }

            if (_oldColor == null)
                _color.Value = System.Windows.Media.Color.FromRgb((byte)Math.Min(255, red), (byte)Math.Min(255, green), (byte)Math.Min(255, blue));
            else
                _color.Value = System.Windows.Media.Color.FromRgb(
                    (byte)(Math.Min(255, red) * 0.1 + _oldColor.R * 0.9),
                    (byte)(Math.Min(255, green) * 0.1 + _oldColor.G * 0.9),
                    (byte)(Math.Min(255, blue) * 0.1 + _oldColor.B * 0.9));

            _oldColor = _color.Value;

            _amplitudes.Value = amplitudes; 
            _frequencies.Value = frequencies;

        }

        private double Blue(double freq)
        {
            var val = Math.Max(1 - Math.Abs((freq - 280) / 300), 0);
            return val * 1.0;
        }

        private double Green(double freq)
        {
            var val = Math.Max(1 - Math.Abs((freq - 600) / 500), 0);
            return val * 0.9;
        }

        private double Red(double freq)
        {
            var val = Math.Max(1 - Math.Abs((freq - 1600) / 1000), 0);
            return val * 0.8;
        }
    }
}
