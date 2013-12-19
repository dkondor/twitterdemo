using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using Elte.GeoVisualizer.Lib;

namespace Sentiment1 {
    /// <summary>
    /// sentiment analysis ábrák rajzolása, teszt
    /// </summary>
    class Sentiment1 {
        static void Main(string[] args) {
            string tag = null; //hashtag, amit keresünk
            int w = 1600;
            int h = 800;
            int k = 9;
            string cstr0 = "Data Source=RETDB02;Initial Catalog=Gadm;Integrated Security=true";
            string cstr1 = null;
            string outf = null;
            for (int i = 0; i < args.Length; i++) if (args[i][0] == '-') switch (args[i][1]) {
                        case 't':
                            tag = args[i + 1];
                            break;
                        case 'w':
                            w = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'h':
                            h = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'k':
                            k = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'c':
                            cstr1 = args[i + 1];
                            break;
                        case 'o':
                            outf = args[i + 1];
                            break;
                    }
            if (tag == null || outf == null) return;
            if (cstr1 == null) cstr1 = cstr0;

            DrawHistogram hist = new DrawHistogram(cstr1, w, h);
            hist.KernelSize = k;
            hist.Col3 = true;
            hist.ColorScale3 = new ColorVal(ColorVal.ColorMaps.BlueRed3);
            hist.DoubleLogScale = true;
            hist.LoadSentimentQuery(tag,0);
            Bitmap bmp = hist.Draw();

            bmp.Save(outf, ImageFormat.Png);
        }
    }
}
