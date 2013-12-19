using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Elte.GeoVisualizer.Lib
{
    public class RenderingContext
    {
        public int Width;
        public int Height;
        public Projection Projection;
        public Random Random = new Random();
    }
}
