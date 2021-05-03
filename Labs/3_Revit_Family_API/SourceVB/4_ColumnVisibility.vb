#Region "Copyright"
''
'' (C) Copyright 2009-2021 by Autodesk, Inc.
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

'Imports System.Linq '' this is in System.Core
'Imports Autodesk.Revit
'Imports Autodesk.Revit.DB
'Imports Autodesk.Revit.UI
'Imports Autodesk.Revit.ApplicationServices

#End Region

#Region "Description"
''' <summary>
''' Revit Family Creation API Lab - 4
'''
''' This command defines a column family, and creates a column family with a L-shape profile.
''' It adds visibility control.
'''
''' Objective:
''' ----------
'''
''' In the previous labs, we have learned the following:
'''
'''   0. set up family environment
'''   1. create a solid
'''   2. set alignment
'''   3. add types
'''   4. add reference planes
'''   5. add parameters
'''   6. add dimensions
'''   7. add formula
'''   8. add materials
'''
''' In this lab, we will learn the following:
'''   9. add visibility control
'''
''' To test this lab, open a family template "Metric Column.rft", and run a command.
'''
''' Context:
''' --------
'''
''' In the previous rfa labs (lab1~3), we have defined a column family, using a L-shape profile.
'''
'''       5 Tw 4
'''        +-+
'''        | | 3          h = height
''' Depth  | +---+ 2
'''        +-----+ Td
'''      0        1
'''      6  Width
'''
''' in addition to what we have learned in the previous labs, we will do the following:
'''   1. add visibility control so that we will have a line representation of a model in coarse view.
'''
''' Desclaimer: code in these labs is written for the purpose of learning the Revit family API.
''' In practice, there will be much room for performance and usability improvement.
''' For code readability, minimum error checking.
''' </summary>
#End Region

