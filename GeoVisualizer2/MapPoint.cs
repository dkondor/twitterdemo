using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elte.GeoVisualizer.Lib
{
    public struct MapPoint
    {
        public double X;
        public double Y;

        public MapPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public MapPoint Plus(MapPoint mp)
        {
            return new MapPoint(X + mp.X, Y + mp.Y);
        }

        public MapPoint Minus(MapPoint mp)
        {
            return new MapPoint(X - mp.X, Y - mp.Y);
        }

        public double Dot(MapPoint mp)
        {
            return X * mp.X + Y * mp.Y;
        }

        public override string ToString()
        {
            // TODO
            return String.Format("{0} {1}", X, Y);
        }
    }
}
