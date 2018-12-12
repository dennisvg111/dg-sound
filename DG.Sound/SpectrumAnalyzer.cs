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
        private TimeSpan currentTime = TimeSpan.Zero;

        private double[] previousFFT;
        private double specFlux;
        private double difference;
        private double timeBetween;
        private int sampleSize;

        private double hzRange;

        private List<double> spectrumFluxes = new List<double>();
        private List<double> smootherValues = new List<double>();
        private double median;
        private double smoothMedian;
        private double beatThreshold;
        private double thresholdSmoother;
        private Beat lastBeatRegistered = new Beat();

        public SpectrumAnalyzer(int sampleSize)
        {
            this.sampleSize = sampleSize;
            median = 0.0f;
            smoothMedian = 0.0f;
            beatThreshold = 0.6f;
            thresholdSmoother = 0.6f;
            previousFFT = new double[sampleSize / 2 + 1];
            for (int i = 0; i < sampleSize / 2; i++)
            {
                previousFFT[i] = 0;
            }
        }

        public void Update(Complex[] fftResults)
        {
            int bins = fftResults.Length / 2;
            double[] spectrum = new double[fftResults.Length / 2];
            for (int n = 0; n < fftResults.Length / 2; n += 2)
            {
                double db = ConvertToDB(fftResults[n]);
                spectrum[n] = db;
            }

            beatThreshold = calculateFluxAndSmoothing(spectrum);

            if (specFlux > beatThreshold && ((uint)currentTime.TotalMilliseconds - timeBetween) > 350)
            {
                //Beat detected
                if (smootherValues.Count > 1)
                {
                    smootherValues.Insert(smootherValues.Count - 1, specFlux);
                }
                else
                {
                    smootherValues.Insert(smootherValues.Count, specFlux);
                }
                if (smootherValues.Count >= 5)
                {
                    smootherValues.Remove(0);
                }

                timeBetween = (uint)currentTime.TotalMilliseconds;

                Beat t = new Beat(currentTime.Minutes, currentTime.Seconds, currentTime.Milliseconds, (float)specFlux);
                Console.WriteLine("BEAT " + t + " -- THRESHOLD: " + beatThreshold.ToString("N5") + " -- DIFF: " + (t - lastBeatRegistered) + "ms");
                lastBeatRegistered = t;
            }
            else if (((uint)currentTime.TotalMilliseconds - timeBetween) > 5000)
            {
                if (thresholdSmoother > 0.4f)
                    thresholdSmoother -= 0.4f;

                timeBetween = (uint)currentTime.TotalMilliseconds;
            }
        }

        private double ConvertToDB(Complex c)
        {
            //double intensityDB = 20 * Math.Log10(Math.Sqrt(c.X * c.X + c.Y * c.Y));
            double intensityDB = 20 * (c.X * c.X + c.Y * c.Y);
            double minDB = -90;
            if (intensityDB < minDB)
            {
                intensityDB = minDB;
            }
            return intensityDB;
        }

        public void SetTime(TimeSpan currentTime)
        {
            this.currentTime = currentTime;
        }

        private double calculateFluxAndSmoothing(double[] currentSpectrum)
        {
            if (currentSpectrum.Any(i => i > 0))
            {

            }
            specFlux = 0.0f;

            //Calculate differences
            for (int i = 0; i < sampleSize / 2; i++)
            {
                difference = currentSpectrum[i] - previousFFT[i];
                if (difference > 0)
                {
                    specFlux += difference;
                }
            }

            //Get our median for threshold
            if (spectrumFluxes.Count > 0 && spectrumFluxes.Count < 10)
            {
                spectrumFluxes.Sort();
                smootherValues.Sort();
                if (spectrumFluxes[spectrumFluxes.Count / 2] > 0)
                {
                    median = spectrumFluxes[spectrumFluxes.Count / 2];
                }
                if (smootherValues.Count > 0 && smootherValues.Count < 5)
                {
                    if (smootherValues[smootherValues.Count / 2] > 0)
                    {
                        smoothMedian = smootherValues[smootherValues.Count / 2];
                    }
                }
            }
            for (int i = 0; i < sampleSize / 2; i++)
            {
                if (spectrumFluxes.Count > 1)
                {
                    spectrumFluxes.Insert(spectrumFluxes.Count - 1, specFlux);
                }
                else
                {
                    spectrumFluxes.Insert(spectrumFluxes.Count, specFlux);
                }
                if (spectrumFluxes.Count >= 10)
                {
                    spectrumFluxes.RemoveAt(0);
                }
            }

            //Copy spectrum for next spectral flux calculation
            for (int j = 0; j < sampleSize / 2; j++)
            {
                previousFFT[j] = currentSpectrum[j];
            }
            if (smoothMedian > 1)
            {
                thresholdSmoother = 0.8f;
            }
            if (smoothMedian > 2 && smoothMedian < 4)
            {
                thresholdSmoother = 1.0f;
            }
            if (smoothMedian > 4 && smoothMedian < 6)
            {
                thresholdSmoother = 2.2f;
            }
            if (smoothMedian > 6)
            {
                thresholdSmoother = 2.4f;
            }
            return thresholdSmoother + median;
        }

    }
}
