using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

/*
 * TODO:
 * régiók: EU, USA, Ázsia -- ezekhez külön raszter térképek generálása, a DrawMentionMap fv.-ben lehetne választani
 * színskála valahova + feliratok hozzá
 * kimenő mention-ökre is query
 */

namespace Elte.GeoVisualizer.Lib
{

    /// <summary>
    /// térkép rajzolása optimalizálva (?):
    /// nagy térkép tárolása, rajzolás raszterként
    /// </summary>
    public class DrawMap {
        private Bitmap[] armap; //nagy felbontású "kép" az ország ID-k vannak benne
        private Bitmap[] armap2; //országhatárok (külön, a végén tesszük rá a kész képre

        public readonly uint regions = 4; //nézetek száma

        private uint mapview2;
        /// <summary>
        /// set the map view to use (must be less than regions)
        /// </summary>
        public Projection.ProjectionView MapView {
            get { return (Projection.ProjectionView)mapview2; }
            set {
                uint val = (uint)value;
                if(val >= regions)
                    throw new ArgumentException("DrawMap.CreateMap(): MapView ID must be less than the number of possible IDs!\n");
                mapview2 = val;
            }
        }
        private uint mapview;

        /// <summary>
        /// If set, the outgoing links will be drawn, else the incoming.
        /// </summary>
        public bool OutLinks;

        /// <summary>
        /// Data source to use (for the regions). Set before drawing the map.
        /// </summary>
        public DataSources.GeomArray ga;

        /// <summary>
        /// Color scale used for drawing
        /// </summary>
        public ColorVal ColorScale;
        private Color[] colors;
        private Dictionary<int, int> ids;
        private int n; //ids mérete
        private Projection[] projections;
        private Projection[] projections2;
        private Graphics g1;
        private Graphics g2;

        /// <summary>
        /// background color
        /// </summary>
        public Color bgcolor;

        /// <summary>
        /// static color (for the destination region if it is left out)
        /// </summary>
        public Color StaticColor;

        private double[] qvals;
        private int[] qids;
        private int qvalues;
        private int dID;

        //normáláshoz
        private Dictionary<int, double> nusers;
        private Dictionary<int, double> nmentions;
        private Dictionary<int, double> nmentionsout;

        /// <summary>
        /// országhatárok rajzolásához használt pen
        /// </summary>
        public Pen pen;

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
                        armap[i] = null; //törlés, újra lesz generálva legközelebb
                        armap2[i] = null;
                    }
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
        /// normalization type
        /// </summary>
        public enum NormType {
            /// <summary>
            /// no normalization (raw counts)
            /// </summary>
            None,
            /// <summary>
            /// normalize by the number of users
            /// </summary>
            UserCount,
            /// <summary>
            /// normalize by the number of mentions
            /// </summary>
            MentionCount
        }


        /// <summary>
        /// connection string to use (must be set manually)
        /// </summary>
        public string cstr;

        /// <summary>
        /// normalization type to use
        /// </summary>
        public NormType Normalization;

        /// <summary>
        /// use logarithmic scale
        /// </summary>
        public bool LogScale;

        /// <summary>
        /// draw the destination as normal (if false, the destination will be drawn with grey)
        /// </summary>
        public bool Dest;

        private double scalemin;
        private double scalemax;
        /// <summary>
        /// set the minimum of the colorscale (relative value, set between 0.0 and 1.0)
        /// </summary>
        public double ScaleMin {
            get { return scalemin; }
            set {
                if (value < 0.0 || value >= 1.0)
                    throw new ArgumentException("DrawMap.ScaleMin: invalid argument (must be in the [0.0,1.0) range)!\n");
                scalemin = value;
                if (scalemax < scalemin) scalemax = 1.0;
            }
        }
        /// <summary>
        /// set the maximum of the colorscale (relative value, set between 0.0 and 1.0)
        /// </summary>
        public double ScaleMax {
            get { return scalemax; }
            set {
                if (value <= 0.0 || value > 1.0)
                    throw new ArgumentException("DrawMap.ScaleMax: invalid argument (must be in the (0.0,1.0] range)!\n");
                scalemax = value;
                if (scalemax < scalemin) scalemin = 0.0;
            }
        }

