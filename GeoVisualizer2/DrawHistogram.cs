using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;

/*
 * TODO:
 * konzisztens interfész a kétfajta színezés közötti választáshoz
 * intenzitás szerinti színezés esetén színskála valahova
 */

namespace Elte.GeoVisualizer.Lib {
    /// <summary>
    /// draw a histogram based on a query (word or hashtag or the result set of any SQL query)
    /// 2013.05.19. -- a Histogram.cs funkcionalitásának jelentős részét itt duplikálom,
    /// így lehetővé válik a köztes adatok (Gaussok összege) megtartása, és a paraméterek
    /// változtatása után gyors új plotolása
    /// </summary>
    public class DrawHistogram {
        private DataSources.CoordArray ca;
        string cstr;
        protected Bitmap[] amap; //pre-generated maps of the world, we draw the histogram over this as an extra layer
        /// <summary>
        /// number of possible map views (pre-generated maps)
        /// </summary>
        public readonly uint regions = 4;

        private uint mapview2;
        /// <summary>
        /// set the map view to use (must be less than regions)
        /// </summary>
        public Projection.ProjectionView MapView {
            get { return (Projection.ProjectionView)mapview2; }
            set {
                uint val = (uint)value;
                if (val >= regions)
                    throw new ArgumentException("DrawMap.CreateMap(): MapView ID must be less than the number of possible IDs!\n");
                if (mapview2 != val) havedata = false;
                mapview2 = val;
            }
        }
        
        private Projection[] projections;

        protected Size[] size;
        protected Size size2; //maximum képméret

        //Gauss eloszlások összege
        private double[,] hist;
        private double[,] c3hist; //harmadik oszlop (idő / sentiment) átlagának a számolása itt
        private double[,] c3hist2; //harmadik oszlop (idő / sentiment) szórásának a számolása itt
        private double[,] kernel; //Gauss-kernel
        private int c3min;
        private int c3max;
        private int c3min1;
        private int c3max1;

        private bool havedata; //vannak-e értelmes adatok, a hist tömbökben
        private bool noauto; //autoscale mellőzése (sentiment analysis eredményénél)
        public bool Col3Auto {
            get {
                if (noauto) return false;
                else return true;
            }
            set {
                if (value) noauto = false;
                else noauto = true;
                havedata = false;
            }
        }

        private bool col3; //használjuk-e a harmadik oszlopot

        private ColorVal cv; //érték szerinti színezéshez színskála
        private ColorVal cv3; //harmadik oszlop szerinti színezéshez színskála
        private Color color1; //statikus szín használata a színezéshez

        private bool dlogs; //use double logarithmic scale
        private bool logs; //use logarithmic scale

        private int kernelsize;

        /// <summary>
        /// Number of (time) bins to use when creating a histogram based on the 3rd column of the query
        /// </summary>
        public int TimeBins;

        /// <summary>
        /// If set, the data is not cached in the memory, each time the histogram is updated, the data is
        /// requested from the server
        /// </summary>
        public bool NoCache {
            get { return ca.NoCache; }
            set { ca.NoCache = value; }
        }

        /// <summary>
        /// background (oceans) color
        /// </summary>
        public Color BgColor;
        /// <summary>
        /// background (land) color
        /// </summary>
        public Color RColor;
        /// <summary>
        /// pen for drawing the region boundarys
        /// </summary>
        public Pen RPen;

        private void InitializeMembers() {
            ca = new DataSources.CoordArray();
            cstr = null;
            hist = null;
            c3hist = null;
            c3hist2 = null;
            havedata = false;
            dlogs = true;
            logs = true;
            color1 = Color.Yellow;
            cv = null;
            cv3 = null;
            kernelsize = 9;
            StdevSat = 0.6;
            colorbar = false;
            colorbarwidth = 20;
            colorbartextwidth = 60;
            colorbartics = 3; //sentiment analysis-hoz jó lesz
            noauto = false;
            TimeBins = 10;
            c3min1 = 1344902400;
            c3max1 = 1355184000;
            amap = new Bitmap[regions];
            projections = new Projection[regions];
            size = new Size[regions];
            for (uint i = 0; i < regions; i++) {
                amap[i] = null;
                projections[i] = null;
                size[i] = new Size(0, 0);
            }
            size2 = new Size(0, 0);
            CreateKernel();


            RColor = Color.FromArgb(255, 22, 22, 22);
            RPen = Pens.Black;
            BgColor = Color.FromArgb(255, 11, 25, 46);
        }

