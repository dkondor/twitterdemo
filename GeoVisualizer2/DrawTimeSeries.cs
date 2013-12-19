using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

/* TODO:
 * színskála az x tengely helyett
 * x tengely feliratok elforgatva
 */

namespace Elte.GeoVisualizer.Lib {
    /// <summary>
    /// Draw a simple time series of some data, supplied by a CoordArray (only the 3rd column is used),
    /// set the min/max, number of bins, and possible logscale before plotting
    /// </summary>
    public class DrawTimeSeries {
        /// <summary>
        /// use log-scale for the y axis
        /// </summary>
        public bool LogScale;
        /// <summary>
        /// minimum timestamp (timestamps before this are ignored)
        /// </summary>
        public double min;
        /// <summary>
        /// maximum timestamp (timestamps after this are ignored)
        /// </summary>
        public double max;
        /// <summary>
        /// size of bins to use
        /// </summary>
        public double binsize;

        private double[] bins;
        private uint[] counts;
        private uint count;
        private uint binmax;

        /// <summary>
        /// Set default values. Use the LoadData() function to load values and the Draw() function to plot.
        /// </summary>
        public DrawTimeSeries() {
            InitializeMembers();
        }

        private void InitializeMembers() {
            bins = null;
            counts = null;
            min = -1.0;
            max = -1.0;
            LogScale = false;
            binsize = 86400.0;
            count = 0;
            binmax = 0;
            ticsfont = null;
            colorx = false;
            cv = null;
            colorbox = 16.0F;
        }

        /// <summary>
        /// return the total number of points used
        /// </summary>
        public uint Count {
            get { return count; }
        }

        //parameters for the plot
        /// <summary>
        /// margin below the x-axis -- it is used to plot the x labels / tics
        /// </summary>
        public float xmargin = 100;
        /// <summary>
        /// margin left to the y-axis
        /// </summary>
        public float ymargin = 100;
        /// <summary>
        /// margin below and over the datapoints (in the plot)
        /// </summary>
        public float xdatamargin = 0.05F;
        /// <summary>
        /// margin before and after the datapoints (in the plot)
        /// </summary>
        public float ydatamargin = 0.05F;
        
        /// <summary>
        /// width of the axes
        /// </summary>
        public float axiswidth = 2;
        /// <summary>
        /// line width (for the plot line)
        /// </summary>
        public float linewidth = 2;
        /// <summary>
        /// axis color
        /// </summary>
        public Color axiscolor = Color.Black;
        /// <summary>
        /// Line color (plot line)
        /// </summary>
        public Color linecolor = Color.DarkRed;

        /// <summary>
        /// number of tics on the y-axis
        /// </summary>
        public uint ytics = 5;
        /// <summary>
        /// number of tics on the x axis
        /// </summary>
        public uint xtics = 8;
        /// <summary>
        /// font to use for the tics
        /// </summary>
        public Font ticsfont;

        /// <summary>
        /// color scale to use for the X-axis
        /// </summary>
        public ColorVal cv;

        /// <summary>
        /// draw the X-axis using a colorscale
        /// </summary>
        public bool colorx;

        /// <summary>
        /// height of the colorbox
        /// </summary>
        public float colorbox;

        /// <summary>
        /// include the year part of the date in the x labels
        /// </summary>
        public bool writeyear = false;

        protected Size size;

        float ticsize;

        float fontsize;

        /// <summary>
        /// Size of the image to draw.
        /// Setting the size also automatically adjusts the font size and line width used for the drawing.
        /// If needed, those can be altered manually, after the size has been set.
        /// </summary>
        public Size Size {
            get { return size; }
            set {
                size = value;
                linewidth = (float)size.Width / 500.0F;
                axiswidth = linewidth;
                float fs = 12.0F * ((float)size.Width) / 800.0F;
                fontsize = fs;
                ticsfont = new Font(FontFamily.GenericSansSerif, fs);
                xmargin = size.Height * 0.15F;
                ymargin = size.Width * 0.09F;
                ticsize = size.Height * 0.03F;
                colorbox = 0.06F * size.Height;
            }
        }

