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
        private Point3DCollection _originalPoints;
        private WPF3DScene _scene;
        private GeometryModel3D _model;

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
            _scene.Add2DUI(viewport2D);
        }
        private void HandleAverageAmplitudeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Random r = new Random(0);
            var scaledValue = (Math.Pow(_soundVisualizerVM.AverageAmplitudeFromLastSampling.Value, 0.7)-20) / 100;
            _model.Transform = new TranslateTransform3D(0, 0, scaledValue - 0.7);

            var newPoints = new Point3DCollection();

            for (int i = 0; i < _originalPoints.Count; i++)
            {
                if (r.Next(20) > (0.16+scaledValue)*10)
                {
                    newPoints.Add(_originalPoints[i]);
                }
                else
                {
                    var oldPoint = _originalPoints[i];
                    var normal = _mesh.Normals[i];
                    var mult = r.Next(2) == 0 ? 1 : -1;
                    var dis = mult * Math.Pow((0.16+scaledValue)/10,2)*2;
                    newPoints.Add(oldPoint + normal * dis);
                }
            }

            _mesh.Positions = newPoints;
        }

        private GeometryModel3D Create3DModel()
        {
            _mesh = SimpleGeometry3D.CreateSphere(new Point3D(0, 0, 0), 0.2, 32, 32);
            _originalPoints = _mesh.Positions;
            var geometry = new GeometryModel3D();
            geometry.Geometry = _mesh;
            geometry.Material = new DiffuseMaterial { Brush = Brushes.SaddleBrown, AmbientColor = Color.FromRgb(150,150,150)};
            return geometry;
        }

        int c = 0;
        private MeshGeometry3D _mesh;
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
