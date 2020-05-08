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
// Migrated to C# by Saikat Bhattacharya
// 
#endregion // Copyright

#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Util;
using System.Collections;
#endregion

namespace UiCs
{
  /// <summary>
  /// User Selection 
  /// 
  /// Note: This exercise uses Revit Into Labs. 
  /// Modify your project setting to place the dlls from both labs in one place.  
  /// 
  /// cf. Developer Guide, Section 7: Selection (pp 89) 
  /// </summary>
  [Transaction(TransactionMode.ReadOnly)]
  public class UISelection : IExternalCommand
  {
    // Member variables
    UIApplication _uiApp;
    UIDocument _uiDoc;

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      // Get access to the top most objects. (we may not use them all in this specific lab.) 
      _uiApp = commandData.Application;
      _uiDoc = _uiApp.ActiveUIDocument;

      // (1) pre-seleceted element is under UIDocument.Selection.Elemens. Classic method. 
      // You can also modify this selection set. 

      //SelElementSet selSet = _uiDoc.Selection.Elements;
      //ShowElementList(selectedElementIds, "Pre-selection: ");

      // 'Autodesk.Revit.UI.Selection.SelElementSet' is obsolete: 
      // 'This class is deprecated in Revit 2015. Use Selection.SetElementIds() 
      // and Selection.GetElementIds() instead.'

      // 'Autodesk.Revit.UI.Selection.Selection.Elements' is obsolete: 
      // 'This property is deprecated in Revit 2015. 
      // Use GetElementIds() and SetElementIds instead.'

      /// Following part is modified code for Revit 2015
      /// 

      ICollection<ElementId> selectedElementIds = _uiDoc.Selection.GetElementIds();

      // Display current number of selected elements
      TaskDialog.Show("Revit", "Number of selected elements: " + selectedElementIds.Count.ToString());

      // We need to re-write the following function 

      ShowElementList(selectedElementIds, "Pre-selection: ");


      /// End of modified code for Revit 2015     



      try
      {
        // (2.1) pick methods basics. 
        // there are four types of pick methods: PickObject, PickObjects, PickElementByRectangle, PickPoint. 
        // Let's quickly try them out. 

        PickMethodsBasics();

        // (2.2) selection object type 
        // in addition to selecting objects of type Element, the user can pick faces, edges, and point on element. 

        PickFaceEdgePoint();

        // (2.3) selection filter 
        // if you want additional selection criteria, such as only to pick a wall, you can use selection filter. 

        ApplySelectionFilter();
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException)
      {
        TaskDialog.Show("UI selection", "You have canceled selection.");
      }
      catch (Exception)
      {
        TaskDialog.Show("UI selection", "Some other exception caught in CancelSelection()");
      }

      // (2.4) canceling selection 
      // when the user cancel or press [Esc] key during the selection, OperationCanceledException will be thrown. 

      CancelSelection();

      // (3) apply what we learned to our small house creation 
      // we put it as a separate command. See at the bottom of the code. 
      // CreateHouseUI

      return Result.Succeeded;
    }

    /// <summary>
    /// Show basic information about the given element. 
    /// </summary>
    public void ShowBasicElementInfo(Element e)
    {
      // Let's see what kind of element we got. 
      string s = "You picked: \n";

      s += ElementToString(e);

      // Show what we got. 

      TaskDialog.Show("Revit UI Lab", s);
    }

    /// <summary>
    /// Pick methods sampler. 
    /// Quickly try: PickObject, PickObjects, PickElementByRectangle, PickPoint. 
    /// Without specifics about objects we want to pick. 
    /// </summary>
    public void PickMethodsBasics()
    {
      // (1) Pick Object (we have done this already. But just for the sake of completeness.) 
      PickMethod_PickObject();

      // (2) Pick Objects 
      PickMethod_PickObjects();

      // (3) Pick Element By Rectangle 
      PickMethod_PickElementByRectangle();

      // (4) Pick Point 
      PickMethod_PickPoint();
    }

    /// <summary>
    /// Minimum PickObject 
    /// </summary>
    public void PickMethod_PickObject()
    {
      Reference r = _uiDoc.Selection.PickObject(ObjectType.Element, "Select one element");
      //Element e = r.Element; // 2011
      Element e = _uiDoc.Document.GetElement(r); // 2012

      ShowBasicElementInfo(e);
    }