        public DrawMap() {
            InitializeMembers();
        }

        private void InitializeMembers() {
            armap =  new Bitmap[regions];
            armap2 = new Bitmap[regions];
            projections =  new Projection[regions];
            projections2 = new Projection[regions];
            for (uint i = 0; i < regions; i++) {
                armap[i] = null;
                armap2[i] = null;
                projections[i] = null;
                projections2[i] = null;
            }
            ColorScale = null;
            colors = null;
            ids = null;
            g1 = null;
            n = 0;
            cstr = null;
            qvals = null;
            qids = null;
            qvalues = 0;
            nusers = null;
            nmentions = null;
            nmentionsout = null;
            scalemin = 0.0;
            scalemax = 1.0;
            colorbar = false;
            colorbarwidth = 20;
            colorbartextwidth = 120;
            colorbartics = 5;
            StaticColor = Color.FromArgb(240, 240, 210);
            pen = null;
            MapView = 0;
            ga = null;
            OutLinks = false;
        }

        /// <summary>
        /// normalizáláshoz szükséges adatok letöltése az adatbázisból
        /// </summary>
        public void GetNormalizations() {
            string userquery = "select id, cnt from dkondor.dbo.region_norm1 order by id";
            string mentionquery = "select id, cnt from dkondor.dbo.region_norm2 order by id";

            string mentionqueryout = "select id, cnt from dkondor.dbo.region_norm3 order by id";


            nusers = new Dictionary<int,double>();
            nmentions = new Dictionary<int,double>();
            nmentionsout = new Dictionary<int, double>();

            SqlConnection scnn = new SqlConnection(cstr);
            scnn.Open();
            SqlCommand cmd = new SqlCommand(userquery, scnn);
            SqlDataReader sr = cmd.ExecuteReader();
            System.Object[] data = new System.Object[2]; //elvileg tudjuk, hogy 2 értéket kapunk
            while (sr.Read()) {
                int cntd = sr.GetValues(data);
                if (cntd < 2) throw new Exception("DrawMap.GetNormalizations(): bad data!\n");
                int id1 = (int)data[0];
                double cnt = (double)(int)data[1];
                nusers.Add(id1, cnt);
            }
            sr.Close();

            SqlCommand cmd2 = new SqlCommand(mentionquery, scnn);
            sr = cmd2.ExecuteReader();
            while (sr.Read()) {
                int cntd = sr.GetValues(data);
                if (cntd < 2) throw new Exception("DrawMap.GetNormalizations(): bad data!\n");
                int id1 = (int)data[0];
                double cnt = (double)(int)data[1];
                nmentions.Add(id1, cnt);
            }
            sr.Close();

            SqlCommand cmd3 = new SqlCommand(mentionqueryout, scnn);
            sr = cmd3.ExecuteReader();
            while (sr.Read()) {
                int cntd = sr.GetValues(data);
                if (cntd < 2) throw new Exception("DrawMap.GetNormalizations(): bad data!\n");
                int id1 = (int)data[0];
                double cnt = (double)(int)data[1];
                nmentionsout.Add(id1, cnt);
            }
            sr.Close();
            scnn.Close();

        }

