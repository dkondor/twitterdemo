using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.SqlServer.Types;

namespace Elte.GeoVisualizer.Lib.Layers
{

    public class Geography : Layer
    {
        /// <summary>
        /// static color to draw all regions with
        /// </summary>
        public Color StaticColor;

        /// <summary>
        /// ColorVal to generate colors to the values associated with each region
        /// Set to an appropriate ColorVal instance if needed
        /// </summary>
        public ColorVal cv;

        /// <summary>
        /// pen used for drawing lines
        /// </summary>
        public Pen pen;

        /// <summary>
        /// default constructor: Beige color for all regions, LightGray pen
        /// </summary>
        public Geography()
        {
            StaticColor = Color.Beige;
            cv = null;
            pen = Pens.LightGray;
        }

        /// <summary>
        /// Also set the color scale
        /// </summary>
        /// <param name="cv"></param>
        public Geography(ColorVal cv1) {
            cv = cv1;
            StaticColor = Color.Beige;
            pen = Pens.LightGray;
        }

        public override void OnRender(RenderingContext context, object[] values)
        {
            
            if(values.Length > 1)
                RenderGeography(context, (SqlGeography)values[0], values[1]);
            else
                RenderGeography(context, (SqlGeography)values[0], (double)1.0);
        }

        protected void RenderGeography(RenderingContext context, SqlGeography geo, object val)
        {
            var numgeo = geo.STNumGeometries().Value;

            if (numgeo > 1)
            {
                for (int i = 0; i < numgeo; i++)
                {
                    RenderGeography(context, geo.STGeometryN(i + 1), val);  // indexed from 1!
                }
            }
            else
            {
                var type = geo.STGeometryType().Value;

                switch (type)
                {

                    case "Polygon":
                        RenderPolygon(context, geo, val);
                        break;
                    case "LineString":
                        RenderPolyline(context, geo);
                        break;
                    case "Point":
                        RenderPoint(context, geo);
                        break;
                    case "CircularString":
                    case "CompoundCurve":
                    case "CurvePolygon":
                    case "GeometryCollection":
                    case "MultiPoint":
                    case "MultiLineString":
                    case "MultiPolygon":
                    default:
                        //throw new NotImplementedException();    // TODO
                        break;
                }
            }
        }

        private void RenderPolyline(RenderingContext context, SqlGeography geo)
        {
            foreach (var pp in MapPoints(context, geo))
            {
                Graphics.DrawLines(pen, pp);
            }
        }

        private void RenderPolygon(RenderingContext context, SqlGeography geo, object val)
        {
            foreach (var pp in MapPoints(context, geo))
            {
                Color c;
                if (val is Color) c = (Color)val;
                else {
                    if (cv != null && (val is Double)) {
                        double val2 = (Double)val;
                        if (val2 >= 0.0) c = cv.GetColor(val2);
                        else c = StaticColor;
                    }
                    else c = StaticColor;
                }
                
                SolidBrush sb = new SolidBrush(c);
                Graphics.FillPolygon(sb, pp);

                Graphics.DrawPolygon(pen, pp);
            }
        }

        private void RenderPoint(RenderingContext context, SqlGeography geo)
        {
            var gp = new GeoPoint(geo.Long.Value, geo.Lat.Value);
            var mp = context.Projection.Map(gp);

            Graphics.DrawLine(pen, (int)mp.X, (int)mp.Y, (int)mp.X + 1, (int)mp.Y + 1);
        }

        private Point[][] MapPoints(RenderingContext context, SqlGeography geo)
        {
            var sink = new MapGeographySink(context.Projection);
            geo.Populate(sink);

            var res = new Point[sink.Shapes.Count][];

            for (int i = 0; i < sink.Shapes.Count; i++)
            {
                var pp = sink.Shapes[i];
                res[i] = new Point[pp.Length];
                for (int j = 0; j < pp.Length; j++)
                {
                    res[i][j] = new Point((int)pp[j].X, (int)pp[j].Y);
                }
            }

            return res;
        }
    }
}
