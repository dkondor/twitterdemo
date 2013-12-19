using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elte.GeoVisualizer.Lib.Projections
{
    public class Equirectangular : Projection
    {
        protected double w;
        protected double h;

        public Equirectangular() {
            w = -1.0;
            h = -1.0;
        }

        /// <summary>
        /// Initialize an equirectangular projection based on one of the predefined projections
        /// </summary>
        /// <param name="v">projection to use</param>
        /// <param name="scale">scaling (pixels per degrees)</param>
        public Equirectangular(ProjectionView v, double scale) {
            InitializeMembers(v, scale);
        }

        public Equirectangular(ProjectionView v, int width, int height) {
            double w1, h1;
            GetBaseSize(v, out w1, out h1);
            double wscale = ((double)width) / w1;
            double hscale = ((double)height) / h1;
            double scale = Math.Min(wscale, hscale);
            InitializeMembers(v, scale);
        }

        private void InitializeMembers(ProjectionView v, double scale) {
            this.MapScale = new MapPoint(scale, scale);
            switch (v) {
                case ProjectionView.World:
                    this.GeoOrigin = new GeoPoint(0, 0);
                    w = scale * 360.0;
                    h = scale * 180.0;
                    this.MapOffset = new MapPoint(w / 2.0, h / 2.0);
                    break;
                case ProjectionView.USA:
                    this.GeoOrigin = new GeoPoint(0.0, 0.0);
                    w = scale * 150.0;
                    h = scale * 87.5;
                    this.MapOffset = new MapPoint(150.0 * scale, 12.5 * scale);
                    break;
                case ProjectionView.EU:
                    this.GeoOrigin = new GeoPoint(14.0, 53.0);
                    w = scale * 38.0;
                    h = scale * 38.0;
                    this.MapOffset = new MapPoint(20.0 * scale, 19.0 * scale);
                    break;
                case ProjectionView.USAWide:
                    this.GeoOrigin = new GeoPoint(-100.0, 40.0);
                    w = scale * 60.0;
                    h = scale * 35.0;
                    this.MapOffset = new MapPoint(30.0 * scale, 20.0 * scale);
                    break;
                case ProjectionView.Asia:
                    this.GeoOrigin = new GeoPoint(0.0, 0.0);
                    w = scale * 137.5;
                    h = scale * 100.0;
                    this.MapOffset = new MapPoint(-42.5 * scale, 50.0 * scale);
                    break;
                default:
                    throw new NotImplementedException("Projection.Equirectangular(): unknown view to set!\n");
            }
        }

        private static void GetBaseSize(ProjectionView v, out double w1, out double h1) {
            switch (v) {
                case ProjectionView.World:
                    w1 = 360.0;
                    h1 = 180.0;
                    break;
                case ProjectionView.USA:
                    w1 = 150.0;
                    h1 = 87.5;
                    break;
                case ProjectionView.EU:
                    w1 = 38.0;
                    h1 = 38.0;
                    break;
                case ProjectionView.USAWide:
                    w1 = 60.0;
                    h1 = 35.0;
                    break;
                case ProjectionView.Asia:
                    w1 = 137.5;
                    h1 = 100.0;
                    break;
                default:
                    throw new NotImplementedException("Projection.Equirectangular(): unknown view to set!\n");
            }
        }

        /// <summary>
        /// Get the size of the generated image based on the current settings
        /// (if the projection was generated using one of the predefined views)
        /// </summary>
        /// <param name="w">width (or -1 if not set)</param>
        /// <param name="h">height (or -1 if not set)</param>
        public void GetImageSize(out int w1, out int h1) {
            w1 = (int)w;
            h1 = (int)h;
        }

        public override MapPoint OnMap(GeoPoint gp)
        {
            return new MapPoint(gp.Lon, gp.Lat);
        }
    }
}
