#Region "Copyright"
'
' Copyright (C) 2009-2015 by Autodesk, Inc.
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
#End Region

#Region "Description"
' Revit Intro Lab - 2
'
' In this lab, you will learn how an element is represended in Revit.
' Disclaimer: minimum error checking to focus on the main topic.
#End Region

''' <summary>
''' DBElement - identifying element
''' </summary>

<Transaction(TransactionMode.Manual)> _
Public Class DBElement
  Implements IExternalCommand

  ' Member variables
  Dim _app As Application
  Dim _doc As Document

  Public Function Execute( _
    ByVal commandData As ExternalCommandData, _
    ByRef message As String, _
    ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute

    ' Get the access to the top most objects.
    ' Notice that we have UI and DB versions for application and Document.
    ' (We list them both here to show two versions.)

    Dim uiApp As UIApplication = commandData.Application
    Dim uiDoc As UIDocument = uiApp.ActiveUIDocument
    _app = uiApp.Application
    _doc = uiDoc.Document

    ' (1) select an object on a screen. (We'll come back to the selection in the UI Lab later.)
    Dim ref As Reference = _
        uiDoc.Selection.PickObject(ObjectType.Element, "Pick an element")

    ' We have picked something.
    Dim e As Element = _doc.GetElement(ref)

    ' (2) let's see what kind of element we got.
    ' Key properties that we need to check are: Class, Category and if an element is ElementType or not.

    ShowBasicElementInfo(e)

    ' (3) now, we are going to identify each major types of element.
    IdentifyElement(e)

    ' Now look at other properties - important ones are parameters, locations and geometry.

    ' (4) first parameters.

    ShowParameters(e, "Element Parameters")

    ' Check to see its type parameter as well

    Dim elemTypeId As ElementId = e.GetTypeId
    Dim elemType As ElementType = _doc.GetElement(elemTypeId)
    ShowParameters(elemType, "Type Parameters")

    ' Okay. We saw a set or parameters for a given element or element type.
    ' How can we access to each parameters. For example, how can we get the value of "length" information?
    ' Here is how:

    RetrieveParameter(e, "Element Parameter (by Name and BuiltInParameter)")
    ' The same logic applies to the type parameter.
    RetrieveParameter(elemType, "Type Parameter (by Name and BuiltInParameter)")

    ' (5) location
    ShowLocation(e)

    ' (6) geometry - the last piece. (Optional)
    ShowGeometry(e)

    ' These are the common proerties.
    ' There may be more properties specific to the given element class,
    ' such as Wall.Width, .Flipped and Orientation. Expore using RevitLookup and RevitAPI.chm.

    ' We are done.
    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' Show basic information about the given element.
  ''' Note: we are intentionally including both element and element type
  ''' here to compare the output on the same dialog.
  ''' Compare, for example, the categories of element and element type.
  ''' </summary>
  Public Sub ShowBasicElementInfo(ByVal e As Element)

    ' Let's see what kind of element we got.
    Dim s As String = "You picked:" + vbCr
    s = s + "  Class name = " + e.GetType.Name + vbCr
    s = s + "  Category = " + e.Category.Name + vbCr
    s = s + "  Element id = " + e.Id.ToString + vbCr + vbCr

    ' And check its type info.
    Dim elemTypeId As ElementId = e.GetTypeId ' since 2011
    Dim elemType As ElementType = _doc.GetElement(elemTypeId)
    s = s + "Its ElementType:" + vbCr
    s = s + "  Class name = " + elemType.GetType.Name + vbCr
    s = s + "  Category = " + elemType.Category.Name + vbCr
    s = s + "  Element type id = " + elemType.Id.ToString + vbCr

    ' Show what we got.
    TaskDialog.Show("Basic Element Info", s)

  End Sub

  ''' <summary>
  ''' Identify the type of the element known to the UI.
  ''' </summary>
  Public Sub IdentifyElement(ByVal e As Element)

    ' An instance of a system family has a designated class.
    ' You can use it identify the type of element.
    ' e.g., walls, floors, roofs.

    Dim s As String = ""

    If TypeOf e Is Wall Then
      s = "Wall"
    ElseIf TypeOf e Is Floor Then
      s = "Floor"
    ElseIf TypeOf e Is RoofBase Then
      s = "Roof"
    ElseIf TypeOf e Is FamilyInstance Then
      ' An instance of a component family is all FamilyInstance.
      ' We'll need to further check its category.
      ' e.g., Doors, Windows, Furnitures.
      If e.Category.Id.IntegerValue = _
          BuiltInCategory.OST_Doors Then
        s = "Door"
      ElseIf e.Category.Id.IntegerValue = _
          BuiltInCategory.OST_Windows Then
        s = "Window"
      ElseIf e.Category.Id.IntegerValue = _
          BuiltInCategory.OST_Furniture Then
        s = "Furniture"
      Else
        s = "Component family instance"  ' e.g. Plant
      End If

      ' Check the base class. e.g., CeilingAndFloor.
    ElseIf TypeOf e Is HostObject Then
      s = "System family instance"
    Else
      s = "Other"
    End If

    s = "You have picked: " + s
    ' Show it.
    TaskDialog.Show("Identify Element", s)

  End Sub

  ''' <summary>
  ''' Show the parameter values of the element
  ''' </summary>
  Public Sub ShowParameters(ByVal e As Element, Optional ByVal header As String = "")

    Dim s As String = String.Empty
    Dim params As ParameterSet = e.Parameters

    For Each param As Parameter In params
      Dim name As String = param.Definition.Name
      ' To get the value, we need to pause the param depending on the storage type
      ' see the helper function below
      Dim val As String = ParameterToString(param)
      s = s + name + " = " + val + vbCr
    Next

    TaskDialog.Show(header, s)

  End Sub

  ''' <summary>
  ''' Helper function: return a string form of a given parameter.
  ''' </summary>
  Public Shared Function ParameterToString(ByVal param As Parameter) As String

    Dim val As String = "none"

    If param Is Nothing Then
      Return val
    End If

    ' To get to the parameter value, we need to pause it depending on its storage type

    Select Case param.StorageType
      Case StorageType.Double
        Dim dVal As Double = param.AsDouble
        val = dVal.ToString

      Case StorageType.Integer
        Dim iVal As Integer = param.AsInteger
        val = iVal.ToString()

      Case StorageType.String
        Dim sVal As String = param.AsString
        val = sVal

      Case StorageType.ElementId
        Dim idVal As ElementId = param.AsElementId
        val = idVal.IntegerValue.ToString

      Case StorageType.None
      Case Else

    End Select

    Return val

  End Function

  ''' <summary>
  ''' Examples of retrieving a specific parameter indivisually.
  ''' (hard coded for simplicity. This function works best with walls and doors).
  ''' </summary>
  Public Sub RetrieveParameter(ByVal e As Element, Optional ByVal header As String = "")

    Dim s As String = String.Empty

    ' As an experiment, let's pick up some arbitrary parameters.
    ' Comments - most of instance has this parameter

    ' (1) by BuiltInParameter.
    Dim param As Parameter = e.Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
    If param IsNot Nothing Then
      s += "Comments (by BuiltInParameter) =  " + ParameterToString(param) + vbCr
    End If

    ' (2) by name.  (Mark - most of instance has this parameter.) if you use this method, it will language specific.

    '' Get' accessor of 'Public ReadOnly Property Parameter(paramName As String) As Autodesk.Revit.DB.Parameter' is obsolete:
    'This property is obsolete in Revit 2015,

    'param = e.Parameter("Mark")

    ' updated for Revit 2015
    param = e.LookupParameter("Mark")

    If param IsNot Nothing Then
      s += "Mark (by Name) = " + ParameterToString(param) + vbCr
    End If

    ' Though the first one is the most commonly used, other possible methods are:
    ' (3) by definition
    ' param = e.Parameter(Definition)
    ' (4) and for shared parameters, you can also use GUID.
    ' parameter = Parameter(GUID)

    ' The following should be in most of type parameter

    param = e.Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS)
    If param IsNot Nothing Then
      s += "Type Comments (by BuiltInParameter) = " + ParameterToString(param) + vbCr
    End If


    '' Get' accessor of 'Public ReadOnly Property Parameter(paramName As String) As Autodesk.Revit.DB.Parameter' is obsolete:
    'This property is obsolete in Revit 2015,

    'param = e.Parameter("Fire Rating")

    ' updated for Revit 2015
    param = e.LookupParameter("Fire Rating")


    If param IsNot Nothing Then
      s += "Fire Rating (by Name) = " + ParameterToString(param) + vbCr
    End If

    ' Using the BuiltInParameter, you can sometimes access one that is not in the parameters set.
    ' Note: this works only for element type.

    param = e.Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)
    If param IsNot Nothing Then
      s += "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM (only by BuiltInParameter) = " + _
      ParameterToString(param) + vbCr
    End If

    param = e.Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM)
    If param IsNot Nothing Then
      s += "SYMBOL_FAMILY_NAME_PARAM (only by BuiltInParameter) = " + _
      ParameterToString(param) + vbCr
    End If

    ' Show it.

    TaskDialog.Show(header, s)

  End Sub

  ''' <summary>
  ''' Show the location information of the given element.
  ''' Location can be LocationPoint (e.g., furniture), and LocationCurve (e.g., wall).
  ''' </summary>
  Public Sub ShowLocation(ByVal e As Element)

    Dim s As String = "Location Information: " + vbCr + vbCr
    Dim loc As Location = e.Location

    If TypeOf loc Is LocationPoint Then

      ' (1) we have a location point
      '
      Dim locPoint As LocationPoint = loc
      Dim pt As XYZ = locPoint.Point
      Dim r As Double = locPoint.Rotation

      s += "LocationPoint" + vbCr
      s += "Point = " + PointToString(pt) + vbCr
      s += "Rotation = " + r.ToString + vbCr

    ElseIf TypeOf loc Is LocationCurve Then

      ' (2) we have a location curve
      '
      Dim locCurve As LocationCurve = loc
      Dim crv As Curve = locCurve.Curve

      s += "LocationCurve" + vbCr
      s += "EndPoint(0)/Start Point = " + PointToString(crv.GetEndPoint(0)) + vbCr
      s += "EndPoint(1)/End point = " + PointToString(crv.GetEndPoint(1)) + vbCr
      s += "Length = " + crv.Length.ToString + vbCr

      ' Location Curve also has property JoinType at the end

      s += "JoinType(0) = " + locCurve.JoinType(0).ToString + vbCr
      s += "JoinType(1) = " + locCurve.JoinType(1).ToString + vbCr

    End If

    ' Show it

    TaskDialog.Show("Show Location", s)

  End Sub

  ''' <summary>
  ''' Helper Function: returns XYZ in a string form.
  ''' </summary>
  Public Shared Function PointToString(ByVal pt As XYZ) As String

    If pt Is Nothing Then
      Return ""
    End If

    Return "(" + pt.X.ToString("F2") + ", " + pt.Y.ToString("F2") + ", " + pt.Z.ToString("F2") + ")"

  End Function

  ''' <summary>
  ''' This is lengthy. So Optional:
  ''' show the geometry information of the given element. Here is how to access it.
  ''' you can go through by RevitLookup, instead.
  ''' </summary>
  Public Sub ShowGeometry(ByVal e As Element)

    ' First, set a geometry option
    Dim opt As Options = _app.Create.NewGeometryOptions
    opt.DetailLevel = ViewDetailLevel.Fine

    ' Get the geometry from the element
    Dim geomElem As GeometryElement = e.Geometry(opt)

    ' If there is a geometry data, retrieve it as a string to show it.
    Dim s As String
    If geomElem Is Nothing Then
      s = "no data"
    Else
      s = GeometryElementToString(geomElem)
    End If

    ' Show it.
    TaskDialog.Show("Show Geometry", s)

  End Sub

  ''' <summary>
  ''' Helper Function: parse the geometry element by geometry type.
  ''' see RevitCommands in the SDK sample for complete implementation.
  ''' </summary>
  Public Shared Function GeometryElementToString(ByVal geomElem As GeometryElement) As String

    Dim str As String = String.Empty

    For Each geomObj As GeometryObject In geomElem

      If TypeOf geomObj Is Solid Then  '  ex. wall

        Dim solid As Solid = geomObj
        'str += GeometrySolidToString(solid)
        str += "Solid" + vbCr

      ElseIf TypeOf geomObj Is GeometryInstance Then ' ex. door/window

        str += "  -- Geometry.Instance -- " & vbCr
        Dim geomInstance As GeometryInstance = geomObj
        Dim geoElem As GeometryElement = geomInstance.SymbolGeometry()
        str += GeometryElementToString(geoElem)

      ElseIf TypeOf geomObj Is Curve Then ' ex.

        Dim curv As Curve = geomObj
        'str += GeometryCurveToString(curv)
        str += "Curve" + vbCr

      ElseIf TypeOf geomObj Is Mesh Then ' ex.

        Dim mesh As Mesh = geomObj
        'str += GeometryMeshToString(mesh)
        str += "Mesh" + vbCr

      Else
        str += "  *** unkown geometry type" & geomObj.GetType.ToString

      End If

    Next

    Return str

  End Function

End Class
