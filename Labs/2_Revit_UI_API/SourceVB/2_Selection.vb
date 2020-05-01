#Region "Copyright"
'
' Copyright (C) 2009-2020 by Autodesk, Inc.
'
' Permission to use, copy, modify, and distribute this software in
' object code form for any purpose and without fee is hereby granted,
' provided that the above copyright notice appears in all copies and
' that both that copyright notice and the limited warranty and
' restricted rights notice below appear in all supporting
' documentation.
'
' AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
' AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
' MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
' DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
' UNINTERRUPTED OR ERROR FREE.
'
' Use, duplication, or disclosure by the U.S. Government is subject to
' restrictions set forth in FAR 52.227-19 (Commercial Computer
' Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
' (Rights in Technical Data and Computer Software), as applicable.
'
' Written by M.Harada 
'
#End Region

#Region "Imports"
' Import the following name spaces in the project properties/references. 
' Note: VB.NET has a slighly different way of recognizing name spaces than C#. 
' if you explicitely set them in each .vb file, you will need to specify full name spaces. 

'Imports System
'Imports Autodesk.Revit.DB
'Imports Autodesk.Revit.UI
'Imports Autodesk.Revit.ApplicationServices  ' Application class
'Imports Autodesk.Revit.Attributes ' specific this if you want to save typing for attributes. e.g., 
'Imports Autodesk.Revit.UI.Selection ' for selection 
'Imports Autodesk.Revit.Exceptions ' for exception added for UI Lab3.  
'Imports RevitIntroVB ' we'll be using commands we defined in Revit Intro labs. 
#End Region

