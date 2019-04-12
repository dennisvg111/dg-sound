using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace DG.Sound
{
    public class BeatReader : IEnumerable<Beat>
    {
        private Thread beatThread;

        private List<Beat> beats;
        private SpectrumAnalyzer analyzer;
        private WaveStream wave;
        private long totalBytes;

        private object _lock = new object();

        private SampleAggregator sampleAggregator;

        public TimeSpan SongDuration { get; set; }

        public event EventHandler<BeatEventArgs> BeatFound;
        public class BeatEventArgs : EventArgs
        {
            public double ProgressPercentage { get; set; }
            public Beat Beat { get; set; }
            public BeatEventArgs(double progressPercentage, Beat beat)
            {
                ProgressPercentage = progressPercentage;
                Beat = beat;
            }
        }

        public BeatReader(WaveStream wave, bool keepAlive, int fftLength = 1024)
        {
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            beats = new List<Beat>();
            this.wave = wave;
            analyzer = new SpectrumAnalyzer(fftLength);
            sampleAggregator = new SampleAggregator(fftLength);

            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;

            SongDuration = wave.TotalTime;
            byte[] buffer = new byte[fftLength];
            int bytes = 1;
            int index = 0;

            beatThread = new Thread(new ThreadStart(() =>
            {
                bool nullRef = false;
                while (wave.CanRead && bytes > 0 && wave.CurrentTime < wave.TotalTime && !nullRef)
                {
                    try
                    {
                        TimeSpan time = wave.CurrentTime;
                        index++;
                        bytes = wave.Read(buffer, 0, fftLength);
                        wave.Seek(index * fftLength, System.IO.SeekOrigin.Begin);
                        totalBytes += bytes;
                        WaveInEventArgs e = new WaveInEventArgs(buffer, bytes);
                        OnDataAvailable(null, e, time);
                    }
                    catch (NullReferenceException)
                    {
                        //thread has been killed
                        nullRef = true;
                    }
                    catch (ObjectDisposedException)
                    {
                        //wave has been disposed
                        nullRef = true;
                    }
                }
            }));
            beatThread.Start();
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e, TimeSpan currentTime)
        {
            analyzer.SetTime(currentTime);
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            int bufferIncrement = wave.WaveFormat.BlockAlign;

            for (int index = 0; index < bytesRecorded; index += bufferIncrement)
            {
                float sample32 = BitConverter.ToSingle(buffer, index);
                sampleAggregator.Add(sample32);
            }
        }

        void FftCalculated(object sender, FftEventArgs e)
        {
            // Do something with e.result!
            Beat beat;
            if (analyzer.Update(e.Result, out beat))
            {
                lock (_lock)
                {
                    beats.Add(beat);
                }
                BeatFound?.Invoke(this, new BeatEventArgs(beat.Milliseconds / SongDuration.TotalMilliseconds * 100f, beat));
            }
        }

        public IEnumerator<Beat> GetEnumerator()
        {
            List<Beat> returnList;
            lock (_lock)
            {
                returnList = beats.ToList();
            }
            return returnList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
