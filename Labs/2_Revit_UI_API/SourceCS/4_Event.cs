#region Copyright
//
// Copyright (C) 2009-2015 by Autodesk, Inc.
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
// Migrated to C# by Saikat Bhattacharya
// 
#endregion // Copyright

#region Namespaces

using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes; // specify this if you want to save typing for attributes. e.g.
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

#endregion

namespace UiCs
{
  /// <summary>
  /// Event 
  /// 
  /// cf. Developer Guide, Section 24 Event(pp278) - list of events you can subscribe 
  /// Appexdix G. API User Interface Guidelines (pp381), Task Dialog (pp404) 
  /// 
  /// External application to register/unregister document changed event. 
  /// Simply reports what has been changed  
  /// </summary>
  [Transaction(TransactionMode.Automatic)]
  public class UIEventApp : IExternalApplication
  {
    // Flag to indicate if we want to show a message at each object modified events. 
    public static bool m_showEvent = false;

    /// <summary>
    /// OnShutdown() - called when Revit ends. 
    /// </summary>
    public Result OnShutdown(UIControlledApplication app)
    {
      // (1) unregister our document changed event hander 
      app.ControlledApplication.DocumentChanged -= UILabs_DocumentChanged;

      return Result.Succeeded;
    }

    /// <summary>
    /// OnStartup() - called when Revit starts. 
    /// </summary>
    public Result OnStartup(UIControlledApplication app)
    {
      // (1) resgister our document changed event hander 
      app.ControlledApplication.DocumentChanged += UILabs_DocumentChanged;

      // (2) register our dynamic model updater (WindowDoorUpdater class definition below.) 
      // We are going to keep doors and windows at the center of the wall. 
      // 
      // Construct our updater. 
      WindowDoorUpdater winDoorUpdater = new WindowDoorUpdater(app.ActiveAddInId);
      // ActiveAddInId is from addin menifest. 
      // Register it 
      UpdaterRegistry.RegisterUpdater(winDoorUpdater);

      // Tell which elements we are interested in being notified about. 
      // We want to know when wall changes its length. 

      ElementClassFilter wallFilter = new ElementClassFilter(typeof(Wall));
      UpdaterRegistry.AddTrigger(winDoorUpdater.GetUpdaterId(), wallFilter, Element.GetChangeTypeGeometry());

      return Result.Succeeded;
    }

    /// <summary>
    /// This is our event handler. Simply report the list of element ids which have been changed. 
    /// </summary>
    public void UILabs_DocumentChanged(object sender, DocumentChangedEventArgs args)
    {
      if (!m_showEvent) return;

      // You can get the list of ids of element added/changed/modified. 
      Document rvtdDoc = args.GetDocument();

      ICollection<ElementId> idsAdded = args.GetAddedElementIds();
      ICollection<ElementId> idsDeleted = args.GetDeletedElementIds();
      ICollection<ElementId> idsModified = args.GetModifiedElementIds();

      // Put it in a string to show to the user. 
      string msg = "Added: ";
      foreach (ElementId id in idsAdded)
      {
        msg += id.IntegerValue.ToString() + " ";
      }

      msg += "\nDeleted: ";
      foreach (ElementId id in idsDeleted)
      {
        msg += id.IntegerValue.ToString() + " ";
      }

      msg += "\nModified: ";
      foreach (ElementId id in idsModified)
      {
        msg += id.IntegerValue.ToString() + " ";
      }

      // Show a message to a user. 
      TaskDialogResult res = default(TaskDialogResult);
      res = TaskDialog.Show("Revit UI Labs - Event", msg, TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel);

      // If the user chooses to cancel, show no more event. 
      if (res == TaskDialogResult.Cancel)
      {
        m_showEvent = false;
      }
    }
  }

  /// <summary>
  /// External command to toggle event message on/off 
  /// </summary> 
  [Transaction(TransactionMode.Automatic)]
  public class UIEvent : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIEventApp.m_showEvent = !UIEventApp.m_showEvent;

