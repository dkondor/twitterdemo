using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elte.GeoVisualizer.Lib.Projections
{
    public class Stereographic : Projection
    {

        public override MapPoint OnMap(GeoPoint gp)
        {
            return new MapPoint(gp.Y / (1 + gp.X), gp.Z / (1 + gp.X));
        }
    }
}
