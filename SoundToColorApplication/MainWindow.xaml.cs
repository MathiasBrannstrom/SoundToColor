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
        private IValueHolder<short[]> _samples;
        private IValueHolder<int> _samplingRate;
        public MainWindow()
        {
            InitializeComponent();

            _samples = new ValueHolder<short[]>();
            _samplingRate = new ValueHolder<int>();

            _soundManager = new SoundManager();
            _soundManager.NewSamples += HandleNewSamples;
            _soundManager.StartRecording();

            _samplingRate.Value = _soundManager.SamplingRate;

            _soundVisualizerVM = new SoundVisualizerVM(_samples, _samplingRate);
            
            _soundVisualizer = new SoundVisualizerControl(_soundVisualizerVM);

            MainGrid.Children.Add(_soundVisualizer);
        }
        
        private void HandleNewSamples(short[] newSamples)
        {
            _samples.Value = newSamples;
        }
    }
}