    /// <summary>
    /// Minimum PickObjects 
    /// Note: when you run this code, you will see "Finish" and "Cancel" buttons in the dialog bar. 
    /// </summary>
    public void PickMethod_PickObjects()
      {
            IList<Reference> refs = _uiDoc.Selection.PickObjects(ObjectType.Element, "Select multiple elemens");

            // Put it in a List form. 
            IList<ElementId> elems = new List<ElementId>();
            foreach (Reference r in refs)
            {
                //elems.Add( r.Element ); // 2011 Warning: 'Autodesk.Revit.DB.Reference.Element' is obsolete: 
                // 'Property will be removed. Use Document.GetElement(Reference) instead'
                elems.Add(r.ElementId); // 2012
            }

            ShowElementList(elems, "Pick Objects: ");
      }


    /// <summary>
    /// Minimum PickElementByRectangle 
    /// </summary>
    public void PickMethod_PickElementByRectangle()
      {
            // Note: PickElementByRectangle returns the list of element. not reference. 
            IList<Element> elems = _uiDoc.Selection.PickElementsByRectangle("Select by rectangle");
            IList<ElementId> eids = new List<ElementId>();
            foreach(Element e in elems)
            {
                eids.Add(e.Id);
            }
            // Show it. 

            ShowElementList(eids, "Pick By Rectangle: ");
      }


    /// <summary>
    /// Minimum PickPoint 
    /// </summary>
    public void PickMethod_PickPoint()
    {
      XYZ pt = _uiDoc.Selection.PickPoint("Pick a point");

      // Show it. 
      string msg = "Pick Point: ";
      msg += PointToString(pt);

      TaskDialog.Show("PickPoint", msg);
    }

    /// <summary>
    /// Pick face, edge, point on an element 
    /// objectType options is applicable to PickObject() and PickObjects() 
    /// </summary>
    public void PickFaceEdgePoint()
    {
      // (1) Face 
      PickFace();

      // (2) Edge 
      PickEdge();

      // (3) Point 
      PickPointOnElement();
    }

    public void PickFace()
    {
      Reference r = _uiDoc.Selection.PickObject(ObjectType.Face, "Select a face");
      Element e = _uiDoc.Document.GetElement(r);

      //Face oFace = r.GeometryObject as Face; // 2011
      Face oFace = e.GetGeometryObjectFromReference(r) as Face; // 2012

      string msg = "";
      if (oFace != null)
      {
        msg = "You picked the face of element " + e.Id.ToString() + "\r\n";
      }
      else
      {
        msg = "no Face picked \n";
      }

      TaskDialog.Show("PickFace", msg);
    }

    public void PickEdge()
    {
      Reference r = _uiDoc.Selection.PickObject(ObjectType.Edge, "Select an edge");
      Element e = _uiDoc.Document.GetElement(r);
      //Edge oEdge = r.GeometryObject as Edge; // 2011
      Edge oEdge = e.GetGeometryObjectFromReference(r) as Edge; // 2012

      // Show it. 
      string msg = "";
      if (oEdge != null)
      {
        msg = "You picked an edge of element " + e.Id.ToString() + "\r\n";
      }
      else
      {
        msg = "no Edge picked \n";
      }

      TaskDialog.Show("PickEdge", msg);
    }

    public void PickPointOnElement()
    {
      Reference r = _uiDoc.Selection.PickObject(
        ObjectType.PointOnElement,
        "Select a point on element");

      Element e = _uiDoc.Document.GetElement(r);
      XYZ pt = r.GlobalPoint;

      string msg = "";
      if (pt != null)
      {
        msg = "You picked the point " + PointToString(pt) + " on an element " + e.Id.ToString() + "\r\n";
      }
      else
      {
        msg = "no Point picked \n";
      }

      TaskDialog.Show("PickPointOnElement", msg);
    }

    /// <summary>
    /// Pick with selection filter 
    /// Let's assume we only want to pick up a wall. 
    /// </summary>
    public void ApplySelectionFilter()
    {
      // Pick only a wall 
      PickWall();

      // Pick only a planar face. 
      PickPlanarFace();
    }