        private void CreateKernel() {
            if (kernel != null) kernel = null;
            kernel = new double[kernelsize, kernelsize];
            double sigma = ((double)kernelsize) / 8.0;
            double sigma2 = 2.0 * sigma * sigma;
            double avg = ((double)(kernelsize-1)) / 2.0;
            for (int i = 0; i < kernelsize; i++) {
                double di = ((double)i - avg);
                for (int j = 0; j < kernelsize; j++) {
                    double dj = ((double)j - avg);
                    double x = -1.0 * (di * di + dj * dj) / sigma2;
                    kernel[i, j] = Math.Exp(x);
                }
            }
        }

        /// <summary>
        /// initialize a histogram drawing class with the default paramteres
        /// need to set the connection string and the size later (before drawing)
        /// </summary>
        public DrawHistogram() {
            InitializeMembers();
        }

        /// <summary>
        /// initialize a histogram drawing class with a given connection string
        /// need to set size later (before drawing)
        /// </summary>
        public DrawHistogram(string cstr1) {
            InitializeMembers();
            cstr = cstr1;
        }

        /// <summary>
        /// initialize a histogram drawing class with a given connection string and size
        /// </summary>
        public DrawHistogram(string cstr1, int w, int h) {
            InitializeMembers();
            cstr = cstr1;

            Size = new Size(w, h);
        }

        /// <summary>
        /// Create a copy of this instance with the same settings (same data source, color scale, etc.), but with a different size
        /// </summary>
        /// <param name="w">the new width</param>
        /// <param name="h">the new height</param>
        /// <returns></returns>
        public DrawHistogram Clone(int w, int h) {
            DrawHistogram hist = new DrawHistogram(cstr,w,h);
            hist.ca = ca;
            hist.color1 = this.color1;
            hist.ColorScale = this.ColorScale;
            hist.ColorScale3 = this.ColorScale3;
            hist.Col3 = this.Col3;
            hist.Col3Auto = this.Col3Auto;
            hist.Col3Min = this.Col3Min;
            hist.Col3Max = this.Col3Max;
            hist.TimeBins = this.TimeBins;

            hist.RColor = this.RColor;
            hist.RPen = this.RPen;
            hist.BgColor = this.BgColor;
            hist.MapView = this.MapView;
            return hist;
        }

        private void CreateArrays() {
            if (hist != null) hist = null;
            if (c3hist != null) c3hist = null;
            if (c3hist2 != null) c3hist2 = null;
            hist = new double[size2.Width, size2.Height];
            c3hist = new double[size2.Width, size2.Height];
            c3hist2 = new double[size2.Width, size2.Height];
            havedata = false;
        }

        /// <summary>
        /// set the size of the resulting image
        /// also (re)set the projection used and the internal arrays
        /// Note: after setting the size, the background map and the internal arrays will be regenerated at the
        /// next call to Draw(), which can take significant time. The DrawMap() and BuildArrays() methods can be
        /// called to take care of this in advance.
        /// </summary>
        public Size Size {
            get { return size2; }
            set {
                size2 = value;
                RecalculateSize();
            }
        }

        private void RecalculateSize() {
            for (uint i = 0; i < regions; i++) {
                int w2 = size2.Width;
                if (colorbar) w2 -= (colorbarwidth + colorbartextwidth);
                Projections.Equirectangular p1 = new Projections.Equirectangular((Projection.ProjectionView)i, w2, size2.Height);
                int h2;
                p1.GetImageSize(out w2, out h2);
                size[i] = new Size(w2, h2);
                projections[i] = p1;
                amap[i] = null; //újra kell generálni
            }

            CreateArrays();
        }

        /// <summary>
        /// Set the size of the kernel to be used for the histogram.
        /// Note that the data arrays will be regenerated on the next call to Draw().
        /// This can be also done manually with the BuildArrays() function.
        /// </summary>
        public int KernelSize {
            get { return kernelsize; }
            set {
                if (value < 1)
                    throw new ArgumentException("DrawHistogram.KernelSize: invalid size (<1)!\n");
                kernelsize = value;
                CreateKernel();
                havedata = false;
            }
        }