        /// <summary>
        /// create the map for further rendering
        /// </summary>
        /// <param name="ga"></param>
        public void CreateMap(Size size, uint r) {
            if (ga == null)
                throw new ArgumentException("DrawMap.CreateMap(): Data source not set!\n");
if (r >= regions)
                throw new ArgumentException("DrawMap.CreateMap(): View ID must be less than the number of possible IDs!\n");
            if (armap[r] != null) armap[r] = null;
            if (armap2[r] != null) armap2[r] = null;

            Size size1;
            if (colorbar) size1 = new Size(size.Width - colorbartextwidth - colorbarwidth, size.Height);
            else size1 = size;
            Size size2;
            {
                Projections.Equirectangular p1 = new Projections.Equirectangular((Projection.ProjectionView)r, size1.Width, size1.Height);
                Projections.Equirectangular p2 = new Projections.Equirectangular((Projection.ProjectionView)r, 5 * size1.Width, 5 * size1.Height);
                projections2[r] = p2;
                projections[r] = p1;
                int w2, h2;
                p1.GetImageSize(out w2, out h2);
                size = new Size(w2, h2);
                p2.GetImageSize(out w2, out h2);
                size2 = new Size(w2, h2);
            }
            Bitmap rmap;
            if (colorbar) rmap = new Bitmap(size.Width + colorbarwidth + colorbartextwidth, size.Height, PixelFormat.Format32bppArgb);
            else rmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            Size realsize;
            if (colorbar) realsize = new Size(size.Width + colorbarwidth + colorbartextwidth, size.Height);
            else realsize = size;
            Bitmap rmap2 = new Bitmap(size2.Width, size2.Height, PixelFormat.Format32bppArgb);
            
            mapview = r;
            g1 = Graphics.FromImage(rmap);
            g1.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g1.Transform = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, rmap.Height);
            g2 = Graphics.FromImage(rmap2);
            g2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g2.Transform = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, rmap2.Height);

            Brush b1 = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
            g2.FillRectangle(b1, new Rectangle(new Point(0, 0), realsize));

            if (ids != null) ids = null;
            ids = new Dictionary<int, int>();

            if (pen == null) pen = new Pen(Color.FromArgb(255, 10, 10, 10));

            ga.OpenForIDs();
            SqlGeography geo;
            int ID;
            int id2 = 0;
            while (ga.ReadNextIDs(out ID, out geo)) {
                id2++;
                ids.Add(ID, id2);
                RenderGeography(geo, id2);
            }
            n = id2; //ID-k: 1-től id2-ig, tárolás: 0-tól id2-1-ig
            colors = new Color[n];
            g1.Dispose();
            g2.Dispose();
            g1 = null;
            g2 = null;

            armap[r] = rmap;
            armap2[r] = rmap2;
        }

        protected void RenderGeography(SqlGeography geo, Int32 val) {
            var numgeo = geo.STNumGeometries().Value;

            if (numgeo > 1) {
                for (int i = 0; i < numgeo; i++) {
                    RenderGeography(geo.STGeometryN(i + 1), val);  // indexed from 1!
                }
            }
            else {
                var type = geo.STGeometryType().Value;

                switch (type) {

                    case "Polygon":
                        RenderPolygon(geo, val);
                        break;
                    case "LineString":
                        RenderPolyline(geo);
                        break;
                    case "Point":
                        RenderPoint(geo);
                        break;
                    case "CircularString":
                    case "CompoundCurve":
                    case "CurvePolygon":
                    case "GeometryCollection":
                    case "MultiPoint":
                    case "MultiLineString":
                    case "MultiPolygon":
                    default:
                        //throw new NotImplementedException();    // TODO
                        break;
                }
            }
        }


        private void RenderPolyline(SqlGeography geo) {
            foreach (var pp in MapPoints(projections2[mapview],geo)) {
                g2.DrawLines(pen, pp);
            }
        }
        
        private void RenderPolygon(SqlGeography geo, Int32 val) {
            foreach (var pp in MapPoints(projections[mapview],geo)) {

                int a = 255;
                int b = val & 255;
                int g = (val / 256) & 255;
                int r = (val / 65536) & 255;
                Color c = Color.FromArgb(a, r, g, b);
                SolidBrush sb = new SolidBrush(c);
                g1.FillPolygon(sb, pp);

                //g2.DrawPolygon(pen, pp);
            }
            foreach (var pp in MapPoints(projections2[mapview], geo)) {
                g2.DrawPolygon(pen, pp);
            }
        }

        private void RenderPoint(SqlGeography geo) {
            var gp = new GeoPoint(geo.Long.Value, geo.Lat.Value);
            var mp = projections2[mapview].Map(gp);

            g2.DrawLine(pen, (int)mp.X, (int)mp.Y, (int)mp.X + 1, (int)mp.Y + 1);
        }

        private Point[][] MapPoints(Projection p, SqlGeography geo) {
            var sink = new MapGeographySink(p);
            geo.Populate(sink);

            var res = new Point[sink.Shapes.Count][];

            for (int i = 0; i < sink.Shapes.Count; i++) {
                var pp = sink.Shapes[i];
                res[i] = new Point[pp.Length];
                for (int j = 0; j < pp.Length; j++) {
                    res[i][j] = new Point((int)pp[j].X, (int)pp[j].Y);
                }
            }

            return res;
        }


        private void OnBeginRender() {
            if (armap[MapView] == null || n == 0 || ids == null || armap2[MapView] == null) {
                throw new Exception("DrawMap.OnBeginRender(): uninitialized members! (first call CreateMap())\n");
            }
            
            if (ColorScale == null) ColorScale = new ColorVal();

            for (int i = 0; i < n; i++) {
                colors[i] = ColorScale.GetColor(0.0);
            }
        }

        /// <summary>
        /// store one region and the corresponding value
        /// </summary>
        /// <param name="context">rendering context (not used)</param>
        /// <param name="values">region ID, value (int, double)</param>
        private void OnRender(int region, double val) {
            int id2 = ids[region] - 1;
            if (val < 0.0) colors[id2] = StaticColor;
            else colors[id2] = ColorScale.GetColor(val);
        }

        private Bitmap OnEndRender(double min, double max) {
            Bitmap rmap = armap[MapView];
            Bitmap rmap2 = armap2[MapView];
            Rectangle rect = new Rectangle(0, 0, rmap.Width, rmap.Height);
            Bitmap bmp = rmap.Clone(rect, PixelFormat.Format32bppArgb);
            
            byte[] buffer;
            int bytes;
            int stride;

            int b = 0;
            int g = 1;
            int r = 2;
            int a = 3;

            BitmapData bitmapData = bmp.LockBits(rect,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);

            int numbytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            buffer = new byte[numbytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, numbytes);

            bytes = 4;
            stride = bitmapData.Stride;

            if (colorbar) rect.Width -= (colorbartextwidth + colorbarwidth);

            for (int i = 0; i < rect.Height; i++) for (int j = 0; j < rect.Width; j++) {
                int k = i*stride + j*bytes;
                int id1 = buffer[k + b] + 256 * buffer[k + g] + 65536 * buffer[k + r];
              /*  Color c1 = Color.FromArgb(
                    buffer[k + a],
                    buffer[k + r],
                    buffer[k + g],
                    buffer[k + b]);
                int id1 = c1.ToArgb();*/
                Color c2;
                if (id1 == 0) { //háttér
                    c2 = bgcolor;
                }
                else {
                    id1--;
                    if (id1 >= n) throw new Exception("Hibás adatok!\n");
                    c2 = colors[id1];
                }
                buffer[k + a] = (byte)c2.A;
                buffer[k + r] = (byte)c2.R;
                buffer[k + g] = (byte)c2.G;
                buffer[k + b] = (byte)c2.B;
            }

            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);

            bmp.UnlockBits(bitmapData);

            Graphics g3 = Graphics.FromImage(bmp);
            g3.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g3.DrawImage(rmap2, rect);
            if (colorbar) {
                g3.FillRectangle(new SolidBrush(Color.White), new Rectangle(rect.Width, 0, colorbartextwidth + colorbarwidth, rect.Height));
                string[] tics = new string[colorbartics];
                for (uint i = 0; i < colorbartics; i++) {
                    double val = min + (max - min) * ((double)i) / ((double)(colorbartics - 1));
                    if (LogScale) val = Math.Pow(10.0, val);
                    if (Normalization == NormType.None) tics[i] = Math.Floor(val).ToString("G4");
                    else {
                        if (val < 0.01) {
                            tics[i] = val.ToString("E2");
                        }
                        else tics[i] = val.ToString("G3");
                    }
                }

                Bitmap bmp2 = Layers.Colorbar.RenderColorBarWithTics(colorbarwidth, rect.Height - 64, colorbartextwidth, ColorScale, tics);
                g3.DrawImage(bmp2, new Point(rect.Width, 32));
            }
            g3.Dispose();
            g3 = null;

            bitmapData = null;
            buffer = null;

            return bmp;
        }


 /*       public void SaveMap(string fn) {
            rmap.Save(fn, ImageFormat.Png);
        }*/


        /// <summary>
        /// Load the mention data from the database
        /// </summary>
        /// <param name="ID">target region</param>
        public void LoadData(int ID) {
            string query;
            //bejövő mention-ök
            if(!OutLinks) query = "select cast(r1 as int), cast(mentioncnt as double precision) from dkondor.dbo.region_links where r2 = " + ID + " order by r1";
            //kimenő mention-ök
            else query = "select cast(r2 as int), cast(mentioncnt as double precision) from dkondor.dbo.region_links where r1 = " + ID + " order by r2";
            //Projection.ProjectionView view = Projection.ProjectionView.World;
            dID = ID;

            List<double> val = new List<double>();
            List<int> ids = new List<int>();
            SqlConnection scnn = new SqlConnection(cstr);
            scnn.Open();
            SqlCommand cmd = new SqlCommand(query, scnn);
            SqlDataReader sr = cmd.ExecuteReader();
            System.Object[] data = new System.Object[2]; //elvileg tudjuk, hogy 1 értéket kapunk
            qvalues = 0;
            while (sr.Read()) {
                int cntd = sr.GetValues(data);
                if (cntd < 2) continue;
                int id1 = (int)data[0];
                ids.Add(id1);
                double d1 = (double)data[1];
                val.Add((double)d1);
                qvalues++;
            }
            scnn.Close();

            qvals = val.ToArray();
            qids = ids.ToArray();
        }


        /// <summary>
        /// Draw the map colored by the mentions
        /// </summary>
        /// <param name="n">normalization to use</param>
        /// <param name="cm">color scale to use</param>
        /// <param name="mapscale">zoom (pixels per degree)</param>
        /// <param name="logscale">use logscale</param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public Bitmap DrawMentionMap() {
            NormType n = Normalization;
            bool logscale = LogScale;
            bool dest = Dest;

            double[] val2 = new double[qvals.Length];
            double min, max;

            min = Double.MaxValue;
            max = Double.MinValue;

            if (n != NormType.None) {
                if (nusers == null || nmentions == null) GetNormalizations();
            }

            for (int i = 0; i < val2.Length; i++) {
                double d1 = qvals[i];
                if (!dest && (qids[i] == dID)) {
                    val2[i] = -1.0;
                }
                else {
                    switch (n) {
                        case NormType.None:
                            break;
                        case NormType.UserCount: {
                                double d2 = nusers[qids[i]];
                                d1 = d1 / d2;
                                //if (logscale) d1 += 1.0; // így pozitív logaritmus értéket kapunk
                            }
                            break;
                        case NormType.MentionCount: {
                                double d2;
                                if (OutLinks) d2 = nmentionsout[qids[i]];
                                else d2 = nmentions[qids[i]];
                                d1 = d1 / d2;
                                //if (logscale) d1 += 1.0;
                            }
                            break;
                        default:
                            throw new NotImplementedException("DrawMap.DrawMentionMap(): unknown normalization type!\n");
                    }
                    if (logscale) d1 = Math.Log10(d1);
                    if (d1 < min) min = d1;
                    if (d1 > max) max = d1;
                    val2[i] = d1;
                }
            }
            if (!logscale) min = 0.0;
            {
                double min2 = min + scalemin * (max - min);
                double max2 = min + scalemax * (max - min);
                min = min2;
                max = max2;
            }
            for (int i = 0; i < val2.Length; i++) {
                if (!dest && (qids[i] == dID)) {
                    val2[i] = -1.0;
                }
                else {
                    if (val2[i] <= min) val2[i] = 0.0;
                    else if (val2[i] >= max) val2[i] = 1.0;
                    else val2[i] = (val2[i] - min) / (max - min);
                }
            }


            OnBeginRender();
            for (int i = 0; i < val2.Length; i++) {
                OnRender(qids[i], val2[i]);
            }
            return OnEndRender(min,max);
        }

        /// <summary>
        /// resize the maps (may take a long time, all of the regions need to be redrawn)
        /// </summary>
        /// <param name="size"></param>
        public void SizeChanged(Size size) {
            for (uint i = 0; i < regions; i++) CreateMap(size, i);
        }
    }
}