    /// <summary>
    /// Selection with wall filter. 
    /// See the bottom of the page to see the selection filter implementation. 
    /// </summary>
    public void PickWall()
    {
      SelectionFilterWall selFilterWall = new SelectionFilterWall();
      Reference r = _uiDoc.Selection.PickObject(ObjectType.Element, selFilterWall, "Select a wall");

      // Show it
      Element e = _uiDoc.Document.GetElement(r);

      ShowBasicElementInfo(e);
    }

    /// <summary>
    /// Selection with planar face. 
    /// See the bottom of the page to see the selection filter implementation. 
    /// </summary>
    public void PickPlanarFace()
    {
      // To call ISelectionFilter.AllowReference, use this. 
      // This will limit picked face to be planar. 
      Document doc = _uiDoc.Document;
      SelectionFilterPlanarFace selFilterPlanarFace = new SelectionFilterPlanarFace(doc);
      Reference r = _uiDoc.Selection.PickObject(ObjectType.Face, selFilterPlanarFace, "Select a planar face");
      Element e = doc.GetElement(r);
      //Face oFace = r.GeometryObject as Face; // 2011
      Face oFace = e.GetGeometryObjectFromReference(r) as Face; // 2012

      string msg = (null == oFace)
        ? "No face picked."
        : "You picked a face on element " + e.Id.ToString();

      TaskDialog.Show("PickPlanarFace", msg);
    }

    /// <summary>
    /// Canceling selection 
    /// When the user presses [Esc] key during the selection, OperationCanceledException will be thrown. 
    /// </summary>
    public void CancelSelection()
    {
      try
      {
        Reference r = _uiDoc.Selection.PickObject(ObjectType.Element, "Select an element, or press [Esc] to cancel");
        Element e = _uiDoc.Document.GetElement(r);

        ShowBasicElementInfo(e);
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException)
      {
        TaskDialog.Show("CancelSelection", "You canceled the selection.");
      }
      catch (Exception ex)
      {
        TaskDialog.Show("CancelSelection", "Other exception caught in CancelSelection(): " + ex.Message);
      }
    }

    #region "Helper Function"
    //==================================================================== 
    // Helper Functions 
    //==================================================================== 

    /// <summary>
    /// Helper function to display info from a list of elements passed onto. 
    /// (Same as Revit Intro Lab3.) 
    /// </summary>
    /// 

    // Following code snippet works for Revit 2014 / 2013
    //public void ShowElementList(IEnumerable elems, string header)
    //{
    //  string s = "\n\n - Class - Category - Name (or Family: Type Name) - Id - " + "\r\n";

    //  int count = 0;
    //  foreach (Element e in elems)
    //  {
    //    count++;
    //    s += ElementToString(e);
    //  }

    //  s = header + "(" + count + ")" + s;

    //  TaskDialog.Show("Revit UI Lab", s);
    //}

    /// Changing this in Revit 2015  
    /// 
    public void ShowElementList(IEnumerable elemIds, string header)
    {
      string s = "\n\n - Class - Category - Name (or Family: Type Name) - Id - " + "\r\n";

      int count = 0;
      foreach (ElementId eId in elemIds)
      {
        count++;
        Element e = _uiDoc.Document.GetElement(eId);
        s += ElementToString(e);
      }

      s = header + "(" + count + ")" + s;

      TaskDialog.Show("Revit UI Lab", s);
    }

    /// end of Changing in Revit 2015

    /// <summary>
    /// Helper function: summarize an element information as a line of text, 
    /// which is composed of: class, category, name and id. 
    /// Name will be "Family: Type" if a given element is ElementType. 
    /// Intended for quick viewing of list of element, for example. 
    /// (Same as Revit Intro Lab3.) 
    /// </summary>
    public string ElementToString(Element e)
    {
      if (e == null)
      {
        return "none";
      }

      string name = "";

      if (e is ElementType)
      {
        Parameter param = e.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
        if (param != null)
        {
          name = param.AsString();
        }
      }
      else
      {
        name = e.Name;
      }

      return e.GetType().Name + "; " + e.Category.Name + "; " + name + "; " + e.Id.IntegerValue.ToString() + "\r\n";
    }

