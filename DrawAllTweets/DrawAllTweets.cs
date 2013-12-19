using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using Elte.GeoVisualizer.Lib;

/*
 * összes koordinátás tweet-ből hisztogram rajzolása
 */
namespace DrawAllTweets {
    class DrawAllTweets {
        static void Main(string[] args) {
            string query0 = "select lon,lat from Twitter..tweet where htm_id > 0 and htm_id != 17042430230528";
            string query = null;
            int width = 4000;
            int heigth = 2000;
            string mapviews = null;
            string outf = null;
            string cstr = "Server=RETDB02;Integrated Security=True;";
            string colormap = null;
            Color bgc = Color.FromArgb(255, 42, 46, 124);
            Color rbgc = Color.Gray;
            Color linec = Color.LightGray;
            float linew = 1.0F;
            ColorConverter cc1 = new ColorConverter();
            int kernelsize = 0;

            for (uint i = 0; i < args.Length; i++) if(args[i][0] == '-') switch(args[i][1]) {
                case 'q':
                            query = args[i + 1];
                            break;
                case 'w':
                            width = Convert.ToInt32(args[i + 1]);
                            break;
                case 'h':
                            heigth = Convert.ToInt32(args[i + 1]);
                            break;
                case 'o':
                            outf = args[i + 1];
                            break;
                case 'c':
                            cstr = args[i + 1];
                            break;
                case 'v':
                            mapviews = args[i + 1];
                            break;
                case 'C':
                            colormap = args[i + 1];
                            break;
                case 'b':
                            bgc = (Color)cc1.ConvertFromString(args[i + 1]);
                            break;
                case 'l':
                            if (args[i][2] == 'c') {
                                linec = (Color)cc1.ConvertFromString(args[i + 1]);
                                break;
                            }
                            if (args[i][2] == 'w') {
                                linew = Convert.ToSingle(args[i + 1]);
                                break;
                            }
                            goto default;
                case 'r':
                            rbgc = (Color)cc1.ConvertFromString(args[i + 1]);
                            break;
                case 'k':
                            kernelsize = Convert.ToInt32(args[i + 1]);
                            break;
                default:
                            Console.Error.WriteLine("Ismeretlen paraméter: {0}", args[i]);
                            break;
            }

            if (query == null) query = query0;

            DrawHistogram dh = new DrawHistogram(cstr, width, heigth);
            dh.NoCache = true;
            if (mapviews != null) dh.MapView = (uint)Projection.ViewFromString(mapviews);
            else dh.MapView = (uint)Projection.ProjectionView.World;

            if (kernelsize == 0) {
                double ksf = Math.Round(((double)dh.Size.Width) / 300.0, 0);
                kernelsize = (int)ksf;
                if (kernelsize <= 0) kernelsize = 1;
            }
            dh.KernelSize = kernelsize;

            dh.BgColor = bgc;
            dh.RColor = rbgc;
            dh.RPen = new Pen(linec, linew);
            dh.LoadCustomQuery(query, false);
            if (colormap != null) {
                Dictionary<string, ColorVal.ColorMaps> cd = ColorVal.GetColorMapStrings();
                ColorVal.ColorMaps cmt = cd[colormap];
                dh.ColorScale = new ColorVal(cmt);
            }
            else dh.ColorScale = new ColorVal(ColorVal.ColorMaps.RedBlueAlpha);
            dh.Col3 = false;

            Bitmap bmp = dh.Draw();
            bmp.Save(outf, ImageFormat.Png);
        }
    }
}
