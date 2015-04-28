#region Copyright
//
// (C) Copyright 2009-2015 by Autodesk, Inc.
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
// Migrated to C# by J. Tammik. Comment/description added by M.Harada 
//
#endregion // Copyright

#region Namespaces
//using System;
//using Autodesk.Revit;
//using Autodesk.Revit.Elements;
//using Autodesk.Revit.Enums;
//using Autodesk.Revit.Geometry;
//using Autodesk.Revit.Parameters;
//using RvtElement = Autodesk.Revit.Element;
//using GeoElement = Autodesk.Revit.Geometry.Element;

using System;
using System.Collections.Generic;
using System.Linq; // in System.Core 
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

#endregion // Namespaces

#region Description
/// <summary>
/// Revit Family Creation API Lab - 4
///
/// This command defines a column family, and creates a column family with a L-shape profile.
/// It adds visibility control.
///
/// Objective:
/// ----------
///
/// In the previous labs, we have learned the following:
///
///   0. set up family environment
///   1. create a solid
///   2. set alignment
///   3. add types
///   4. add reference planes
///   5. add parameters
///   6. add dimensions
///   7. add formula
///   8. add materials
///
/// In this lab, we will learn the following:
///
///   9. add visibility control
///
/// To test this lab, open a family template "Metric Column.rft", and run a command.
///
/// Context:
/// --------
///
/// In the previous rfa labs 1 to 3, we have defined a column family, using a L-shape profile.
///
///       5 Tw 4
///        +-+
///        | | 3          h = height
/// Depth  | +---+ 2
///        +-----+ Td
///       0        1
///       6  Width
///
/// in addition to what we have learned in the previous labs, we will do the following:
///
///   1. add visibility control so that we will have a line representation of a model in coarse view.
///
/// Desclaimer: code in these labs is written for the purpose of learning the Revit family API.
/// In practice, there will be much room for performance and usability improvement.
/// For code readability, minimum error checking.
/// </summary>
#endregion // Description

