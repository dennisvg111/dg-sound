using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DG.Sound
{
    public class Beat
    {
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }
        public float BeatFrequency { get; set; }

        public Beat()
        {
            Minutes = 0;
            Seconds = 0;
            Milliseconds = 0;
        }
        public Beat(int m, int s, int mil)
        {
            Minutes = m;
            Seconds = s;
            Milliseconds = mil;
        }
        public Beat(int m, int s, int mil, float f)
        {
            Minutes = m;
            Seconds = s;
            Milliseconds = mil;
            BeatFrequency = f;
        }

        public void SetTime(int m, int s, int mil)
        {
            Minutes = m;
            Seconds = s;
            Milliseconds = mil;
        }

        public static implicit operator TimeSpan(Beat input)
        {
            return new TimeSpan(0, 0, input.Minutes, input.Seconds, input.Milliseconds);
        }

        public static int operator -(Beat a, Beat b)
        {
            return (int)(((TimeSpan)a - b).TotalMilliseconds);
        }

        public override string ToString()
        {
            return $"{TimeString}, {BeatFrequency.ToString("N3")}";
        }

        public string TimeString
        {
            get
            {
                return $"{Minutes.ToString("D2")}:{Seconds.ToString("D2")}:{Milliseconds.ToString("D3")}";
            }
        }
    }
}
