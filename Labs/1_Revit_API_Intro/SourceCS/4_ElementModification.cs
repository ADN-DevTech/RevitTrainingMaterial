#region Copyright
//
// Copyright (C) 2009-2020 by Autodesk, Inc.
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
// Migrated to C# by Adam Nagy 
// 
#endregion // Copyright

#region Namespaces
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Util;
#endregion

#region Description
// Revit Intro Lab 4 
// 
// In this lab, you will learn how to modify elements. 
// There are two places to look at when you want to modify an element. 
// (1) at each element level, such as by modifying each properties, parameters and location. 
// (2) use transformation utility methods, such as move, rotate and mirror. 
// 
// for #2, ElementTransformUtils.MoveElement, RotateElement, etc., see pp113 of developer guide. 
// 
// Disclaimer: minimum error checking to focus on the main topic. 
// 
#endregion

namespace IntroCs
{
  /// <summary>
  /// Element Modification 
  /// </summary>  
  [Transaction(TransactionMode.Manual)]
  public class ElementModification : IExternalCommand
  {
    // Member variables 
    Application _app;
    Document _doc;

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      // Get the access to the top most objects. 
      UIApplication rvtUIApp = commandData.Application;
      UIDocument uiDoc = rvtUIApp.ActiveUIDocument;
      _app = rvtUIApp.Application;
      _doc = uiDoc.Document;

      // Select a door on screen. (We'll come back to the selection in the UI Lab later.) 
      Reference r = uiDoc.Selection.PickObject(ObjectType.Element, "Pick a wall, please");
      // We have picked something. 
      Element e = _doc.GetElement(r);

      using (Transaction transaction = new Transaction(_doc))
      {
        transaction.Start("Modify Element");
        // (1) element level modification 
        // Modify element's properties, parameters, location. 

        ModifyElementPropertiesWall(e);
        //ModifyElementPropertiesDoor(e);
        _doc.Regenerate();

        // Select an object on a screen. (We'll come back to the selection in the UI Lab later.) 
        Reference r2 = uiDoc.Selection.PickObject(ObjectType.Element, "Pick another element");
        // We have picked something. 
        Element e2 = _doc.GetElement(r2);

        // (2) you can also use transformation utility to move and rotate. 
        ModifyElementByTransformUtilsMethods(e2);
        transaction.Commit();
      }

      return Result.Succeeded;
    }

    /// <summary>
    /// A sampler function to demonstrate how to modify an element through its properties. 
    /// Using a wall as an example here. 
    /// </summary> 
    public void ModifyElementPropertiesWall(Element e)
    {
      // Constant to this function. 
      // This is for wall. e.g., "Basic Wall: Exterior - Brick on CMU" 
      // You can modify this to fit your need. 

      const string wallFamilyName = Util.Constant.WallFamilyName;
      const string wallTypeName = "Exterior - Brick on CMU";
      const string wallFamilyAndTypeName = wallFamilyName + ": " + wallTypeName;

      // For simplicity, we assume we can only modify a wall 
      if (!(e is Wall))
      {
        TaskDialog.Show(
          "Modify element properties - wall",
          "Sorry, I only know how to modify a wall. Please select a wall.");
        return;
      }
      Wall aWall = (Wall)e;

      string msg = "Wall changed:\r\n\r\n"; // Keep the message to the user. 

      // (1) change its family type to a different one. 
      // To Do: change this to enhance import symbol later. 

      Element newWallType = ElementFiltering.FindFamilyType(_doc, typeof(WallType), wallFamilyName, wallTypeName, null);

      if (newWallType != null)
      {
        aWall.WallType = (WallType)newWallType;
        msg += "Wall type to: " + wallFamilyAndTypeName + "\r\n";
      }
      //TaskDialog.Show( 
      //  "Modify element properties - wall", 
      //  msg ) 

      // (2) change its parameters. 
      // As a way of exercise, let's constrain top of the wall to the level1 and set an offset. 

      // Find the level 1 using the helper function we defined in the lab3. 
      Level level1 = (Level)ElementFiltering.FindElement(_doc, typeof(Level), "Level 1", null);
      if (level1 != null)
      {
        // Top Constraint 
        aWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level1.Id);
        msg += "Top Constraint to: Level 1\r\n";
      }

