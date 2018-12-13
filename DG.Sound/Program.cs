using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DG.Sound
{
    internal class Program
    {
        private static BeatReader beatReader;
        private static Beat lastBeat = new Beat();

        [STAThread]
        static void Main(string[] args)
        {            
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var wave = new WaveChannel32(new Mp3FileReader(ofd.FileName)))
            {
                beatReader = new BeatReader(wave, true);
                beatReader.BeatFound += BeatReader_BeatFound;

                Console.ReadLine();
            }
        }

        private static void BeatReader_BeatFound(object sender, BeatReader.BeatEventArgs e)
        {
            Console.WriteLine("BEAT " + e.Beat + " -- DIFF: " + (e.Beat - lastBeat) + "ms");
            lastBeat = e.Beat;
        }
    }
}
