using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using Elte.GeoVisualizer.Lib;

namespace TestTS {
    /// <summary>
    /// timeseries plot tesztelése
    /// pl.:
    /// PS U:\My Documents\twitter\twitterdemo\TestTS\bin\Debug> .\TestTS.exe -r 1345420800 1350691200 -q "select 0.0, 0.0, datediff(second,'1970-01-01 0:00:00.000',created_at) from dkondor.dbo.gtext" -o gtext1.png -b 604800
    /// </summary>
    class TestTS {
        static void Main(string[] args) {
            string query = null; //query (3 oszlopot kell, hogy visszaadjon)
            int w = 800;
            int h = 300;
            string cstr0 = "Data Source=RETDB02;Initial Catalog=Gadm;Integrated Security=true";
            string cstr1 = null;
            string outf = null;
            int min = 0;
            int max = 0;
            int binsize = 86400;
            for (int i = 0; i < args.Length; i++) if (args[i][0] == '-') switch (args[i][1]) {
                        case 'q':
                            query = args[i + 1];
                            break;
                        case 'w':
                            w = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'h':
                            h = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'c':
                            cstr1 = args[i + 1];
                            break;
                        case 'o':
                            outf = args[i + 1];
                            break;
                        case 'r':
                            min = Convert.ToInt32(args[i + 1]);
                            max = Convert.ToInt32(args[i + 2]);
                            break;
                        case 'b':
                            binsize = Convert.ToInt32(args[i + 1]);
                            break;
                    }
            if (cstr1 == null) cstr1 = cstr0;

            Elte.GeoVisualizer.Lib.DataSources.CoordArray ca = new Elte.GeoVisualizer.Lib.DataSources.CoordArray();
            ca.LoadData(query, cstr1, true);

            DrawTimeSeries ts = new DrawTimeSeries();
            ts.LogScale = false;
            ts.min = min;
            ts.max = max;
            ts.binsize = binsize;
            ts.LoadData(ca, 2);

            ts.cv = new ColorVal(ColorVal.ColorMaps.Red);
            ts.colorx = true;

            Bitmap bmp = ts.Draw(w,h);
            bmp.Save(outf, ImageFormat.Png);
        }
    }
}
