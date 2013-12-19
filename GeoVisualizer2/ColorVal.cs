using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;

/*
 * TODO:
 * új színskálák (RGB interpolálás)
 * minden színskálához egy "semleges" szín meghatározása (a mention map esetén)
 * inverted opció
 * esetleg: letisztázás, nem használt opciók törlése
 */

namespace Elte.GeoVisualizer.Lib {
    /// <summary>
    /// Class for converting values to colors for visualization
    /// </summary>
    /// <example>
    /// Color c = ColorVal.ColorRed(0.45);
    /// </example>
    public sealed class ColorVal {
        /// <summary>
        /// pre-defined color scales
        /// </summary>
        public enum ColorMaps {
            /// <summary>
            /// greyscale
            /// </summary>
            Grey,
            /// <summary>
            /// the gnuplot default colormap (static RGB map with 100 distinct colors)
            /// </summary>
            BlueRed,
            /// <summary>
            /// colormap based on the gnuplot scale, but with constant saturation and value, the hue changes linearly
            /// </summary>
            BlueRedS,
            /// <summary>
            /// constant-hue red colormap
            /// </summary>
            Red,
            /// <summary>
            /// constant-hue orange colormap
            /// </summary>
            Orange,
            /// <summary>
            /// constant-hue yellow colormap
            /// </summary>
            Yellow,
            /// <summary>
            /// constant-hue green colormap
            /// </summary>
            Green,
            /// <summary>
            /// constant-hue blue colormap
            /// </summary>
            Blue,
            /// <summary>
            /// constant-hue purple colormap
            /// </summary>
            Purple,
            /// <summary>
            /// constant-hue pink colormap
            /// </summary>
            Pink,
            /// <summary>
            /// constant-saturation, blue-green-yellow-red
            /// </summary>
            ThermalInv,
            /// <summary>
            /// constant-saturation, red-yellow-green-blue
            /// </summary>
            Thermal,
            /// <summary>
            /// fixed blue-to-red palette
            /// </summary>
            Palette,
            /// <summary>
            /// Red-blue-cyan, interpolated in RGB space.
            /// </summary>
            RedBlue,
            /// <summary>
            /// Red-blue-cyan-transparent, RGB interpolation
            /// </summary>
            RedBlueAlpha,
            /// <summary>
            /// Blue-to-red, to be used for the sentiment maps
            /// </summary>
            BlueRed3,
            /// <summary>
            /// YlOrBr (sequential, for the mention maps)
            /// </summary>
            YlOrBr,
            /// <summary>
            /// YGOrBu (sequential, for the mention maps)
            /// </summary>
            YGOrBu,
            /// <summary>
            /// YlGr (sequential, for the mention maps)
            /// </summary>
            YlGr,
            /// <summary>
            /// BrPG (diverging, for time coloring or sentiment)
            /// </summary>
            BrBG,
            /// <summary>
            /// PiYG (diverging, for time coloring or sentiment)
            /// </summary>
            PiYG,
            /// <summary>
            /// RdYlGn (diverging, for time coloring or sentiment)
            /// </summary>
            RdYlGn,
            /// <summary>
            /// Spectral (diverging, for time coloring or sentiment)
            /// </summary>
            Spectral,
            /// <summary>
            /// Yellow with transparency (for the text search)
            /// </summary>
            YellowT,

            YlOrBrT,

            RedT,

            RedYellowT
        };

        /// <summary>
        /// Get a ColorMap parameter from a user-supplied string
        /// Deprecated, use GetColorMapStrings to present a list to the user
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static ColorMaps ColorMapsFromString(string map) {
            if (map == "Grey") return ColorMaps.Grey;
            if (map == "BlueRed" || map == "Gnuplot") return ColorMaps.BlueRed;
            if (map == "BlueRedS") return ColorMaps.BlueRedS;
            if (map == "Red") return ColorMaps.Red;
            if (map == "Orange") return ColorMaps.Orange;
            if (map == "Yellow") return ColorMaps.Yellow;
            if (map == "Green") return ColorMaps.Green;
            if (map == "Blue") return ColorMaps.Blue;
            if (map == "Purple") return ColorMaps.Purple;
            if (map == "Pink") return ColorMaps.Pink;
            if (map == "ThermalInv") return ColorMaps.ThermalInv;
            if (map == "Thermal") return ColorMaps.Thermal;
            if (map == "Palette") return ColorMaps.Palette;
            if (map == "RedBlue") return ColorMaps.RedBlue;
            throw new ArgumentException("ColorVal.ColorMapsFromString: unknown colormap type!\n");
        }

        /// <summary>
        /// Get a list of (some) available, predefined colormaps to be presented to the user
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ColorMaps> GetColorMapStrings() {
            Dictionary<string, ColorMaps> dict = new Dictionary<string, ColorMaps>();
            //these are for the mention map
            dict.Add("Greyscale", ColorMaps.Grey);
            dict.Add("Blue", ColorMaps.Blue);
            dict.Add("Red", ColorMaps.Red);
            dict.Add("Purple", ColorMaps.Purple);
            dict.Add("YlOrBr", ColorMaps.YlOrBr);
            dict.Add("YGOrBu", ColorMaps.YGOrBu);
            dict.Add("YlGr", ColorMaps.YlGr);

            //this is for the text/hashtag histogram, for displaying intensity
            dict.Add("Red to blue with transparency", ColorMaps.RedBlueAlpha);
            dict.Add("Yellow with transparency", ColorMaps.YellowT);

            //??
            dict.Add("Red to blue", ColorMaps.RedBlue);
            dict.Add("Blue to red", ColorMaps.BlueRed);
            dict.Add("Thermal", ColorMaps.Thermal);
            dict.Add("Thermal (inverted)", ColorMaps.ThermalInv);
            dict.Add("Blue to red 2", ColorMaps.Palette);
            dict.Add("Blue to red 3", ColorMaps.BlueRed3);

            //diverging colorscales for the histogram with time / sentiment coloring
            dict.Add("BrBG", ColorMaps.BrBG);
            dict.Add("PiYG", ColorMaps.PiYG);
            dict.Add("RdYlGn", ColorMaps.RdYlGn);
            dict.Add("Spectral", ColorMaps.Spectral);

            dict.Add("RedYellowT", ColorMaps.RedYellowT);

            return dict;
        }


