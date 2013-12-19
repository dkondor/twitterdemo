#region Written by Tamas Budavari (budavari@jhu.edu)
/*
 * Spherical.Constant
 * 
 * SphericalLib is a .NET library to deal with geometry on the unit sphere.
 * It is intented to be used for complex survey footprint descriptions in
 * standalone applications as well as in web services and inside SQL Server.
 * 
 * Written by Tamas Budavari (budavari@jhu.edu)
 * See bottom of file for revision history
 * 
 * Current revision:
 *   ID:          $Id: Constant.cs 5928 2010-08-09 21:07:45Z budavari $
 *   Revision:    $Revision: 5928 $
 *   Date:        $Date: 2010-08-09 23:07:45 +0200 (H, 09 aug. 2010) $
 */
using System;
#endregion

namespace Elte.GeoVisualizer.Lib
{
    /// <summary>
	/// Holds relevant constants for the project.
	/// </summary>
	public static class Constants
    {
        /// <summary>
        /// Constant for the theoretical limit of the relative error of double precision numbers.
        /// </summary>
        /// <remarks>
        /// With p=53 significand (IEEE-754), it is <c>Math.Pow(2,-53)</c>, which is roughly 1.1e-16.
        /// </remarks>
        public static readonly double DoublePrecision = Math.Pow(2,-53);

        /// <summary>
        /// Constant for the theoretical limit of the relative error of double precision numbers times two.
        /// </summary>
        /// <remarks>
        /// This is, for example, the maximum relative error in the difference of two numbers, <c>z = x-y</c>.
        /// </remarks>
        public static readonly double DoublePrecision2x = 2 * DoublePrecision;
        internal static readonly double DoublePrecision4x = 4 * DoublePrecision;

        /// <summary>
        /// Constant factor in radians for determining when two points are the same.
        /// </summary>
        public static readonly double Tolerance = 2e-8;
        /// <summary>
        /// Constant is the cosine of the tolerance.
        /// </summary>
        public static readonly double CosTolerance = Math.Cos(Tolerance);
        /// <summary>
        /// Constant is the sine of the tolerance.
        /// </summary>
        public static readonly double SinTolerance = Math.Sin(Tolerance);

        internal static readonly double TolHalf = Tolerance / 2;
        internal static readonly double CosHalf = Math.Cos(TolHalf);
        internal static readonly double SinHalf = Math.Sin(TolHalf);
        internal static readonly double TolArea = TolHalf * TolHalf * Math.PI;

        internal static readonly double SafeLimit = 1e-7;
        internal static readonly double CosSafe = Math.Cos(SafeLimit);
        internal static readonly double SinSafe = Math.Sin(SafeLimit);

        internal static readonly int HashDigit = 15;
        internal static readonly double HashMagic = 2; //29;

        /// <summary>
        /// Constant factor for converting degrees to radians.
        /// </summary>
        public static readonly double Degree2Radian = Math.PI / 180;

        /// <summary>
        /// Constant factor for converting radians to degrees.
        /// </summary>
        public static readonly double Radian2Degree = 1 / Degree2Radian;

        /// <summary>
        /// Constant factor for converting arc minutes to radians.
        /// </summary>
        public static readonly double Arcmin2Radian = Degree2Radian / 60;

        /// <summary>
        /// Constant factor for converting radians to arc minutes.
        /// </summary>
        public static readonly double Radian2Arcmin = 1 / Arcmin2Radian;

        /// <summary>
        /// Constant factor for converting square radians to square degrees.
        /// </summary>
        public static readonly double SquareRadian2SquareDegree = Radian2Degree * Radian2Degree;

        /// <summary>
        /// Constant factor for area of surface of the unit sphere in square degrees.
        /// </summary>
        public static readonly double WholeSphereInSquareDegree = 4 * Math.PI * SquareRadian2SquareDegree;

        /// <summary>
        /// The XML namespace for serialization of classes.
        /// </summary>
        public const string NameSpace = "ivo://voservices.org/spherical";

        /// <summary>
        /// Region keyword.
        /// </summary>
        internal static readonly string KeywordRegion = "REGION";

        /// <summary>
        /// Convex keyword.
        /// </summary>
        internal static readonly string KeywordConvex = "CONVEX";

		/// <summary>
		/// Revision from CVS.
		/// </summary>
		public static readonly string Revision = "$Revision: 5928 $"; 	
    }
}

