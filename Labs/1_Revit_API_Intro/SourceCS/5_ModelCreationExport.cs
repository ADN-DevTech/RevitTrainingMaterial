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
// Written by M.Harada 
//
#endregion

#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Util;
using IntroCs;
#endregion

#region Description
// Revit Intro Lab 5 - Element Creation.  
//
// In this lab, you will learn how to create revit models. 
// To test this, use "DefaultMetric" template. 
// 
// Disclaimer: minimum error checking to focus on the main topic. 
//
// ** This version of model creation is callable from outside of  ** 
// ** the original class. Intended to be used from Revit UI Labs. ** 
// ** The main functionality is identical to 5_ModelCreation.cs.  **
// 
#endregion

namespace IntroCs
{
  [Transaction(TransactionMode.Manual)]
  public class ModelCreationExport : IExternalCommand
  {
    // Member variables 
    Application _app;
    Document _doc;

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      // Get the acess to the top most objects. 
      UIApplication rvtUIApp = commandData.Application;
      UIDocument uiDoc = rvtUIApp.ActiveUIDocument;
      _app = rvtUIApp.Application;
      _doc = uiDoc.Document;

      // Let's make a simple "house" composed of four walls, a window 
      // and a door. 
      CreateHouse(_doc);

      return Result.Succeeded;
    }

    public void CreateHouse_v1()
    {
      // Simply create four walls with rectangular profile. 
      List<Wall> walls = CreateWalls(_doc);

      // Add a door to the second wall 
      AddDoor(_doc, walls[0]);

      // Add windows to the rest of the walls. 
      for (int i = 1; i <= 3; i++)
      {
        AddWindow(_doc, walls[i]);
      }

      // (optional) add a roof over the walls' rectangular profile. 
      AddRoof(_doc, walls);
    }

    public static void CreateHouse(Document rvtDoc)
    {
      using (Transaction transaction = new Transaction(rvtDoc))
      {
        transaction.Start("Create House");
        // Simply create four walls with rectangular profile. 
        List<Wall> walls = CreateWalls(rvtDoc);

        // Add a door to the second wall 
        AddDoor(rvtDoc, walls[0]);

        // Add windows to the rest of the walls. 
        for (int i = 1; i <= 3; i++)
        {
          AddWindow(rvtDoc, walls[i]);
        }

        // (optional) add a roof over the walls' rectangular profile. 
        AddRoof(rvtDoc, walls);
        transaction.Commit();
      }
    }

    /// <summary>
    /// There are five override methods for creating walls. 
    /// We assume you are using metric template, where you have
    /// "Level 1" and "Level 2"
    /// cf. Developer Guide page 117 
    /// </summary>
    public List<Wall> CreateWalls_v1()
    {
      // Hard coding the size of the house for simplicity 
      double width = Constant.MmToFeet(10000.0);
      double depth = Constant.MmToFeet(5000.0);

      // Get the levels we want to work on. 
      // Note: hard coding for simplicity. Modify here you use a different template. 
      Level level1 = ElementFiltering.FindElement(_doc, typeof(Level), "Level 1", null) as Level;
      if (level1 == null)
      {
        TaskDialog.Show("Create walls",
          "Cannot find (Level 1). Maybe you use a different template? Try with DefaultMetric.rte.");
        return null;
      }

      Level level2 = ElementFiltering.FindElement(_doc, typeof(Level), "Level 2", null) as Level;
      if (level2 == null)
      {
        TaskDialog.Show("Create walls",
          "Cannot find (Level 2). Maybe you use a different template? Try with DefaultMetric.rte.");
        return null;
      }

      // Set four corner of walls.
      // 5th point is for combenience to loop through.  
      double dx = width / 2.0;
      double dy = depth / 2.0;

      List<XYZ> pts = new List<XYZ>(5);
      pts.Add(new XYZ(-dx, -dy, 0.0));
      pts.Add(new XYZ(dx, -dy, 0.0));
      pts.Add(new XYZ(dx, dy, 0.0));
      pts.Add(new XYZ(-dx, dy, 0.0));
      pts.Add(pts[0]);

      // Flag for structural wall or not. 
      bool isStructural = false;

      // Save walls we create. 
      List<Wall> walls = new List<Wall>(4);

      // Loop through list of points and define four walls. 
      for (int i = 0; i <= 3; i++)
      {
        // Define a base curve from two points. 
        Line baseCurve = Line.CreateBound(pts[i], pts[i + 1]);
        // Create a wall using the one of overloaded methods. 

        //Wall aWall = _doc.Create.NewWall(baseCurve, level1, isStructural); // 2012
        Wall aWall = Wall.Create(_doc, baseCurve, level1.Id, isStructural); // since 2013

        // Set the Top Constraint to Level 2 
        aWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
        // Save the wall.
        walls.Add(aWall);
      }

      // This is important. we need these lines to have shrinkwrap working. 
      _doc.Regenerate();
      _doc.AutoJoinElements();

      return walls;

    }