        /// <summary>
        /// Set to true if the histogram should be colored according to the 3rd column returned by the query.
        /// </summary>
        public bool Col3 {
            get { return col3; }
            set { col3 = value; }
        }

        /// <summary>
        /// Set the maximum for the color scale (if the points are colored by the 3rd column).
        /// Note that currently calling the BuildArrays() funtion resets these to autoscale.
        /// </summary>
        public int Col3Max {
            get { return c3max; }
            set {
                c3max = value;
                havedata = false;
            }
        }

        /// <summary>
        /// Set the minimum for the color scale (if the points are colored by the 3rd column).
        /// Note that currently calling the BuildArrays() funtion resets these to autoscale.
        /// </summary>
        public int Col3Min {
            get { return c3min; }
            set {
                c3min = value;
                havedata = false;
            }
        }

        /// <summary>
        /// Set the color scale used when plotting the values (i.e. use color for the value).
        /// </summary>
        public ColorVal ColorScale {
            get { return cv; }
            set { cv = value; }
        }

        /// <summary>
        /// Set a static color to be used for plotting (i.e. the value will determine the opacity).
        /// Also invalidates any previously supplied ColorScale value.
        /// </summary>
        public Color StaticColor {
            get { return color1; }
            set {
                color1 = value;
                if (color1 != null) cv = null;
            }
        }

        /// <summary>
        /// Set the color scale used when coloring is done according to a 3rd column (time or sentiment).
        /// </summary>
        public ColorVal ColorScale3 {
            get { return cv3; }
            set { cv3 = value; }
        }

        /// <summary>
        /// Set logarithmic scale when computing values.
        /// </summary>
        public bool LogScale {
            get { return logs; }
            set {
                logs = value;
                dlogs = false;
            }
        }

        /// <summary>
        /// Set double logarithmic scale (i.e. log(log(x+1)+1) ) when computing values.
        /// </summary>
        public bool DoubleLogScale {
            get { return dlogs; }
            set {
                dlogs = value;
                if (dlogs) logs = true;
            }
        }

        /// <summary>
        /// Scaling factor for the standard deviation when setting the saturation of points.
        /// If set to 0.0, the standard deviation will not be used, all points will have maximum saturation
        /// </summary>
        public double StdevSat;

        private bool colorbar;
        /// <summary>
        /// If set, a color bar will be drawn next to the map.
        /// The background map needs to be regenerated, which can take a significant time.
        /// </summary>
        public bool DrawColorBar {
            get { return colorbar; }
            set {
                if (value != colorbar) {
                    colorbar = value;
                    for (uint i = 0; i < regions; i++) {
                        amap[i] = null; //törlés, újra lesz generálva legközelebb
                    }
                    if(!size2.IsEmpty) RecalculateSize();
                }
            }
        }

        /// <summary>
        /// set the width of the colorbar
        /// </summary>
        public int colorbarwidth;

        /// <summary>
        /// set the text width of the colorbar
        /// </summary>
        public int colorbartextwidth;
        
        /// <summary>
        /// tics (numbers) to display next to the colorbar
        /// </summary>
        public int colorbartics;

        /// <summary>
        /// Draw the background map where the histograms can be overlayed
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="cstr">connection string to use</param>
        /// <returns></returns>
        public static Bitmap DrawMap(int w, int h, string cstr, Projection p, Color RColor, Color BgColor, Pen RPen) {
            return DrawMap1(w, h, cstr, "select Geom from dkondor.dbo.region03", p, RColor, BgColor, RPen);
        }

        /// <summary>
        /// Draw the background map, where the histograms can be overlayed
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        public void DrawMaps() {
            if (cstr == null)
                throw new ArgumentNullException("DrawHistogram.DrawMap: the connection string was not specified!");
            if (size2.Width < 1 || size2.Height < 1)
                throw new ArgumentException("DrawHistogram.DrawMap: size was not set!\n");

          //  for (uint i = 0; i < regions; i++) {
            {
                uint i = (uint)MapView;
                int w1 = size[i].Width;
                if (colorbar) w1 += colorbartextwidth + colorbarwidth;
                amap[i] = DrawMap(w1, size[i].Height, cstr, projections[i], RColor, BgColor, RPen);
                if (colorbar) {
                    Graphics g1 = Graphics.FromImage(amap[i]);
                    g1.FillRectangle(new SolidBrush(Color.White),
                        new Rectangle(size[i].Width, 0, colorbartextwidth + colorbarwidth, size[i].Height));
                    g1.Dispose();
                }
            }
        }