namespace FamilyCs
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class RvtCmd_FamilyCreateColumnVisibility : IExternalCommand
    {
        // member variables for top level access to the Revit database
        //
        Application _app;
        Document _doc;

        // command main
        //
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            // objects for the top level access
            //
            _app = commandData.Application.Application;
            _doc = commandData.Application.ActiveUIDocument.Document;

            // (0) This command works in the context of family editor only.
            //     We also check if the template is for an appropriate category if needed.
            //     Here we use a Column(i.e., Metric Column.rft) template.
            //     Although there is no specific checking about metric or imperial, our lab only works in metric for now.
            //
            if (!isRightTemplate(BuiltInCategory.OST_Columns))
            {
                Util.ErrorMsg("Please open Metric Column.rft");
                return Result.Failed;
            }
            using (Transaction transaction = new Transaction(_doc))
            {
                try
                {
                    if (transaction.Start("CreateFamily") == TransactionStatus.Started)
                    {
                        // (1.1) add reference planes
                        addReferencePlanes();

                        // (1.2) create a simple extrusion. We create a L-shape extrusion.
                        Extrusion pSolid = createSolid();
                        _doc.Regenerate();

                        // (2) add alignment
                        addAlignments(pSolid);

                        // (3.1) add parameters
                        addParameters();

                        // (3.2) add dimensions
                        addDimensions();

                        // (3.3) add types
                        addTypes();

                        // (4.1) add formulas
                        addFormulas();

                        // (4.2) add materials
                        addMaterials(pSolid);

                        // (5.1) add visibilities
                        addLineObjects();
                        changeVisibility(pSolid);
                        transaction.Commit();
                    }
                    else
                    {
                        TaskDialog.Show("ERROR", "Start transaction failed!");
                        return Result.Failed;
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("ERROR", ex.ToString());
                    if (transaction.GetStatus() == TransactionStatus.Started)
                        transaction.RollBack();
                    return Result.Failed;
                }
            }
            // finally return
            return Result.Succeeded;
        }

        // ============================================
        //   (0) check if we have a correct template
        // ============================================
        bool isRightTemplate(BuiltInCategory targetCategory)
        {
            // This command works in the context of family editor only.
            //
            if (!_doc.IsFamilyDocument)
            {
                Util.ErrorMsg("This command works only in the family editor.");
                return false;
            }

            // Check the template for an appropriate category here if needed.
            //
            Category cat = _doc.Settings.Categories.get_Item(targetCategory);
            if (_doc.OwnerFamily == null)
            {
                Util.ErrorMsg("This command only works in the family context.");
                return false;
            }
            if (!cat.Id.Equals(_doc.OwnerFamily.FamilyCategory.Id))
            {
                Util.ErrorMsg("Category of this family document does not match the context required by this command.");
                return false;
            }

            // if we come here, we should have a right one.
            return true;
        }

        // ==============================
        //   (1.1) add reference planes
        // ==============================
        void addReferencePlanes()
        {
            //
            // we are defining a simple L-shape profile like the following:
            //
            //  5 tw 4
            //   +-+
            //   | | 3          h = height
            // d | +---+ 2
            //   +-----+ td
            //  0        1
            //  6  w
            //
            //
            // we want to add ref planes along (1) 2-3 and (2)3-4.
            // Name them "OffsetH" and "OffsetV" respectively. (H for horizontal, V for vertical).
            //
            double tw = mmToFeet(150.0);  // thickness added for Lab2.  Hard-coding for simplicity.
            double td = mmToFeet(150.0);

            //
            // (1) add a horizonal ref plane 2-3.
            //
            // get a plan view
            View pViewPlan = (View)findElement(typeof(ViewPlan), "Lower Ref. Level");

            // we have predefined ref plane: Left/Right/Front/Back
            // get the ref plane at Front, which is aligned to line 2-3
            ReferencePlane refFront = (ReferencePlane)findElement(typeof(ReferencePlane), "Front");

            // get the bubble and free ends from front ref plane and offset by td.
            //
            XYZ p1 = refFront.BubbleEnd;
            XYZ p2 = refFront.FreeEnd;
            XYZ pBubbleEnd = new XYZ(p1.X, p1.Y + td, p1.Z);
            XYZ pFreeEnd = new XYZ(p2.X, p2.Y + td, p2.Z);

            // create a new one reference plane and name it "OffsetH"
            //
            ReferencePlane refPlane = _doc.FamilyCreate.NewReferencePlane(pBubbleEnd, pFreeEnd, XYZ.BasisZ, pViewPlan);
            refPlane.Name = "OffsetH";

            //
            // (2) do the same to add a vertical reference plane.
            //

            // find the ref plane at left, which is aligned to line 3-4
            ReferencePlane refLeft = (ReferencePlane)findElement(typeof(ReferencePlane), "Left");

            // get the bubble and free ends from front ref plane and offset by td.
            //
            p1 = refLeft.BubbleEnd;
            p2 = refLeft.FreeEnd;
            pBubbleEnd = new XYZ(p1.X + tw, p1.Y, p1.Z);
            pFreeEnd = new XYZ(p2.X + tw, p2.Y, p2.Z);

            // create a new reference plane and name it "OffsetV"
            //
            refPlane = _doc.FamilyCreate.NewReferencePlane(pBubbleEnd, pFreeEnd, XYZ.BasisZ, pViewPlan);
            refPlane.Name = "OffsetV";
        }

        // =================================================================
        //   (1.2) create a simple solid by extrusion with L-shape profile
        // =================================================================
        Extrusion createSolid()
        {
            //
            // (1) define a simple L-shape profile
            //
            CurveArrArray pProfile = createProfileLShape();

            //
            // (2) create a sketch plane
            //
            // we need to know the template. If you look at the template (Metric Column.rft) and "Front" view,
            // you will see "Reference Plane" at "Lower Ref. Level". We are going to create an extrusion there.
            // findElement() is a helper function that find an element of the given type and name.  see below.
            //
            ReferencePlane pRefPlane = findElement(typeof(ReferencePlane), "Reference Plane") as ReferencePlane;  // need to know from the template
            //SketchPlane pSketchPlane = _doc.FamilyCreate.NewSketchPlane(pRefPlane.Plane);  // Revit 2013
            //SketchPlane pSketchPlane = SketchPlane.Create(_doc, pRefPlane.Plane);  // Revit 2014
            SketchPlane pSketchPlane = SketchPlane.Create(_doc, pRefPlane.GetPlane());  // Revit 2016

            // (3) height of the extrusion
            //
            // same as profile, you will need to know your template. unlike UI, the alightment will not adjust the geometry.
            // You will need to have the exact location in order to set alignment.
            // Here we hard code for simplicity. 4000 is the distance between Lower and Upper Ref. Level.
            // as an exercise, try changing those values and see how it behaves.
            //
            double dHeight = mmToFeet(4000.0);

            // (4) create an extrusion here. at this point. just an box, nothing else.
            //
            bool bIsSolid = true;  // as oppose to void.
            return _doc.FamilyCreate.NewExtrusion(bIsSolid, pProfile, pSketchPlane, dHeight);
        }

        // ===========================================
        //   (1.2a) create a simple L-shaped profile
        // ===========================================
        CurveArrArray createProfileLShape()
        {
            //
            // define a simple L-shape profile
            //
            //  5 tw 4
            //   +-+
            //   | | 3          h = height
            // d | +---+ 2
            //   +-----+ td
            //  0        1
            //  6  w
            //

            // sizes (hard coded for simplicity)
            // note: these need to match reference plane. otherwise, alignment won't work.
            // as an exercise, try changing those values and see how it behaves.
            //
            double w = mmToFeet(600.0); // those are hard coded for simplicity here. in practice, you may want to find out from the references)
            double d = mmToFeet(600.0);
            double tw = mmToFeet(150.0); // thickness added for Lab2
            double td = mmToFeet(150.0);

            // define vertices
            //
            const int nVerts = 6; // the number of vertices

            XYZ[] pts = new XYZ[] {
        new XYZ(-w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, (-d / 2.0) + td, 0.0),
        new XYZ((-w / 2.0) + tw, (-d / 2.0) + td, 0.0),
        new XYZ((-w / 2.0) + tw, d / 2.0, 0.0),
        new XYZ(-w / 2.0, d / 2.0, 0.0),
        new XYZ(-w / 2.0, -d / 2.0, 0.0) }; // the last one is to make the loop simple

            // define a loop. define individual edges and put them in a curveArray
            //
            CurveArray pLoop = _app.Create.NewCurveArray();
            for (int i = 0; i < nVerts; ++i)
            {
                //Line line = _app.Create.NewLineBound(pts[i], pts[i + 1]);  // Revit 2013
                Line line = Line.CreateBound(pts[i], pts[i + 1]);  // Revit 2014
                pLoop.Append(line);
            }

            // then, put the loop in the curveArrArray as a profile
            //
            CurveArrArray pProfile = _app.Create.NewCurveArrArray();
            pProfile.Append(pLoop);
            // if we come here, we have a profile now.

            return pProfile;
        }

        // ==============================================
        //   (1.2b) create a simple rectangular profile
        // ==============================================
        CurveArrArray createProfileRectangle()
        {
            //
            // define a simple rectangular profile
            //
            //  3     2
            //   +---+
            //   |   | d    h = height
            //   +---+
            //  0     1
            //  4  w
            //

            // sizes (hard coded for simplicity)
            // note: these need to match reference plane. otherwise, alignment won't work.
            // as an exercise, try changing those values and see how it behaves.
            //
            double w = mmToFeet(600.0);
            double d = mmToFeet(600.0);

            // define vertices
            //
            const int nVerts = 4; // the number of vertices

            XYZ[] pts = new XYZ[] {
        new XYZ(-w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, d / 2.0, 0.0),
        new XYZ(-w / 2.0, d / 2.0, 0.0),
        new XYZ(-w / 2.0, -d / 2.0, 0.0) };

            // define a loop. define individual edges and put them in a curveArray
            //
            CurveArray pLoop = _app.Create.NewCurveArray();
            for (int i = 0; i < nVerts; ++i)
            {
                //Line line = _app.Create.NewLineBound(pts[i], pts[i + 1]);  // Revit 2013
                Line line = Line.CreateBound(pts[i], pts[i + 1]);  // Revit 2014
                pLoop.Append(line);
            }

            // then, put the loop in the curveArrArray as a profile
            //
            CurveArrArray pProfile = _app.Create.NewCurveArrArray();
            pProfile.Append(pLoop);
            // if we come here, we have a profile now.

            return pProfile;
        }

        // ======================================
        //   (2.1) add alignments
        // ======================================
        void addAlignments(Extrusion pBox)
        {
            //
            // (1) we want to constrain the upper face of the column to the "Upper Ref Level"
            //

            // which direction are we looking at?
            //
            View pView = findElement(typeof(View), "Front") as View;

            // find the upper ref level
            // findElement() is a helper function. see below.
            //
            Level upperLevel = findElement(typeof(Level), "Upper Ref Level") as Level;
            Reference ref1 = upperLevel.GetPlaneReference();

            // find the face of the box
            // findFace() is a helper function. see below.
            //
            PlanarFace upperFace = findFace(pBox, new XYZ(0.0, 0.0, 1.0)); // find a face whose normal is z-up.
            Reference ref2 = upperFace.Reference;

            // create alignments
            //
            _doc.FamilyCreate.NewAlignment(pView, ref1, ref2);

            //
            // (2) do the same for the lower level
            //

            // find the lower ref level
            // findElement() is a helper function. see below.
            //
            Level lowerLevel = findElement(typeof(Level), "Lower Ref. Level") as Level;
            Reference ref3 = lowerLevel.GetPlaneReference();

            // find the face of the box
            // findFace() is a helper function. see below.
            PlanarFace lowerFace = findFace(pBox, new XYZ(0.0, 0.0, -1.0)); // find a face whose normal is z-down.
            Reference ref4 = lowerFace.Reference;

            // create alignments
            //
            _doc.FamilyCreate.NewAlignment(pView, ref3, ref4);

            //
            // (3)  same idea for the Right/Left/Front/Back
            //
            // get the plan view
            // note: same name maybe used for different view types. either one should work.
            View pViewPlan = findElement(typeof(ViewPlan), "Lower Ref. Level") as View;

            // find reference planes
            ReferencePlane refRight = findElement(typeof(ReferencePlane), "Right") as ReferencePlane;
            ReferencePlane refLeft = findElement(typeof(ReferencePlane), "Left") as ReferencePlane;
            ReferencePlane refFront = findElement(typeof(ReferencePlane), "Front") as ReferencePlane;
            ReferencePlane refBack = findElement(typeof(ReferencePlane), "Back") as ReferencePlane;
            ReferencePlane refOffsetV = findElement(typeof(ReferencePlane), "OffsetV") as ReferencePlane; // added for L-shape
            ReferencePlane refOffsetH = findElement(typeof(ReferencePlane), "OffsetH") as ReferencePlane; // added for L-shape

            // find the face of the box
            // Note: findFace need to be enhanced for this as face normal is not enough to determine the face.
            //
            PlanarFace faceRight = findFace(pBox, new XYZ(1.0, 0.0, 0.0), refRight); // modified for L-shape
            PlanarFace faceLeft = findFace(pBox, new XYZ(-1.0, 0.0, 0.0));
            PlanarFace faceFront = findFace(pBox, new XYZ(0.0, -1.0, 0.0));
            PlanarFace faceBack = findFace(pBox, new XYZ(0.0, 1.0, 0.0), refBack); // modified for L-shape
            PlanarFace faceOffsetV = findFace(pBox, new XYZ(1.0, 0.0, 0.0), refOffsetV); // added for L-shape
            PlanarFace faceOffsetH = findFace(pBox, new XYZ(0.0, 1.0, 0.0), refOffsetH); // added for L-shape

            // create alignments
            //
            _doc.FamilyCreate.NewAlignment(pViewPlan, refRight.GetReference(), faceRight.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refLeft.GetReference(), faceLeft.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refFront.GetReference(), faceFront.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refBack.GetReference(), faceBack.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refOffsetV.GetReference(), faceOffsetV.Reference); // added for L-shape
            _doc.FamilyCreate.NewAlignment(pViewPlan, refOffsetH.GetReference(), faceOffsetH.Reference); // added for L-shape
        }

        // ======================================
        //   (3.1) add parameters
        // ======================================
        void addParameters()
        {
            FamilyManager mgr = _doc.FamilyManager;

            // API parameter group for Dimension is PG_GEOMETRY:
            //
            FamilyParameter paramTw = mgr.AddParameter(
              "Tw", BuiltInParameterGroup.PG_GEOMETRY,
              ParameterType.Length, false);

            FamilyParameter paramTd = mgr.AddParameter(
              "Td", BuiltInParameterGroup.PG_GEOMETRY,
              ParameterType.Length, false);

            // set initial values:
            //
            double tw = mmToFeet(150.0);
            double td = mmToFeet(150.0);
            mgr.Set(paramTw, tw);
            mgr.Set(paramTd, td);

            // (2)  add a parameter for material finish
            // we are adding material arameter in addMaterials function. 
            // See addMaterials for the actual implementation.
            //
        }

        // ======================================
        //   (3.2) add dimensions
        // ======================================
        void addDimensions()
        {
            // find the plan view
            //
            View pViewPlan = findElement(typeof(ViewPlan), "Lower Ref. Level") as View;

            // find reference planes
            //
            ReferencePlane refLeft = findElement(typeof(ReferencePlane), "Left") as ReferencePlane;
            ReferencePlane refFront = findElement(typeof(ReferencePlane), "Front") as ReferencePlane;
            ReferencePlane refOffsetV = findElement(typeof(ReferencePlane), "OffsetV") as ReferencePlane; // OffsetV is added for L-shape
            ReferencePlane refOffsetH = findElement(typeof(ReferencePlane), "OffsetH") as ReferencePlane; // OffsetH is added for L-shape

            //
            // (1)  add dimension between the reference planes 'Left' and 'OffsetV', and label it as 'Tw'
            //

            // define a dimension line
            //
            XYZ p0 = refLeft.FreeEnd;
            XYZ p1 = refOffsetV.FreeEnd;
            //Line pLine = _app.Create.NewLineBound(p0, p1);  // Revit 2013
            Line pLine = Line.CreateBound(p0, p1);

            // define references
            //
            ReferenceArray pRefArray = new ReferenceArray();
            pRefArray.Append(refLeft.GetReference());
            pRefArray.Append(refOffsetV.GetReference());

            // create a dimension
            //
            Dimension pDimTw = _doc.FamilyCreate.NewDimension(pViewPlan, pLine, pRefArray);

            // add label to the dimension
            //
            FamilyParameter paramTw = _doc.FamilyManager.get_Parameter("Tw");
            //pDimTw.Label = paramTw;  // Revit 2013
            pDimTw.FamilyLabel = paramTw;  // Revit 2014

            //
            // (2)  do the same for dimension between 'Front' and 'OffsetH', and lable it as 'Td'
            //

            // define a dimension line
            //
            p0 = refFront.FreeEnd;
            p1 = refOffsetH.FreeEnd;
            //pLine = _app.Create.NewLineBound(p0, p1); // Revit 2013
            pLine = Line.CreateBound(p0, p1);  // Revit 2014

            // define references
            //
            pRefArray = new ReferenceArray();
            pRefArray.Append(refFront.GetReference());
            pRefArray.Append(refOffsetH.GetReference());

            // create a dimension
            //
            Dimension pDimTd = _doc.FamilyCreate.NewDimension(pViewPlan, pLine, pRefArray);

            // add label to the dimension
            //
            FamilyParameter paramTd = _doc.FamilyManager.get_Parameter("Td");
            //pDimTd.Label = paramTd;  // Revit 2013
            pDimTd.FamilyLabel = paramTd;
        }

        // ======================================
        //   (3.3) add types
        // ======================================
        void addTypes()
        {
            // addType(name, Width, Depth)
            //
            //addType("600x900", 600.0, 900.0)
            //addType("1000x300", 1000.0, 300.0)
            //addType("600x600", 600.0, 600.0)

            // addType(name, Width, Depth, Tw, Td)
            //
            addType("600x900", 600.0, 900.0, 150.0, 225.0);
            addType("1000x300", 1000.0, 300.0, 250.0, 75.0);
            addType("600x600", 600.0, 600.0, 150.0, 150.0);
        }

        // add one type (version 2)
        //
        void addType(string name, double w, double d, double tw, double td)
        {
            // get the family manager from the current doc
            FamilyManager pFamilyMgr = _doc.FamilyManager;

            // add new types with the given name
            //
            FamilyType type1 = pFamilyMgr.NewType(name);

            // look for 'Width' and 'Depth' parameters and set them to the given value
            //

            // first 'Width'
            //
            FamilyParameter paramW = pFamilyMgr.get_Parameter("Width");
            double valW = mmToFeet(w);
            if (paramW != null)
            {
                pFamilyMgr.Set(paramW, valW);
            }

            // same idea for 'Depth'
            //
            FamilyParameter paramD = pFamilyMgr.get_Parameter("Depth");
            double valD = mmToFeet(d);
            if (paramD != null)
            {
                pFamilyMgr.Set(paramD, valD);
            }

            // let's set "Tw' and 'Td'
            //
            FamilyParameter paramTw = pFamilyMgr.get_Parameter("Tw");
            double valTw = mmToFeet(tw);
            if (paramTw != null)
            {
                pFamilyMgr.Set(paramTw, valTw);
            }
            FamilyParameter paramTd = pFamilyMgr.get_Parameter("Td");
            double valTd = mmToFeet(td);
            if (paramTd != null)
            {
                pFamilyMgr.Set(paramTd, valTd);
            }
        }

        // add one type (version 1)
        //
        void addType(string name, double w, double d)
        {
            // get the family manager from the current doc
            FamilyManager pFamilyMgr = _doc.FamilyManager;

            // add new types with the given name
            //
            FamilyType type1 = pFamilyMgr.NewType(name);

            // look for 'Width' and 'Depth' parameters and set them to the given value
            //
            // first 'Width'
            //
            FamilyParameter paramW = pFamilyMgr.get_Parameter("Width");
            double valW = mmToFeet(w);
            if (paramW != null)
            {
                pFamilyMgr.Set(paramW, valW);
            }

            // same idea for 'Depth'
            //
            FamilyParameter paramD = pFamilyMgr.get_Parameter("Depth");
            double valD = mmToFeet(d);
            if (paramD != null)
            {
                pFamilyMgr.Set(paramD, valD);
            }
        }

        // ======================================
        //  (4.1) add formula
        // ======================================
        public void addFormulas()
        {
            // we will add the following fomulas
            //   Tw = Width / 4.0
            //   Td = Depth / 4.0
            //

            // first get the parameter
            FamilyManager pFamilyMgr = _doc.FamilyManager;

            FamilyParameter paramTw = pFamilyMgr.get_Parameter("Tw");
            FamilyParameter paramTd = pFamilyMgr.get_Parameter("Td");

            // set the formula
            pFamilyMgr.SetFormula(paramTw, "Width / 4.0");
            pFamilyMgr.SetFormula(paramTd, "Depth / 4.0");
        }

        // ======================================
        //  (4.2) add materials
        // ======================================
        //
        // in Revit 2010, you cannot modify asset.
        // SPR# 155053 - WishList: Ability to access\modify properties in Render Appearance of Materials using API.
        // To Do in future: you can extend this functionality to create a new one in future.
        //
        // This function is only for reference.  this is not used. If use this way, it is fixed at solid level. You cannot change it.
        //
        public void addMaterialsToSolid(Extrusion pSolid)
        {
            // We assume Material type "Glass" exists. Template "Metric Column.rft" include "Glass",
            // which in fact is the only interesting one to see the effect.
            // In practice, you will want to include in your template.
            //
            // To Do: For the exersize, create it with more appropriate ones in UI, then use the name here.
            //
            Material pMat = findElement(typeof(Material), "Glass") as Material;
            if (pMat != null)
            {
                // no material with the given name.
                ElementId idMat = pMat.Id;

                // 'Autodesk.Revit.DB.Element.get_Parameter(string)' is obsolete 
                // 'This property is obsolete in Revit 2015, 
                //Parameter paramMat = pSolid.get_Parameter("Material");

                //pSolid.get_Parameter("Material").Set(idMat);

                /// Updated for Revit 2015

                pSolid.LookupParameter("Material").Set(idMat);
            }
        }

        public void addMaterials(Extrusion pSolid)
        {
            // We assume Material type "Glass" exists. Template "Metric Column.rft" include "Glass",
            // which in fact is the only interesting one to see the effect.
            // In practice, you will want to include in your template.
            //
            // To Do: For the exersize, create it with more appropriate ones in UI, then use the name here.
            //

            // (1)  get the materials id that we are intersted in (e.g., "Glass")
            //
            Material pMat = findElement(typeof(Material), "Glass") as Material;
            if (pMat != null)
            {
                ElementId idMat = pMat.Id;
                // (2a) this add a material to the solid base.  but then, we cannot change it for each column.
                //
                //pSolid.Parameter("Material").Set(idMat)

                // (2b) add a parameter for material finish
                //
                // this time we use instance parameter so that we can change it at instance level.
                //
                FamilyManager pFamilyMgr = _doc.FamilyManager;
                FamilyParameter famParamFinish = pFamilyMgr.AddParameter("ColumnFinish", BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, true);

                // (2b.1) associate material parameter to the family parameter we just added
                //

                // 'Autodesk.Revit.DB.Element.get_Parameter(string)' is obsolete 
                // 'This property is obsolete in Revit 2015, 
                //Parameter paramMat = pSolid.get_Parameter("Material");

                /// Updated for Revit 2015

                Parameter paramMat = pSolid.LookupParameter("Material");
                pFamilyMgr.AssociateElementParameterToFamilyParameter(paramMat, famParamFinish);

                // (2b.2) for our convenience, let's add another type with Glass finish
                //
                addType("Glass", 600.0, 600.0);
                pFamilyMgr.Set(famParamFinish, idMat);
            }
        }

        // =====================================================================
        //  (5.1.1) create simple line objects to be displayed in coarse level
        // =====================================================================
        public void addLineObjects()
        {
            //
            // define a simple L-shape detail line object
            //
            //  0
            //   +        h = height
            //   |        (we also want to draw a vertical line here at point 1)
            // d |
            //   +-----+
            //  1       2
            //      w
            //

            // sizes
            double w = mmToFeet(600.0); // modified to match reference plane. otherwise, alignment won't work.
            double d = mmToFeet(600.0);
            double h = mmToFeet(4000.0); // distance between Lower and Upper Ref Level.
            double t = mmToFeet(50.0); // slight offset for visbility

            // define vertices
            //
            XYZ[] pts = new XYZ[] { new XYZ((-w / 2.0) + t, d / 2.0, 0.0), new XYZ((-w / 2.0) + t, (-d / 2.0) + t, 0.0), new XYZ(w / 2.0, (-d / 2.0) + t, 0.0) };
            XYZ ptH = new XYZ((-w / 2.0) + t, (-d / 2.0) + t, h);  // this is for vertical line.

            //
            // (2) create a sketch plane
            //
            // we need to know the template. If you look at the template (Metric Column.rft) and "Front" view,
            // you will see "Reference Plane" at "Lower Ref. Level". We are going to create a sketch plane there.
            // findElement() is a helper function that find an element of the given type and name.  see below.
            // Note: we did the same in creating a profile.
            //
            ReferencePlane pRefPlane = findElement(typeof(ReferencePlane), "Reference Plane") as ReferencePlane;
            //SketchPlane pSketchPlane = _doc.FamilyCreate.NewSketchPlane(pRefPlane.Plane);  // Revit 2013
            //SketchPlane pSketchPlane = SketchPlane.Create(_doc, pRefPlane.Plane);  // Revit 2014
            SketchPlane pSketchPlane = SketchPlane.Create(_doc, pRefPlane.GetPlane());  // Revit 2016

            // for vertical line, we draw a straight vertical line at the point[1] 
            XYZ normal = new XYZ(1.0, 0.0, 0.0);
            Plane pGeomPlaneH = _app.Create.NewPlane(normal, pts[1]);
            //SketchPlane pSketchPlaneH = _doc.FamilyCreate.NewSketchPlane(pGeomPlaneH);  // Revit 2013
            SketchPlane pSketchPlaneH = SketchPlane.Create(_doc, pGeomPlaneH);  // Revit 2014

            // (3) create line objects: two symbolic curves on a plan and one model curve representing a column like a vertical stick.
            //
            // Revit 2013
            //Line geomLine1 = _app.Create.NewLine(pts[0], pts[1], true);
            //Line geomLine2 = _app.Create.NewLine(pts[1], pts[2], true);
            //Line geomLineH = _app.Create.NewLine(pts[1], ptH, true);

            // Revit 2014
            Line geomLine1 = Line.CreateBound(pts[0], pts[1]);
            Line geomLine2 = Line.CreateBound(pts[1], pts[2]);
            //Line geomLineH = _app.Create.NewLine(pts[1], ptH, true); // Revit 2013
            Line geomLineH = Line.CreateBound(pts[1], ptH);

            SymbolicCurve pLine1 = _doc.FamilyCreate.NewSymbolicCurve(geomLine1, pSketchPlane);
            SymbolicCurve pLine2 = _doc.FamilyCreate.NewSymbolicCurve(geomLine2, pSketchPlane);

            ModelCurve pLineH = _doc.FamilyCreate.NewModelCurve(geomLineH, pSketchPlaneH); // this is vertical line

            // set the visibilities of two lines to coarse only
            //
            FamilyElementVisibility pVis = new FamilyElementVisibility(FamilyElementVisibilityType.ViewSpecific);
            pVis.IsShownInFine = false;
            pVis.IsShownInMedium = false;

            pLine1.SetVisibility(pVis);
            pLine2.SetVisibility(pVis);

            FamilyElementVisibility pVisH = new FamilyElementVisibility(FamilyElementVisibilityType.Model);
            pVisH.IsShownInFine = false;
            pVisH.IsShownInMedium = false;

            pLineH.SetVisibility(pVisH);
        }

        // =================================================================
        //  (5.1.2) set the visibility of the solid not to show in coarse
        // =================================================================
        public void changeVisibility(Extrusion pSolid)
        {
            // set the visibility of the model not to shown in coarse.
            //
            FamilyElementVisibility pVis = new FamilyElementVisibility(FamilyElementVisibilityType.Model);
            pVis.IsShownInCoarse = false;

            pSolid.SetVisibility(pVis);
        }

        #region Helper Functions

        // =============================================================================================
        //  helper function: given a solid, find a planar face with the given normal (version 2)
        //  this is a slightly enhaced version of previous version and checks if the face is on the given reference plane.
        // =============================================================================================
        PlanarFace findFace(Extrusion pBox, XYZ normal, ReferencePlane refPlane)
        {
            // get the geometry object of the given element
            //
            Options op = new Options();
            op.ComputeReferences = true;
            GeometryElement geomElem = pBox.get_Geometry(op);

            // loop through the array and find a face with the given normal
            //
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid) // solid is what we are interested in.
                {
                    Solid pSolid = geomObj as Solid;
                    FaceArray faces = pSolid.Faces;
                    foreach (Face pFace in faces)
                    {
                        PlanarFace pPlanarFace = pFace as PlanarFace;
                        // check to see if they have same normal
                        if ((pPlanarFace != null) && pPlanarFace.FaceNormal.IsAlmostEqualTo(normal))
                        {
                            // additionally, we want to check if the face is on the reference plane
                            //
                            XYZ p0 = refPlane.BubbleEnd;
                            XYZ p1 = refPlane.FreeEnd;
                            // Line pCurve = _app.Create.NewLineBound(p0, p1); // Revit 2013
                            Line pCurve = Line.CreateBound(p0, p1);  // Revit 2014
                            if (pPlanarFace.Intersect(pCurve) == SetComparisonResult.Subset)
                            {
                                return pPlanarFace; // we found the face
                            }
                        }
                    }
                }

                // will come back later as needed.
                //
                //else if (geomObj is Instance)
                //{
                //}
                //else if (geomObj is Curve)
                //{
                //}
                //else if (geomObj is Mesh)
                //{
                //}
            }

            // if we come here, we did not find any.
            return null;
        }

        // =============================================================
        //  helper function: find a planar face with the given normal
        // =============================================================
        PlanarFace findFace(Extrusion pBox, XYZ normal)
        {
            // get the geometry object of the given element
            //
            Options op = new Options();
            op.ComputeReferences = true;
            GeometryElement geomElem = pBox.get_Geometry(op);

            // loop through the array and find a face with the given normal
            //
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)  // solid is what we are interested in.
                {
                    Solid pSolid = (Solid)geomObj;
                    FaceArray faces = pSolid.Faces;
                    foreach (Face pFace in faces)
                    {
                        PlanarFace pPlanarFace = (PlanarFace)pFace;
                        if ((pPlanarFace != null) && pPlanarFace.FaceNormal.IsAlmostEqualTo(normal)) // we found the face
                        {
                            return pPlanarFace;
                        }
                    }
                }

                // will come back later as needed.
                //
                //else if (geomObj is Instance)
                //{
                //}
                //else if (geomObj is Curve)
                //{
                //}
                //else if (geomObj is Mesh)
                //{
                //}
            }

            // if we come here, we did not find any.
            return null;
        }

        // ==================================================================================
        //  helper function: find an element of the given type and the name.
        //  You can use this, for example, to find Reference or Level with the given name.
        // ==================================================================================
        Element findElement(Type targetType, string targetName)
        {
            // get the elements of the given type
            //
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            collector.WherePasses(new ElementClassFilter(targetType));

            // parse the collection for the given name
            // using LINQ query here. 
            // 
            var targetElems = from element in collector where element.Name.Equals(targetName) select element;
            List<Element> elems = targetElems.ToList<Element>();

            if (elems.Count > 0)
            {  // we should have only one with the given name. 
                return elems[0];
            }

            // cannot find it.
            return null;
        }

        // ===============================================
        //   helper function: convert millimeter to feet
        // ===============================================
        double mmToFeet(double mmVal)
        {
            return mmVal / 304.8;
        }

        #endregion // Helper Functions
    }
}