    /// <summary>
    /// Second version modified for Revit UI Labs.
    /// </summary>
    public static List<Wall> CreateWalls(Document rvtDoc)
    {
      // Hard coding the lower-left and upper-right corners of walls. 
      XYZ pt1 = new XYZ(Constant.MmToFeet(-5000.0), Constant.MmToFeet(-2500.0), 0.0);
      XYZ pt2 = new XYZ(Constant.MmToFeet(5000.0), Constant.MmToFeet(2500.0), 0.0);

      List<Wall> walls = CreateWalls(rvtDoc, pt1, pt2);

      return walls;
    }

    /// <summary>
    /// Create walls with a rectangular profile from two coner points. 
    /// </summary>
    public static List<Wall> CreateWalls(Document rvtDoc, XYZ pt1, XYZ pt2)
    {
      // Set the lower-left (x1, y1) and upper-right (x2, y2) corners of a house. 
      double x1 = pt1.X;
      double x2 = pt2.X;
      if (pt1.X > pt2.X)
      {
        x1 = pt2.X;
        x2 = pt1.X;
      }

      double y1 = pt1.Y;
      double y2 = pt2.Y;
      if (pt1.Y > pt2.X)
      {
        y1 = pt2.Y;
        y2 = pt1.Y;
      }

      // Set four corner of walls from two croner point.
      // 5th point is for combenience to loop through.  
      List<XYZ> pts = new List<XYZ>(5);
      pts.Add(new XYZ(x1, y1, pt1.Z));
      pts.Add(new XYZ(x2, y1, pt1.Z));
      pts.Add(new XYZ(x2, y2, pt1.Z));
      pts.Add(new XYZ(x1, y2, pt1.Z));
      pts.Add(pts[0]);

      // Get the levels we want to work on. 
      // Note: hard coding for simplicity. Modify here you use a different template. 
      Level level1 = ElementFiltering.FindElement(rvtDoc, typeof(Level), "Level 1", null) as Level;
      if (level1 == null)
      {
        TaskDialog.Show(
          "Create walls", "Cannot find (Level 1). Maybe you use a different template? Try with DefaultMetric.rte."
        );
        return null;
      }

      Level level2 = ElementFiltering.FindElement(rvtDoc, typeof(Level), "Level 2", null) as Level;
      if (level2 == null)
      {
        TaskDialog.Show(
          "Create walls", "Cannot find (Level 2). Maybe you use a different template? Try with DefaultMetric.rte."
        );
        return null;
      }

      // Flag for structural wall or not. 
      bool isStructural = false;

      // Save walls we create. 
      List<Wall> walls = new List<Wall>(4);

      // Loop through list of points and define four walls. 
      for (int i = 0; i <= 3; i++)
      {
        // define a base curve from two points. 
        Line baseCurve = Line.CreateBound(pts[i], pts[i + 1]);
        // create a wall using the one of overloaded methods. 
        //Wall aWall = rvtDoc.Create.NewWall(baseCurve, level1, isStructural); // 2012
        Wall aWall = Wall.Create(rvtDoc, baseCurve, level1.Id, isStructural); // since 2013
        // set the Top Constraint to Level 2 
        aWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
        // save the wall.
        walls.Add(aWall);
      }
      // This is important. we need these lines to have shrinkwrap working. 
      rvtDoc.Regenerate();
      rvtDoc.AutoJoinElements();

      return walls;

    }