      // Hard coding for simplicity here. 
      double topOffset = Constant.MmToFeet(5000.0);
      // Top Offset Double 
      aWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(topOffset);
      // Structural Usage = Bearing(1) 
      //aWall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM).Set(1); // This is read only 
      // Comments - String 
      aWall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("Modified by API");

      msg += "Top Offset to: 5000.0\r\n";
      msg += "Structural Usage to: Bearing\r\n";
      msg += "Comments added: Modified by API\r\n";
      //TaskDialog.Show("Modify element properties - wall", msg ); 

      // (3) Optional: change its location, using location curve 
      // LocationCurve also has move and rotation methods. 
      // Note: constaints affect the result. 
      // Effect will be more visible with disjoined wall. 
      // To test this, you may want to draw a single standing wall, 
      // and run this command. 

      LocationCurve wallLocation = (LocationCurve)aWall.Location;

      XYZ pt1 = wallLocation.Curve.GetEndPoint(0);
      XYZ pt2 = wallLocation.Curve.GetEndPoint(1);

      // Hard coding the displacement value for simility here. 
      double dt = Constant.MmToFeet(1000.0);
      XYZ newPt1 = new XYZ(pt1.X - dt, pt1.Y - dt, pt1.Z);
      XYZ newPt2 = new XYZ(pt2.X - dt, pt2.Y - dt, pt2.Z);

      // Create a new line bound. 
      Line newWallLine = Line.CreateBound(newPt1, newPt2);

      // Finally change the curve. 
      wallLocation.Curve = newWallLine;

      msg += "Location: start point moved -1000.0 in X-direction\r\n";

      // Message to the user. 

      TaskDialog.Show("Modify element properties - wall", msg);
    }

    /// <summary>
    /// A sampler function to demonstrate how to modify an element through its properties. 
    /// Using a door as an example here. 
    /// </summary> 
    public void ModifyElementPropertiesDoor(Element e)
    {
      // Constant to this function. 
      // This is for a door. e.g., "M_Single-Flush: 0762 x 2032mm" 
      // You can modify this to fit your need. 

      const string doorFamilyName = Util.Constant.DoorFamilyName;
      const string doorTypeName = Util.Constant.DoorTypeName2;
      const string doorFamilyAndTypeName = doorFamilyName + ": " + doorTypeName;

      // For simplicity, we assume we can only modify a door 
      if (!(e is FamilyInstance))
      {
        TaskDialog.Show(
          "Modify element properties - door",
          "Sorry, I only know how to modify a door. Please select a door.");
        return;
      }
      FamilyInstance aDoor = (FamilyInstance)e;

      string msg = "Door changed:\n\n";

      // (1) change its family type to a different one. 

      Element newDoorType = ElementFiltering.FindFamilyType(_doc, typeof(FamilySymbol), doorFamilyName, doorTypeName, BuiltInCategory.OST_Doors);

      if (newDoorType != null)
      {
        aDoor.Symbol = (FamilySymbol)newDoorType;
        msg += "Door type to: " + doorFamilyAndTypeName + "\r\n";
        //TaskDialog.Show("Modify element properties - door", msg);
      }

      // (2) change its parameters. 
      // leave this as your exercise. 


      // message to the user. 
      TaskDialog.Show("Modify element properties - door", msg);
    }

    /// <summary>
    /// A sampler function that demonstrates how to modify an element 
    /// transform utils methods. 
    /// </summary> 
    public void ModifyElementByTransformUtilsMethods(Element e)
    {
      string msg = "The element changed:\n\n";

      // Try move 
      double dt = Constant.MmToFeet(1000.0);

      // Hard cording for simplicity. 
      XYZ v = new XYZ(dt, dt, 0.0);

      ElementTransformUtils.MoveElement(e.Document, e.Id, v); // 2012

      msg += "move by (1000, 1000, 0)\r\n";

      // Try rotate: 15 degree around z-axis. 
      XYZ pt1 = XYZ.Zero;
      XYZ pt2 = XYZ.BasisZ;
      Line axis = Line.CreateBound(pt1, pt2);

      ElementTransformUtils.RotateElement(e.Document, e.Id, axis, Math.PI / 12.0); // 2012

      msg += "rotate by 15 degree around Z-axis\r\n";

      TaskDialog.Show("Modify element by utils methods", msg);
    }

  }

}
