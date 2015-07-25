using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SoundToColorApplication
{
    interface ISound2ColorMapping
    {
        Color Color {get;set;}

        double IntensityMultiplier { get; set; }

        double GetIntensityFromSoundFrequency(double freq);
    }

    interface ILinearSound2ColorMapping : ISound2ColorMapping
    {
        double SoundFrequencyMidpoint { get; set; }
        double SoundFrequencySpanWidth { get; set; }
    }

    public abstract class Sound2ColorMapping : ISound2ColorMapping
    {
        public Color Color { get; set; }

        private double _intensityMultiplier = 1;
        public double IntensityMultiplier 
        { 
            get 
            { 
                return _intensityMultiplier; 
            } 
            set 
            { 
                _intensityMultiplier = value; 
            } 
        }

        public abstract double GetIntensityFromSoundFrequency(double freq);
    }

    public class LinearSound2ColorMapping : Sound2ColorMapping, ILinearSound2ColorMapping
    {
        public double SoundFrequencyMidpoint { get; set; }

        public double SoundFrequencySpanWidth { get; set; }

        public override double GetIntensityFromSoundFrequency(double freq)
        {
            var val = Math.Max(1 - Math.Abs((freq - SoundFrequencyMidpoint) / SoundFrequencySpanWidth), 0);
            return val*IntensityMultiplier;
        }
    }
}
