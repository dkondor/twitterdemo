using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.SqlServer.Types;


namespace Elte.GeoVisualizer.Lib
{
    class MapGeographySink : IGeographySink
    {
        private Projection projection;
        private List<MapPoint[]> shapes;
        private List<GeoPoint> gbuffer;
        private List<MapPoint> mbuffer;

        public List<MapPoint[]> Shapes
        {
            get { return shapes; }
        }

        public MapGeographySink(Projection projection)
        {
            this.projection = projection;
        }

        public void BeginGeography(OpenGisGeographyType type)
        {
            shapes = new List<MapPoint[]>();
        }

        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            gbuffer = new List<GeoPoint>();
            mbuffer = new List<MapPoint>();

            AddPoint(latitude, longitude, z, m);
        }

        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            var gp = new GeoPoint(longitude, latitude);
            var mp = projection.Map(gp);

            /*
            var gstart = gbuffer[gbuffer.Count - 1];
            var mstart = mbuffer[mbuffer.Count - 1];
            var delta = mstart.Minus(mp);

            double tolerance = 5;

            if (Math.Abs(delta.X) > tolerance || Math.Abs(delta.Y) > tolerance) // TODO: add tolerance to projection class
            {
                var numpoints = (int)(Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) / tolerance);
                var gpoints = projection.Interpolate(gstart, gp, numpoints);

                for (int i = 1; i < gpoints.Length - 1; i++)
                {
                    gbuffer.Add(gpoints[i]);
                    mbuffer.Add(projection.Map(gpoints[i]));
                }
            }*/

            gbuffer.Add(gp);
            mbuffer.Add(mp);
        }

        private void AddPoint(double latitude, double longitude, double? z, double? m)
        {
            var gp = new GeoPoint(longitude, latitude);
            var mp = projection.Map(gp);

            gbuffer.Add(gp);
            mbuffer.Add(mp);
        }

        public void EndFigure()
        {
            shapes.Add(mbuffer.ToArray());
        }

        public void EndGeography()
        {
            //
        }

        public void SetSrid(int srid)
        {
            //throw new NotImplementedException();
        }
    }
}