<Transaction(TransactionMode.Manual)>
Public Class RvtCmd_FamilyCreateColumnVisibility
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

    _app = commandData.Application.Application
    _doc = commandData.Application.ActiveUIDocument.Document

    ''  (0) This command works in the context of family editor only.
    ''      We also check if the template is for an appropriate category if needed.
    ''      Here we use a Column(i.e., Metric Column.rft) template.
    ''      Although there is no specific checking about metric or imperial, our lab only works in metric for now.
    ''
    If Not isRightTemplate(BuiltInCategory.OST_Columns) Then
      MsgBox("Please open Metric Column.rft")
      Return Result.Failed
    End If

    Using transaction As Transaction = New Transaction(_doc)
      Try
        If transaction.Start("CreateFamily") <> TransactionStatus.Started Then
          TaskDialog.Show("ERROR", "Start transaction failed!")
          Return Result.Failed
        End If
        ''  (1.1) add reference planes
        ''
        addReferencePlanes()

        ''  (1.2) create a simple extrusion. This time we create a L-shape.
        Dim pSolid As Extrusion = createSolid()
        _doc.Regenerate()

        '' try this:
        '' if you comment alignment code below and execute only up to here,
        '' you will see the column's top will not follow the upper level.

        ''  (2) add alignment
        addAlignments(pSolid)

        ''  (3.1) add parameters
        ''
        addParameters()

        ''  (3.2) add dimensions
        ''
        addDimensions()

        ''  (3.3) add types
        ''
        addTypes()

        ''  (4.1) add formula
        ''
        addFormulas()

        ''  (4.2) add materials
        ''
        addMaterials(pSolid)

        ''  (5.1) add visibilities
        ''
        addLineObjects()
        changeVisibility(pSolid)
        transaction.Commit()
      Catch ex As Exception
        TaskDialog.Show("ERROR", ex.ToString())
        If transaction.GetStatus() = TransactionStatus.Started Then
          transaction.RollBack()
        End If
        Return Result.Failed
      End Try
    End Using
    ' finally, return
    Return Result.Succeeded

  End Function

  '' ============================================
  ''   (0) checks if we have a correct template
  '' ============================================
  Function isRightTemplate(ByVal targetCategory As BuiltInCategory) As Boolean

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
  ''   (1.1) add reference planes
  '' ============================================
  Sub addReferencePlanes()

    ''
    ''  we are defining a simple L-shaped profile like the following:
    ''
    ''  5 tw 4
    ''   +-+
    ''   | | 3          h = height
    '' d | +---+ 2
    ''   +-----+ td
    ''  0        1
    ''  6  w
    ''
    ''
    ''  we want to add ref planes along (1) 2-3 and (2)3-4.
    ''  Name them "OffsetH" and "OffsetV" respectively. (H for horizontal, V for vertical).
    ''
    ''
    Dim tw As Double = mmToFeet(150) ' thickness added for Fam Lab2. Hard-coding for simplicity.
    Dim td As Double = mmToFeet(150)

    ''
    '' (1) add a horizonal ref plane 2-3.
    ''
    ''  I don't quite understand the definition of bubble and free end.  Need to ask eng team
    ''  (some explanation from Case 1242566)
    ''
    ''  get a plan view
    Dim pViewPlan As View = findElement(GetType(ViewPlan), "Lower Ref. Level")

    ''  we have predefined ref plane: front/back/left/right
    ''  get the ref plane at front, which is aligned to line 2-3
    Dim refFront As ReferencePlane = findElement(GetType(ReferencePlane), "Front")

    ''  get the bubble and free ends from front ref plane and offset by td.
    ''
    Dim p1 As XYZ = refFront.BubbleEnd
    Dim p2 As XYZ = refFront.FreeEnd
    Dim pBubbleEnd As New XYZ(p1.X, p1.Y + td, p1.Z)
    Dim pFreeEnd As New XYZ(p2.X, p2.Y + td, p2.Z)

    ''  create the new one
    ''
    Dim refPlane As ReferencePlane = _doc.FamilyCreate.NewReferencePlane(pBubbleEnd, pFreeEnd, XYZ.BasisZ, pViewPlan)
    refPlane.Name = "OffsetH"

    ''
    '' (2) do the same to add a vertical ref plane.
    ''

    ''  find the ref plane at left, which is aligned to line 3-4
    Dim refLeft As ReferencePlane = findElement(GetType(ReferencePlane), "Left")

    ''  get the bubble and free ends from front ref plane and offset by td.
    ''
    p1 = refLeft.BubbleEnd
    p2 = refLeft.FreeEnd
    pBubbleEnd = New XYZ(p1.X + tw, p1.Y, p1.Z)
    pFreeEnd = New XYZ(p2.X + tw, p2.Y, p2.Z)

    ''  create the new one
    ''
    refPlane = _doc.FamilyCreate.NewReferencePlane(pBubbleEnd, pFreeEnd, XYZ.BasisZ, pViewPlan)
    refPlane.Name = "OffsetV"

  End Sub

  '' ============================================
  ''   (1.2) create a simple solid by extrusion with L-shape profile
  '' ============================================
  Function createSolid() As Extrusion

    ''
    ''  (1) define a simple L-shape profile
    ''
    'Dim pProflie As CurveArrArray = createBox()
    Dim pProfile As CurveArrArray = createProfileLShape() '' Lab2

    ''
    ''  (2) create a sketch plane
    ''
    ''  we need to know the template. If you look at the template (Metric Column.rft) and "Front" view,
    ''  you will see "Reference Plane" at "Lower Ref. Level". We are going to create an extrusion there.
    ''  findElement() is a helper function that find an element of the given type and name.  see below.
    ''
    Dim pRefPlane As ReferencePlane = findElement(GetType(ReferencePlane), "Reference Plane") ' need to know from the template
    'Dim pSketchPlane As SketchPlane = _doc.FamilyCreate.NewSketchPlane(pRefPlane.Plane)  ' Revit 2013
    'Dim pSketchPlane As SketchPlane = SketchPlane.Create(_doc, pRefPlane.Plane)  ' Revit 2014
    Dim pSketchPlane As SketchPlane = SketchPlane.Create(_doc, pRefPlane.GetPlane())  ' Revit 2016

    ''  (3) height of the extrusion
    ''
    ''  same as profile, you will need to know your template. unlike UI, the alightment will not adjust the geometry.
    ''  You will need to have the exact location in order to set alignment.
    ''  Here we hard code for simplicity. 4000 is the distance between Lower and Upper Ref. Level.
    ''
    Dim dHeight As Double = mmToFeet(4000) '' distance between Lower and Upper Ref Level.

    ''  (4) create an extrusion here. at this point. just an box, nothing else.
    ''
    Dim bIsSolid As Boolean = True ' as oppose to void.
    Dim pSolid As Extrusion = _doc.FamilyCreate.NewExtrusion(bIsSolid, pProfile, pSketchPlane, dHeight)

    Return pSolid

  End Function

  '' ============================================
  ''   (1.2a) create a simple L-shaped profile
  '' ============================================
  Function createProfileLShape() As CurveArrArray

    ''
    ''  define a simple L-shaped profile
    ''
    ''  5 tw 4
    ''   +-+
    ''   | | 3          h = height
    '' d | +---+ 2
    ''   +-----+ td
    ''  0        1
    ''  6  w
    ''

    ''  sizes (hard coded for simplicity)
    ''  note: these need to match reference plane. otherwise, alignment won't work.
    ''  as an exercise, try changing those values and see how it behaves.
    ''
    Dim w As Double = mmToFeet(600)  '' those are hard coded for simplicity here. in practice, you may want to find out from the references)
    Dim d As Double = mmToFeet(600)
    Dim tw As Double = mmToFeet(150) '' thickness added for Lab2
    Dim td As Double = mmToFeet(150)

    ''  define vertices
    ''
    Const nVerts As Integer = 6 '' the number of vertices
    Dim pts() As XYZ = {New XYZ(-w / 2, -d / 2, 0), New XYZ(w / 2, -d / 2, 0), New XYZ(w / 2, -d / 2 + td, 0), _
                        New XYZ(-w / 2 + tw, -d / 2 + td, 0), New XYZ(-w / 2 + tw, d / 2, 0), New XYZ(-w / 2, d / 2, 0), _
                        New XYZ(-w / 2, -d / 2, 0)} ' the last one is to make the loop simple

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
  ''   (1.2b) create a simple rectangular profile
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
  ''   (2.1) add alignments
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
    Dim ref1 As Reference = upperLevel.GetPlaneReference()

    ''  find the face of the box
    ''  findFace() is a helper function. see below.
    ''
    Dim upperFace As PlanarFace = findFace(pBox, New XYZ(0, 0, 1)) ' find a face whose normal is z-up.
    Dim ref2 As Reference = upperFace.Reference

    '' create alignments
    ''
    _doc.FamilyCreate.NewAlignment(pView, ref1, ref2)

    ''
    ''  (2) do the same for the lower level
    ''

    ''  find the lower ref level
    ''  findElement() is a helper function. see below.
    ''
    Dim lowerLevel As Level = findElement(GetType(Level), "Lower Ref. Level")
    Dim ref3 As Reference = lowerLevel.GetPlaneReference()

    ''  find the face of the box
    ''  findFace() is a helper function. see below.
    ''
    Dim lowerFace As PlanarFace = findFace(pBox, New XYZ(0, 0, -1)) ' find a face whose normal is z-down.
    Dim ref4 As Reference = lowerFace.Reference

    '' create alignments
    ''
    _doc.FamilyCreate.NewAlignment(pView, ref3, ref4)

    ''
    ''  (3)  same idea for the width and depth.
    ''
    ''  get the plan view
    ''  note: same name maybe used for different view types. either one should work.
    Dim pViewPlan As View = findElement(GetType(ViewPlan), "Lower Ref. Level")

    ''  find reference planes
    ''
    Dim refRight As ReferencePlane = findElement(GetType(ReferencePlane), "Right")
    Dim refLeft As ReferencePlane = findElement(GetType(ReferencePlane), "Left")
    Dim refFront As ReferencePlane = findElement(GetType(ReferencePlane), "Front")
    Dim refBack As ReferencePlane = findElement(GetType(ReferencePlane), "Back")
    Dim refOffsetV As ReferencePlane = findElement(GetType(ReferencePlane), "OffsetV") ' added for L-shape
    Dim refOffsetH As ReferencePlane = findElement(GetType(ReferencePlane), "OffsetH") ' added for L-shape


    ''  find the face of the box
    ''  note: findFace needs to be enhanced for this as face normal is not enough to determine the face.
    ''
    Dim faceRight As PlanarFace = findFace(pBox, New XYZ(1, 0, 0), refRight) ' modified for L-shape
    Dim faceLeft As PlanarFace = findFace(pBox, New XYZ(-1, 0, 0))
    Dim faceFront As PlanarFace = findFace(pBox, New XYZ(0, -1, 0))
    Dim faceBack As PlanarFace = findFace(pBox, New XYZ(0, 1, 0), refBack) ' modified for L-shape
    Dim faceOffsetV As PlanarFace = findFace(pBox, New XYZ(1, 0, 0), refOffsetV) ' added for L-shape
    Dim faceOffsetH As PlanarFace = findFace(pBox, New XYZ(0, 1, 0), refOffsetH) ' added for L-shape

    '' create alignments
    ''
    _doc.FamilyCreate.NewAlignment(pViewPlan, refRight.GetReference(), faceRight.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refLeft.GetReference(), faceLeft.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refFront.GetReference(), faceFront.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refBack.GetReference(), faceBack.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refOffsetV.GetReference(), faceOffsetV.Reference)
    _doc.FamilyCreate.NewAlignment(pViewPlan, refOffsetH.GetReference(), faceOffsetH.Reference)

  End Sub

  '' ============================================
  ''   (3.1) add parameters
  '' ============================================
  Sub addParameters()

        ''  (1)  add dimensional parameters, Tw and Td.
        ''
        ''  parameter group for Dimension is PG_GEOMETRY in API
        ''
        Dim builtinParamgroupTypeId As ForgeTypeId = New ForgeTypeId(BuiltInParameterGroup.PG_GEOMETRY.ToString())
        Dim parameterTypeId As ForgeTypeId = New ForgeTypeId(SpecTypeId.Length.ToString())
        Dim paramTw As FamilyParameter = _doc.FamilyManager.AddParameter("Tw", builtinParamgroupTypeId, parameterTypeId, False)
        Dim paramTd As FamilyParameter = _doc.FamilyManager.AddParameter("Td", builtinParamgroupTypeId,parameterTypeId, False)

    ''  give initial values
    ''
    Dim tw As Double = mmToFeet(150.0) ' hard coded for simplicity
    Dim td As Double = mmToFeet(150.0)
    _doc.FamilyManager.Set(paramTw, tw)
    _doc.FamilyManager.Set(paramTd, td)

    ''  (2)  add a parameter for material finish
    ''       we are adding material arameter in addMaterials function. See addMaterials for the actual implementation.
    ''

  End Sub

  '' ============================================
  ''   (3.2) add dimensions
  '' ============================================
  Sub addDimensions()

    ''  find the plan view
    ''
    Dim pViewPlan As View = findElement(GetType(ViewPlan), "Lower Ref. Level")

    ''  find reference planes
    ''
    Dim refLeft As ReferencePlane = findElement(GetType(ReferencePlane), "Left")
    Dim refFront As ReferencePlane = findElement(GetType(ReferencePlane), "Front")
    Dim refOffsetV As ReferencePlane = findElement(GetType(ReferencePlane), "OffsetV") ' added for L-shape
    Dim refOffsetH As ReferencePlane = findElement(GetType(ReferencePlane), "OffsetH") ' added for L-shape

    ''
    ''  (1)  add dimension between the reference planes 'Left' and 'OffsetV', and label it as 'Tw
    ''

    ''  define a dimension line
    ''
    Dim p0 As XYZ = refLeft.FreeEnd
    Dim p1 As XYZ = refOffsetV.FreeEnd
    'Dim pLine As Line = _app.Create.NewLineBound(p0, p1)  ' Revit 2013
    Dim pLine As Line = Line.CreateBound(p0, p1)  ' Revit 2014

    ''  define references
    ''
    Dim pRefArray As New ReferenceArray
    pRefArray.Append(refLeft.GetReference())
    pRefArray.Append(refOffsetV.GetReference())

    ''  create a dimension
    ''
    Dim pDimTw As Dimension = _doc.FamilyCreate.NewDimension(pViewPlan, pLine, pRefArray)

    ''  add label to the dimension
    ''
    Dim paramTw As FamilyParameter = _doc.FamilyManager.Parameter("Tw")
    'pDimTw.Label = paramTw  ' Revit 2013
    pDimTw.FamilyLabel = paramTw  ' Revit 2014

    ''
    ''  (2)  do the same for dimension between 'Front' and 'OffsetH', and lable it as 'Td
    ''

    ''  define a dimension line
    ''
    p0 = refFront.FreeEnd
    p1 = refOffsetH.FreeEnd
    'pLine = _app.Create.NewLineBound(p0, p1)  ' Revit 2013
    pLine = Line.CreateBound(p0, p1)  ' Revit 2014

    ''  define references
    ''
    pRefArray = New ReferenceArray
    pRefArray.Append(refFront.GetReference())
    pRefArray.Append(refOffsetH.GetReference())

    ''  create a dimension
    ''
    Dim pDimTd As Dimension = _doc.FamilyCreate.NewDimension(pViewPlan, pLine, pRefArray)

    ''  add label to the dimension
    ''
    Dim paramTd As FamilyParameter = _doc.FamilyManager.Parameter("Td")
    'pDimTd.Label = paramTd  ' Revit 2013
    pDimTd.FamilyLabel = paramTd  ' Revit 2014

  End Sub

  '' ============================================
  ''   (3.3) add types
  '' ============================================
  Sub addTypes()

    ''  addType(name, Width, Depth)
    ''
    'addType("600x900", 600.0, 900.0)
    'addType("1000x300", 1000.0, 300.0)
    'addType("600x600", 600.0, 600.0)

    ''  addType(name, Width, Depth, Tw, Td)
    ''
    addType("600x900", 600.0, 900.0, 150, 225)
    addType("1000x300", 1000.0, 300.0, 250, 75)
    addType("600x600", 600.0, 600.0, 150, 150)

  End Sub

  ''  add one type (version 2)
  ''
  Sub addType(ByVal name As String, ByVal w As Double, ByVal d As Double, ByVal tw As Double, ByVal td As Double)

    ''  get the family manager from the current doc
    Dim pFamilyMgr As FamilyManager = _doc.FamilyManager

    ''  add new types with the given name
    ''
    Dim type1 As FamilyType = pFamilyMgr.NewType(name)

    ''  look for 'Width' and 'Depth' parameters and set them to the given value
    ''
    Dim paramW As FamilyParameter = pFamilyMgr.Parameter("Width")
    Dim valW As Double = mmToFeet(w)
    If paramW IsNot Nothing Then
      pFamilyMgr.Set(paramW, valW)
    End If

    Dim paramD As FamilyParameter = pFamilyMgr.Parameter("Depth")
    Dim valD As Double = mmToFeet(d)
    If paramD IsNot Nothing Then
      pFamilyMgr.Set(paramD, valD)
    End If

    ''  let's set "Tw' and 'Td
    ''
    Dim paramTw As FamilyParameter = pFamilyMgr.Parameter("Tw")
    Dim valTw As Double = mmToFeet(tw)
    If paramTw IsNot Nothing Then
      pFamilyMgr.Set(paramTw, valTw)
    End If

    Dim paramTd As FamilyParameter = pFamilyMgr.Parameter("Td")
    Dim valTd As Double = mmToFeet(td)
    If paramTd IsNot Nothing Then
      pFamilyMgr.Set(paramTd, valTd)
    End If

  End Sub

  ''  add one type (version 1)
  ''
  Sub addType(ByVal name As String, ByVal w As Double, ByVal d As Double)

    ''  get the family manager from the current doc
    Dim pFamilyMgr As FamilyManager = _doc.FamilyManager

    ''  add new types with the given name
    ''
    Dim type1 As FamilyType = pFamilyMgr.NewType(name)

    ''  look for 'Width' and 'Depth' parameters and set them to the given value
    ''
    ''  first 'Width
    ''
    Dim paramW As FamilyParameter = pFamilyMgr.Parameter("Width")
    Dim valW As Double = mmToFeet(w)
    If paramW IsNot Nothing Then
      pFamilyMgr.Set(paramW, valW)
    End If

    ''  same idea for 'Depth
    ''
    Dim paramD As FamilyParameter = pFamilyMgr.Parameter("Depth")
    Dim valD As Double = mmToFeet(d)
    If paramD IsNot Nothing Then
      pFamilyMgr.Set(paramD, valD)
    End If

  End Sub

  '' ============================================
  ''   (4.1) add formula
  '' ============================================
  Sub addFormulas()

    ''  we will add the following fomulas
    ''    Tw = Width / 4.0
    ''    Td = Depth / 4.0
    ''

    ''  first get the parameter
    Dim pFamilyMgr As FamilyManager = _doc.FamilyManager

    'Dim paramW As FamilyParameter = pFamilyMgr.Parameter("Width")
    'Dim paramD As FamilyParameter = pFamilyMgr.Parameter("Depth")
    Dim paramTw As FamilyParameter = pFamilyMgr.Parameter("Tw")
    Dim paramTd As FamilyParameter = pFamilyMgr.Parameter("Td")

    ''  set the formula
    pFamilyMgr.SetFormula(paramTw, "Width / 4.0")
    pFamilyMgr.SetFormula(paramTd, "Depth / 4.0")

  End Sub

  '' ============================================
  ''   (4.2) add materials
  '' ============================================
  ''
  ''  in Revit 2010, you cannot modify asset.
  ''  SPR# 155053 - WishList: Ability to access\modify properties in Render Appearance of Materials using API.
  ''  To Do in future: you can extend this functionality to create a new one in future.
  ''
  Sub addMaterialsToSolid(ByVal pSolid As Extrusion)

    ''  We assume Material type "Glass" exists. Template "Metric Column.rft" include "Glass",
    ''  which in fact is the only interesting one to see the effect.
    ''  In practice, you will want to include in your template.
    ''
    ''  To Do: For the exercise, create it with more appropriate ones in UI, then use the name here.
    ''
    Dim pMat As Material = findElement(GetType(Material), "Glass") ' hard coded fot simplicity.
    If pMat Is Nothing Then
      ''  no material with the given name.
      Return
    End If
    Dim idMat As ElementId = pMat.Id
    ''pSolid.Parameter("Material").Set(idMat)

    ''  'Get' accessor of 'Public ReadOnly Property Parameter(paramName As String) As Autodesk.Revit.DB.Parameter' is obsolete: 
    '' 'This property is obsolete in Revit 2015    

    '' Updated for Revit 2015

    pSolid.LookupParameter("Material").Set(idMat)


  End Sub

  Sub addMaterials(ByVal pSolid As Extrusion)

    ''  We assume Material type "Glass" exists. Template "Metric Column.rft" include "Glass",
    ''  which in fact is the only interesting one to see the effect.
    ''  In practice, you will want to include in your template.
    ''
    ''  To Do: For the exercise, create it with more appropriate ones in UI, then use the name here.
    ''

    ''  (1)  get the materials id that we are intersted in (e.g., "Glass")
    ''
    Dim pMat As Material = findElement(GetType(Material), "Glass") ' hard coded fot simplicity.
    If pMat Is Nothing Then
      ''  no material with the given name.
      Return
    End If
    Dim idMat As ElementId = pMat.Id

        ''  (2a) this add a material to the solid base.  but then, we cannot change it for each column.
        ''
        'pSolid.Parameter("Material").Set(idMat)

        ''  (2b) add a parameter for material finish
        ''
        ''  this time we use instance parameter so that we can change it at instance level.
        ''
        Dim pFamilyMgr As FamilyManager = _doc.FamilyManager
        Dim builtinParamgroupTypeId As ForgeTypeId = New ForgeTypeId(BuiltInParameterGroup.PG_MATERIALS.ToString())
        Dim parameterTypeId As ForgeTypeId = New ForgeTypeId(SpecTypeId.Reference.Material.ToString())
        Dim famParamFinish As FamilyParameter = pFamilyMgr.AddParameter("ColumnFinish", builtinParamgroupTypeId, parameterTypeId, True)

        ''  (2b.1) associate material parameter to the family parameter we just added
        ''

        'Dim paramMat As Parameter = pSolid.Parameter("Material")

        ''  'Get' accessor of 'Public ReadOnly Property Parameter(paramName As String) As Autodesk.Revit.DB.Parameter' is obsolete: 
        '' 'This property is obsolete in Revit 2015    

        '' Updated for Revit 2015

        Dim paramMat As Parameter = pSolid.LookupParameter("Material")

    pFamilyMgr.AssociateElementParameterToFamilyParameter(paramMat, famParamFinish)

    ''  (2b.2) for our combeniencem, let's add another type with Glass finish
    ''
    addType("Glass", 600.0, 600.0)
    pFamilyMgr.Set(famParamFinish, idMat)

  End Sub

  '' ============================================
  ''   (5.1.1) create simple line objects to be displayed in coarse level
  '' ============================================
  Sub addLineObjects()

    ''
    ''  define a simple L-shape detail line object
    ''
    ''  0
    ''   +               h = height
    ''   |              (we also want to draw a vertical line here at point 1)
    '' d |
    ''   +-----+
    ''  1       2
    ''      w
    ''

    ''  sizes
    Dim w As Double = mmToFeet(600) '' modified to match reference plane. otherwise, alignment won't work.
    Dim d As Double = mmToFeet(600)
    Dim h As Double = mmToFeet(4000) '' distance between Lower and Upper Ref Level.
    Dim t As Double = mmToFeet(50) '' slight offset for visbility

    ''  define vertices
    ''
    Dim pts() As XYZ = {New XYZ(-w / 2 + t, d / 2, 0), New XYZ(-w / 2 + t, -d / 2 + t, 0), New XYZ(w / 2, -d / 2 + t, 0)}
    Dim ptH As XYZ = New XYZ(-w / 2 + t, -d / 2 + t, h) ' this is for vertical line.

    ''
    ''  (2) create a sketch plane
    ''
    ''  we need to know the template. If you look at the template (Metric Column.rft) and "Front" view,
    ''  you will see "Reference Plane" at "Lower Ref. Level". We are going to create a sketch plane there.
    ''  findElement() is a helper function that find an element of the given type and name.  see below.
    ''  Note: we did the same in creating a profile.
    ''
    Dim pRefPlane As ReferencePlane = findElement(GetType(ReferencePlane), "Reference Plane") ' need to know from the template
    'Dim pSketchPlane As SketchPlane = _doc.FamilyCreate.NewSketchPlane(pRefPlane.Plane)  ' Revit 2013
    'Dim pSketchPlane As SketchPlane = SketchPlane.Create(_doc, pRefPlane.Plane)  ' Revit 2014
    Dim pSketchPlane As SketchPlane = SketchPlane.Create(_doc, pRefPlane.GetPlane())  ' Revit 2014

    ''  for vertical line, we draw a straight vertical line at point (1) 
    Dim normal As New XYZ(1, 0, 0)
    'Dim pGeomPlaneH As Plane = _app.Create.NewPlane(normal, pts(1)) ' Revit 2016
    Dim pGeomPlaneH As Plane = Plane.CreateByNormalAndOrigin(normal, pts(1)) ' Revit 2017
    'Dim pSketchPlaneH As SketchPlane = _doc.FamilyCreate.NewSketchPlane(pGeomPlaneH)  ' Revit 2013
    Dim pSketchPlaneH As SketchPlane = SketchPlane.Create(_doc, pGeomPlaneH)  ' Revit 2014

    ''  (4) create line objects: two symbolic curves on a plan and one model curve representing a column like a vertical stick.
    ''
    '' Revit 2013
    'Dim geomLine1 As Line = _app.Create.NewLine(pts(0), pts(1), True)
    'Dim geomLine2 As Line = _app.Create.NewLine(pts(1), pts(2), True)
    'Dim geomLineH As Line = _app.Create.NewLine(pts(1), ptH, True)

    '' Revit 2014
    Dim geomLine1 As Line = Line.CreateBound(pts(0), pts(1))
    Dim geomLine2 As Line = Line.CreateBound(pts(1), pts(2))
    Dim geomLineH As Line = Line.CreateBound(pts(1), ptH)

    Dim pLine1 As SymbolicCurve = _doc.FamilyCreate.NewSymbolicCurve(geomLine1, pSketchPlane)
    Dim pLine2 As SymbolicCurve = _doc.FamilyCreate.NewSymbolicCurve(geomLine2, pSketchPlane)
    Dim pLineH As ModelCurve = _doc.FamilyCreate.NewModelCurve(geomLineH, pSketchPlaneH) ' this is vertical line

    ''  set the visibilities of two lines to coarse only
    ''
    Dim pVis As FamilyElementVisibility = New FamilyElementVisibility(FamilyElementVisibilityType.ViewSpecific)
    pVis.IsShownInFine = False
    pVis.IsShownInMedium = False

    pLine1.SetVisibility(pVis)
    pLine2.SetVisibility(pVis)

    Dim pVisH As FamilyElementVisibility = New FamilyElementVisibility(FamilyElementVisibilityType.Model)
    pVisH.IsShownInFine = False
    pVisH.IsShownInMedium = False

    pLineH.SetVisibility(pVisH)

  End Sub

  '' ============================================
  ''   (5.1.2) set the visibility of the solid not to show in coarse
  '' ============================================
  Sub changeVisibility(ByVal pSolid As Extrusion)

    ''  set the visibility of the model not to shown in coarse.
    ''
    Dim pVis As FamilyElementVisibility = New FamilyElementVisibility(FamilyElementVisibilityType.Model)
    pVis.IsShownInCoarse = False

    pSolid.SetVisibility(pVis)

  End Sub

  ''============================================
  ''
  ''  Helper functions
  ''
  ''============================================