    /// <summary>
    /// Helper Function: returns XYZ in a string form. 
    /// (Same as Revit Intro Lab2) 
    /// </summary>
    public static string PointToString(XYZ pt)
    {
      if (pt == null)
      {
        return "";
      }

      return "(" + pt.X.ToString("F2") + ", " + pt.Y.ToString("F2") + ", " + pt.Z.ToString("F2") + ")";
    }
    #endregion
  }

  /// <summary>
  /// Selection filter that limit the type of object being picked as wall. 
  /// </summary>
  class SelectionFilterWall : ISelectionFilter
  {
    public bool AllowElement(Element e)
    {
      return e is Wall;
    }

    public bool AllowReference(Reference reference, XYZ position)
    {
      return true;
    }
  }

  /// <summary>
  /// Selection filter that limit the reference type to be planar face 
  /// </summary>
  class SelectionFilterPlanarFace : ISelectionFilter
  {
    Document _doc;

    public SelectionFilterPlanarFace(Document doc)
    {
      _doc = doc;
    }

    public bool AllowElement(Element e)
    {
      return true;
    }

    public bool AllowReference(Reference r, XYZ position)
    {
      // Example: if you want to allow only planar faces 
      // and do some more checking, add this:

      // Optimal geometry object access in ISelectionFilter.AllowReference:
      //
      // In 2012 we get the warning 'Property GeometryObject As Autodesk.Revit.DB.GeometryObject is obsolete: Property will be removed. Use Element.GetGeometryObjectFromReference(Reference) instead'.
      // C:\a\doc\revit\blog\draft\geometry_object_access_in_ISelectionFilter_AllowReference.htm

      //if( r.GeometryObject is PlanarFace ) // 2011

      ElementId id = r.ElementId;
      //Element e = _doc.get_Element(id); // For 2012
      Element e = _doc.GetElement(id); // For 2013

      if (e.GetGeometryObjectFromReference(r) is PlanarFace)
      {
        // Do additional checking here if needed

        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Create House with UI added 
  /// 
  /// Ask the user to pick two corner points of walls
  /// then ask to choose a wall to add a front door. 
  /// </summary>
  [Transaction(TransactionMode.Manual)]
  public class UICreateHouse : IExternalCommand
  {
    UIApplication _uiApp;
    UIDocument _uiDoc;
    Document _doc;

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      // Get the access to the top most objects. (we may not use them all in this specific lab.) 
      _uiApp = commandData.Application;
      _uiDoc = _uiApp.ActiveUIDocument;
      _doc = _uiDoc.Document;

    //  using (Transaction transaction = new Transaction(_doc))
      {
     //   transaction.Start("Create House");
        CreateHouseInteractive(_uiDoc);
     //   transaction.Commit();
      }

      return Result.Succeeded;
    }

    /// <summary>
    /// Create a simple house with user interactions. 
    /// The user is asked to pick two corners of rectangluar footprint of a house, 
    /// then which wall to place a front door. 
    /// </summary>
    public static void CreateHouseInteractive(UIDocument uiDoc)
    {
      using (Transaction transaction = new Transaction(uiDoc.Document))
      {
        transaction.Start("Create House interactive");
        // (1) Walls 
        // Pick two corners to place a house with an orthogonal rectangular footprint 
        XYZ pt1 = uiDoc.Selection.PickPoint("Pick the first corner of walls");
        XYZ pt2 = uiDoc.Selection.PickPoint("Pick the second corner");

        // Simply create four walls with orthogonal rectangular profile from the two points picked. 
        List<Wall> walls = IntroCs.ModelCreationExport.CreateWalls(uiDoc.Document, pt1, pt2);

        // (2) Door 
        // Pick a wall to add a front door to
        SelectionFilterWall selFilterWall = new SelectionFilterWall();
        Reference r = uiDoc.Selection.PickObject(ObjectType.Element, selFilterWall, "Select a wall to place a front door");
        Wall wallFront = uiDoc.Document.GetElement(r) as Wall;

        // Add a door to the selected wall 
        IntroCs.ModelCreationExport.AddDoor(uiDoc.Document, wallFront);

        // (3) Windows 
        // Add windows to the rest of the walls. 
        for (int i = 0; i <= 3; i++)
        {
          if (!(walls[i].Id.IntegerValue == wallFront.Id.IntegerValue))
          {
            IntroCs.ModelCreationExport.AddWindow(uiDoc.Document, walls[i]);
          }
        }

        // (4) Roofs 
        // Add a roof over the walls' rectangular profile. 

        IntroCs.ModelCreationExport.AddRoof(uiDoc.Document, walls);
        transaction.Commit();
      }
    }
  }
}
