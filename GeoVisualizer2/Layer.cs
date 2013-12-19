using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.SqlServer.Types;

namespace Elte.GeoVisualizer.Lib
{
    public abstract class Layer
    {
        private string text;
        private DataSource dataSource;
        
        private Bitmap bitmap;
        private BitmapData bitmapData;
        private byte[] buffer;

        private Graphics graphics;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public DataSource DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        public virtual IEnumerable<Layer> Layers
        {
            get { return null; }
        }

        protected Graphics Graphics
        {
            get { return graphics; }
        }

        public Layer()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
        }

        public virtual void OnBeginRender(RenderingContext context)
        {
            bitmap = new Bitmap(context.Width, context.Height, PixelFormat.Format32bppArgb);
            
            graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //graphics.Clear(Color.Blue);

            graphics.Transform = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, context.Height);

            //graphics.Clear(Color.FromArgb(0xFF, Color.White));
            
        }

        public abstract void OnRender(RenderingContext context, object[] values);

        public virtual Bitmap OnEndRender()
        {
            graphics.Dispose();
            graphics = null;

            return bitmap;
        }

        public void LockBits(out byte[] buffer, out int bytesPerPixel, out int stride)
        {
            bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int numbytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            this.buffer = new byte[numbytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, this.buffer, 0, numbytes);

            buffer = this.buffer;
            bytesPerPixel = 4;
            stride = bitmapData.Stride;
        }

        public void UnlockBits()
        {
            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);

            bitmap.UnlockBits(bitmapData);

            bitmapData = null;
            buffer = null;
        }

    }
}
