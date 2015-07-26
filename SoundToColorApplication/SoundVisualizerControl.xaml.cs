using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;

namespace SoundToColorApplication
{
    /// <summary>
    /// Interaction logic for SoundVisualizerControl.xaml
    /// </summary>
    public partial class SoundVisualizerControl : UserControl
    {
        private SoundVisualizerVM _viewModel;
        public SoundVisualizerControl(SoundVisualizerVM viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            UpdateSound2ColorMappingLines(_viewModel.Sound2ColorMappings);
            _viewModel.Frequencies.PropertyChanged += HandleFrequenciesChanged;
            _viewModel.Amplitudes.PropertyChanged += HandleAmplitudesChanged;
            _viewModel.Color.PropertyChanged += HandleColorChanged;
            _viewModel.AverageIntensity.PropertyChanged += HandleAverageIntensityChanged;

            CreateFrequencyLabels();
        }

        private void CreateFrequencyLabels()
        {
            for (int freq = 0; freq <= 8000; freq += 100)
            {
                var x = Frequency2Pixel(new Frequency(freq));
                var label = new Label { Content = freq, HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(0)};

                label.RenderTransform = new TranslateTransform(x,0);
                FrequencyLabelsGrid.Children.Add(label);
            }
        }

        private void HandleAverageIntensityChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var y = AverageAmplitude.ActualHeight - (_viewModel.AverageIntensity.Value-_viewModel.MinIntensity)/(_viewModel.MaxIntensity-_viewModel.MinIntensity)*AverageAmplitude.ActualHeight;
            var firstPoint = new Point(0, y);
            var pathSegments = new List<PathSegment> { new LineSegment(new Point(AverageAmplitude.ActualWidth, y), true) };


            PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoint, pathSegments, false) });

            AverageAmplitude.Child = new Path() { Data = pg, Stroke = Brushes.White, StrokeThickness = 2, VerticalAlignment = VerticalAlignment.Center };
        }

        private void HandleColorChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Background = new SolidColorBrush(_viewModel.Color.Value);
        }

        private void HandleAmplitudesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pathSegments = new List<PathSegment>();

            var firstPoint = new Point();
            foreach (var kvp in _viewModel.Amplitudes.Value)
            {
                var point = new Point(((double)kvp.Key)/_viewModel.Amplitudes.Value.Count*AmplitudeCurve.ActualWidth, kvp.Value/300);

                if (firstPoint == null)
                    firstPoint = point;
                else
                    pathSegments.Add(new LineSegment(point, true));
            }

            PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoint, pathSegments, false) });

            AmplitudeCurve.Child = new Path() { Data = pg, Stroke = Brushes.Black, StrokeThickness = 2, VerticalAlignment = VerticalAlignment.Center };
        }

        private void HandleFrequenciesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pathSegments = new List<PathSegment>();

            var firstPoint = new Point();
            foreach(var kvp in _viewModel.Frequencies.Value) 
            {
                var point = new Point(Frequency2Pixel(kvp.Key), -kvp.Value*0.002);

                if (firstPoint == null)
                    firstPoint = point;
                else
                    pathSegments.Add(new LineSegment(point, true));
            }

            PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoint, pathSegments, false) });

            FrequencyCurve.Child = new Path() { Data = pg, Stroke = Brushes.Black, StrokeThickness = 2, VerticalAlignment= VerticalAlignment.Bottom };
        }

        public void UpdateSound2ColorMappingLines(List<ISound2ColorMapping> sound2ColorMappings)
        {
            ColorMappings.Children.Clear();
            foreach (var mapping in sound2ColorMappings)
            {
                var pathSegments = new List<PathSegment>();

                var firstPoint = new Point();
                for (int x = 0; x < 200; x++)
                {
                    var freq = new Frequency(x * 20);
                    var point = new Point(Frequency2Pixel(freq), -mapping.GetIntensityFromSoundFrequency(freq) * 50);

                    if (x == 0)
                        firstPoint = point;
                    else
                        pathSegments.Add(new LineSegment(point, true));
                }
                PathGeometry pg = new PathGeometry(new[] { new PathFigure(firstPoint, pathSegments, false) });

                ColorMappings.Children.Add(new Path() { Data = pg, Stroke = new SolidColorBrush(mapping.Color), StrokeThickness = 2, VerticalAlignment = VerticalAlignment.Bottom });
            }
        }

        private double Frequency2Pixel(Frequency freq)
        {
            return freq.Value ;
        }
    }
}