        /// <summary>
        /// Draw the background map, where the histograms can be overlayed
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="cstr">connection string to use</param>
        /// <param name="query">query which returns the geometries</param>
        /// <param name="p">projection to use</param>
        /// <returns></returns>
        public static Bitmap DrawMap1(int w, int h, string cstr, string query, Projection p, Color RColor, Color BgColor, Pen RPen) {
            if (w < 1 || h < 1 || p == null)
                throw new ArgumentException("DrawHistogram.DrawMap1(): invalid parameters!\n");
            
            Map map1 = new Map();
            map1.Projection = p;

            Layers.Geography geo = new Layers.Geography();
            DataSources.SqlQuery sq = new DataSources.SqlQuery();
            sq.ConnectionString = cstr;
            sq.Command = new SqlCommand(query);
            geo.DataSource = sq;
            //geo.StaticColor = Color.LightGray;

            //geo.StaticColor = Color.FromArgb(0, 30, 30);
            geo.StaticColor = RColor;
            geo.pen = RPen;

            Layers.Background bg = new Layers.Background();
            //bg.Color = Color.FromArgb(255, 21, 23, 62);
            bg.Color = BgColor;
            bg.DataSource = DataSource.Null;

            map1.Layers.Add(geo);
            map1.Layers.Add(bg);

            Bitmap bmp;
            bmp = new Bitmap(w, h);
            map1.Render(bmp, new Rectangle(0, 0, w, h));
            return bmp;
        }
        

        /// <summary>
        /// Generate the query used to retrieve data when using full-text search
        /// </summary>
        /// <param name="query">search term</param>
        /// <param name="max">maximum number of points to search for (top N), or 0 to retrieve all results</param>
        /// <returns>the query to execute</returns>
        public static string BuildTextQuery(string query, uint max) {
            string query1;
            if (max > 0) query1 = "select top " + max + " lon,lat,datediff(second,'1970-01-01 0:00:00.000',created_at) " +
                 "from Twitter..tweet where contains([text],'" + query + "') and htm_id > 0 and htm_id != 17042430230528";
            else query1 = "select lon,lat,datediff(second,'1970-01-01 0:00:00.000',created_at) " +
                "from Twitter..tweet where contains([text],'" + query + "') and htm_id > 0 and htm_id != 17042430230528";
            return query1;
        }

        /// <summary>
        /// Load the data to display using full-text search
        /// </summary>
        /// <param name="query">search term</param>
        /// <param name="max">maximum number of points to search for (top N)</param>
        /// <returns>the query string executed</returns>
        public string LoadTextQuery(string query, uint max) {
            string query1 = BuildTextQuery(query, max);

            if (cstr == null)
                throw new ArgumentNullException("DrawHistogram.LoadTextQuery(): no connection string was supplied!\n");
            ca.LoadData(query1, cstr, true);
            havedata = false;
            //noauto = false;

            return query1;
        }

        /// <summary>
        /// Generate the query used to retrieve data when searching for tweets with hastags
        /// </summary>
        /// <param name="query">search term (one hashtag)</param>
        /// <param name="max">maximum number of points to search for (top N), or 0 to retrieve all results</param>
        /// <returns>the query to execute</returns>
        public static string BuildHashtagQuery(string query, uint max) {
            string query1;
            if (max > 0) query1 = "select top " + max + " lon,lat,utime " + //!! TODO: tábla az összes hashtag-el !!
                 "from dkondor.dbo.thnew where tag = '" + query + "'";
            else query1 = "select lon,lat,utime " +
                "from dkondor.dbo.thnew where tag = '" + query + "'";
            return query1;
        }

        /// <summary>
        /// Load the data to display using search for one hashtag
        /// </summary>
        /// <param name="query">search term (one hashtag)</param>
        /// <param name="max">maximum number of points to search for (top N), or 0 to retrieve all results</param>
        /// <returns>the query string executed</returns>
        public string LoadHashtagQuery(string query, uint max) {
            string query1 = BuildHashtagQuery(query, max);

            if (cstr == null)
                throw new ArgumentNullException("DrawHistogram.LoadHashtagQuery(): no connection string was supplied!\n");
            ca.LoadData(query1, cstr, true);
            havedata = false;
            noauto = false;

            return query1;
        }

