using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities;

namespace SoundToColorApplication
{
    class SoundVisualizerVM
    {
        private const double factor = 100.0 / Int16.MaxValue;
        private const double translation = 300;
        private Color _oldColor;
        private IValueHolderReadOnly<int> _samplingRate;
        private ValueHolder<Color> _color;

        public IValueHolderReadOnly<Color> Color { get { return _color; } }

        public IValueHolderReadOnly<short[]> Amplitudes { get; private set; }

        public IValueHolderReadOnly<double[]> Frequencies { get; private set; }

        public List<Path> Paths { get; private set; }

        public List<Path> ColorMappingPaths { get; private set; }

        private List<ISound2ColorMapping> _sound2ColorMappings;

        public SoundVisualizerVM(IValueHolderReadOnly<short[]> amps, IValueHolderReadOnly<int> samplingRate)
        {
            _color = new ValueHolder<Color>();
            _samplingRate = samplingRate;
            Frequencies = new ValueHolder<double[]>();
            Amplitudes = amps;

            _sound2ColorMappings = new List<ISound2ColorMapping>{ 
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(0,0,255),
                    IntensityMultiplier = 1,
                    SoundFrequencyMidpoint = 280,
                    SoundFrequencySpanWidth = 300},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(0,255,0),
                    IntensityMultiplier = 0.9,
                    SoundFrequencyMidpoint = 600,
                    SoundFrequencySpanWidth = 500},
                new LinearSound2ColorMapping{
                    Color = System.Windows.Media.Color.FromRgb(255,0,0),
                    IntensityMultiplier = 0.8,
                    SoundFrequencyMidpoint = 1600,
                    SoundFrequencySpanWidth = 1000}};

            Amplitudes.PropertyChanged += HandleAmplitudesChanged;
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

        public void UpdateSound2ColorMappingLines()
        {
            ColorMappingPaths = new List<Path>();
            foreach (var mapping in _sound2ColorMappings)
            {
                var pathSegments = new List<PathSegment>();

                var firstPoint = new Point();
                for (int x = 0; x < 200; x++)
                {
                    var freq = x * 20;
                    var point = new Point(Frequency2Pixel(freq), mapping.GetIntensityFromSoundFrequency(freq)*50 + translation * 2);

                    if (x == 0)
                    {
                        firstPoint = point;
                    }
                    else
                    {
                        pathSegments.Add(new LineSegment(point,true));
                    }
                }
                PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoint, pathSegments, false) });

                ColorMappingPaths.Add(new Path() { Data = pg, Stroke = new SolidColorBrush(mapping.Color), StrokeThickness = 2 });
            }
        }

        private void Update()
        {
            var amps = Amplitudes.Value;

            double[] y2, idx2Freq;

            FrequencyAnalyzer.Analyze(amps, _samplingRate.Value, out y2, out idx2Freq);
            var y = amps;

            List<PathSegment> ampList = new List<PathSegment>();
            List<PathSegment> freqList = new List<PathSegment>();

            Point firstPointAmp = new Point();
            Point firstPointFreq = new Point();


            var red = 0.0;
            var green = 0.0;
            var blue = 0.0;

            for (int x = 0; x < y.Length; x++)
            {
                var r = Red(idx2Freq[x]);
                var g = Green(idx2Freq[x]);
                var b = Blue(idx2Freq[x]);

                if (x < y.Length / 2)
                {
                    red += y2[x] * r * 0.002;
                    blue += y2[x] * b * 0.002;
                    green += y2[x] * g * 0.002;
                }

                var ampPoint = new Point(4 * x, y[x] * factor + translation);
                var freqPoint = new Point(Frequency2Pixel(idx2Freq[x]), -y2[x] * factor + translation * 2);

                if (x == 0)
                {
                    firstPointAmp = ampPoint;
                    firstPointFreq = freqPoint;
                }
                else
                {
                    ampList.Add(new LineSegment(ampPoint, true));

                    if (x <= y.Length / 2)
                    {
                        freqList.Add(new LineSegment(freqPoint, true));
                    }
                }
            }

            if (_oldColor == null)
                _color.Value = System.Windows.Media.Color.FromRgb((byte)Math.Min(255, red), (byte)Math.Min(255, green), (byte)Math.Min(255, blue));
            else
                _color.Value = System.Windows.Media.Color.FromRgb(
                    (byte)(Math.Min(255, red) * 0.1 + _oldColor.R * 0.9),
                    (byte)(Math.Min(255, green) * 0.1 + _oldColor.G * 0.9),
                    (byte)(Math.Min(255, blue) * 0.1 + _oldColor.B * 0.9));

            _oldColor = _color.Value;

            var firstPoints = new[] { firstPointAmp, firstPointFreq};
            var segmentLists = new[] { ampList, freqList };
            var color = new[] { Brushes.Black, Brushes.Black};

            var paths = new List<Path>();

            for (int i = 0; i < firstPoints.Length; i++)
            {
                PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoints[i], segmentLists[i], false) });
                paths.Add(new Path() { Data = pg, Stroke = color[i], StrokeThickness = 2 });
            }

            Paths = paths;
        }

        private double Frequency2Pixel(double freq)
        {
            return freq / 2;
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