#region Revision History
/* Revision History

        $Log: not supported by cvs2svn $
        Revision 1.9  2007/03/27 18:46:23  budavari
        Updated hashcode methods for speed

        Revision 1.8  2007/03/08 15:43:51  budavari
        Moved Tolerance and company to Constant from Global

        Revision 1.7  2007/02/12 17:39:46  budavari
        Decreased value for SafeLimit

        Revision 1.6  2007/01/05 19:21:45  budavari
        Updated SafeLimit

        Revision 1.5  2006/12/13 02:02:16  budavari
        Updated doc string

        Revision 1.4  2006/10/06 22:11:54  budavari
        Safe limit constants.

        Revision 1.3  2006/10/05 19:38:04  budavari
        Small cosmetical changes.

        Revision 1.2  2006/10/04 20:40:59  budavari
        Tweaks to call Global less.

        Revision 1.1  2006/10/04 04:04:46  budavari
        Got rid of Limits and introduced Global

        Revision 1.18  2006/10/02 18:09:26  budavari
        Added constand for double precision.

        Revision 1.17  2006/09/21 15:53:22  budavari
        *** empty log message ***

        Revision 1.16  2006/09/20 16:50:20  budavari
        Using defaults from Limits.Default

        Revision 1.15  2006/09/12 18:35:37  budavari
        Updated doc strings, etc...

        Revision 1.14  2006/07/31 15:44:15  dobos
        *** empty log message ***

        Revision 1.13  2006/07/29 02:32:57  budavari
        New NameSpace

        Revision 1.12  2006/07/26 15:56:42  budavari
        Lowered maxiter to 100

        Revision 1.11  2006/07/22 18:41:48  budavari
        Default limits of Tolerance and Epsilon lowered to 1e-13

        Revision 1.10  2006/07/18 15:48:37  budavari
        Updated boiler plate

        Revision 1.9  2006/07/17 21:32:22  budavari
        Updated doc strings and DLL

        Revision 1.8  2006/07/14 00:47:00  budavari
        No more lazy eval & some cleanup of precision params, etc.

        Revision 1.7  2006/07/04 16:23:25  budavari
        Minor updates...

        Revision 1.6  2006/06/21 14:09:08  budavari
        Added ESign.Unknown to avoid Nullable in Convex

        Revision 1.5  2006/04/24 15:18:33  budavari
        Tolerance 1e-13 was too small, back to 1e-10

        Revision 1.4  2006/03/31 15:27:00  budavari
        small additions and typo fix

        Revision 1.3  2006/03/08 19:49:08  gyuri
        Attributes of static fields in safe assemblies must be marked  readonly in Visual C#

        Revision 1.2  2006/02/28 16:26:59  budavari
        Major revision: public properties

        Revision 1.1  2006/01/24 15:56:05  budavari
        Re-structured directory layout to accommodate more projects

        Revision 1.16  2006/01/19 17:27:10  budavari
        Extended sign enum added

        Revision 1.15  2005/12/02 15:02:57  budavari
        RootStatus.Any and Const.Epsilon=1e-15 added

        Revision 1.14  2005/11/30 21:13:09  budavari
        Surface of unit sphere in sq deg added to constants

        Revision 1.13  2005/11/30 20:55:39  budavari
        Cosmetics... #region specifications etc added

        Revision 1.12  2005/11/30 15:55:06  budavari
        Doc added

        Revision 1.11  2005/11/30 15:32:37  budavari
        Header changes

        Revision 1.10  2005/11/29 14:01:19  budavari
        Some default values changed...

        Revision 1.9  2005/11/24 16:27:16  budavari
        Region areas compared to SQL code and releated fixes of degenerate arcs.
        Looking good except maybe for one patch (convex 0) of the 'footprint',
        which has 2 degenerate triangles and patch area changes when shifting
        pivot point.

        Revision 1.8  2005/11/18 05:58:09  budavari
        Working on boolean operations on regions...

        Revision 1.7  2005/11/17 01:58:54  budavari
        Done with the convex stuff... Hurray!!! It can even deal with the "orange
        slices" to some extent: the problem is the area - coming up next...

        Revision 1.6  2005/11/16 18:56:59  budavari
        Only degenerate roots are missing... Also added sample convex static class.

        Revision 1.5  2005/11/15 22:20:20  budavari
        Using Generics - almost there

        Revision 1.4  2005/11/15 15:25:36  budavari
        Everything is a class, things are coming together for area computation, etc. Will moved to C# 2.0

        Revision 1.3  2005/11/04 22:49:36  budavari
        Re-write to have Point and Halfspace as struct again and other tinkerings. XML is still fine...

        Revision 1.2  2005/11/03 15:22:01  budavari
        Fairly major revision to make sure the objects serialize to XML nicely:
          - Added separate containers of halfspaces and convexes
          - Customized XML output for compactness and friendly proxy interface
          - Implemented simple .Parse() methods taking cartesion representation

        Revision 1.1.1.1  2005/10/28 14:13:43  budavari
        Initial import of the SphericalLib library. It is pretty complete in terms
        of the classes but lacking the simplication and other advanced methods.
		
*/
#endregion