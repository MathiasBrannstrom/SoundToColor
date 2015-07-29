using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Utilities;

namespace SoundToColorApplication
{
    public interface ISound2ColorMapping
    {
        Color Color {get;set;}

        double IntensityMultiplier { get; set; }

        double GetIntensityFromSoundFrequency(Frequency freq);
    }

    public interface ILinearSound2ColorMapping : ISound2ColorMapping
    {
        Frequency SoundFrequencyMidpoint { get; set; }
        Frequency SoundFrequencySpanWidth { get; set; }
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

        public abstract double GetIntensityFromSoundFrequency(Frequency freq);
    }

    /// <summary>
    /// This Sound2Color mapping gives high intensities for the color at a 
    /// given midpoint and then linearly drops in both directions based on
    /// a given width.
    /// </summary>
    public class LinearSound2ColorMapping : Sound2ColorMapping, ILinearSound2ColorMapping
    {
        public Frequency SoundFrequencyMidpoint { get; set; }

        public Frequency SoundFrequencySpanWidth { get; set; }

        public override double GetIntensityFromSoundFrequency(Frequency freq)
        {
            var val = Math.Max(1 - Math.Abs((freq.Value - SoundFrequencyMidpoint.Value) / SoundFrequencySpanWidth.Value), 0);
            return val*IntensityMultiplier;
        }
    }
}