        /// <summary>
        /// Generate the query used to retrieve data when searching for tweets with hastags and sentiment value
        /// </summary>
        /// <param name="query">search term (one hashtag)</param>
        /// <param name="max">maximum number of points to search for (top N), or 0 to retrieve all results</param>
        /// <returns>the query to execute</returns>
        public static string BuildSentimentQuery(string query, uint max) {
            string query1;
            if (max > 0) query1 = "select top " + max + " lon,lat,sentiment " +
                 "from dkondor.dbo.sentiment1 where tag = '" + query + "' and sentiment != 0"; //2013.05.20. -- semlegesek kihagyása
            else query1 = "select lon,lat,sentiment " +
                "from dkondor.dbo.sentiment1 where tag = '" + query + "' and sentiment != 0";
            return query1;
        }

        /// <summary>
        /// Load the data to display using search for one hashtag with sentiment values
        /// </summary>
        /// <param name="query">search term (one hashtag)</param>
        /// <param name="max">maximum number of points to search for (top N), or 0 to retrieve all results</param>
        /// <returns>the query string executed</returns>
        public string LoadSentimentQuery(string query, uint max) {
            string query1 = BuildSentimentQuery(query, max);

            if (cstr == null)
                throw new ArgumentNullException("DrawHistogram.LoadTextQuery(): no connection string was supplied!\n");
            ca.LoadData(query1, cstr, true);
            havedata = false;
            noauto = true;
            c3min = -1;
            c3max = 1;

            return query1;
        }

        public void LoadCustomQuery(string query, bool c3) {
            ca.LoadData(query, cstr, c3);
            havedata = false;
            noauto = true;
            c3min = -1;
            c3max = 1;
        }

        public int GetDataNum() {
            return ca.GetDataNum();
        }



