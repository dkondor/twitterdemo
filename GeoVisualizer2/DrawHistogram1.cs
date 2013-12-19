using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace Elte.GeoVisualizer.Lib {
    /// <summary>
    /// draw a histogram based on a query (word or hashtag)
    /// </summary>
    public class DrawHistogram {
        DataSources.CoordArray ca;
        //bool data;
        string cstr;
        protected Bitmap map; //statikus térkép, erre rajzolunk rá

        protected Size size;

        public DrawHistogram() {
            //data = false;
            ca = new DataSources.CoordArray();
            cstr = null;
            map = null;
        }

        public DrawHistogram(string cstr1) {
            //data = false;
            ca = new DataSources.CoordArray();
            cstr = cstr1;
            map = null;
        }

        public DrawHistogram(string cstr1, int w, int h) {
            //data = false;
            ca = new DataSources.CoordArray();
            cstr = cstr1;
            size.Width = w;
            size.Height = h;
            map = DrawMap(w, h, cstr);
        }

        public Size Size {
            get { return size; }
            set {
                if (cstr == null)
                    throw new ArgumentNullException("DrawHistogram.Size: the connection string was not specified!");
                size = value;
                map = DrawMap(size.Width, size.Height, cstr);
            }
        }

        /// <summary>
        /// Draw the background map, where the histograms can be overlayed
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="cstr">connection string to use</param>
        /// <returns></returns>
        public static Bitmap DrawMap(int w, int h, string cstr) {
            return DrawMap1(w, h, cstr, "select Geom from dkondor.dbo.region03");
        }

        /// <summary>
        /// Draw the background map, where the histograms can be overlayed
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        public void DrawMap(int w, int h) {
            if (cstr == null)
                throw new ArgumentNullException("DrawHistogram.Size: the connection string was not specified!");
            if (size == null) size = new Size();
            size.Width = w;
            size.Height = h;
            map = DrawMap(w, h, cstr);
        }

        /// <summary>
        /// Draw the background map, where the histograms can be overlayed
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="cstr">connection string to use</param>
        /// <param name="query">query which returns the geometries</param>
        /// <returns></returns>
        public static Bitmap DrawMap1(int w, int h, string cstr, string query) {
            Map map1 = new Map();
            double scale = ((double)w) / 360.0;
            map1.Projection = new Projections.Equirectangular(Projection.ProjectionView.World, scale);

            Layers.Geography geo = new Layers.Geography();
            DataSources.SqlQuery sq = new DataSources.SqlQuery();
            sq.ConnectionString = cstr;
            sq.Command = new SqlCommand(query);
            geo.DataSource = sq;
            geo.StaticColor = Color.LightGray;

            geo.StaticColor = Color.FromArgb(0, 30, 30);
            geo.pen = Pens.Black;

            Layers.Background bg = new Layers.Background();
            bg.Color = Color.FromArgb(255, 21, 23, 62);
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
            if (max > 0) query1 = "select top " + max + " lon,lat,tday " + //!! TODO: időpont itt is másodpercben kellene, hogy egységes legyen !!
                 "from dkondor.dbo.th200ll where tag = '" + query + "'";
            else query1 = "select lon,lat,tday " +
                "from dkondor.dbo.th200ll where tag = '" + query + "'";
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
            if (max > 0) query1 = "select top " + max + " lon,lat,datediff(second,'1970-01-01 0:00:00.000',created_at) " +
                 "from dkondor.dbo.thstweetids3 where tag = " + query; //!! TODO: adatok rendezése, kigyűjtése, query megírása !!
            else query1 = "select lon,lat,datediff(second,'1970-01-01 0:00:00.000',created_at) " +
                "from dkondor.dbo.thstweetids3 where tag = " + query;
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

            return query1;
        }

        public int GetDataNum() {
            return ca.GetDataNum();
        }



        /// <summary>
        /// Draw a histogram of the previously loaded data
        /// </summary>
        /// <returns></returns>
        public Bitmap Draw() {
            if (map == null) {
                if (size.Width == 0 || size.Height == 0) {
                    size.Width = 1600;
                    size.Height = 800;
                }
                DrawMap(size.Width, size.Height);
            }

            Map map1 = new Map();
            double scale = ((double)(size.Width)) / 360.0;
            map1.Projection = new Projections.Equirectangular(Projection.ProjectionView.World, scale);

            Layers.Histogram h1 = new Layers.Histogram();
            h1.DataSource = ca;
            h1.Color = Color.AliceBlue;

            /*h1.Col3 = false;
            Layers.ColorVal cv = new Layers.ColorVal();
            cv.CMType = ColorVal.ColorMapType.ConstSat;
            cv.CSType = ColorVal.ConstSatType.ThermalInv;
            h1.ColorVal1 = cv;
            */

            Layers.Background bg = new Layers.Background();
            bg.Color = Color.FromArgb(0, 0, 0, 0);
            bg.DataSource = DataSource.Null;

            map1.Layers.Add(h1);
            map1.Layers.Add(bg);

            Rectangle rect = new Rectangle(new Point(0, 0), size);
            Bitmap bmp = map.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            map1.Render(bmp, rect);
            return bmp;
        }
    }
}