#Region "Helper Functions"

  '' ============================================
  ''   helper function: given a solid, find a planar face with the given normal (version 2)
  ''   this is a slightly enhanced version which checks if the face is on the given reference plane.
  '' ============================================
  Function findFace(ByVal pBox As Extrusion, ByVal normal As XYZ, ByVal refPlane As ReferencePlane) As PlanarFace

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
            ''  check to see if they have same normal
            If pPlanarFace.FaceNormal.IsAlmostEqualTo(normal) Then

              '' additionally, we want to check if the face is on the reference plane
              ''
              Dim p0 As XYZ = refPlane.BubbleEnd
              Dim p1 As XYZ = refPlane.FreeEnd
              'Dim pCurve As Line = _app.Create.NewLineBound(p0, p1)  ' Revit 2013
              Dim pCurve As Line = Line.CreateBound(p0, p1)  ' Revit 2014
              Dim res As SetComparisonResult = pPlanarFace.Intersect(pCurve)
              If res = SetComparisonResult.Subset Then
                Return (pPlanarFace) '' we found the face
              End If

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

  '' ============================================
  ''   helper function: find a planar face with the given normal (version 1)
  ''   this only work with a simple rectangilar box.
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
            If pPlanarFace.FaceNormal.IsAlmostEqualTo(normal) Then '' we found the face
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