        /// <summary>
        /// Load the data from the given data source
        /// </summary>
        /// <param name="ds">data source to use</param>
        /// <param name="col">column containing the timestamps</param>
        public uint LoadData(DataSource ds, uint col) {
            if (min < 0.0 || max < 0.0 || max <= min)
                throw new ArgumentException("DrawTimeSeries.LoadData(): minimum and/or maximum set to invalid value!\n");
            if (binsize >= max - min)
                throw new ArgumentException("DrawTimeSeries.LoadData(): binsize must be less than the interval!\n");
            uint nbins = (uint)Math.Ceiling((max - min) / binsize);
            if (bins != null) bins = null;
            if (counts != null) counts = null;
            bins = new double[nbins];
            counts = new uint[nbins];
            for (uint i = 0; i < nbins; i++) {
                counts[i] = 0;
                bins[i] = min + i * binsize + binsize / 2.0;
            }
            count = 0;

            ds.Open();
            string[] names = ds.GetColumnNames();
            if (names.Length < col) throw new Exception("DrawTimeSeries.LoadData(): too few columns in the data source!\n");
            object[] data = new object[names.Length];
            while (ds.ReadNext(data)) {
                double x;
                Type t = data[col].GetType();
                if (t.Equals(typeof(Double))) x = (double)data[col];
                else if (t.Equals(typeof(Single))) x = (double)(float)data[col];
                else if (t.Equals(typeof(Int32))) x = (double)(int)data[col];
                else {
                    IConvertible data1 = data[col] as IConvertible;
                    if (data1 == null)
                        throw new Exception("DrawTimeSeries.LoadData(): cannot convert data to double!\n");
                    x = data1.ToDouble(null);
                }

                if (x < min) continue;
                if (x >= max) continue;
                uint bin = (uint)Math.Floor((x - min) / binsize);
                counts[bin]++;
                count++;
            } // while

            binmax = 0;
            for (uint i = 0; i < nbins; i++) if (counts[i] > binmax) binmax = counts[i];
            return count;
        }