        /// <summary>
        /// Draw a histogram of the previously loaded data.
        /// Mostly a copy of the EndRender() function from Histogram.cs
        /// </summary>
        /// <returns></returns>
        public Bitmap Draw() {
            if (amap[mapview2] == null) {
                DrawMaps();
            }

            if (!havedata) BuildArrays();

            if (col3) if (cv3 == null) cv3 = new ColorVal(ColorVal.ColorMaps.ThermalInv);
            double max = GetMax();

            byte[] buffer;
            int bytes;
            int stride;

            int b = 0;
            int g = 1;
            int r = 2;
            int a = 3;

            Bitmap bitmap = new Bitmap(size[mapview2].Width, size[mapview2].Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
//Layer.LockBits() {
            BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int numbytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            buffer = new byte[numbytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, numbytes);
            stride = bitmapData.Stride;
            bytes = 4;
//}
            double c3max2 = (double)c3max;
            double c3min2 = (double)c3min;
            double stmax = GetMaxStdev();
            //Console.WriteLine(stmax / (c3max2 - c3min2));

            for (int i = 0; i < size[mapview2].Width; i++) {
                for (int j = 0; j < size[mapview2].Height; j++) {
                    int k = j * stride + i * bytes;

                    if (col3) {
                        //szín az átlaghoz tartozó hue alapján
                        // + kiszámítjuk a szórást, az alapján határozzuk meg a telítettséget
                        double val = hist[i, j];
                        if (val > 0.0) {
                            double sum = c3hist[i, j];
                            double sum2 = c3hist2[i, j];
                            double avg = sum / val; //átlag
                            double stdev2 = sum2 / val - avg * avg;
                            if (avg > c3max2) avg = c3max2;
                            if (avg < c3min2) avg = c3min2;
                            avg = (avg - c3min2) / (c3max2 - c3min2);

                            Color c1;
                            if (StdevSat > 0.0) {
                                double stdev = 0.0;
                                if (stdev2 > 0.0) stdev = Math.Sqrt(stdev2); //szórás

                                //normálás, 0 és 1 közé (??)
                                stdev = stdev / (c3max2 - c3min2);
                                //!! 2013.05.13. gangnamusa térképhez átírva
                                stdev /= StdevSat;
                                if (stdev > 1.0) stdev = 1.0;

                                //kérdés: mennyi lehet a szórás? [0,1] közötti eloszlásfüggvényre 0.5, de 1-nél mindenképpen kisebb
                                double h;
                                double s;
                                double v;
                                cv3.GetColorHSV(avg, out h, out s, out v); //csak h-t használjuk, v = 1.0, s = sqrt(1-stdev)
                                v = 1.0;
                                s = 1.0 - Math.Sqrt(stdev);
                                c1 = ColorVal.hsvtorgb(h, s, v);
                            }       
                            else
                                c1 = cv3.GetColor(avg);
                            //double logval = Math.Log10(Math.Log10(val + 1) + 1); //kérdés, hogy ez szükséges-e? -- valószínűleg egyébként semmi sem látszana
                            double logval;
                            if (logs) {
                                if (dlogs) logval = Math.Log10(Math.Log10(val + 1) + 1);
                                else logval = Math.Log10(val + 1); //2013.05.15. -- csak egy logaritmust veszünk
                            }
                            else logval = val;

                            buffer[k + r] = (byte)c1.R;
                            buffer[k + g] = (byte)c1.G;
                            buffer[k + b] = (byte)c1.B;
                            buffer[k + a] = (byte)((logval / max) * 255.0);
                        }
                        else {
                            buffer[k + r] = 0;
                            buffer[k + g] = 0;
                            buffer[k + b] = 0;
                            buffer[k + a] = 0;
                        }
                    }

                    else {
                        double val = hist[i, j];
                        double logval;
                        if (logs) {
                            if (dlogs) logval = Math.Log10(Math.Log10(val + 1) + 1);
                            else logval = Math.Log10(val + 1); //2013.05.15. -- csak egy logaritmust veszünk
                        }
                        else logval = val;

                        if (cv != null) { // a megadott színt használjuk
                            if (logval > 0.0) {
                                Color c1 = cv.GetColor(logval / max);
                                buffer[k + r] = c1.R;
                                buffer[k + g] = c1.G;
                                buffer[k + b] = c1.B;
                                buffer[k + a] = c1.A;
                            }
                        }
                        else {
                            if (logval > 0.0) {
                                buffer[k + r] = (byte)color1.R;
                                buffer[k + g] = (byte)color1.G;
                                buffer[k + b] = (byte)color1.B;
                            }
                            buffer[k + a] = (byte)((logval / max) * color1.A);
                        }
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);

            bitmap.UnlockBits(bitmapData);

            bitmapData = null;
            buffer = null;

            Rectangle rect;
            if (colorbar) rect = new Rectangle(0, 0, size[mapview2].Width + colorbartextwidth + colorbarwidth, size[mapview2].Height);
            else rect = new Rectangle(new Point(0, 0), size[mapview2]);
            Bitmap bmp = amap[mapview2].Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics g2 = Graphics.FromImage(bmp);
            g2.DrawImage(bitmap, new Point(0,0));

            if (colorbar) {
                ColorVal cv1;
                if (col3) cv1 = cv3;
                else cv1 = cv;

                string[] tics = new string[colorbartics];
                for (int i = 0; i < colorbartics; i++) { //!! ez csak akkor fog működni, ha a col3 == true beállítást használjuk
                    double val = c3min + (c3max - c3min) * ((double)i) / ((double)(colorbartics-1)); //!! de a demoban csak ezt akarjuk használni
                    tics[i] = val.ToString();
                }

                Bitmap bmp2 = Layers.Colorbar.RenderColorBarWithTics(colorbarwidth, size[mapview2].Height - 64, colorbartextwidth, cv1, tics);
                g2.DrawImage(bmp2, new Point(size[mapview2].Width, 32));
            }
            g2.Dispose();

            return bmp;
        }

        /// <summary>
        /// create the histograms to be drawn later
        /// </summary>
        public void BuildArrays() {
            if (size2.Width < 1 || size2.Height < 1 || size[mapview2].Width < 1 || size[mapview2].Height < 1)
                throw new ArgumentException("DrawHistogram.BuildArrays(): size not set!\n");
            if (projections[mapview2] == null)
                throw new ArgumentException("DrawHistogram.BuildArrays(): projection not set (this is a bug in the program)!\n");
            //régi elemek törlése
            for (int i = 0; i < size[mapview2].Width; i++) for (int j = 0; j < size[mapview2].Height; j++) {
                hist[i, j] = 0.0;
                if (col3) {
                    c3hist[i, j] = 0.0;
                    c3hist2[i, j] = 0.0;
                }
            }
            if (!noauto) {
                c3min = c3min1;
                c3max = c3max1;
            }

            ca.Open(); //exception, ha még nem töltöttünk be adatokat
            object[] d1 = new object[3];
            while (ca.ReadNext(d1)) {
                OnRender(d1);
            }
            ca.Close();

            havedata = true;
        }

        /// <summary>
        /// Histogram.OnRender() átmásolva
        /// </summary>
        /// <param name="context"></param>
        /// <param name="values"></param>
        private void OnRender(object[] values) {
            double lon = (double)values[0];
            double lat = (double)values[1];
            int c3 = 0;
            if (col3) {
                c3 = (int)values[2];
                if (c3 < c3min || c3 > c3max) return;
            }

            var mp = projections[mapview2].Map(new GeoPoint(lon, lat));

            for (int ik = 0; ik < kernel.GetLength(0); ik++) {
                for (int jk = 0; jk < kernel.GetLength(1); jk++) {
                    int i = (int)mp.X + ik - (kernel.GetLength(0) - 1) / 2;
                    int j = size[mapview2].Height - (int)mp.Y + jk - (kernel.GetLength(1) - 1) / 2;

                    if (i >= 0 && i < size[mapview2].Width && j >= 0 && j < size[mapview2].Height) {
                        if (col3) {
                            c3hist[i, j] += kernel[ik, jk] * c3;
                            c3hist2[i, j] += kernel[ik, jk] * c3 * c3;
                        }
                        hist[i, j] += kernel[ik, jk];
                    }
                }
            }
        }




        private double GetMaxLog() {
            double max1 = double.MinValue;
            for (int i = 0; i < size[mapview2].Width; i++) {
                for (int j = 0; j < size[mapview2].Height; j++) {
                    double val = hist[i, j];
                    double logval =Math.Log10(val + 1);
                    max1 = Math.Max(max1, logval);
                }
            }
            return max1;
        }

        private double GetMaxDLog() {
            double max1 = double.MinValue;
            for (int i = 0; i < size[mapview2].Width; i++) {
                for (int j = 0; j < size[mapview2].Height; j++) {
                    double val = hist[i, j];
                    double logval = Math.Log10(Math.Log10(val + 1) + 1);
                    max1 = Math.Max(max1, logval);
                }
            }
            return max1;
        }

        private double GetMaxLin() {
            double max1 = double.MinValue;
            for (int i = 0; i < size[mapview2].Width; i++) {
                for (int j = 0; j < size[mapview2].Height; j++) {
                    double val = hist[i, j];
                    max1 = Math.Max(max1, val);
                }
            }
            return max1;
        }

        private double GetMax() {
            if (logs) {
                if (dlogs) return GetMaxDLog();
                else return GetMaxLog();
            }
            else return GetMaxLin();
        }

        private double GetMaxStdev() {
            double max1 = double.MinValue;
            double c3max2 = (double)c3max;
            double c3min2 = (double)c3min;
            for (int i = 0; i < size[mapview2].Width; i++) {
                for (int j = 0; j < size[mapview2].Height; j++) {
                    double val = hist[i, j];
                    if (val == 0.0) continue;
                    double sum = c3hist[i, j];
                    double sum2 = c3hist2[i, j];
                    double avg = sum / val; //átlag
                    double stdev2 = sum2 / val - avg * avg;
                    max1 = Math.Max(max1, stdev2);
                }
            }
            if (max1 <= 0.0) return 0.0;
            return Math.Sqrt(max1);
        }

        /// <summary>
        /// draw the time series from the loaded data
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public Bitmap DrawTimeSeries(DrawTimeSeries dt) {
            dt.cv = ColorScale3;
            if (ca.GetDataNum() < 2 || Col3Min >= Col3Max) {
                dt.min = c3min1;
                dt.max = c3max1;
                dt.binsize = ((double)(c3max1-c3min1)) / ((double)TimeBins);
            }
            else {
                dt.min = Col3Min;
                dt.max = Col3Max;
                dt.binsize = (Col3Max - Col3Min) / (double)TimeBins;
            }
            dt.LoadData(ca, 2);
            return dt.Draw();
        }
    }
}


