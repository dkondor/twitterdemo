using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Elte.GeoVisualizer.Lib.Layers
{
    public class Histogram : Layer
    {
        private Color color;

        private Layer alpha;
        private double[,] hist;
        private double[,] c3hist;
        private double[,] c3hist2;
        private double[,] kernel;

        /// <summary>
        /// default color to use (if no third column is given)
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public Layer Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }

        private bool col3;
        private bool rendering; //true, ha éppen renderel, ekkor az ncolors változót nem lehet átállítani

        /// <summary>
        /// If set, the query should return a third column which is used to determine the color of points.
        /// The range can be set with the C3Min and C3Max fields, or determined automatically
        /// </summary>
        public bool Col3 {
            get { return col3; }
            set {
                if (rendering) {
                    throw new Exception("Histogram: cannot set mode while rendering!\n");
                }
                col3 = value;
                if (col3) {
                    if (cv == null) cv = new ColorVal();
                    cv.CMType = ColorVal.ColorMapType.ConstSat;
                    cv.CSType = ColorVal.ConstSatType.SineRed;
                }
            }
        }

        private double c3min; //színskálához használt oszlop minimuma
        private double c3max; //maximuma
        private bool c3auto; //autoscale használata a harmadik oszlop adatain
        private ColorVal cv;

        /// <summary>
        /// Minimum possible value in the third column
        /// Can be set explicitly before running
        /// </summary>
        public double C3Min {
            get { return c3min; }
            set {
                c3auto = false;
                c3min = value;
            }
        }

        /// <summary>
        /// Maximum possible value in the third column
        /// Can be set explicitly before running
        /// </summary>
        public double C3Max {
            get { return c3max; }
            set {
                c3auto = false;
                c3max = value;
            }
        }

        /// <summary>
        /// set autoscale
        /// </summary>
        public bool C3Auto {
            get { return c3auto; }
            set {
                if (rendering && value)
                    throw new Exception("Cannot set autoscale while rendering!\n");
                c3auto = value;
            }
        }

        /// <summary>
        /// colorscale to use when using a third column for color -- warning: only the hue value is used,
        /// the saturation and value are calculated based on the histogram data
        /// </summary>
        public ColorVal ColorVal1 {
            get { return cv; }
            set {
                if (rendering) {
                    throw new Exception("Histogram: cannot set colorscale while rendering!\n");
                }
                cv = value;
            }
        }

        public override IEnumerable<Layer> Layers
        {
            get
            {
                if (alpha == null)
                {
                    yield break;
                }
                else
                {
                    yield return alpha;
                }
            }
        }

        public Histogram()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.color = Color.White;
            c3min = 0.0;
            c3max = 1.0;
            col3 = false;
            c3auto = false;
            cv = null;
        }

        public override void OnBeginRender(RenderingContext context)
        {
            base.OnBeginRender(context);

            hist = new double[context.Width, context.Height];
            if (col3) {
                c3hist2 = new double[context.Width, context.Height];
                c3hist = new double[context.Width, context.Height];
                if (c3auto) {
                    c3min = Double.MaxValue;
                    c3max = Double.MinValue;
                }
            }


     /*       kernel = new double[3, 3]
            { {0, 1, 0},
              {1, 3, 1},
              {0, 1, 0} }; */
            kernel = new double[9, 9] {
{   7.4466e-03,  2.4724e-02,  5.5023e-02,  8.2085e-02,  8.2085e-02,  5.5023e-02,  2.4724e-02,  7.4466e-03,  1.5034e-03},
{   2.4724e-02,  8.2085e-02,  1.8268e-01,  2.7253e-01,  2.7253e-01,  1.8268e-01,  8.2085e-02,  2.4724e-02,  4.9916e-03},
{   5.5023e-02,  1.8268e-01,  4.0657e-01,  6.0653e-01,  6.0653e-01,  4.0657e-01,  1.8268e-01,  5.5023e-02,  1.1109e-02},
{   8.2085e-02,  2.7253e-01,  6.0653e-01,  9.0484e-01,  9.0484e-01,  6.0653e-01,  2.7253e-01,  8.2085e-02,  1.6573e-02},
{   8.2085e-02,  2.7253e-01,  6.0653e-01,  9.0484e-01,  9.0484e-01,  6.0653e-01,  2.7253e-01,  8.2085e-02,  1.6573e-02},
{   5.5023e-02,  1.8268e-01,  4.0657e-01,  6.0653e-01,  6.0653e-01,  4.0657e-01,  1.8268e-01,  5.5023e-02,  1.1109e-02},
{   2.4724e-02,  8.2085e-02,  1.8268e-01,  2.7253e-01,  2.7253e-01,  1.8268e-01,  8.2085e-02,  2.4724e-02,  4.9916e-03},
{   7.4466e-03,  2.4724e-02,  5.5023e-02,  8.2085e-02,  8.2085e-02,  5.5023e-02,  2.4724e-02,  7.4466e-03,  1.5034e-03},
{   1.5034e-03,  4.9916e-03,  1.1109e-02,  1.6573e-02,  1.6573e-02,  1.1109e-02,  4.9916e-03,  1.5034e-03,  3.0354e-04}
            };

            rendering = true;
        }

        public override void OnRender(RenderingContext context, object[] values)
        {
            double lon = (double)values[0];
            double lat = (double)values[1];
            double c3 = 0.0;

            if (col3) {
                Type t = values[2].GetType();
                if(t.Equals(typeof(System.Double))) {
                    c3 = (double)values[2];
                }
                else {
                    if (t.Equals(typeof(System.Single))) {
                        c3 = (double)((float)values[2]);
                    }
                    else {
                        if (t.Equals(typeof(System.Int32))) {
                            c3 = (double)((int)values[2]);
                        }
                        else throw new Exception("Histogram.OnRender(): type of values[2] is not known!\n");
                    }
                }

                if (c3auto) {
                    if (c3 > c3max) c3max = c3;
                    if (c3 < c3min) c3min = c3;
                }

         /*       if (c3 > c3max) c3 = c3max; -- normalizálás a végén (OnEndRender() közben)
                else if (c3 < c3min) c3 = c3min;
                c3 = (c3 - c3min) / (c3max - c3min); */
            }

            var mp = context.Projection.Map(new GeoPoint(lon, lat));

            for (int ik = 0; ik < kernel.GetLength(0); ik++)
            {
                for (int jk = 0; jk < kernel.GetLength(1); jk++)
                {
                    int i = (int)mp.X + ik - (kernel.GetLength(0) - 1) / 2;
                    int j = context.Height - (int)mp.Y + jk - (kernel.GetLength(1) - 1) / 2;

                    if (i >= 0 && i < hist.GetLength(0) &&
                        j >= 0 && j < hist.GetLength(1))
                    {
                        if (col3) {
                            c3hist[i, j] += kernel[ik, jk] * c3;
                            c3hist2[i, j] += kernel[ik, jk] * c3 * c3;
                            hist[i, j] += kernel[ik, jk];
                        }
                        else hist[i, j] += kernel[ik, jk];
                    }
                }
            }
        }

        public override System.Drawing.Bitmap OnEndRender()
        {
            byte[] abuffer = null;
            int abytes;
            int astride;

            if (alpha != null)
            {
                alpha.OnEndRender();

                alpha.LockBits(out abuffer, out abytes, out astride);
                //alpha.UnlockBits();
            }

            double max = GetMax();

            byte[] buffer;
            int bytes;
            int stride;

            int b = 0;
            int g = 1;
            int r = 2;
            int a = 3;

            LockBits(out buffer, out bytes, out stride);

            for (int i = 0; i < hist.GetLength(0); i++)
            {
                for (int j = 0; j < hist.GetLength(1); j++)
                {
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
                            double stdev = 0.0;
                            if (stdev2 > 0.0) stdev = Math.Sqrt(sum2 / val - avg * avg); //szórás

                            //normálás, 0 és 1 közé
                            if (avg > c3max) avg = c3max;
                            if (avg < c3min) avg = c3min;
                            avg = (avg - c3min) / (c3max - c3min);
                            stdev = stdev / (c3max - c3min);
                            //!! 2013.05.13. gangnamusa térképhez átírva
                            stdev /= 1.5;
                            if (stdev > 1.0) stdev = 1.0;

                            //kérdés: mennyi lehet a szórás? [0,1] közötti eloszlásfüggvényre 0.5, de 1-nél mindenképpen kisebb
                            double h;
                            double s;
                            double v;
                            cv.GetColorHSV(avg, out h, out s, out v); //csak h-t használjuk, v = 1.0, s = sqrt(1-stdev)
                            v = 1.0;
                            s = 1.0 - Math.Sqrt(stdev);
                            Color c1 = ColorVal.hsvtorgb(h, s, v);

                            //double logval = Math.Log10(Math.Log10(val + 1) + 1); //kérdés, hogy ez szükséges-e? -- valószínűleg egyébként semmi sem látszana
                            double logval = Math.Log10(val + 1); //2013.05.15. -- csak egy logaritmust veszünk

                            buffer[k + r] = (byte)c1.R;
                            buffer[k + g] = (byte)c1.G;
                            buffer[k + b] = (byte)c1.B;

                            if (alpha != null)
                                buffer[k + a] = (byte)((logval / max) * color.A / 256.0 * abuffer[k + a]);
                            else
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
                        double v = Math.Log10(Math.Log10(hist[i, j] + 1) + 1);

                        if (cv != null) { // a megadott színt használjuk
                            if (v > 0.0) {
                                Color c1 = cv.GetColor(v / max);
                                buffer[k + r] = c1.R;
                                buffer[k + g] = c1.G;
                                buffer[k + b] = c1.B;
                                if (alpha != null)
                                    buffer[k + a] = (byte)(c1.A * abuffer[k + a] / 256);
                                else buffer[k + a] = c1.A;
                            }
                        }
                        else {
                            if (v > 0.0) {
                                buffer[k + r] = (byte)color.R;
                                buffer[k + g] = (byte)color.G;
                                buffer[k + b] = (byte)color.B;
                            }

                            if (alpha != null)
                                buffer[k + a] = (byte)((v / max) * color.A / 256.0 * abuffer[k + a]);
                            else
                                buffer[k + a] = (byte)((v / max) * color.A);
                        }
                    }
                }
            }

            UnlockBits();
            rendering = false;

            hist = null;
            if (col3) {
                c3hist = null;
                c3hist2 = null;
            }

            return base.OnEndRender();
        }

        private double GetMax()
        {
            double max1 = double.MinValue;
            
            for (int i = 0; i < hist.GetLength(0); i++)
            {
                for (int j = 0; j < hist.GetLength(1); j++)
                {
                    double val = hist[/*k,*/ i, j];
                    //double val = hist[k, i, j];
                    double logval = Math.Log10(Math.Log10(val + 1) + 1);
                    //double logval = Math.Log10(val + 1); //2013.05.15. -- csak egy logaritmust veszünk
                    max1 = Math.Max(max1, logval);
                }
            }

            return max1;
        }
    }
}