        /// <summary>
        /// Plot the time series with the previously set parameters.
        /// The LoadData() function should have been called previously to load the data to plot.
        /// </summary>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <returns>the resulting image</returns>
        public Bitmap Draw(int w, int h) {
            if (bins == null || counts == null)
                throw new Exception("DrawTimeSeries.Draw(): no data loaded! (Use the LoadData function first)\n");
            Bitmap bmp = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 0, 0)), new Rectangle(0, 0, w, h));

            Pen axispen = new Pen(axiscolor);
            axispen.Width = axiswidth;
            Pen linepen = new Pen(linecolor);
            linepen.Width = linewidth;

            float width = (float)w;
            float height = (float)h;
            float plotleft = width * ydatamargin + (float)ymargin;
            float plotright = width * (1 - ydatamargin);
            float plotbottom = height * (1 - xdatamargin) - (float)xmargin;
            float plottop = height * xdatamargin;
            float plotheight = plotbottom - plottop;
            float plotwidth = plotright - plotleft;

            //draw the axes
            {
                float xaxisy = h - (float)xmargin;
                float xaxismin = (float)ymargin / 2;
                float xaxismax = w;
                g.DrawLine(axispen, new PointF(xaxismin, xaxisy), new PointF(xaxismax, xaxisy));
                if (colorx) {
                    if (cv == null)
                        throw new ArgumentNullException("DrawTimeSeries.Draw(): no colorscale specified!\n");
                    Bitmap bmp2 = Layers.Colorbar.RenderColorbar1((int)plotwidth, (int)colorbox, cv, true);
                    g.DrawImage(bmp2, new PointF(plotleft, height - xmargin + 2.0F));
                }

                float yaxisx = (float)ymargin;
                float yaxismin = 0;
                float yaxismax = h - (float)xmargin / 2;
                g.DrawLine(axispen, new PointF(yaxisx, yaxismin), new PointF(yaxisx, yaxismax));
            }

            //draw the points
            double bmd = (double)binmax;
            if (binmax == 0) bmd = 1.0;
            if (LogScale) bmd = Math.Log10(bmd);
            float x1, y1;
            {
                double lasty = (double)counts[0];
                if (LogScale) {
                    if (lasty > 0.0) {
                        lasty = Math.Log10(lasty) / bmd;
                    }
                }
                else {
                    lasty = lasty / bmd;
                }
                x1 = (float)(plotleft);
                y1 = (float)(plottop + ((float)(1.0 - lasty)) * plotheight);
            }
            for (uint i = 1; i < counts.Length; i++) {
                float x, y;
                double y2;
                x = ((float)i) / ((float)(counts.Length-1));
                y2 = (double)counts[i];
                if (LogScale) {
                    if (y2 > 0.0) {
                        y2 = Math.Log10(y2) / bmd;
                    }
                }
                else {
                    y2 = y2 / bmd;
                }
                x = (plotleft + x * plotwidth);
                y = (plottop + ((float)(1.0 - y2)) * plotheight);

                g.DrawLine(linepen, new PointF(x1, y1), new PointF(x, y));
                x1 = x;
                y1 = y;
            }

            DrawYTics(g, plotleft, plotwidth, plotbottom, plotheight, axispen);
            float xfpos = height - xmargin;
            if (colorx) xfpos += colorbox + 4.0F;
            DrawXTics(g, plotleft, plotwidth, plotbottom, plotheight, height - xmargin, xfpos, axispen);

            g.Dispose();
            g = null;
            return bmp;
        } //Draw

        public Bitmap Draw() {
            return Draw(Size.Width, Size.Height);
        }


        protected void DrawYTics(Graphics g, float plotleft, float plotwidth, float plotbottom, float plotheight, Pen lp) {
            if (ytics == 0) return;
            if (ticsfont == null) ticsfont = new Font(FontFamily.GenericSansSerif, (float)12.0);
            //0
            float fonth = (float)ticsfont.GetHeight();
            float fontmargin = (float)2.0;
            SolidBrush brush = new SolidBrush(axiscolor);
            RectangleF rect = new RectangleF(fontmargin, plotbottom - fonth/2.0F, (float)ymargin - 2.0F*fontmargin, fonth);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;
            format.LineAlignment = StringAlignment.Center;
            g.DrawLine(lp, new PointF((float)ymargin, plotbottom), new PointF((float)ymargin + ticsize, plotbottom));
            g.DrawString("0", ticsfont, brush, rect, format);

            uint ytics2 = ytics;
            double bmd = (double)binmax;
            if (binmax == 0) {
                ytics2 = 0;
                bmd = 1.0;
            }
            if (LogScale) bmd = Math.Log10(bmd);
            for (uint i = 1; i < ytics; i++) {
                float pos = (float)i / ((float)(ytics - 1));
                float y = plotbottom - pos * plotheight;
                g.DrawLine(lp, new PointF((float)ymargin, y), new PointF((float)ymargin + ticsize, y));
                rect = new RectangleF(fontmargin, y - fonth/2.0F, (float)ymargin - 2.0F*fontmargin, fonth);
                double val = bmd * ((double)pos);
                if (LogScale) val = Math.Pow(10.0, val);
                string num = Math.Floor(val).ToString();
                g.DrawString(num, ticsfont, brush, rect, format);
            }
        }

        protected string DateString(DateTime dt) {
            if (writeyear) return dt.ToShortDateString();
            else return dt.ToString("MM.dd.");
        }

        protected void DrawXTics(Graphics g, float plotleft, float plotwidth, float plotbottom, float plotheight, float ticsypos, float textypos, Pen lp) {
            if (xtics == 0) return;
            if (ticsfont == null) ticsfont = new Font(FontFamily.GenericSansSerif, (float)12.0);

            bool hour = binsize < 86400.0;

            float fontlen = 150.0F;
            float fonth = (float)ticsfont.GetHeight();
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            {
                string t1 = DateString(dt);
                if (hour) t1 += Environment.NewLine + dt.ToShortTimeString();
                SizeF s = g.MeasureString(t1, ticsfont);
                fontlen = s.Width + fontsize*2.0F;
                fonth = s.Height;
            }

            if (hour) fonth *= 2.0F;
            float fontmargin = (float)2.0;
            float ticsize = (float)8.0;
            SolidBrush brush = new SolidBrush(axiscolor);
            RectangleF rect = new RectangleF(plotleft - fontlen / 2.0F, textypos + fontmargin, fontlen, fonth);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Near;
            g.DrawLine(lp, new PointF(plotleft, ticsypos), new PointF(plotleft, ticsypos - ticsize));
            {
                DateTime dt2 = dt.AddSeconds(min);
                string day = DateString(dt2);
                if (hour) day += Environment.NewLine + dt2.ToShortTimeString();
                g.DrawString(day, ticsfont, brush, rect, format);
            }

            double bmd = (double)binmax;
            if (LogScale) bmd = Math.Log10(bmd);
            for (uint i = 1; i < xtics; i++) {
                float pos = (float)i / ((float)(xtics - 1));
                float x = plotleft + pos * plotwidth;
                g.DrawLine(lp, new PointF(x, ticsypos), new PointF(x, ticsypos - ticsize));
                rect = new RectangleF(x - fontlen / 2.0F, textypos + fontmargin, fontlen, fonth);
                double val = min + ((double)pos) * (max - min);
                {
                    DateTime dt2 = dt.AddSeconds(val);
                    string day = DateString(dt2);
                    if (hour) day += Environment.NewLine + dt2.ToShortTimeString();
                    g.DrawString(day, ticsfont, brush, rect, format);
                }
            }
        }



    }
}