        public static Dictionary<string, ColorMaps> GetColorMapsMention() {
            Dictionary<string, ColorMaps> dict = new Dictionary<string, ColorMaps>();
            //these are for the mention map
            dict.Add("Blue", ColorMaps.Blue);
            dict.Add("Red", ColorMaps.Red);
            dict.Add("Purple", ColorMaps.Purple);
            dict.Add("Greyscale", ColorMaps.Grey);
            dict.Add("YlOrBr", ColorMaps.YlOrBr);
            dict.Add("YGOrBu", ColorMaps.YGOrBu);
            dict.Add("YlGr", ColorMaps.YlGr);
            dict.Add("Blue to red", ColorMaps.BlueRed3);

            return dict;
        }

        public static Dictionary<string, ColorMaps> GetColorMapsIntensity() {
            Dictionary<string, ColorMaps> dict = new Dictionary<string, ColorMaps>();
            //these are for the mention map
            //this is for the text/hashtag histogram, for displaying intensity
            dict.Add("Red to blue", ColorMaps.RedBlueAlpha);
            dict.Add("Yellow", ColorMaps.YellowT);
            dict.Add("YlOrBr", ColorMaps.YlOrBrT);
            dict.Add("Red", ColorMaps.RedT);
            return dict;
        }

        public static Dictionary<string, ColorMaps> GetColorMapsTime() {
            Dictionary<string, ColorMaps> dict = new Dictionary<string, ColorMaps>();
            //??
            dict.Add("Red to blue", ColorMaps.RedBlue);
            dict.Add("Blue to red", ColorMaps.BlueRed);
            dict.Add("Thermal", ColorMaps.Thermal);
            dict.Add("Thermal (inverted)", ColorMaps.ThermalInv);
            dict.Add("Blue to red 2", ColorMaps.Palette);
            dict.Add("Blue to red 3", ColorMaps.BlueRed3);

            //diverging colorscales for the histogram with time / sentiment coloring
            dict.Add("BrBG", ColorMaps.BrBG);
            dict.Add("PiYG", ColorMaps.PiYG);
            dict.Add("RdYlGn", ColorMaps.RdYlGn);
            dict.Add("Spectral", ColorMaps.Spectral);

            return dict;
        }

        public static Dictionary<string, ColorMaps> GetColorMapsSentiment() {
            Dictionary<string, ColorMaps> dict = new Dictionary<string, ColorMaps>();

            dict.Add("Spectral", ColorMaps.Spectral);
            dict.Add("Thermal", ColorMaps.Thermal);
            dict.Add("Blue to red", ColorMaps.BlueRed3);

            //diverging colorscales for the histogram with time / sentiment coloring
            dict.Add("BrBG", ColorMaps.BrBG);
            dict.Add("PiYG", ColorMaps.PiYG);
            dict.Add("RdYlGn", ColorMaps.RdYlGn);

            return dict;
        }


        private int hue;
        /// <summary>
        /// Can be used to set the hue manually in the case of the constant-hue colormaps
        /// </summary>
        public int Hue {
            get { return hue; }
            set {
                if (!(type == ColorMapType.ConstHueLight || type == ColorMapType.ConstHue))
                    throw new Exception("ColorVal.Hue: CMType must be set to one of the constant-hue types to use this option!\n");
                hue = value;
            }
        }

        /// <summary>
        /// use 1.0-val instead of val
        /// </summary>
        public bool Inverted;

        /// <summary>
        /// the color generation methods
        /// </summary>
        public enum ColorMapType {
            /// <summary>
            /// greyscale, simply use the supplied value (without gamma correction)
            /// </summary>
            Grey,
            /// <summary>
            /// the default colormap from gnuplot (calculated by given formulas for R,G and B components)
            /// </summary>
            BlueRed,
            /// <summary>
            /// colormaps with constant hue (can be set with the Hue field, or use some of the predefined values of ColorMaps)
            /// </summary>
            ConstHue,
            /// <summary>
            /// colormaps with constant hue, goes from dark-to-light
            /// </summary>
            ConstHueLight,
            /// <summary>
            /// colormaps with constant saturation and value, the hue changes
            /// </summary>
            ConstSat,
            /// <summary>
            /// Colormap calculated by interpolating in RGB space
            /// </summary>
            RGBInt,
            /// <summary>
            /// choose colors from a fixed palette
            /// </summary>
            Palette
        };

        /// <summary>
        /// methods / formulae to generate hue values from the specified value in the 
        /// case of the colormaps with constant saturation
        /// </summary>
        public enum ConstSatType {
            /// <summary>
            /// linear interpolation between two endpoints, the default is 240 and 420 (blu--red--yellow)
            /// </summary>
            Linear,
            /// <summary>
            /// linear + (sin(pi*val)^2)/8 -- less pink, more red
            /// </summary>
            SineRed,
            /// <summary>
            /// red-yellow-green-blue scale
            /// </summary>
            Thermal,
            /// <summary>
            /// blue-green-yellow-red
            /// </summary>
            ThermalInv
        };

