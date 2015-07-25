using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Utilities;
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
        private IValueHolder<short[]> _amplitudes;
        private IValueHolder<int> _samplingRate;
        public MainWindow()
        {
            InitializeComponent();

            _amplitudes = new ValueHolder<short[]>();
            _samplingRate = new ValueHolder<int>();

            _soundManager = new SoundManager();
            _soundManager.NewSamples += HandleNewSamples;
            _soundManager.StartRecording();

            _samplingRate.Value = _soundManager.SamplingRate;

            _soundVisualizerVM = new SoundVisualizerVM(_amplitudes, _samplingRate);
            
            _soundVisualizer = new SoundVisualizerControl();
            MainGrid.Children.Add(_soundVisualizer);
        }
        
        private void HandleNewSamples(short[] newSamples)
        {
            _amplitudes.Value = newSamples;
            _soundVisualizer.Background = new SolidColorBrush(_soundVisualizerVM.Color.Value);

            _soundVisualizer.MainGrid.Children.Clear();
            //This code should be replaced soon.
            foreach (var path in _soundVisualizerVM.Paths)
            {
                _soundVisualizer.MainGrid.Children.Add(path);
            }
        }
    }
}
