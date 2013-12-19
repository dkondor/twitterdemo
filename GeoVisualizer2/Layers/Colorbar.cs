using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
//using Microsoft.SqlServer.Types;

namespace Elte.GeoVisualizer.Lib.Layers
{
    /// <summary>
    /// Draw a colorbar in a specified size
    /// </summary>
    public class Colorbar
    {
        /// <summary>
        /// Only static methods -- do not instantiate
        /// </summary>
        public Colorbar()
        {
        }

        /// <summary>
        /// create the colorbar
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="cv">ColorVal instance set to the appropriate color scale</param>
        /// <returns></returns>
        public static Bitmap RenderColorbar1(int width, int height, ColorVal cv, bool horiz)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //graphics.Clear(Color.Blue);

            int step;
			if(horiz) step = width;
			else step = height;

            for (int a = 0; a < step; a++) {
                double val = ((double)a)/((double)step);
				if(!horiz) val = 1.0-val;
				int x = a;
                int x2 = a+1;
                Color c = cv.GetColor(val);
                SolidBrush sb = new SolidBrush(c);
                Rectangle rect2;
				if(horiz) rect2 = new Rectangle(x,0,x2,height);
				else rect2 = new Rectangle(0, x, width, x2);

                graphics.FillRectangle(sb, rect2);

            }

            graphics.Dispose();
            graphics = null;

            return bitmap;
        }
		
		public static Bitmap RenderColorbar(int width, int height, ColorVal cv) {
			return RenderColorbar1(width,height,cv,false);
		}
		
		public static Bitmap RenderColorbarHorizontal(int width, int height, ColorVal cv) {
			return RenderColorbar1(width,height,cv,true);
		}

        public static Bitmap RenderColorBarWithTics(int width, int height, int textwidth, ColorVal cv, string[] tics) {
            Bitmap bmp1 = RenderColorbar1(width, height, cv, false);
            Bitmap bmp2 = new Bitmap(width + textwidth, height);
            Graphics g1 = Graphics.FromImage(bmp2);
            g1.DrawImage(bmp1, new Point(textwidth, 0));
            
            float hf = (float)height;
            float wf = (float)textwidth - 2.0F;
            float th = 24.0F; //!! TODO: ezt a betűméretből számolni !!
            StringAlignment alignright = StringAlignment.Far;
            StringAlignment aligntop;
            StringFormat format = new StringFormat();
            format.Alignment = alignright;
            SolidBrush brush = new SolidBrush(Color.Black);
            Font font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.SansSerif), 12.0F);
            for (int i = 0; i < tics.Length; i++) {
                float pos = 1.0F - ((float)i) / ((float)(tics.Length - 1));
                RectangleF rect;
                if (i == 0) {
                    rect = new RectangleF(0.0F, hf - th, wf, th);
                    aligntop = StringAlignment.Far;
                }
                else if (i == tics.Length - 1) {
                    rect = new RectangleF(0.0F, 0.0F, wf, th);
                    aligntop = StringAlignment.Near;
                }
                else {
                    rect = new RectangleF(0.0F, hf * pos - th / 2.0F, wf, th);
                    aligntop = StringAlignment.Center;
                }
                format.LineAlignment = aligntop;
                g1.DrawString(tics[i], font, brush, rect, format);
            }

            g1.Dispose();
            return bmp2;
        }

    }
}