''' <summary>
''' User Selection 
''' 
''' Note: This exercise uses Revit Into Labs. 
''' Modify your project setting to place the dlls from both labs in one place.  
''' 
''' cf. Developer Guide, Section 7: Selection (pp 89) 
''' </summary>
<Transaction(TransactionMode.ReadOnly)>
Public Class UISelection
  Implements IExternalCommand

  ' Member variables 
  Dim _uiApp As UIApplication
  Dim _uiDoc As UIDocument

  Public Function Execute(ByVal commandData As ExternalCommandData,
                          ByRef message As String,
                          ByVal elements As ElementSet) _
                          As Result _
                          Implements IExternalCommand.Execute

    ' Get the access to the top most objects. (we may not use them all in this specific lab.) 
    _uiApp = commandData.Application
    _uiDoc = _uiApp.ActiveUIDocument

    ' (1) pre-selecetd element is under UIDocument.Selection.Elemens. Classic method.  
    ' You can also modify this selection set. 

    'Autodesk.Revit.UI.Selection.SelElementSet' is obsolete: 
    'This class is deprecated in Revit 2015. 
    ' Use Selection.SetElementIds() and Selection.GetElementIds() instead.'

    'Dim selSet As SelElementSet = _uiDoc.Selection.Elements  ' For Revit 2014 or earlier

    ' Updated for Revit 2015

    Dim selSet As ICollection(Of ElementId) = _uiDoc.Selection.GetElementIds()


    ShowElementList(selSet, "Pre-selection: ")

    Try
      ' (2.1) pick methods basics.  
      ' there are four types of pick methods: PickObject, PickObjects, PickElementByRectangle, PickPoint. 
      ' Let's quickly try them out. 

      PickMethodsBasics()

      ' (2.2) selection object type    
      ' in addition to selecting objects of type Element, the user can pick faces, edges, and point on element. 

      PickFaceEdgePoint()

      ' (2.3) selection filter  
      ' if you want additional selection criteria, such as only to pick a wall, you can use selection filter. 

      ApplySelectionFilter()

    Catch err As Autodesk.Revit.Exceptions.OperationCanceledException
      TaskDialog.Show("UI selection", "You have canceled selection.")

    Catch ex As Exception
      TaskDialog.Show("UI selection", "Some other exception caught in CancelSelection()")

    End Try

    ' (2.4) canceling selection  
    ' when the user cancel or press [Esc] key during the selection, OperationCanceledException will be thrown. 

    CancelSelection()

    ' (3) apply what we learned to our small house creation 
    '  we put it as a separate command. See at the bottom of the code.  
    ' '''

    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' Show basic information about the given element. 
  ''' </summary>
  Public Sub ShowBasicElementInfo(ByVal e As Element)

    ' Let's see what kind of element we got. 
    Dim s As String = "You picked:" + vbCr

    s += ElementToString(e)

    ' Show what we got. 
    TaskDialog.Show("Revit UI Lab", s)

  End Sub

  ''' <summary>
  ''' Pick methods sampler. 
  ''' Quickly try: PickObject, PickObjects, PickElementByRectangle, PickPoint. 
  ''' Without specifics about objects we want to pick. 
  ''' </summary>
  Sub PickMethodsBasics()

    ' (1) Pick Object (we have done this already. But just for the sake of completeness.) 
    PickMethod_PickObject()

    ' (2) Pick Objects 
    PickMethod_PickObjects()

    ' (3) Pick Element By Rectangle 
    PickMethod_PickElementByRectangle()

    ' (4) Pick Point 
    PickMethod_PickPoint()

  End Sub

  ''' <summary>
  ''' Minimum PickObject 
  ''' </summary>
  Sub PickMethod_PickObject()

    Dim r As Reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select one element")
    Dim e As Element = _uiDoc.Document.GetElement(r)
    ShowBasicElementInfo(e)

  End Sub

  ''' <summary>
  ''' Minimum PickObjects 
  ''' Note: when you run this code, you will see "Finish" and "Cancel" buttons in the dialog bar. 
  ''' </summary>
  Sub PickMethod_PickObjects()

    Dim refs As IList(Of Reference) =
        _uiDoc.Selection.PickObjects(ObjectType.Element, "Select multiple elemens")

    ' put it in a List form. 
    Dim elems As IList(Of Element) = New List(Of Element)
    For Each r As Reference In refs
      elems.Add(_uiDoc.Document.GetElement(r))
    Next
    ShowElementList(elems, "Pick Objects: ")

  End Sub

  ''' <summary>
  ''' Minimum PickElementByRectangle 
  ''' </summary>
  Sub PickMethod_PickElementByRectangle()

    ' Note: PickElementByRectangle returns the list of element. not reference. 
    Dim elems As IList(Of Element) =
        _uiDoc.Selection.PickElementsByRectangle("Select by rectangle")

    ' Show it

    ShowElementList(elems, "Pick By Rectangle: ")

  End Sub

  ''' <summary>
  ''' Minimum PickPoint 
  ''' </summary>
  Sub PickMethod_PickPoint()

    Dim pt As XYZ = _uiDoc.Selection.PickPoint("Pick a point")

    Dim msg As String = "Pick Point: "
    msg += PointToString(pt)
    TaskDialog.Show("PickPoint", msg)

  End Sub

  ''' <summary>
  ''' Pick face, edge, point on an element 
  ''' objectType options is applicable to PickObject() and PickObjects() 
  ''' </summary>
  Sub PickFaceEdgePoint()

    ' (1) Face 
    PickFace()

    ' (2) Edge 
    PickEdge()

    ' (3) Point 
    PickPointOnElement()

  End Sub

  Sub PickFace()

    Dim r As Reference = _uiDoc.Selection.PickObject(ObjectType.Face, "Select a face")
    Dim e As Element = _uiDoc.Document.GetElement(r)
    'Dim oFace As Face = r.GeometryObject ' 2011
    Dim oFace As Face = e.GetGeometryObjectFromReference(r) ' 2012

    ' show a message to the user. 
    Dim msg As String = ""
    If oFace IsNot Nothing Then
      msg = "You picked the face of element " + e.Id.ToString + vbCr
    Else
      msg = "no Face picked" + vbCr
    End If
    TaskDialog.Show("PickFace", msg)

  End Sub

  Sub PickEdge()

    Dim r As Reference = _uiDoc.Selection.PickObject(ObjectType.Edge, "Select an edge")
    Dim e As Element = _uiDoc.Document.GetElement(r)
    'Dim oEdge As Edge = r.GeometryObject ' 2011
    Dim oEdge As Edge = e.GetGeometryObjectFromReference(r) ' 2012

    Dim msg As String = ""
    If oEdge IsNot Nothing Then
      msg = "You picked an edge of element " + e.Id.ToString + vbCr
    Else
      msg = "no Edge picked" + vbCr
    End If
    TaskDialog.Show("PickEdge", msg)

  End Sub

  Sub PickPointOnElement()

    Dim r As Reference =
        _uiDoc.Selection.PickObject(
            ObjectType.PointOnElement,
            "Select a point on element")

    Dim e As Element = _uiDoc.Document.GetElement(r)
    Dim pt As XYZ = r.GlobalPoint

    Dim msg As String
    If pt Is Nothing Then
      msg = "No point picked."
    Else
      msg = "You picked the point " + PointToString(pt) _
          + " on element " + e.Id.ToString
    End If
    TaskDialog.Show("PickPointOnElement", msg)

  End Sub

  ''' <summary>
  ''' Pick with selection filter 
  ''' Let's assume we only want to pick up a wall. 
  ''' </summary>
  Sub ApplySelectionFilter()

    ' Pick only a wall 
    PickWall()

    ' Pick only a planar face. 
    PickPlanarFace()

  End Sub

  ''' <summary>
  ''' Selection with wall filter. 
  ''' See the bottom of the page to see the selection filter implementation. 
  ''' </summary>
  Sub PickWall()

    Dim selFilterWall As New SelectionFilterWall
    Dim r As Reference = _uiDoc.Selection.PickObject(ObjectType.Element, selFilterWall, "Select a wall")

    ' Show it
    Dim e As Element = _uiDoc.Document.GetElement(r)
    ShowBasicElementInfo(e)

  End Sub

  ''' <summary>
  ''' Selection with planar face. 
  ''' See the bottom of the page to see the selection filter implementation. 
  ''' </summary>
  Sub PickPlanarFace()

    ' To call ISelectionFilter.AllowReference, use this.  
    ' This will limit picked face to be planar. 
    Dim doc As Document = _uiDoc.Document
    Dim selFilterPlanarFace As New SelectionFilterPlanarFace(doc)
    Dim r As Reference = _uiDoc.Selection.PickObject(ObjectType.Face, selFilterPlanarFace, "Select a planar face")
    Dim e As Element = doc.GetElement(r)

    'Dim oFace As Face = r.GeometryObject ' 2011
    Dim oFace As Face = e.GetGeometryObjectFromReference(r) ' 2012

    ' Show a message to the user. 
    Dim msg As String = ""
    If oFace IsNot Nothing Then
      msg = "You picked the face of element " + e.Id.ToString + vbCr
    Else
      msg = "no Face picked" + vbCr
    End If
    TaskDialog.Show("PickPlanarFace", msg)


  End Sub

  ''' <summary>
  ''' Canceling selection 
  ''' When the user presses [Esc] key during the selection, OperationCanceledException will be thrown. 
  ''' </summary>
  Sub CancelSelection()

    Try
      Dim r As Reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select one element, or press [Esc] to cancel")
      Dim e As Element = _uiDoc.Document.GetElement(r)
      ShowBasicElementInfo(e)

    Catch err As Autodesk.Revit.Exceptions.OperationCanceledException
      TaskDialog.Show("CancelSelection", "You have canceled selection.")

    Catch ex As Exception
      TaskDialog.Show("CancelSelection", "Some other exception caught in CancelSelection()")
    End Try

  End Sub


#Region "Helper Function"
  '====================================================================
  ' Helper Functions 
  '====================================================================

  ''' <summary>
  ''' Helper function to display info from a list of elements passed onto. 
  ''' (Same as Revit Intro Lab3.) 
  ''' </summary>
  Public Sub ShowElementList(elemIds As IEnumerable, header As String)
    Dim s As String = vbLf & vbLf & " - Class - Category - Name (or Family: Type Name) - Id - " & vbCrLf
    Dim count As Integer = 0
    For Each eId As ElementId In elemIds
      count += 1
      Dim e As Element = Me._uiDoc.Document.GetElement(eId)
      s += Me.ElementToString(e)
    Next
    s = header + "(" + count.ToString() + ")" + s
    TaskDialog.Show("Revit UI Lab", s)
  End Sub

  ''' <summary>
  ''' Helper function: summarize an element information as a line of text, 
  ''' which is composed of: class, category, name and id. 
  ''' Name will be "Family: Type" if a given element is ElementType. 
  ''' Intended for quick viewing of list of element, for example. 
  ''' (Same as Revit Intro Lab3.) 
  ''' </summary>
  Function ElementToString(ByVal e As Element) As String

    If e Is Nothing Then
      Return "none"
    End If

    Dim name As String = ""

    If TypeOf e Is ElementType Then
      Dim param As Parameter = e.Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)
      If param IsNot Nothing Then
        name = param.AsString
      End If
    Else
      name = e.Name
    End If

    Return e.GetType.Name + "; " + e.Category.Name + "; " _
    + name + "; " + e.Id.IntegerValue.ToString + vbCr

  End Function

  ''' <summary>
  ''' Helper Function: returns XYZ in a string form. 
  ''' (Same as Revit Intro Lab2) 
  ''' </summary>
  Public Shared Function PointToString(ByVal pt As XYZ) As String

    If pt Is Nothing Then
      Return ""
    End If

    Return "(" + pt.X.ToString("F2") + ", " + pt.Y.ToString("F2") + ", " + pt.Z.ToString("F2") + ")"

  End Function