        private ColorMapType type;

        public ColorMapType CMType {
            get { return type; }
            set {
                type = value;
                if (type == ColorMapType.ConstHue || type == ColorMapType.ConstHueLight)
                    hue = 0; //default: red
            }
        }

        private ConstSatType stype;

        public ConstSatType CSType {
            get { return stype; }
            set {
                if (type != ColorMapType.ConstSat)
                    throw new Exception("ColorVal.CSType: CMType must be set to ColorMapType.ConstSat to use this option!\n");
                stype = value;
            }
        }

        private RGBIntVal rgbval;

        public RGBIntVal RGBVal {
            get { return rgbval; }
            set { rgbval = value; }
        }

        /// <summary>
        /// If set, use a dark-to-light colorscale.
        /// Can be used only when generating constant-hue colormaps.
        /// </summary>
        public bool Light {
            get {
                if (type == ColorMapType.ConstHueLight) return true;
                else return false;
            }
            set {
                if (!(type == ColorMapType.ConstHueLight || type == ColorMapType.ConstHue))
                    throw new Exception("ColorVal.Light: CMType must be set to one of the constant-hue types to use this option!\n");
                if (value) type = ColorMapType.ConstHueLight;
                else type = ColorMapType.ConstHue;
            }
        }

        /// <summary>
        /// Create a color generator with the default colormap (red)
        /// </summary>
        public ColorVal() {
            hue = 0;
            type = ColorMapType.ConstHue;
        }

        /// <summary>
        /// Create a color generator with one of the predefined scales
        /// </summary>
        /// <param name="cm"></param>
        public ColorVal(ColorMaps cm) {
            type = ColorMapType.ConstHue;
            switch (cm) {
                case ColorMaps.Red:
                    hue = 0;
                    break;
                case ColorMaps.Yellow:
                    hue = 60;
                    break;
                case ColorMaps.Orange:
                    hue = 30;
                    break;
                case ColorMaps.Green:
                    hue = 120;
                    break;
                case ColorMaps.Blue:
                    hue = 240;
                    break;
                case ColorMaps.Purple:
                    hue = 260;
                    break;
                case ColorMaps.Pink:
                    hue = 300;
                    break;
                case ColorMaps.Grey:
                    type = ColorMapType.Grey;
                    break;
                case ColorMaps.BlueRed:
                    type = ColorMapType.BlueRed;
                    break;
                case ColorMaps.BlueRedS:
                    type = ColorMapType.ConstSat;
                    stype = ConstSatType.SineRed;
                    break;
                case ColorMaps.ThermalInv:
                    type = ColorMapType.ConstSat;
                    stype = ConstSatType.ThermalInv;
                    break;
                case ColorMaps.Thermal:
                    type = ColorMapType.ConstSat;
                    stype = ConstSatType.Thermal;
                    break;
                case ColorMaps.Palette:
                    type = ColorMapType.Palette;
                    break;
                case ColorMaps.RedBlue:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.RedBlue;
                    break;
                case ColorMaps.RedBlueAlpha:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.RedBlueAlpha;
                    break;
                case ColorMaps.BlueRed3:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.BlueRed;
                    break;
                case ColorMaps.YlOrBr:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.YlOrBr;
                    break;
                case ColorMaps.YGOrBu:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.YGOrBu;
                    break;
                case ColorMaps.YlGr:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.YlGr;
                    break;
                case ColorMaps.BrBG:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.BrBG;
                    break;
                case ColorMaps.PiYG:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.PiYG;
                    break;
                case ColorMaps.RdYlGn:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.RdYlGn;
                    break;
                case ColorMaps.Spectral:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.Spectral;
                    break;
                case ColorMaps.YellowT:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.YellowT;
                    break;
                case ColorMaps.YlOrBrT:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.YlOrBrT;
                    break;
                case ColorMaps.RedT:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.RedT;
                    break;
                case ColorMaps.RedYellowT:
                    type = ColorMapType.RGBInt;
                    rgbval = RGBIntVal.RedYellowT;
                    break;
                default:
                    throw new NotImplementedException("ColorVal(): specified colorscale is not implemented!\n");
            }
        }

        /// <summary>
        /// Create a color generator with the given hue
        /// </summary>
        /// <param name="hue1"></param>
        public ColorVal(int hue1) {
            type = ColorMapType.ConstHue;
            hue = hue1;
        }

        /// <summary>
        /// Generate color based on the previously set color scale
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0]</param>
        /// <returns></returns>
        public Color GetColor(double val) {
            if (Inverted) val = 1.0 - val;
            switch (type) {
                case ColorMapType.ConstHue:
                    return ColorHue(val, hue);
                case ColorMapType.ConstHueLight:
                    return ColorHueLight(val, hue);
                case ColorMapType.ConstSat:
                    return ColorConstSat(val, stype);
                case ColorMapType.Grey:
                    return ColorGrey(val);
                case ColorMapType.BlueRed:
                    return ColorBlueRed(val);
                case ColorMapType.Palette:
                    return ColorPalette(val);
                case ColorMapType.RGBInt:
                    return ColorRGBI(val, rgbval);
                default:
                    throw new NotImplementedException("ColorVal.GetColor: unknown ColorMapType!\n");
            }
        }

