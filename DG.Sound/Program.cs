using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DG.Sound
{
    class Program
    {
        private static int fftLength = 1024; // NAudio fft wants powers of two!
        private static SpectrumAnalyzer analyzer = new SpectrumAnalyzer();
        private static WaveStream wave;
        private static long totalBytes;

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private static SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        [STAThread]
        static void Main(string[] args)
        {
            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            wave = new WaveChannel32(new Mp3FileReader(ofd.FileName));
            string totalTime = wave.TotalTime.ToString();
            var x = wave.Length;
            byte[] buffer = new byte[fftLength];
            int offset = 0;
            int bytes = 1;
            while (wave.CanRead && bytes > 0 && wave.CurrentTime <= wave.TotalTime)
            {
                bytes = wave.Read(buffer, 0, fftLength);
                totalBytes += bytes;
                offset += bytes;
                WaveInEventArgs e = new WaveInEventArgs(buffer, bytes);
                OnDataAvailable(null, e, wave.CurrentTime);
            }
        }

        static void OnDataAvailable(object sender, WaveInEventArgs e, TimeSpan currentTime)
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

        static void FftCalculated(object sender, FftEventArgs e)
        {
            // Do something with e.result!
            analyzer.Update(e.Result);
        }
    }
}