#End Region

End Class

''' <summary>
''' Selection filter that limit the type of object being picked as wall. 
''' </summary>
Class SelectionFilterWall
  Implements ISelectionFilter

  Public Function AllowElement(ByVal e As Element) _
    As Boolean Implements ISelectionFilter.AllowElement

    If e.Category Is Nothing Then Return False
    If e.Category.Id.IntegerValue.Equals(BuiltInCategory.OST_Walls) Then Return True
    Return False

  End Function

  Public Function AllowReference( _
    ByVal reference As Reference, _
    ByVal position As XYZ) _
    As Boolean Implements ISelectionFilter.AllowReference

    Return True

  End Function

End Class

''' <summary>
''' Selection filter that limit the reference type to be planar face 
''' </summary>
Class SelectionFilterPlanarFace
  Implements ISelectionFilter

  Dim _doc As Document

  Public Sub New(ByVal doc As Document)
    _doc = doc
  End Sub

  Public Function AllowElement(ByVal e As Element) _
    As Boolean Implements ISelectionFilter.AllowElement

    Return True

  End Function

  Public Function AllowReference( _
    ByVal r As Reference, _
    ByVal position As XYZ) _
    As Boolean Implements ISelectionFilter.AllowReference

    ' Example: if you want to allow only planar faces
    ' and do some more checking, add this:

    'If (TypeOf (r.GeometryObject) Is PlanarFace) Then ' 2011

    Dim id As ElementId = r.ElementId
    'Dim e As Element = _doc.Element(id) 'For 2012
    Dim e As Element = _doc.GetElement(id) ' For 2013

    If (TypeOf (e.GetGeometryObjectFromReference(r)) Is PlanarFace) Then ' 2012

      ' Do additional checking here if needed

      Return True
    End If
    Return False

  End Function