        public void GetColorHSV(double val, out double h, out double s, out double v) {
            if (Inverted) val = 1.0 - val;
            switch (type) {
                case ColorMapType.BlueRed:
                    rgbtohsv(ColorBlueRed(val), out h, out s, out v);
                    return;
                case ColorMapType.ConstSat:
                    ColorConstSatHSV(val, stype, out h, out s, out v);
                    return;
                case ColorMapType.Grey:
                    h = 0.0;
                    s = 0.0;
                    v = val;
                    if (v > 1.0) v = 1.0;
                    if (v < 0.0) v = 0.0;
                    return;
                case ColorMapType.ConstHue:
                    ColorHueHSV(val, hue, out h, out s, out v);
                    return;
                case ColorMapType.ConstHueLight:
                    ColorHueLightHSV(val, hue, out h, out s, out v);
                    return;
                case ColorMapType.Palette:
                    Color c = ColorPalette(val);
                    rgbtohsv(c, out h, out s, out v);
                    return;
                case ColorMapType.RGBInt:
                    ColorRGBIHSV(val, rgbval, out h, out s, out v);
                    return;
                default:
                    throw new NotImplementedException("ColorVal.GetColorHSV: unknown ColorMapType!\n");
            }
        }

        /// <summary>
        /// generate color from a fixed blue-green-red palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorPalette(double val) {
            if (val >= 1.0) val = 0.99;
            if (val < 0.0) val = 0.0;
            short v2 = (short)Math.Floor(100.0 * val);
            return Color.FromArgb(colormap2[3 * v2], colormap2[3 * v2 + 1], colormap2[3 * v2 + 2]);
        }

        /// <summary>
        /// Generate color using the default palette formulas taken from the Gnuplot program
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorBlueRed(double val) {
            if (val > 1.0) val = 1.0;
            if (val < 0.0) val = 0.0;

            double dr = Math.Sqrt(val);
            double dg = val * val * val;
            double db = Math.Sin(360 * val * Math.PI / 180.0);
            if (db < 0.0) db = 0.0;

            ushort ur = (ushort)(Math.Floor(dr * 256.0));
            ushort ug = (ushort)(Math.Floor(dg * 256.0));
            ushort ub = (ushort)(Math.Floor(db * 256.0));
            if (ur > 255) ur = 255;
            if (ug > 255) ug = 255;
            if (ub > 255) ub = 255;

            return Color.FromArgb(ur, ug, ub);
        }

        /// <summary>
        /// Generate color from a simple scale with constant (maximum) saturation and value
        /// the colors are similar to the gnuplot scale
        /// return the HSV values without converting them
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        public static void ColorConstSatHSV(double val, out double h, out double s, out double v) {
            if (val > 1.0) val = 1.0;
            if (val < 0.0) val = 0.0;
            v = 1.0;
            s = 1.0;
            h = 240.0 + val * 180.0;
        }

        /// <summary>
        /// Generate color from a scale with constant (maximum) saturation and value
        /// the colors are chosen according to the st parameter
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="st">formula to use</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        public static void ColorConstSatHSV(double val, ConstSatType st, out double h, out double s, out double v) {
            if (st == ConstSatType.Linear) {
                ColorConstSatHSV(val, out h, out s, out v);
                return;
            }
            if (val > 1.0) val = 1.0;
            if (val < 0.0) val = 0.0;
            v = 1.0;
            s = 1.0;
            switch (st) {
                case ConstSatType.SineRed: {
                        double x = val;
                        double y = Math.Sin(Math.PI * x);
                        x += y * y / 8.0;
                        h = 240.0 + x * 180.0;
                    }
                    return;
                case ConstSatType.Thermal: {
                        double x = val;
                        double y = Math.Sin(2.0 * Math.PI * x);
                        x -= 0.15 * y;
                        h = 240.0 * x;
                    }
                    return;
                case ConstSatType.ThermalInv: {
                        double x = 1.0 - val;
                        double y = Math.Sin(2.0 * Math.PI * x);
                        x -= 0.08 * y;
                        h = 240.0 * x;
                    }
                    return;
                default:
                    throw new NotImplementedException("ColorConstSatHSV: unknown ConstSatType!\n");
            }
        }

        /// <summary>
        /// Generate a color from a simple scale with constant (maximum) saturation and value
        /// the colors are similar to the gnuplot scale
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="st">formula to use</param>
        /// <returns></returns>
        public static Color ColorConstSat(double val) {
            double h, s, v;
            ColorConstSatHSV(val, out h, out s, out v);
            return hsvtorgb(h, s, v);
        }

        /// <summary>
        /// Generate a color from a simple scale with constant (maximum) saturation and value
        /// the colors are similar to the gnuplot scale
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorConstSat(double val, ConstSatType st) {
            double h, s, v;
            ColorConstSatHSV(val, st, out h, out s, out v);
            return hsvtorgb(h, s, v);
        }

        /// <summary>
        /// Generate HSV values one of the RGB interpolators
        /// FIXME: does not return the alpha value
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="rgbf">formula to use</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        /// <exception cref="NotImplementedException">
        /// Is thrown when a requested operation is not implemented for a given type.
        /// </exception>
        public static void ColorRGBIHSV(double val, RGBIntVal rgbf, out double h, out double s, out double v) {
            Color c = ColorRGBI(val, rgbf);
            rgbtohsv(c, out h, out s, out v);
        }

        /// <summary>
        /// Generate color using interpolation in RGB space
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0]</param>
        /// <param name="hsvf1">formula to use</param>
        /// <returns></returns>
        public static Color ColorRGBI(double val, RGBIntVal rgbi) {
            int which = (int)rgbi;
            if (which >= rgbintnum)
                throw new NotImplementedException("ColorRGBI: unknown interpolation!\n");
            return ColorRGBI1(val, which);
        }


        /// <summary>
        /// RGB Interpolators
        /// </summary>
        public enum RGBIntVal {
            /// <summary>
            /// Red-blue-cyan map, possibly like this: http://users.humboldt.edu/mstephens/hate/hate_map.html
            /// </summary>
            RedBlue = 0,
            /// <summary>
            /// Red-blue-cyan map, possibly like this: http://users.humboldt.edu/mstephens/hate/hate_map.html,
            /// with alpha component
            /// </summary>
            RedBlueAlpha = 1,
            /// <summary>
            /// Blue-to-red, to be used for the sentiment maps
            /// Diverging Color Maps for Scientific Visualization
            /// (Expanded)
            /// Kenneth Moreland
            /// Sandia National Laboratories
            /// </summary>
            BlueRed = 2,
            /// <summary>
            /// YlOrBr (sequential, for the mention maps)
            /// </summary>
            YlOrBr = 3,
            /// <summary>
            /// YGOrBu (sequential, for the mention maps)
            /// </summary>
            YGOrBu = 4,
            /// <summary>
            /// YlGr (sequential, for the mention maps)
            /// </summary>
            YlGr = 5,
            /// <summary>
            /// BrPG (diverging, for time coloring or sentiment)
            /// </summary>
            BrBG = 6,
            /// <summary>
            /// PiYG (diverging, for time coloring or sentiment)
            /// </summary>
            PiYG = 7,
            /// <summary>
            /// RdYlGn (diverging, for time coloring or sentiment)
            /// </summary>
            RdYlGn = 8,
            /// <summary>
            /// Spectral (diverging, for time coloring or sentiment)
            /// </summary>
            Spectral = 9,
            /// <summary>
            /// Yellow with transparency (for the text search)
            /// </summary>
            YellowT = 10,

            YlOrBrT = 11,

            RedT = 12,

            RedYellowT = 13
        };

        const int rgbintnum = 14;

        /*
         * gnuplot> set palette defined (0 '#c70000', 0.35 '#8f099e',
         * 0.6 '#0000ff', 1.0 '#40fcff')
         * 83 15 150
         */
        public readonly static double[][] argbrbi = {
			new double[]{0.0, 1.0, 0.77734375, 0.0, 0.0,
			0.35, 1.0, 0.55859375, 0.03515625, 0.6171875,
			0.45, 1.0, 0.32421875, 0.05859375, 0.5859375,
			0.6, 1.0, 0.0, 0.0, 1.0,
			1.0, 1.0, 0.25, 0.984375, 1.0},
            new double[]{0.0, 1.0, 0.77734375, 0.0, 0.0,
			0.30, 1.0, 0.55859375, 0.03515625, 0.6171875,
			0.42, 1.0, 0.32421875, 0.05859375, 0.5859375,
			0.54, 1.0, 0.0, 0.0, 1.0,
			0.75, 0.5, 0.25, 0.984375, 1.0,
            1.0, 0.0, 0.25, 0.984375, 1.0},
			new double[]{
0.0,1.0,0.7058823529,0.0156862745,0.1490196078,
0.03125,1.0,0.7529411765,0.1568627451,0.1843137255,
0.0625,1.0,0.7960784314,0.2431372549,0.2196078431,
0.09375,1.0,0.8352941176,0.3137254902,0.2588235294,
0.125,1.0,0.8705882353,0.3764705882,0.3019607843,
0.15625,1.0,0.8980392157,0.4392156863,0.3450980392,
0.1875,1.0,0.9254901961,0.4980392157,0.3882352941,
0.21875,1.0,0.9450980392,0.5529411765,0.4352941176,
0.25,1.0,0.9568627451,0.6039215686,0.4823529412,
0.28125,1.0,0.968627451,0.6509803922,0.5294117647,
0.3125,1.0,0.968627451,0.6941176471,0.5803921569,
0.34375,1.0,0.968627451,0.7333333333,0.6274509804,
0.375,1.0,0.9607843137,0.768627451,0.6784313725,
0.40625,1.0,0.9450980392,0.8,0.7254901961,
0.4375,1.0,0.9254901961,0.8274509804,0.7725490196,
0.46875,1.0,0.8980392157,0.8470588235,0.8196078431,
0.5,1.0,0.8666666667,0.8666666667,0.8666666667,
0.53125,1.0,0.8352941176,0.8588235294,0.9019607843,
0.5625,1.0,0.8,0.8509803922,0.9333333333,
0.59375,1.0,0.7607843137,0.8352941176,0.9568627451,
0.625,1.0,0.7215686275,0.8156862745,0.9764705882,
0.65625,1.0,0.6823529412,0.7882352941,0.9921568627,
0.6875,1.0,0.6392156863,0.7607843137,1,
0.71875,1.0,0.5960784314,0.7254901961,1,
0.75,1.0,0.5529411765,0.6901960784,0.9960784314,
0.78125,1.0,0.5098039216,0.6470588235,0.9843137255,
0.8125,1.0,0.4666666667,0.6039215686,0.968627451,
0.84375,1.0,0.4235294118,0.5568627451,0.9450980392,
0.875,1.0,0.3843137255,0.5098039216,0.9176470588,
0.90625,1.0,0.3411764706,0.4588235294,0.8823529412,
0.9375,1.0,0.3019607843,0.4078431373,0.8431372549,
0.96875,1.0,0.2666666667,0.3529411765,0.8,
1.0,1.0,0.231372549,0.2980392157,0.7529411765
},

//YlOrBr
new double[]{
    0.0,1.0,1.0,0.9960784314,0.8980392157,
0.1428571429,1.0,1.0,0.968627451,0.737254902,
0.2857142857,1.0,0.9960784314,0.8901960784,0.568627451,
0.4285714286,1.0,0.9960784314,0.768627451,0.3098039216,
0.5714285714,1.0,0.9960784314,0.6,0.1568627451,
0.7142857143,1.0,0.9254901961,0.4392156863,0.0745098039,
0.8571428571,1.0,0.8,0.2980392157,0.0039215686,
1.0,1.0,0.5490196078,0.1764705882,0.0117647059
},

//YGOrBu
new double[]{
    0.0,1.0,1.0,0.9960784314,0.8509803922,
0.1428571429,1.0,0.9294117647,0.9725490196,0.6941176471,
0.2857142857,1.0,0.7803921569,0.9137254902,0.7058823529,
0.4285714286,1.0,0.4980392157,0.8039215686,0.7333333333,
0.5714285714,1.0,0.2549019608,0.7137254902,0.768627451,
0.7142857143,1.0,0.1098039216,0.568627451,0.7529411765,
0.8571428571,1.0,0.1333333333,0.368627451,0.6588235294,
1.0,1.0,0.0470588235,0.1725490196,0.5176470588
},

//YlGr
new double[]{
    0.0,1.0,1.0,0.9960784314,0.8980392157,
0.1428571429,1.0,0.968627451,0.9882352941,0.7254901961,
0.2857142857,1.0,0.8509803922,0.9411764706,0.6392156863,
0.4285714286,1.0,0.6784313725,0.8666666667,0.5568627451,
0.5714285714,1.0,0.4705882353,0.7764705882,0.4745098039,
0.7142857143,1.0,0.2549019608,0.6705882353,0.3647058824,
0.8571428571,1.0,0.137254902,0.5176470588,0.2588235294,
1.0,1.0,0,0.3490196078,0.1960784314
},

//BrBG
new double[]{
    0.0,1.0,0.5490196078,0.3176470588,0.0352941176,
0.1428571429,1.0,0.7490196078,0.5058823529,0.1764705882,
0.2857142857,1.0,0.8745098039,0.7607843137,0.4901960784,
0.4285714286,1.0,0.9647058824,0.9098039216,0.7647058824,
0.5714285714,1.0,0.7803921569,0.9176470588,0.8980392157,
0.7142857143,1.0,0.4980392157,0.8039215686,0.7568627451,
0.8571428571,1.0,0.2078431373,0.5921568627,0.5607843137,
1.0,1.0,0,0.4,0.3647058824
},

//PiYG
new double[]{
    0.0,1.0,0.7725490196,0.1411764706,0.4901960784,
0.1428571429,1.0,0.8705882353,0.4666666667,0.6823529412,
0.2857142857,1.0,0.9450980392,0.7137254902,0.8549019608,
0.4285714286,1.0,0.9921568627,0.8784313725,0.937254902,
0.5714285714,1.0,0.9019607843,0.9607843137,0.8156862745,
0.7142857143,1.0,0.7215686275,0.8823529412,0.5254901961,
0.8571428571,1.0,0.4980392157,0.737254902,0.2509803922,
1.0,1.0,0.3019607843,0.5725490196,0.1333333333
},

//RdYlGn
new double[]{
    0.0,1.0,0.8431372549,0.1882352941,0.1529411765,
0.1428571429,1.0,0.9568627451,0.431372549,0.2588235294,
0.2857142857,1.0,0.9921568627,0.6823529412,0.3803921569,
0.4285714286,1.0,0.9960784314,0.8784313725,0.5450980392,
0.5714285714,1.0,0.8509803922,0.937254902,0.5450980392,
0.7142857143,1.0,0.6509803922,0.8509803922,0.4117647059,
0.8571428571,1.0,0.4,0.7411764706,0.3843137255,
1.0,1.0,0.1019607843,0.5960784314,0.3137254902
},

//Spectral
new double[]{
    0.0,1.0,0.8431372549,0.1882352941,0.1529411765,
0.1428571429,1.0,0.9568627451,0.431372549,0.2588235294,
0.2857142857,1.0,0.9921568627,0.6823529412,0.3803921569,
0.4285714286,1.0,0.9960784314,0.8784313725,0.5450980392,
0.5714285714,1.0,0.9019607843,0.9607843137,0.5921568627,
0.7142857143,1.0,0.6705882353,0.8666666667,0.6431372549,
0.8571428571,1.0,0.4,0.7607843137,0.6470588235,
1.0,1.0,0.1921568627,0.5333333333,0.7411764706
},

//YellowT
new double[]{
    0.0,1.0,0.909803922, 0.592156863, 0.0,
    1.0,0.0,0.956862745, 0.968627451, 0.525490196
},

//YlOrBrT, intenzitáshoz módosítva
new double[] {
        0,1,0.5490196078,0.1764705882,0.0117647059,
0.1,1,0.8,0.2980392157,0.0039215686,
0.2,1,0.9254901961,0.4392156863,0.0745098039,
0.3,1,0.9960784314,0.6,0.1568627451,
0.4,1,0.9960784314,0.768627451,0.3098039216,
0.5,1,0.9960784314,0.8901960784,0.568627451,
0.6,1,1,0.968627451,0.737254902,
0.7,0.5,1,0.9960784314,0.8980392157,
1,0,1,0.9960784314,0.8980392157},

//RedT
new double[] {
    0.0,1.0,0.929411765,0.094117647,0.0,
    1.0,0.0,1.0,0.396078431,0.329411765
},

//RedYellowT -- FECE32 -- 0.996078431, 0.807843137, 0.196078431
            new double[]{0.0, 1.0, 1.0, 0.0, 0.0,
            //~ 0.2, 1.0, 0.77734375, 0.3, 0.0,
			0.42, 1.0, 0.996078431, 0.807843137, 0.196078431,
			0.6, 1.0, 1.0, 1.0, 0.2,
			0.85, 0.5, 1.0, 1.0, 1.0,
            1.0, 0.0, 1.0, 1.0, 1.0}
		};

        public static Color ColorRGBI1(double val, int which) {
            double da, dr, dg, db;
            if (val > 1.0) val = 1.0;
            if (val < 0.0) val = 0.0;
            val = 1.0 - val;
            int i = 1;
            int np = argbrbi[which].Length / 5; //5 komponens: val,A,R,G,B
            while (i < np - 1) {
                if (val < argbrbi[which][5 * i]) break;
                i++;
            }
            double x = (val - argbrbi[which][5 * (i - 1)]) / (argbrbi[which][5 * i] - argbrbi[which][5 * (i - 1)]);
            da = argbrbi[which][5 * (i - 1) + 1] + x * (argbrbi[which][5 * i + 1] - argbrbi[which][5 * (i - 1) + 1]);
            dr = argbrbi[which][5 * (i - 1) + 2] + x * (argbrbi[which][5 * i + 2] - argbrbi[which][5 * (i - 1) + 2]);
            dg = argbrbi[which][5 * (i - 1) + 3] + x * (argbrbi[which][5 * i + 3] - argbrbi[which][5 * (i - 1) + 3]);
            db = argbrbi[which][5 * (i - 1) + 4] + x * (argbrbi[which][5 * i + 4] - argbrbi[which][5 * (i - 1) + 4]);
            int ua = (int)(da * 256.0);
            int ur = (int)(dr * 256.0);
            int ug = (int)(dg * 256.0);
            int ub = (int)(db * 256.0);
            if (ua > 255) ua = 255;
            if (ur > 255) ur = 255;
            if (ug > 255) ug = 255;
            if (ub > 255) ub = 255;
            return Color.FromArgb(ua, ur, ug, ub);
        }

        /// <summary>
        /// Generate greyscale color
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0]</param>
        /// <returns></returns>
        public static Color ColorGrey(double val) {
            if (val >= 1.0) val = 0.999;
            if (val < 0.0) val = 0.0;
            short v2 = (short)Math.Floor(256.0 * val);
            return Color.FromArgb(v2, v2, v2);
        }

        /// <summary>
        /// Generate color with a given hue, goes from light to dark
        /// return the HSV values, do not convert to RGB
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="hue">color to use (hue value)</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        public static void ColorHueHSV(double val, int hue, out double h, out double s, out double v) {
            if (val > 1.0) val = 1.0;
            if (val < 0.0) val = 0.0;
            h = (double)hue;
            s = 0.1 + 0.85 * val;
            v = 0.9 - 0.6 * val;
        }

        /// <summary>
        /// Generate color with a given hue, goes from light to dark
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="hue">color to use (hue value)</param>
        /// <returns></returns>
        public static Color ColorHue(double val, int hue) {
            double h, s, v;
            ColorHueHSV(val, hue, out h, out s, out v);
            return hsvtorgb(h, s, v);
        }

        /// <summary>
        /// Generate color with a given hue, goes from dark to light
        /// return the HSV values, do not convert to RGB
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="hue">color to use (hue value)</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        public static void ColorHueLightHSV(double val, int hue, out double h, out double s, out double v) {
            if (val > 1.0) val = 1.0;
            if (val < 0.0) val = 0.0;
            h = (double)hue;
            s = 0.95 - 0.85 * val;
            v = 0.9 - 0.6 * val;
        }

        /// <summary>
        /// Generate color with a given hue, goes from dark to light
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <param name="hue">color to use (hue value)</param>
        /// <returns></returns>
        public static Color ColorHueLight(double val, int hue) {
            double h, s, v;
            ColorHueLightHSV(val, hue, out h, out s, out v);
            return hsvtorgb(h, s, v);
        }


        /*
         * Red -- 0
         * Orange -- 30
         * Yellow -- 60
         * Green -- 120
         * Blue -- 240
         * Purple -- 260
         * Pink -- 300
         */

        /// <summary>
        /// Generate color from the Red palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorRed(double val) {
            return ColorHue(val, 0);
        }

        /// <summary>
        /// Generate color from the Orange palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorOrange(double val) {
            return ColorHue(val, 30);
        }

        /// <summary>
        /// Generate color from the Yellow palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorYellow(double val) {
            return ColorHue(val, 60);
        }

        /// <summary>
        /// Generate color from the Green palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorGreen(double val) {
            return ColorHue(val, 120);
        }

        /// <summary>
        /// Generate color from the Blue palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorBlue(double val) {
            return ColorHue(val, 240);
        }

        /// <summary>
        /// Generate color from the Purple palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorPurple(double val) {
            return ColorHue(val, 260);
        }

        /// <summary>
        /// Generate color from the Pink palette
        /// </summary>
        /// <param name="val">the value to convert (should be in the range [0.0,1.0])</param>
        /// <returns></returns>
        public static Color ColorPink(double val) {
            return ColorHue(val, 300);
        }

        /// <summary>
        /// Convert HSV color to RGB (based on the Wikipedia article: http://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV )
        /// </summary>
        /// <param name="h">hue</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        /// <returns></returns>
        public static Color hsvtorgb(double h, double s, double v) {
            double h360 = Math.Floor(h / 360.0);
            h -= 360.0 * h360; //[0;360] közötti tartományba transzformáljuk
            h /= 60.0; // h \in [0;6]
            double c = s * v;

            //h mod 2 kiszámítása
            double h2 = Math.Floor(h / 2.0);
            double hmod2 = h - 2.0 * h2;
            hmod2 = Math.Abs(hmod2 - 1);
            double x = c * (1 - hmod2);

            double r = 0.0, g = 0.0, b = 0.0;
            uint hf = (uint)Math.Floor(h); //egész rész
            if (hf >= 6) hf = 0;
            switch (hf) {
                case 0:
                    r = c;
                    g = x;
                    b = 0;
                    break;
                case 1:
                    r = x;
                    g = c;
                    b = 0;
                    break;
                case 2:
                    r = 0;
                    g = c;
                    b = x;
                    break;
                case 3:
                    r = 0;
                    g = x;
                    b = c;
                    break;
                case 4:
                    r = x;
                    g = 0;
                    b = c;
                    break;
                case 5:
                    r = c;
                    g = 0;
                    b = x;
                    break;
            }
            double m = v - c;
            r += m;
            g += m;
            b += m;
            //szaturáció
            if (r < 0.0) r = 0.0;
            if (g < 0.0) g = 0.0;
            if (b < 0.0) b = 0.0;
            int ur = (int)(r * 256.0);
            int ug = (int)(g * 256.0);
            int ub = (int)(b * 256.0);
            if (ur > 255) ur = 255;
            if (ug > 255) ug = 255;
            if (ub > 255) ub = 255;

            return Color.FromArgb(ur, ug, ub);
        }

        /// <summary>
        /// Convert a Color to HSV (alpha value is ignored)
        /// source: http://www.javascripter.net/faq/rgb2hsv.htm
        /// </summary>
        /// <param name="c">Color to convert.</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        public static void rgbtohsv(Color c, out double h, out double s, out double v) {
            rgbtohsv(c.R, c.G, c.B, out h, out s, out v);
        }

        /// <summary>
        /// Convert RGB values to HSV
        /// source: http://www.javascripter.net/faq/rgb2hsv.htm
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <param name="h">hue (\in [0,360))</param>
        /// <param name="s">saturation (\in [0,1])</param>
        /// <param name="v">value (\in [0,1])</param>
        public static void rgbtohsv(uint r, uint g, uint b, out double h, out double s, out double v) {
            if (r > 255 || g > 255 || b > 255) throw new Exception("ColorVal.rgbtohsv1(): values out of range!\n");

            uint minRGB = Math.Min(r, Math.Min(g, b));
            uint maxRGB = Math.Max(r, Math.Max(g, b));

            // Black-gray-white
            if (minRGB == maxRGB) {
                v = ((double)minRGB) / 255.0;
                h = 0.0;
                s = 0.0;
                return;
            }

            double dr = ((double)r) / 255.0;
            double dg = ((double)g) / 255.0;
            double db = ((double)b) / 255.0;
            double dmax = ((double)maxRGB) / 255.0;
            double dmin = ((double)minRGB) / 255.0;

            // Colors other than black-gray-white:
            double d = ((r == minRGB) ? dg - db : ((b == minRGB) ? dr - dg : db - dr));
            double h1 = (double)((r == minRGB) ? 3.0 : ((b == minRGB) ? 1.0 : 5.0));
            h = 60.0 * (h1 - d / (dmax - dmin));
            s = (dmax - dmin) / dmax;
            v = dmax;
        }

        /// <summary>
        /// Convert a System.Drawing.Color to a '#RRGGBB' string.
        /// </summary>
        /// <returns>
        /// String representation of the color.
        /// </returns>
        /// <param name='c'>
        /// Color to convert.
        /// </param>
        public static string ColorToString(Color c) {
            StringBuilder sb = new StringBuilder("#", 7);
            sb.Append(c.R.ToString("X2"));
            sb.Append(c.G.ToString("X2"));
            sb.Append(c.B.ToString("X2"));
            return sb.ToString();
        }

        //constant blue-to-red colormap
        public readonly static int[] colormap2 = { 0,0,255,
0,1,255,
0,2,255,
0,4,255,
0,5,255,
0,7,255,
0,9,255,
0,11,255,
0,13,255,
0,15,255,
0,18,253,
0,21,251,
0,24,250,
0,27,248,
0,30,245,
0,34,243,
0,37,240,
0,41,237,
0,45,234,
0,49,230,
0,53,226,
0,57,222,
0,62,218,
0,67,214,
0,71,209,
0,76,204,
0,82,199,
0,87,193,
0,93,188,
0,98,182,
0,104,175,
0,110,169,
0,116,162,
7,123,155,
21,129,148,
34,136,141,
47,142,133,
60,149,125,
71,157,117,
83,164,109,
93,171,100,
104,179,91,
113,187,92,
123,195,73,
132,203,63,
140,211,53,
148,220,43,
156,228,33,
163,237,22,
170,246,11,
176,255,0,
183,248,0,
188,241,0,
194,234,0,
199,227,0,
204,220,0,
209,214,0,
213,207,0,
217,200,0,
221,194,0,
224,188,0,
227,181,0,
230,175,0,
233,169,0,
236,163,0,
238,157,0,
240,151,0,
243,145,0,
244,140,0,
246,134,0,
248,129,0,
249,123,0,
250,118,0,
251,112,0,
252,107,0,
253,102,0,
254,97,0,
255,92,0,
255,87,0,
255,82,0,
255,78,0,
255,73,0,
255,68,0,
255,64,0,
255,59,0,
255,55,0,
255,51,0,
255,47,0,
255,43,0,
255,39,0,
255,35,0,
255,31,0,
255,27,0,
255,23,0,
255,20,0,
255,16,0,
255,13,0,
255,10,0,
255,8,0,
255,3,0};

    }
}
