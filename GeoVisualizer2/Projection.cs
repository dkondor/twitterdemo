using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Elte.GeoVisualizer.Lib
{
    public abstract class Projection
    {
        /// <summary>
        /// some predefined projections -- currently only implemented by the Equirectangular projection class
        /// </summary>
        public enum ProjectionView {
            World = 0,
            USA = 1,
            EU = 2,
            USAWide = 4,
            Asia = 3
        }
        public static ProjectionView ViewFromString(string view1) {
            string view = view1.ToLowerInvariant();
            if (view == "world") return ProjectionView.World;
            if (view == "usa") return ProjectionView.USA;
            if (view == "us") return ProjectionView.USA;
            if (view == "usawide") return ProjectionView.USAWide;
            if (view == "uswide") return ProjectionView.USAWide;
            if (view == "asia") return ProjectionView.Asia;
            if (view == "eu") return ProjectionView.EU;
            if (view == "europe") return ProjectionView.EU;
            if (view == "europa") return ProjectionView.EU;
            throw new ArgumentException("Projection.ViewFromString: projection view " + view1 + "undefined!\n");
        }

        private MapPoint mapScale;
        private double mapYaw;
        private MapPoint mapOffset;
        private MapPoint mapMin;        // in transformed coordinates
        private MapPoint mapMax;        // in transformed coordinates

        private GeoPoint geoOrigin;
        private double geoYaw;
        private GeoPoint geoMin;
        private GeoPoint geoMax;

        private MapPoint mapX;
        private MapPoint mapY;

        private GeoPoint geoX;
        private GeoPoint geoY;
        private GeoPoint geoZ;

        public MapPoint MapOffset
        {
            get { return mapOffset; }
            set { mapOffset = value; }
        }

        public MapPoint MapScale
        {
            get { return mapScale; }
            set
            {
                mapScale = value;
                CalculateMapTransformation();
            }
        }

        public double MapYaw
        {
            get { return mapYaw; }
            set
            {
                mapYaw = value;
                CalculateMapTransformation();
            }
        }

        public MapPoint MapMin
        {
            get { return mapMin; }
            set { mapMin = value; }
        }

        public MapPoint MapMax
        {
            get { return mapMax; }
            set { mapMax = value; }
        }

        public GeoPoint GeoOrigin
        {
            get { return geoOrigin; }
            set
            {
                geoOrigin = value;
                CalculateGeoTransformation();
            }
        }

        public double GeoYaw
        {
            get { return geoYaw; }
            set
            {
                geoYaw = value;
                CalculateGeoTransformation();
            }
        }

        public GeoPoint GeoMin
        {
            get { return geoMin; }
            set { geoMin = value; }
        }

        public GeoPoint GeoMax
        {
            get { return geoMax; }
            set { geoMax = value; }
        }

        public Projection()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.mapScale = new MapPoint(10, 10);     // TODO
            this.mapYaw = 0;
            this.mapOffset = new MapPoint(0, 0);
            this.mapMin = new MapPoint(-1, -1);
            this.mapMax = new MapPoint(1, 1);

            CalculateMapTransformation();

            this.geoOrigin = new GeoPoint(0, 0);
            this.geoYaw = 0;
            this.geoMin = new GeoPoint(-45, 0);
            this.geoMax = new GeoPoint(45, 90);

            CalculateGeoTransformation();
        }

        private void CalculateMapTransformation()
        {
            var t = mapYaw * Constants.Degree2Radian;
            var cost = Math.Cos(t);
            var sint = Math.Sin(t);

            mapX = new MapPoint(mapScale.X * cost, mapScale.X * sint);
            mapY = new MapPoint(mapScale.Y * -sint, mapScale.Y * cost);
        }

        protected MapPoint Transform(MapPoint mp)
        {
            var tmp = new MapPoint(mp.Dot(mapX), mp.Dot(mapY));
            return tmp.Plus(mapOffset);
        }

        private void CalculateGeoTransformation()
        {
            double f, t, p;

            f = GeoYaw * Constants.Degree2Radian;
            t = GeoOrigin.Lat * Constants.Degree2Radian;
            p = -GeoOrigin.Lon * Constants.Degree2Radian;

            var sinf = Math.Sin(f);
            var cosf = Math.Cos(f);
            var sint = Math.Sin(t);
            var cost = Math.Cos(t);
            var sinp = Math.Sin(p);
            var cosp = Math.Cos(p);

            geoX = new GeoPoint(cost * cosp, -cost * sinp, sint);
            geoY = new GeoPoint(cosf * sinp + sinf * sint * cosp,
                                  cosf * cosp - sinf * sint * sinp,
                                  -sinf * cost);
            geoZ = new GeoPoint(sinf * sinp - cosf * sint * cosp,
                                  sinf * cosp + cosf * sint * sinp,
                                  cosf * cost);
        }

        protected GeoPoint Transform(GeoPoint gp)
        {
            return new GeoPoint(gp.Dot(geoX), gp.Dot(geoY), gp.Dot(geoZ));
        }

        public MapPoint Map(GeoPoint gp)
        {
            var tgp = Transform(gp);
            var mp = OnMap(tgp);
            var tmp = Transform(mp);

            return tmp;
        }

        public GeoPoint[] Interpolate(GeoPoint gstart, GeoPoint gend, int numpoints)
        {
            // Must be a power of two
            numpoints = (int)Math.Sqrt(numpoints - 1);
            numpoints *= numpoints;
            numpoints++;

            var gpoints = new GeoPoint[numpoints];

            gpoints[0] = gstart;
            gpoints[numpoints - 1] = gend;

            CalculateMidpoints(gpoints, 0, numpoints - 1);

            return gpoints;
        }

        private void CalculateMidpoints(GeoPoint[] points, int a, int b)
        {
            int m = (a + b) / 2;
            points[m] = CalculateMidpoint(points[a], points[b]);

            if (b - a > 2)
            {
                CalculateMidpoints(points, a, m);
                CalculateMidpoints(points, m, b);
            }
        }

        public GeoPoint CalculateMidpoint(GeoPoint a, GeoPoint b)
        {
            var nx = a.X + b.X;
            var ny = a.Y + b.Y;
            var nz = a.Z + b.Z;

            double l = Math.Sqrt(nx * nx + ny * ny + nz * nz);

            return new GeoPoint(nx / l, ny / l, nz / l);
        }

        public abstract MapPoint OnMap(GeoPoint p);

        //

        public void ZoomOut()
        {
            GeoOrigin = new GeoPoint(0, 0);
            GeoMin = new GeoPoint(-180, -90);
            GeoMax = new GeoPoint(180, 90);
        }

        public void ZoomTo(double minLon, double maxLon, double minLat, double maxLat)
        {
            GeoMin = new GeoPoint(minLon, minLat);
            GeoMax = new GeoPoint(maxLon, maxLat);

            LimitZoom();
        }

        public void ZoomTo(SqlGeography[] geos)
        {
            ResetZoom();

            for (int i = 0; i < geos.Length; i++)
            {
                ZoomToInternal(geos[i]);
            }

            LimitZoom();
        }

        public void ZoomTo(SqlGeography geo)
        {
            ResetZoom();

            ZoomToInternal(geo);

            LimitZoom();
        }

        public void RoundUpZoom(double precision)
        {
            var min = GeoMin;
            var max = GeoMax;

            min.Lon = Math.Floor(min.Lon / precision) * precision;
            min.Lat = Math.Floor(min.Lat / precision) * precision;
            max.Lon = Math.Ceiling(max.Lon / precision) * precision;
            max.Lat = Math.Ceiling(max.Lat / precision) * precision;

            GeoMin = min;
            GeoMax = max;
        }

        private void ZoomToInternal(SqlGeography geo)
        {
            var numgeo = geo.STNumGeometries().Value;

            if (numgeo == 0)
            {
                throw new ArgumentException("Geography contains no geometries.");
            }
            else if (numgeo > 1)
            {
                for (int i = 0; i < numgeo; i++)
                {
                    ZoomToInternal(geo.STGeometryN(i + 1));
                }
            }
            else
            {
                ZoomToSingle(geo);
            }
        }

        private void ZoomToSingle(SqlGeography geo)
        {
            var min = GeoMin;
            var max = GeoMax;
            var numpoint = geo.STNumPoints().Value;

            for (int i = 0; i < numpoint; i++)
            {
                var p = geo.STPointN(i + 1);
                var lon = p.Long.Value;
                var lat = p.Lat.Value;

                min.Lon = Math.Min(min.Lon, lon);
                min.Lat = Math.Min(min.Lat, lat);
                max.Lon = Math.Max(max.Lon, lon);
                max.Lat = Math.Max(max.Lat, lat);
            }
            GeoMin = min;
            GeoMax = max;
        }

        private void LimitZoom()
        {
            var min = GeoMin;
            var max = GeoMax;

            // TODO

            GeoMin = min;
            GeoMax = max;
        }

        private void ResetZoom()
        {
            GeoMin = new GeoPoint(double.MaxValue, double.MaxValue);
            GeoMax = new GeoPoint(double.MinValue, double.MinValue);
        }
    }
}