End Class

''' <summary>
''' Create House with UI added 
''' 
''' Ask the user to pick two corner points of walls
''' then ask to choose a wall to add a front door. 
''' </summary>
<Transaction(TransactionMode.Manual)> _
Public Class UICreateHouse
  Implements IExternalCommand

  ' member variables 
  Dim _uiApp As UIApplication
  Dim _uiDoc As UIDocument
  Dim _doc As Document

  Public Function Execute(ByVal commandData As ExternalCommandData, _
                          ByRef message As String, _
                          ByVal elements As ElementSet) _
                          As Result _
                          Implements IExternalCommand.Execute

    ' Get the access to the top most objects. (we may not use them all in this specific lab.) 
    _uiApp = commandData.Application
    _uiDoc = _uiApp.ActiveUIDocument
    _doc = _uiDoc.Document

    Using transaction As Transaction = New Transaction(_doc)
      transaction.Start("Create House")
      CreateHouseInteractive(_uiDoc)
      transaction.Commit()
    End Using
    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' Create a simple house with user interactions. 
  ''' The user is asked to pick two corners of rectangluar footprint of a house, 
  ''' then which wall to place a front door. 
  ''' </summary>
  Public Shared Sub CreateHouseInteractive(ByVal uiDoc As UIDocument)

    Using transaction As Transaction = New Transaction(uiDoc.Document)
      transaction.Start("Create House Interactive")
      ' (1) Walls 
      ' Pick two corners to place a house with an orthogonal rectangular footprint 
      Dim pt1 As XYZ = uiDoc.Selection.PickPoint("Pick the first corner of walls")
      Dim pt2 As XYZ = uiDoc.Selection.PickPoint("Pick the second corner")

      ' Simply create four walls with orthogonal rectangular profile from the two points picked.  
      Dim walls As List(Of Wall) = IntroVb.ModelCreationExport.CreateWalls(uiDoc.Document, pt1, pt2)

      ' (2) Door 
      ' Pick a wall to add a front door to
      Dim selFilterWall As New SelectionFilterWall
      Dim r As Reference = uiDoc.Selection.PickObject( _
          ObjectType.Element, selFilterWall, "Select a wall to place a front door")
      Dim wallFront As Wall = uiDoc.Document.GetElement(r)

      ' Add a door to the selected wall
      IntroVb.ModelCreationExport.AddDoor(uiDoc.Document, wallFront)

      ' (3) Windows 
      ' Add windows to the rest of the walls. 
      For i As Integer = 0 To 3
        If Not (walls(i).Id.IntegerValue = wallFront.Id.IntegerValue) Then
          IntroVb.ModelCreationExport.AddWindow(uiDoc.Document, walls(i))
        End If
      Next

      ' (4) Roofs 
      ' Add a roof over the walls' rectangular profile. 
      IntroVb.ModelCreationExport.AddRoof(uiDoc.Document, walls)
      transaction.Commit()
    End Using


  End Sub

End Class