    /// <summary>
    /// Add a door to the center of the given wall. 
    /// cf. Developer Guide p137. NewFamilyInstance() for Doors and Window. 
    /// </summary>

    public static void AddDoor(Document rvtDoc, Wall hostWall)
    {
      // Hard coding the door type we will use. 
      // e.g., "M_Single-Flush: 0915 x 2134mm 
      const string doorFamilyName = Util.Constant.DoorFamilyName;
      const string doorTypeName = Util.Constant.DoorTypeName;
      const string doorFamilyAndTypeName = doorFamilyName + ": " + doorTypeName;

      // Get the door type to use. 
      FamilySymbol doorType =
        ElementFiltering.FindFamilyType(
          rvtDoc, typeof(FamilySymbol), doorFamilyName, doorTypeName, BuiltInCategory.OST_Doors
        ) as FamilySymbol;
      if (doorType == null)
      {
        TaskDialog.Show(
          "Add door", "Cannot find (" + doorFamilyAndTypeName +
          "). Maybe you use a different template? Try with DefaultMetric.rte."
        );
      }
      if (!doorType.IsActive)
        doorType.Activate();

      // Get the start and end points of the wall. 
      LocationCurve locCurve = (LocationCurve)hostWall.Location;
      XYZ pt1 = locCurve.Curve.GetEndPoint(0);
      XYZ pt2 = locCurve.Curve.GetEndPoint(1);
      // Calculate the mid point. 
      XYZ pt = (pt1 + pt2) / 2.0;

      // One more thing - we want to set the reference as a bottom of the wall or level1. 
      ElementId idLevel1 = hostWall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId();
      //Level level1 = (Level)_doc.get_Element(idLevel1); // 2012
      Level level1 = rvtDoc.GetElement(idLevel1) as Level; // since 2013

      // Finally, create a door. 
      FamilyInstance aDoor = rvtDoc.Create.NewFamilyInstance(pt, doorType, hostWall, level1, StructuralType.NonStructural);

    }

    /// <summary>
    /// Add a window to the center of the wall given. 
    /// cf. Developer Guide p137. NewFamilyInstance() for Doors and Window. 
    /// Basically the same idea as a door except that we need to set sill hight. 
    /// </summary>

    public static void AddWindow(Document rvtDoc, Wall hostWall)
    {
      // Hard coding the window type we will use. 
      // e.g., "M_Fixed: 0915 x 1830mm 
      const string windowFamilyName = Util.Constant.WindowFamilyName;
      const string windowTypeName = Util.Constant.WindowTypeName;
      const string windowFamilyAndTypeName = windowFamilyName + ": " + windowTypeName;
      double sillHeight = Constant.MmToFeet(915);

      // Get the door type to use. 
      FamilySymbol windowType =
        ElementFiltering.FindFamilyType(
          rvtDoc, typeof(FamilySymbol), windowFamilyName, windowTypeName, BuiltInCategory.OST_Windows
        ) as FamilySymbol;
      if (windowType == null)
      {
        TaskDialog.Show(
          "Add window", "Cannot find (" + windowFamilyAndTypeName +
          "). Maybe you use a different template? Try with DefaultMetric.rte.");
      }

      if (!windowType.IsActive)
        windowType.Activate();

      // Get the start and end points of the wall. 
      LocationCurve locCurve = (LocationCurve)hostWall.Location;
      XYZ pt1 = locCurve.Curve.GetEndPoint(0);
      XYZ pt2 = locCurve.Curve.GetEndPoint(1);
      // Calculate the mid point. 
      XYZ pt = (pt1 + pt2) / 2.0;

      // One more thing - we want to set the reference as a bottom of the wall or level1. 
      ElementId idLevel1 = hostWall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId();

      //Level level1 = (Level)_doc.get_Element(idLevel1); // 2012
      Level level1 = rvtDoc.GetElement(idLevel1) as Level; // since 2013

      // Finally create a window. 
      FamilyInstance aWindow = rvtDoc.Create.NewFamilyInstance(pt, windowType, hostWall, level1, StructuralType.NonStructural);

      aWindow.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(sillHeight);

    }

