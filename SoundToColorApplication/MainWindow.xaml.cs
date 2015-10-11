using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Utilities;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
namespace SoundToColorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SoundManager _soundManager;
        private SoundVisualizerVM _soundVisualizerVM;
        private SoundVisualizerControl _soundVisualizer;
        private Viewport3D _viewport3D;

        private IValueHolder<short[]> _samples;
        private IValueHolder<int> _samplingRate;

        public MainWindow()
        {
            InitializeComponent();

            _samples = new ValueHolder<short[]>();
            _samplingRate = new ValueHolder<int>();

            _soundManager = new SoundManager(); 
            _soundManager.NewSamples += HandleNewSamples;
            _soundManager.StartRecording(0, 30);

            _samplingRate.Value = _soundManager.SamplingRate;

            _soundVisualizerVM = new SoundVisualizerVM(_samples, _samplingRate);
            
            _soundVisualizer = new SoundVisualizerControl(_soundVisualizerVM);
            //MainGrid.Children.Add(_soundVisualizer);
            
            _scene = new WPF3DScene();
            MainGrid.Children.Add(_scene);
            _model = Create3DModel();
            _scene.AddModel(_model);
            AddUIP3DPlane();
            _soundVisualizerVM.AverageAmplitudeFromLastSampling.PropertyChanged += HandleAverageAmplitudeChanged;

            DeviceButton.Click += DeviceButton_Click;
            DeviceButton.Content = _soundManager.GetAvailableDevices().Keys.First().ProductName;
        }

        private void AddUIP3DPlane()
        {
            var rectangle = SimpleGeometry3D.CreateRectangle(2, 3);
            var viewport2D = new Viewport2DVisual3D();
            viewport2D.Geometry = rectangle;
            viewport2D.Visual = _soundVisualizer;
            var material = new DiffuseMaterial { Brush = Brushes.Black };
            Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);
            viewport2D.Material = material;
            viewport2D.Transform = new TranslateTransform3D(0, -0.5, 0);
            //var brush = new VisualBrush { AutoLayoutContent = true };
            //brush.Visual = _soundVisualizer;
            //geometry.Material = new DiffuseMaterial { Brush = brush };
            //geometry.BackMaterial = new DiffuseMaterial { Brush = Brushes.Gray };

            _scene.Add2DUI(viewport2D);
        }

        private void HandleAverageAmplitudeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _model.Transform = new TranslateTransform3D(0, 0, (Math.Pow(_soundVisualizerVM.AverageAmplitudeFromLastSampling.Value, 0.7)-20) / 100 - 0.7);
        }

        private GeometryModel3D Create3DModel()
        {
            var mesh = SimpleGeometry3D.CreateSphere(new Point3D(0, 0, 0), 0.2, 16, 16);

            var geometry = new GeometryModel3D();
            geometry.Geometry = mesh;
            geometry.Material = new DiffuseMaterial { Brush = Brushes.SaddleBrown};
            geometry.BackMaterial = new DiffuseMaterial { Brush = Brushes.Gray };
            return geometry;
        }

        int c = 0;
        private WPF3DScene _scene;
        private GeometryModel3D _model;
        private void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var devices = _soundManager.GetAvailableDevices();
            c= (c+1)%devices.Count;
            DeviceButton.Content = devices.Keys.ToList()[c].ProductName;
            _soundManager.StopRecording();
            _soundManager.StartRecording(c);
        }

        private void HandleNewSamples(short[] newSamples)
        {
            _samples.Value = newSamples;
        }

        private void HandlePlaybackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (PlaybackButton.IsChecked == true)
            {
                if (_soundManager.IsPlaying)
                    _soundManager.StopPlayback();

                _soundManager.StartPlayBack();
            }
            else
            {
                _soundManager.StopPlayback();
            }
        }

    }
}
