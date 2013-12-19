using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
//using System.Windows.Media;
using System.ComponentModel;
using Elte.GeoVisualizer.Lib;
using System.IO;

namespace GeoTest1 {
    class Program {

        const string cstr = "Data Source=RETDB02;Initial Catalog=Gadm;Integrated Security=true";

        static void Main(string[] args) {
            System.Threading.Thread.CurrentThread.CurrentCulture =
            System.Globalization.CultureInfo.InvariantCulture;

            string mapin = null; //bemeneti fájl (ID -- érték párokkal)
            string gquery = "select ID,Geom from dkondor.dbo.region03 order by ID";
            string gquery2 = "select ID,Geom from dkondor.dbo.region04 order by ID";
            bool l2 = false; //ha true, akkor a részletesebb térkép használata (több ország állam szinten)
            bool autoscale = true;
            double min = 0.0;
            double max = 1.0;
            string color1 = null;
            bool colors = false;
            bool colorbar = false;

            string output = null;

            int w = 1800;
            int h = 900;
            Color bgc = Color.FromArgb(255, 42, 46, 124);
            Color linec = Color.LightGray;
            Color rbgc = Color.Gray;
            ColorConverter cc1 = new ColorConverter();
            string hfixcolor = null;
            string pview = null;
            int RFocus = -1;
            float linew = 1.0F;

            for (int i = 0; i < args.Length; i++) if (args[i][0] == '-') switch (args[i][1]) {
                        case 'i':
                            mapin = args[i + 1];
                            break;
                        case '2':
                            l2 = true;
                            break;
                        case 's': //skála
                            min = Convert.ToDouble(args[i + 1]);
                            max = Convert.ToDouble(args[i + 2]);
                            autoscale = false;
                            i += 2;
                            break;
                        case 'c': //használandó színskála
                            color1 = args[i + 1];
                            break;
                        case 'C':
                            colors = true;
                            break;
                        case 'w':
                            w = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'h':
                            h = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'o':
                            output = args[i + 1];
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
                        case 'H':
                            hfixcolor = args[i + 1];
                            break;
                        case 'p':
                            pview = args[i + 1];
                            break;
                        case 'f':
                            RFocus = Convert.ToInt32(args[i + 1]);
                            break;
                        default:
                            Console.Error.WriteLine("Unknown switch: {0}!", args[i]);
                            break;
                    }

            if (mapin == null) {
                Console.Error.WriteLine("Error: no input file specified!");
                return;
            }
            if (output == null) {
                Console.Error.WriteLine("Error: no output file specified!");
                return;
            }

            ColorVal cv = null;
            if (hfixcolor != null) {
                Color hc1 = (Color)cc1.ConvertFromString(hfixcolor);
                double h1, s, v;
                ColorVal.rgbtohsv(hc1, out h1, out s, out v);
                cv = new ColorVal((int)h1);
                cv.Light = true;
                cv.Inverted = true;
            }
            else {
                Dictionary<string, ColorVal.ColorMaps> cd = ColorVal.GetColorMapStrings();
                ColorVal.ColorMaps cmt = ColorVal.ColorMaps.Green;
                if (color1 != null) cmt = cd[color1];
                cv = new ColorVal(cmt);
            }

            MapData md = new MapData(gquery);

            List<double> val = new List<double>();
            List<int> ids = new List<int>();
            List<System.Drawing.Color> cl1 = new List<System.Drawing.Color>();

            StreamReader ins = new StreamReader(mapin);

            if (autoscale) {
                min = Double.MaxValue;
                max = Double.MinValue;
            }
            uint sor = 0;
            while (!ins.EndOfStream) {
                sor++;
                string dl1 = ins.ReadLine();
                int j = 0;
                while (j < dl1.Length) {
                    if (dl1[j] == ' ' || dl1[j] == '\t') j++;
                    else break;
                }
                if (j == dl1.Length) continue; //üres sor
                if (dl1[j] == '#') continue; //komment sor

                string[] dl2 = dl1.Substring(j).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                if (dl2.Length < 2) {
                    Console.Error.WriteLine("Error: invalid data in line {0}!", sor);
                    continue;
                }

                int id1;
                double val1;
                Color c1;

                if (!Int32.TryParse(dl2[0], out id1)) {
                    Console.Error.WriteLine("Error: invalid data in line {0}!", sor);
                    continue;
                }

                if (colors) {
                    var c2 = cc1.ConvertFromString(dl2[1]);
                    if (c2 == null) {
                        throw new InvalidDataException("Error: invalid color name!");
                    }
                    c1 = (Color)c2;
                    ids.Add(id1);
                    cl1.Add(c1);
                }
                else {
                    if (!Double.TryParse(dl2[1], out val1)) {
                        Console.Error.WriteLine("Error: invalid data in line {0}!", sor);
                        continue;
                    }
                    ids.Add(id1);
                    val.Add(val1);
                    /*   if (autoscale) {
                           if (val1 < min) min = val1;
                           if (val1 > max) max = val1;
                       }*/
                }
            }
            ins.Close();

            Elte.GeoVisualizer.Lib.DataSources.GeomArray ga = new Elte.GeoVisualizer.Lib.DataSources.GeomArray();
            ga.StaticColor = rbgc;

            if (l2) ga.LoadGeom(gquery2, cstr);
            else ga.LoadGeom(gquery, cstr);

            if (colors) {
                Color[] ca = cl1.ToArray();
                int[] ia = ids.ToArray();
                ga.LoadArrayColors(ia, ca);
            }
            else {
                //normálás, 0 és 1 közé
                int[] ia = ids.ToArray();
                double[] va = val.ToArray();
                int n = va.Length;
                if (autoscale) {
                    for (int i = 0; i < n; i++) {
                        if (va[i] < min) min = va[i];
                        if (va[i] > max) max = va[i];
                    }
                    if (max == min) min = max - 1.0;
                }
                for (int i = 0; i < n; i++) {
                    va[i] = (va[i] - min) / (max - min);
                    if (va[i] < 0.0) va[i] = 0.0;
                    if (va[i] > 1.0) va[i] = 1.0;
                }
                ga.LoadArray(ia, va);
            }
            ga.RFocus = RFocus;

            var map = new Map();
            {
                Elte.GeoVisualizer.Lib.Projection.ProjectionView pv = Projection.ProjectionView.World;
                if (pview != null) {
                    if (pview == "EU") pv = Projection.ProjectionView.EU;
                    if (pview == "USA") pv = Projection.ProjectionView.USA;
                    if (pview == "Asia") pv = Projection.ProjectionView.Asia;
                    if (pview == "USAWide" || pview == "USAW") pv = Projection.ProjectionView.USAWide;
                }
                Elte.GeoVisualizer.Lib.Projections.Equirectangular proj = new Elte.GeoVisualizer.Lib.Projections.Equirectangular(pv, w, h);
                proj.GetImageSize(out w, out h);
                map.Projection = proj;
            }

            var geo = new Elte.GeoVisualizer.Lib.Layers.Geography(cv);
            geo.DataSource = ga;
            geo.pen = new Pen(linec,linew);

            var bg = new Elte.GeoVisualizer.Lib.Layers.Background();
            bg.Color = bgc;
            bg.DataSource = DataSource.Null;

            map.Layers.Add(geo);
            map.Layers.Add(bg);

            Bitmap bmp = new Bitmap(w, h);
            map.Render(bmp, new Rectangle(0, 0, w, h));

            bmp.Save(output, ImageFormat.Png);
        }
    }
}