    /// <summary>
    /// Add a roof over the rectangular profile of the walls we created earlier.
    /// </summary>
    public static void AddRoof(Document rvtDoc, List<Wall> walls)
    {
      // Hard coding the roof type we will use. 
      // e.g., "Basic Roof: Generic - 400mm"  
      const string roofFamilyName = "Basic Roof";
      const string roofTypeName = Util.Constant.RoofTypeName;
      const string roofFamilyAndTypeName = roofFamilyName + ": " + roofTypeName;

      // Find the roof type
      RoofType roofType =
        ElementFiltering.FindFamilyType(
          rvtDoc, typeof(RoofType), roofFamilyName, roofTypeName, null
        ) as RoofType;
      if (roofType == null)
      {
        TaskDialog.Show(
          "Add roof", "Cannot find (" + roofFamilyAndTypeName +
          "). Maybe you use a different template? Try with DefaultMetric.rte.");
      }

      // Wall thickness to adjust the footprint of the walls
      // to the outer most lines. 
      // Note: this may not be the best way. 
      // but we will live with this for this exercise. 
      //Dim wallThickness As Double = _
      //walls(0).WallType.CompoundStructure.Layers.Item(0).Thickness() ' 2011
      double wallThickness = walls[0].WallType.GetCompoundStructure().GetLayers()[0].Width;
      // 2012
      double dt = wallThickness / 2.0;
      List<XYZ> dts = new List<XYZ>(5);
      dts.Add(new XYZ(-dt, -dt, 0.0));
      dts.Add(new XYZ(dt, -dt, 0.0));
      dts.Add(new XYZ(dt, dt, 0.0));
      dts.Add(new XYZ(-dt, dt, 0.0));
      dts.Add(dts[0]);

      // Set the profile from four walls 
      CurveArray footPrint = new CurveArray();
      for (int i = 0; i <= 3; i++)
      {
        LocationCurve locCurve = (LocationCurve)walls[i].Location;
        XYZ pt1 = locCurve.Curve.GetEndPoint(0) + dts[i];
        XYZ pt2 = locCurve.Curve.GetEndPoint(1) + dts[i + 1];
        Line line = Line.CreateBound(pt1, pt2);
        footPrint.Append(line);
      }

      // Get the level2 from the wall.

      ElementId idLevel2 = walls[0].get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();

      //Level level2 = (Level)_doc.get_Element(idLevel2); // 2012
      Level level2 = rvtDoc.GetElement(idLevel2) as Level; // since 2013

      // Footprint to model curve mapping.

      ModelCurveArray mapping = new ModelCurveArray();

      // Create a roof.

      FootPrintRoof aRoof = rvtDoc.Create.NewFootPrintRoof(
        footPrint, level2, roofType, out mapping);

      // Set the slope.

      foreach (ModelCurve modelCurve in mapping)
      {
        aRoof.set_DefinesSlope(modelCurve, true);
        aRoof.set_SlopeAngle(modelCurve, 0.5);
      }

      // Performed automatically by transaction commit.
      //rvtDoc.Regenerate();
      //rvtDoc.AutoJoinElements();
    }
  }
}