      return Result.Succeeded;
    }

  }

  [Transaction(TransactionMode.Automatic)]
  public class UIEventOn : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIEventApp.m_showEvent = true;

      return Result.Succeeded;
    }
  }

  [Transaction(TransactionMode.Automatic)]
  public class UIEventOff : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIEventApp.m_showEvent = false;

      return Result.Succeeded;
    }
  }

  //======================================================== 
  // dynamic model update - derive from IUpdater class 
  //======================================================== 

  public class WindowDoorUpdater : IUpdater
  {
    // Unique id for this updater = addin GUID + GUID for this specific updater. 
    UpdaterId m_updaterId = null;

    // Flag to indicate if we want to perform 
    public static bool m_updateActive = false;

    /// <summary>
    /// Constructor 
    /// </summary>
    public WindowDoorUpdater(AddInId id)
    {
      m_updaterId = new UpdaterId(id, new Guid("EF43510F-38CB-4980-844C-72174A674D56"));
    }

    /// <summary>
    /// This is the main function to do the actual job. 
    /// For this exercise, we assume that we want to keep the door and window always at the center. 
    /// </summary>
    public void Execute(UpdaterData data)
    {
      if (!m_updateActive) return;

      Document rvtDoc = data.GetDocument();
      ICollection<ElementId> idsModified = data.GetModifiedElementIds();

      foreach (ElementId id in idsModified)
      {
        //  Wall aWall = rvtDoc.get_Element(id) as Wall; // For 2012
        Wall aWall = rvtDoc.GetElement(id) as Wall; // For 2013
        CenterWindowDoor(rvtDoc, aWall);
      }
    }

    /// <summary>
    /// Helper function for Execute. 
    /// Checks if there is a door or a window on the given wall. 
    /// If it does, adjust the location to the center of the wall. 
    /// For simplicity, we assume there is only one door or window. 
    /// (TBD: or evenly if there are more than one.) 
    /// </summary>
    public void CenterWindowDoor(Document rvtDoc, Wall aWall)
    {
      // Find a winow or a door on the wall. 
      FamilyInstance e = FindWindowDoorOnWall(rvtDoc, aWall);
      if (e == null) return;

      // Move the element (door or window) to the center of the wall. 

      // Center of the wall

      LocationCurve wallLocationCurve = aWall.Location as LocationCurve;

      //XYZ pt1 = wallLocationCurve.Curve.get_EndPoint( 0 ); // 2013
      //XYZ pt2 = wallLocationCurve.Curve.get_EndPoint( 1 ); // 2013
      XYZ pt1 = wallLocationCurve.Curve.GetEndPoint( 0 ); // 2014
      XYZ pt2 = wallLocationCurve.Curve.GetEndPoint( 1 ); // 2014
      
      XYZ midPt = (pt1 + pt2) * 0.5;

      LocationPoint loc = e.Location as LocationPoint;
      loc.Point = new XYZ(midPt.X, midPt.Y, loc.Point.Z);
    }

    /// <summary>
    /// Helper function 
    /// Find a door or window on the given wall. 
    /// If it does, return it. 
    /// </summary>
    public FamilyInstance FindWindowDoorOnWall(Document rvtDoc, Wall aWall)
    {
      // Collect the list of windows and doors 
      // No object relation graph. So going hard way. 
      // List all the door instances 
      var windowDoorCollector = new FilteredElementCollector(rvtDoc);
      windowDoorCollector.OfClass(typeof(FamilyInstance));

      ElementCategoryFilter windowFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
      ElementCategoryFilter doorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
      LogicalOrFilter windowDoorFilter = new LogicalOrFilter(windowFilter, doorFilter);

      windowDoorCollector.WherePasses(windowDoorFilter);
      IList<Element> windowDoorList = windowDoorCollector.ToElements();

      // This is really bad in a large model!
      // You might have ten thousand doors and windows.
      // It would make sense to add a bounding box containment or intersection filter as well.

      // Check to see if the door or window is on the wall we got. 
      foreach (FamilyInstance e in windowDoorList)
      {
        if (e.Host.Id.Equals(aWall.Id))
        {
          return e;
        }
      }

      // If you come here, you did not find window or door on the given wall. 

      return null;
    }

    /// <summary>
    /// This will be shown when the updater is not loaded. 
    /// </summary>
    public string GetAdditionalInformation()
    {
      return "Door/Window updater: keeps doors and windows at the center of walls.";
    }

    /// <summary>
    /// Specify the order of executing updaters. 
    /// </summary>
    public ChangePriority GetChangePriority()
    {
      return ChangePriority.DoorsOpeningsWindows;
    }

    /// <summary>
    /// Return updater id. 
    /// </summary>
    public UpdaterId GetUpdaterId()
    {
      return m_updaterId;
    }

    /// <summary>
    /// User friendly name of the updater 
    /// </summary>
    public string GetUpdaterName()
    {
      return "Window/Door Updater";
    }
  }

  /// <summary>
  /// External command to toggle windowDoor updater on/off 
  /// </summary> 
  [Transaction(TransactionMode.Automatic)]
  public class UIDynamicModelUpdate : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      if (WindowDoorUpdater.m_updateActive)
      {
        WindowDoorUpdater.m_updateActive = false;
      }
      else
      {
        WindowDoorUpdater.m_updateActive = true;
      }
      return Result.Succeeded;
    }
  }

  [Transaction(TransactionMode.Automatic)]
  public class UIDynamicModelUpdateOn : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      WindowDoorUpdater.m_updateActive = true;

      return Result.Succeeded;
    }
  }

  [Transaction(TransactionMode.Automatic)]
  public class UIDynamicModelUpdateOff : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      WindowDoorUpdater.m_updateActive = false;

      return Result.Succeeded;
    }
  }
}
