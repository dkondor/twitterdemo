using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Elte.GeoVisualizer.Lib.Layers
{
    public class Background : Layer
    {
        private Color color;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public override void OnBeginRender(RenderingContext context)
        {
            base.OnBeginRender(context);

            Graphics.Clear(color);
        }

        public override void OnRender(RenderingContext context, object[] values)
        {
            
        }
    }
}
