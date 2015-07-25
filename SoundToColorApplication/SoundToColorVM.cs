using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SoundToColorApplication
{
    /// <summary>
    /// Currently holds some UI logic as well, should be moved.
    /// </summary>
    class SoundToColorVM
    {
        private const double factor = 100.0 / Int16.MaxValue;
        private const double translation = 300;
        private Color _oldColor;

        public Color ConvertedColor { get; private set; }

        public List<Path> Paths { get; private set; }

        public void NewSamples(short[] samples, int samplingRate)
        {
            var amps = samples;

            double[] y2, idx2Freq;

            FrequencyAnalyzer.Analyze(amps, samplingRate, out y2, out idx2Freq);
            var y = amps;

            List<PathSegment> ampList = new List<PathSegment>();
            List<PathSegment> freqList = new List<PathSegment>();
            List<PathSegment> listBlue = new List<PathSegment>();
            List<PathSegment> listGreen = new List<PathSegment>();
            List<PathSegment> listRed = new List<PathSegment>();

            Point firstPointAmp = new Point();
            Point firstPointFreq = new Point();

            Point firstPointBlue = new Point();
            Point firstPointGreen = new Point();
            Point firstPointRed = new Point();

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
                var freqPoint = new Point(8 * x, -y2[x] * factor + translation * 2);
                var bluePoint = new Point(8 * x, b * 50 + translation * 2);
                var greenPoint = new Point(8 * x, g * 50 + translation * 2);
                var redPoint = new Point(8 * x, r * 50 + translation * 2);

                if (x == 0)
                {
                    firstPointAmp = ampPoint;
                    firstPointFreq = freqPoint;
                    firstPointBlue = bluePoint;
                    firstPointGreen = greenPoint;
                    firstPointRed = redPoint;
                }
                else
                {
                    ampList.Add(new LineSegment(ampPoint, true));

                    if (x <= y.Length / 2)
                    {
                        freqList.Add(new LineSegment(freqPoint, true));
                        listBlue.Add(new LineSegment(bluePoint, true));
                        listGreen.Add(new LineSegment(greenPoint, true));
                        listRed.Add(new LineSegment(redPoint, true));
                    }
                }
            }

            if (_oldColor == null)
                ConvertedColor = Color.FromRgb((byte)Math.Min(255, red), (byte)Math.Min(255, green), (byte)Math.Min(255, blue));
            else
                ConvertedColor = Color.FromRgb(
                    (byte)(Math.Min(255, red) * 0.1 + _oldColor.R * 0.9),
                    (byte)(Math.Min(255, green) * 0.1 + _oldColor.G * 0.9),
                    (byte)(Math.Min(255, blue) * 0.1 + _oldColor.B * 0.9));

           _oldColor = ConvertedColor;


            var firstPoints = new[] { firstPointAmp, firstPointFreq, firstPointBlue, firstPointGreen, firstPointRed };
            var segmentLists = new[] { ampList, freqList, listBlue, listGreen, listRed };
            var color = new[] { Brushes.Black, Brushes.Black, Brushes.Blue, Brushes.Green, Brushes.Red };

            var paths = new List<Path>();

            for (int i = 0; i < firstPoints.Length; i++)
            {
                PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoints[i], segmentLists[i], false) });
                paths.Add(new Path() { Data = pg, Stroke = color[i], StrokeThickness = 2 });
            }
            Paths = paths;
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
