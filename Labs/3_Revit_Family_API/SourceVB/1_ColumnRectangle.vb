#Region "Copyright"
''
'' (C) Copyright 2009-2013 by Autodesk, Inc.
''
'' Permission to use, copy, modify, and distribute this software in
'' object code form for any purpose and without fee is hereby granted,
'' provided that the above copyright notice appears in all copies and
'' that both that copyright notice and the limited warranty and
'' restricted rights notice below appear in all supporting
'' documentation.
''
'' AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
'' AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
'' MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
'' DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
'' UNINTERRUPTED OR ERROR FREE.
''
'' Use, duplication, or disclosure by the U.S. Government is subject to
'' restrictions set forth in FAR 52.227-19 (Commercial Computer
'' Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
'' (Rights in Technical Data and Computer Software), as applicable.
''
'' Written by M.Harada 
''
#End Region

#Region "Imports"
'' Import the following name spaces in the project properties/references. 
'' Note: VB.NET has a slighly different way of recognizing name spaces than C#. 
'' if you explicitely set them in each .vb file, you will need to specify full name spaces. 

'Imports System.Linq  
'Imports Autodesk.Revit
'Imports Autodesk.Revit.DB
'Imports Autodesk.Revit.UI
'Imports Autodesk.Revit.ApplicationServices

#End Region

#Region "Description"
''' <summary>
''' Revit Family Creation API Lab - 1
'''
''' This command defines a minimum family, and creates a column family with a rectangular
''' profile with three types.
'''
''' Objective:
''' ----------
'''
''' In this lab, we learn the following:
'''
'''   0. set up family environment
'''   1. create a simple solid
'''   2. set alignment
'''   3. add types
'''
''' To test this lab, open a family template "Metric Column.rft", and run a command.
'''
''' Context:
''' --------
'''
''' In this lab, we will define a simple rectangular profile like the follow sketch:
'''
'''   3     2
'''    +---+
'''    |   | d    h = height
'''    +---+
'''   0     1
'''   4  w
'''
''' We then, create a box-shape solid using extrusion, align each face of the solid to
''' exisiting reference planes, and define three types with dimensional variations.
'''
''' Desclaimer: code in these labs is written for the purpose of learning the Revit family API.
''' In practice, there will be much room for performance and usability improvement.
''' For code readability, minimum error checking.
''' </summary>
#End Region

<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)> _
Public Class RvtCmd_FamilyCreateColumnRectangle
    Implements IExternalCommand

    '' member variables for top level access to the Revit database
    ''
  Dim _app As Application
  Dim _doc As Document

  ''  command main
  ''
  Public Function Execute( _
      ByVal commandData As ExternalCommandData, _
      ByRef message As String, _
      ByVal elements As ElementSet) _
      As Result _
      Implements IExternalCommand.Execute

    ''  objects for the top level access
    ''
    _app = commandData.Application.Application
    _doc = commandData.Application.ActiveUIDocument.Document

    '' (0) This command works in the context of family editor only.
    ''     We also check if the template is for an appropriate category if needed.
    ''     Here we use a Column(i.e., Metric Column.rft) template.
    ''     Although there is no specific checking about metric or imperial, our lab only works in metric for now.
    ''
    If Not isRightTemplate(BuiltInCategory.OST_Columns) Then
      MsgBox("Please open Metric Column.rft")
      Return Result.Failed
    End If

    '' (1) create a simple extrusion. just a simple box for now.
    Dim pSolid As Extrusion = createSolid()

    '' We need to regenerate so that we can build on this new geometry
    _doc.Regenerate()

    ''  try this:
    ''  if you comment addAlignment and addTypes calls below and execute only up to here,
    ''  you will see the column's top will not follow the upper level.

    '' (2) add alignment
    addAlignments(pSolid)

    ''  try this: at each stage of adding a function here, you should be able to see the result in UI.

    '' (3) add types
    addTypes()

    ''  finally, return
    Return Result.Succeeded

  End Function

  '' ============================================
  ''   (0) checks if we have a correct template
  '' ============================================
  Function isRightTemplate( _
      ByVal targetCategory As BuiltInCategory _
  ) As Boolean

    ''  This command works in the context of family editor only.
    ''
    If Not _doc.IsFamilyDocument Then
      MsgBox("This command works only in the family editor.")
      Return False
    End If

    ''  Check the template for an appropriate category here if needed.
    ''
    Dim cat As Category = _doc.Settings.Categories.Item(targetCategory)
    If _doc.OwnerFamily Is Nothing Then
      MsgBox("This command only works in the family context.")
      Return False
    End If
    If Not cat.Id.Equals(_doc.OwnerFamily.FamilyCategory.Id) Then
      MsgBox("Category of this family document does not match the context required by this command.")
      Return False
    End If

    ''  if we come here, we should have a right one.
    Return True

  End Function

  '' ============================================
  ''   (1) create a simple solid by extrusion
  '' ============================================
  Function createSolid() As Extrusion

    ''
    ''  (1) define a simple rectangular profile
    ''
    ''  3     2
    ''   +---+
    ''   |   | d    h = height
    ''   +---+
    ''  0     1
    ''  4  w
    ''
    Dim pProfile As CurveArrArray = createProfileRectangle()
    ''
    ''  (2) create a sketch plane
    ''
    ''  we need to know the template. If you look at the template (Metric Column.rft) and "Front" view,
    ''  you will see "Reference Plane" at "Lower Ref. Level". We are going to create an extrusion there.
    ''  findElement() is a helper function that find an element of the given type and name.  see below.
    ''
    Dim pRefPlane As ReferencePlane = findElement(GetType(ReferencePlane), "Reference Plane") ' need to know from the template
        'Dim pSketchPlane As SketchPlane = _doc.FamilyCreate.NewSketchPlane(pRefPlane.Plane)  ' Revit 2013
        Dim pSketchPlane As SketchPlane = SketchPlane.Create(_doc, pRefPlane.Plane)  ' Revit 2014

    ''  (3) height of the extrusion
    ''
    ''  once again, you will need to know your template. unlike UI, the alightment will not adjust the geometry.
    ''  You will need to have the exact location in order to set alignment.
    ''  Here we hard code for simplicity. 4000 is the distance between Lower and Upper Ref. Level.
    ''  as an exercise, try changing those values and see how it behaves.
    ''
    Dim dHeight As Double = mmToFeet(4000) '' distance between Lower and Upper Ref Level.

    ''  (4) create an extrusion here. at this point. just an box, nothing else.
    ''
    Dim bIsSolid As Boolean = True ' as oppose to void.
    Dim pSolid As Extrusion = _doc.FamilyCreate.NewExtrusion(bIsSolid, pProfile, pSketchPlane, dHeight)

    Return pSolid

  End Function

  '' ============================================
  ''   (1.1) create a simple rectangular profile
  '' ============================================
  Function createProfileRectangle() As CurveArrArray

    ''
    ''  define a simple rectangular profile
    ''
    ''  3     2
    ''   +---+
    ''   |   | d    h = height
    ''   +---+
    ''  0     1
    ''  4  w
    ''

    ''  sizes (hard coded for simplicity)
    ''  note: these need to match reference plane. otherwise, alignment won't work.
    ''  as an exercise, try changing those values and see how it behaves.
    ''
    Dim w As Double = mmToFeet(600) ' hard coded for simplicity here. in practice, you may want to find out from the references)
    Dim d As Double = mmToFeet(600)

    ''  define vertices
    ''
    Const nVerts As Integer = 4 '' the number of vertices
    Dim pts() As XYZ = {New XYZ(-w / 2, -d / 2, 0), New XYZ(w / 2, -d / 2, 0), New XYZ(w / 2, d / 2, 0), New XYZ(-w / 2, d / 2, 0), New XYZ(-w / 2, -d / 2, 0)} ' the last one is to make the loop simple

    ''  define a loop. define individual edges and put them in a curveArray
    ''
    Dim pLoop As CurveArray = _app.Create.NewCurveArray
    Dim lines(nVerts - 1) As Line
    For i As Integer = 0 To nVerts - 1
            'lines(i) = _app.Create.NewLineBound(pts(i), pts(i + 1))  ' Revit 2013
            lines(i) = Line.CreateBound(pts(i), pts(i + 1))  ' Revit 2014
      pLoop.Append(lines(i))
    Next

    ''  then, put the loop in the curveArrArray as a profile
    ''
    Dim pProfile As CurveArrArray = _app.Create.NewCurveArrArray
    pProfile.Append(pLoop)
    ''  if we come here, we have a profile now.

    Return pProfile

  End Function

  '' ============================================
  ''   (2) add alignments
  '' ============================================
  Sub addAlignments(ByVal pBox As Extrusion)

    ''
    ''  (1) we want to constrain the upper face of the column to the "Upper Ref Level"
    ''

    ''  which direction are we looking at?
    ''
    Dim pView As View = findElement(GetType(View), "Front")

    ''  find the upper ref level
    ''  findElement() is a helper function. see below.
    ''
    Dim upperLevel As Level = findElement(GetType(Level), "Upper Ref Level")
    Dim ref1 As Reference = upperLevel.PlaneReference

    ''  find the face of the box
    ''  findFace() is a helper function. see below.
    ''
    Dim upperFace As PlanarFace = findFace(pBox, New XYZ(0, 0, 1)) ' find a face whose normal is z-up.
    Dim ref2 As Reference = upperFace.Reference

    ''  create alignments
    ''
    _doc.FamilyCreate.NewAlignment(pView, ref1, ref2)

    ''
    ''  (2) do the same for the lower level
    ''

    ''  find the lower ref level
    ''  findElement() is a helper function. see below.
    ''
    Dim lowerLevel As Level = findElement(GetType(Level), "Lower Ref. Level")
    Dim ref3 As Reference = lowerLevel.PlaneReference

    ''  find the face of the box
    ''  findFace() is a helper function. see below.
    ''
    Dim lowerFace As PlanarFace = findFace(pBox, New XYZ(0, 0, -1)) ' find a face whose normal is z-down.
    Dim ref4 As Reference = lowerFace.Reference

    '' create alignments
    ''
    _doc.FamilyCreate.NewAlignment(pView, ref3, ref4)

    ''
    ''  (3)  same idea for the Right/Left/Front/Back
    ''
    ''  get the plan view
    ''  note: same name maybe used for different view types. either one should work.
    Dim pViewPlan As View = findElement(GetType(ViewPlan), "Lower Ref. Level")

    ''  find reference planes
    Dim refRight As ReferencePlane = findElement(GetType(ReferencePlane), "Right")
    Dim refLeft As ReferencePlane = findElement(GetType(ReferencePlane), "Left")
    Dim refFront As ReferencePlane = findElement(GetType(ReferencePlane), "Front")
    Dim refBack As ReferencePlane = findElement(GetType(ReferencePlane), "Back")

    ''  find the face of the box
    Dim faceRight As PlanarFace = findFace(pBox, New XYZ(1, 0, 0))
    Dim faceLeft As PlanarFace = findFace(pBox, New XYZ(-1, 0, 0))
    Dim faceFront As PlanarFace = findFace(pBox, New XYZ(0, -1, 0))
    Dim faceBack As PlanarFace = findFace(pBox, New XYZ(0, 1, 0))

    ''  create alignments
    ''
    _doc.FamilyCreate.NewAlignment(pViewPlan, refRight.Reference, faceRight.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refLeft.Reference, faceLeft.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refFront.Reference, faceFront.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refBack.Reference, faceBack.Reference)

  End Sub

  '' ============================================
  ''   (3) add types
  '' ============================================
  Sub addTypes()

    ''  addType(name, Width, Depth)
    ''
    addType("600x900", 600.0, 900.0)
    addType("1000x300", 1000.0, 300.0)
    addType("600x600", 600.0, 600.0)

  End Sub

  ''  add one type
  ''
  Sub addType(ByVal name As String, ByVal w As Double, ByVal d As Double)

    ''  get the family manager from the current doc
    Dim pFamilyMgr As FamilyManager = _doc.FamilyManager

    ''  add new types with the given name
    ''
    Dim type1 As FamilyType = pFamilyMgr.NewType(name)

    ''  look for 'Width' and 'Depth' parameters and set them to the given value
    ''
    ''  first 'Width'
    ''
    Dim paramW As FamilyParameter = pFamilyMgr.Parameter("Width")
    Dim valW As Double = mmToFeet(w)
    If paramW IsNot Nothing Then
      pFamilyMgr.Set(paramW, valW)
    End If

    ''  same idea for 'Depth'
    ''
    Dim paramD As FamilyParameter = pFamilyMgr.Parameter("Depth")
    Dim valD As Double = mmToFeet(d)
    If paramD IsNot Nothing Then
      pFamilyMgr.Set(paramD, valD)
    End If

  End Sub

  ''============================================
  ''
  ''  Helper functions
  ''
  ''============================================

#Region "Helper Functions"

  '' ============================================
  ''   helper function: find a planar face with the given normal
  '' ============================================
  Function findFace(ByVal pBox As Extrusion, ByVal normal As XYZ) As PlanarFace

    '' get the geometry object of the given element
    ''
    Dim op As New Options
    op.ComputeReferences = True
        Dim geomElem As GeometryElement = pBox.Geometry(op)

    '' loop through the array and find a face with the given normal
    ''
        For Each geomObj As GeometryObject In geomElem

            If TypeOf geomObj Is Solid Then  ''  solid is what we are interested in.

                Dim pSolid As Solid = geomObj
                Dim faces As FaceArray = pSolid.Faces

                For Each pFace As Face In faces
                    Dim pPlanarFace As PlanarFace = pFace
                    If Not (pPlanarFace Is Nothing) Then
                        If pPlanarFace.Normal.IsAlmostEqualTo(normal) Then '' we found the face
                            Return (pPlanarFace)
                        End If
                    End If
                Next

            ElseIf TypeOf geomObj Is GeometryInstance Then

                '' will come back later as needed.

            ElseIf TypeOf geomObj Is Curve Then

                '' will come nack later as needed.

            ElseIf TypeOf geomObj Is Mesh Then

                '' will come back later as needed.

            Else
                '' what else do we have?

            End If
        Next

    '' if we come here, we did not find any.
    Return Nothing

  End Function

  '' ==================================================================================
  ''   helper function: find an element of the given type and the name.
  ''   You can use this, for example, to find Reference or Level with the given name.
  '' ==================================================================================
  Function findElement(ByVal targetType As Type, ByVal targetName As String) As Element

    '' get the elements of the given type
    ''
    Dim collector = New FilteredElementCollector(_doc)
    collector.WherePasses(New ElementClassFilter(targetType))

    '' parse the collection for the given name
    '' using LINQ query here.   
    '' 
    Dim targetElems = From element In collector Where element.Name.Equals(targetName) Select element
    Dim elems As List(Of Element) = targetElems.ToList()

    If elems.Count > 0 Then '' we should have only one with the given name.  
      Return elems(0)
    End If

    '' cannot find it.
    Return Nothing

  End Function

  '' ============================================
  ''   convert millimeter to feet
  '' ============================================
  Function mmToFeet(ByVal mmVal As Double) As Double

    Return mmVal / 304.8 '' * 0.00328;

  End Function
#End Region

End Class
