using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SoundToColorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SoundManager _soundManager;
        private SoundToColorVM _soundToColorVM;
        public MainWindow()
        {
            InitializeComponent();
            _soundManager = new SoundManager();
            _soundManager.NewSamples += HandleNewSamples;
            _soundManager.StartRecording();
            _soundToColorVM = new SoundToColorVM();
        }
        
        private void HandleNewSamples(short[] newSamples)
        {
            grid.Children.Clear();
  
            _soundToColorVM.NewSamples(newSamples, _soundManager.SamplingRate);
            grid.Background = new SolidColorBrush(_soundToColorVM.ConvertedColor);
            foreach (var path in _soundToColorVM.Paths)
            {
                grid.Children.Add(path);
            }
        }
    }
}
