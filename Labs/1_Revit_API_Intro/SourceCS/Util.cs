#region Copyright
//
// Copyright (C) 2010-2014 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
// 
#endregion // Copyright

// Instruction: 
// To switch between Imperial vs. Matric constants. 
// either define a the following pre-processor directive: 
//#define USE_IMPERIAL_FAMILY_NAMES_AND_TYPES
// or add "!" in the #if statement. 
//#if !USE_IMPERIAL_FAMILY_NAMES_AND_TYPES 
// In .Net, you can also use Conditional attribute. 

using System;
using Autodesk.Revit.DB; // for XYZ

namespace Util
{
  public class Format
  {
    public static string RealString(double a)
    {
      return a.ToString("0.##");
    }

    public static string PointString(XYZ p)
    {
      return string.Format("({0},{1},{2})",
        RealString(p.X), RealString(p.Y),
        RealString(p.Z));
    }
  }

  /// <summary>
  /// Define some string constants for family and type 
  /// names which vary between imperial and metric content.
  /// Please note that a professional application is 
  /// language independent, i.e. avoid use of strings 
  /// like this or switches them automatically.
  /// </summary>
  /// 

  public class Constant
  {
    /// <summary>
    /// Conversion factor to convert millimetres to feet.
    /// </summary>
    const double _mmToFeet = 0.0032808399;

    public static double MmToFeet(double mmValue)
    {
      return mmValue * _mmToFeet;
    }
      

#if USE_IMPERIAL_FAMILY_NAMES_AND_TYPES 

    // Imperial family names and types:

    public const string DoorFamilyName = "Single-Flush"; // "M_Single-Flush" 
    public const string DoorTypeName = "30\" x 80\""; // "0915 x 2134mm" 
    public const string DoorTypeName2 = "30\" x 80\""; // "0762 x 2032mm"

    public const string RoofTypeName = "Generic - 9\""; // "Generic - 400mm"

    public const string WallFamilyName = "Basic Wall";
    public const string WallTypeName = "Generic - 8\""; // "Generic - 200mm" 

    public const string WindowFamilyName = "Fixed"; // "M_Fixed" 
    public const string WindowTypeName = "16\" x 24\""; // "0915 x 1830mm" 

#else // if not USE_IMPERIAL_FAMILY_NAMES_AND_TYPES

    // Metric family names and types:

    public const string DoorFamilyName = "M_Single-Flush";
    public const string DoorTypeName = "0915 x 2134mm";
    public const string DoorTypeName2 = "0762 x 2032mm";

    public const string RoofTypeName = "Generic - 400mm";

    public const string WallFamilyName = "Basic Wall";
    public const string WallTypeName = "Generic - 200mm";

    public const string WindowFamilyName = "M_Fixed";
    public const string WindowTypeName = "0915 x 1830mm";

#endif // USE_IMPERIAL_FAMILY_NAMES_AND_TYPES

  }
}
