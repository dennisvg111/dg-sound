using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DG.Sound
{
    class SpectrumAnalyzer
    {
        private int bins = 0;
        private TimeSpan currentTime = TimeSpan.Zero;

        public void Update(Complex[] fftResults)
        {
            if (fftResults.Length / 2 != bins)
            {
                bins = fftResults.Length / 2;
            }

            int binsEachCharacter = bins / Console.WindowWidth;

            for (int n = 0; n < fftResults.Length / 2; n += binsEachCharacter)
            {
                // averaging out bins
                double yPos = 0;
                for (int b = 0; b < 1; b++)
                {
                    yPos += GetYPosLog(fftResults[n + b]);
                }
                AddResult(n / binsEachCharacter, yPos / 1);
            }
        }

        private double GetYPosLog(Complex c)
        {
            // not entirely sure whether the multiplier should be 10 or 20 in this case.
            // going with 10 from here http://stackoverflow.com/a/10636698/7532
            double intensityDB = 10 * Math.Log10(Math.Sqrt(c.X * c.X + c.Y * c.Y));
            double minDB = -90;
            if (intensityDB < minDB) intensityDB = minDB;
            double percent = intensityDB / minDB;
            // we want 0dB to be at the top (i.e. yPos = 0)
            double yPos = percent * Console.WindowHeight;
            return yPos;
        }

        private void AddResult(int index, double power)
        {
            for (int i = 0; i < power; i++)
            {
                ConsoleLib.ConsoleWriter.WriteCharacterAt(index, i, '#', ConsoleColor.Black);
            }
            for (int i = (int)power; i < Console.WindowHeight; i++)
            {
                ConsoleLib.ConsoleWriter.WriteCharacterAt(index, i, '#', ConsoleColor.Green);
            }
            string time = currentTime.ToString();
            int timeChar = 0;
            foreach (var @char in time)
            {
                ConsoleLib.ConsoleWriter.WriteCharacterAt(timeChar, 0, @char, ConsoleColor.White);
                timeChar++;
            }
        }

        public void SetTime(TimeSpan currentTime)
        {
            this.currentTime = currentTime;
        }
    }
}
