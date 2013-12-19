using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elte.GeoVisualizer.Lib
{
    public struct GeoPoint
    {
        private bool hasLonLat;
        private double lon;
        private double lat;

        private bool hasCartesian;
        private double x;
        private double y;
        private double z;

        public double Lon
        {
            get
            {
                if (!hasLonLat)
                {
                    Xyz2LonLat();
                }
                return lon;
            }
            set
            {
                lon = value;
                hasLonLat = true;
                hasCartesian = false;
            }
        }

        public double Lat
        {
            get
            {
                if (!hasLonLat)
                {
                    Xyz2LonLat();
                }
                return lat;
            }
            set
            {
                lat = value;
                hasLonLat = true;
                hasCartesian = false;
            }
        }

        public double X
        {
            get
            {
                if (!hasCartesian)
                {
                    LonLat2Xyz();
                }
                return x;
            }
        }

        public double Y
        {
            get
            {
                if (!hasCartesian)
                {
                    LonLat2Xyz();
                }
                return y;
            }
        }

        public double Z
        {
            get
            {
                if (!hasCartesian)
                {
                    LonLat2Xyz();
                }
                return z;
            }
        }

        /*
        public GeoPoint()
        {
            this.hasLonLat = false;
            this.lon = this.lat = 0;

            this.hasCartesian = false;
            this.x = this.y = this.z = 0;
        }*/

        public GeoPoint(double lon, double lat)
        {
            this.hasLonLat = true;
            this.lon = lon;
            this.lat = lat;

            this.hasCartesian = false;
            this.x = this.y = this.z = 0;
        }

        public GeoPoint(double x, double y, double z)
        {
            this.hasLonLat = false;
            this.lon = this.lat = 0;

            this.hasCartesian = true;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}", Lat, Lon); // TODO
        }

        private void Xyz2LonLat()
        {
            Xyz2LonLat(x, y, z, out lon, out lat);
        }

        public static void Xyz2LonLat(double x, double y, double z, out double lon, out double lat)
        {
            double epsilon = Constants.DoublePrecision2x;

            double rdec;
            if (z >= 1) rdec = Math.PI / 2;
            else if (z <= -1) rdec = -Math.PI / 2;
            else rdec = Math.Asin(z);
            lat = rdec * Constants.Radian2Degree;

            double cd = Math.Cos(rdec);
            if (cd > epsilon || cd < -epsilon)  // is the vector pointing to the poles?
            {
                if (y > epsilon || y < -epsilon)  // is the vector in the x-z plane?
                {
                    double arg = x / cd;
                    double acos;
                    if (arg <= -1)
                    {
                        acos = 180;
                    }
                    else if (arg >= 1)
                    {
                        acos = 0;
                    }
                    else
                    {
                        acos = Math.Acos(arg) * Constants.Radian2Degree;
                    }
                    if (y < 0.0)
                    {
                        lon = 360 - acos;
                    }
                    else
                    {
                        lon = acos;
                    }
                }
                else
                {
                    lon = (x < 0.0 ? 180.0 : 0.0);
                }
            }
            else
            {
                lon = 0.0;
            }

            if (lon > 180)
            {
                lon -= 360;
            }

            return;
        }

        private void LonLat2Xyz()
        {
            LonLat2Xyz(lon, lat, out x, out y, out z);
            hasCartesian = true;
        }

        public static void LonLat2Xyz(double lon, double lat, out double x, out double y, out double z)
        {
            double epsilon = Constants.DoublePrecision2x;

            double diff;
            double cd = Math.Cos(lat * Constants.Degree2Radian);

            diff = 90.0 - lat;
            // First, compute Z, consider cases, where declination is almost
            // +/- 90 degrees
            if (diff < epsilon && diff > -epsilon)
            {
                x = 0.0;
                y = 0.0;
                z = 1.0;
                return;
            }
            diff = -90.0 - lat;
            if (diff < epsilon && diff > -epsilon)
            {
                x = 0.0;
                y = 0.0;
                z = -1.0;
                return;
            }
            z = Math.Sin(lat * Constants.Degree2Radian);
            //
            // If we get here, then 
            // at least z is not singular
            //
            double quadrant;
            double qint;
            int iint;
            quadrant = lon / 90.0; // how close is it to an integer?
            // if quadrant is (almost) an integer, force x, y to particular
            // values of quad:
            // quad,   (x,y)
            // 0       (1,0)
            // 1,      (0,1)
            // 2,      (-1,0)
            // 3,      (0,-1)
            // q>3, make q = q mod 4, and reduce to above
            // q mod 4 should be 0.
            qint = (double)((int)quadrant);
            if (Math.Abs(qint - quadrant) < epsilon)
            {
                iint = (int)qint;
                iint %= 4;
                if (iint < 0) iint += 4;
                switch (iint)
                {
                    case 0:
                        x = cd;
                        y = 0.0;
                        break;
                    case 1:
                        x = 0.0;
                        y = cd;
                        break;
                    case 2:
                        x = -cd;
                        y = 0.0;
                        break;
                    case 3:
                    default:
                        x = 0.0;
                        y = -cd;
                        break;
                }
            }
            else
            {
                x = Math.Cos(lon * Constants.Degree2Radian) * cd;
                y = Math.Sin(lon * Constants.Degree2Radian) * cd;
            }
            return;
        }

        public double Dot(GeoPoint gp)
        {
            return X * gp.X + Y * gp.Y + Z * gp.Z;
        }
    }
}
