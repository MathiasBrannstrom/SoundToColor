using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SoundToColorApplication
{
    class SoundManager
    {
        private WaveIn _recorder;
        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOut _player;

        public int SamplingRate { get; private set; }
        public int BytesPerSample { get; private set; }

        public void StartPlayBack()
        {
            if (_player != null)
                throw new InvalidOperationException("Can't begin playback when already playing");
            if(_recorder == null)
                throw new InvalidOperationException("Can't begin playback when not recording");

            _player = new WaveOut();
            _bufferedWaveProvider = new BufferedWaveProvider(_recorder.WaveFormat);

            _player.Init(_bufferedWaveProvider);
            _player.Play();
        }

        public void StopPlayback()
        {
            _player.Stop();
            _player.Dispose();
            _bufferedWaveProvider.ClearBuffer();
            _bufferedWaveProvider = null;
            _player = null;
        }

        public void StartRecording()
        {
            if (_recorder != null)
                throw new InvalidOperationException("Can't begin listening when already listening");

            _recorder = new WaveIn();
            _recorder.BufferMilliseconds = 50;
            _recorder.DataAvailable += HandleDataAvailable;
            
            SamplingRate = _recorder.WaveFormat.SampleRate;

            BytesPerSample = _recorder.WaveFormat.BitsPerSample / 8;

            _recorder.StartRecording();
        }

        public void StopRecording()
        {
            if (_recorder != null)
            {
                _recorder.StopRecording();
                _recorder.DataAvailable -= HandleDataAvailable;
                _recorder = null;
                SamplingRate = -1;
                BytesPerSample = -1;
            }
        }

        private void HandleDataAvailable(object sender, WaveInEventArgs e)
        {
            short[] samples = new short[e.BytesRecorded / BytesPerSample];

            for (int i = 0; i < e.BytesRecorded; i += BytesPerSample)
                samples[i / BytesPerSample] = BitConverter.ToInt16(e.Buffer, i);

            NewSamples(samples);

            if (_player != null)
            {
                _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }

        public void Dispose()
        {
            StopRecording();
            StopPlayback();
        }

        public delegate void NewSamplesEventHandler(short[] newSamples);
        public event NewSamplesEventHandler NewSamples;

    }
}
