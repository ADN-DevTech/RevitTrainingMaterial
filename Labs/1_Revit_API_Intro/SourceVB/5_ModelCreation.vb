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
'Imports Autodesk.Revit.ApplicationServices
'Imports Autodesk.Revit.Attributes ' specific this if you want to save typing for attributes. e.g., 
'Imports Autodesk.Revit.DB.Structure ' added for Lab5. 
'Imports RevitIntroVB.ElementFiltering  ' added for Lab4. 
Imports IntroVb.Util.Constant
#End Region

#Region "Description"
' Revit Intro Lab 5 
'
' In this lab, you will learn how to create revit models. 
' To test this, use "DefaultMetric" template. 
' 
' Disclaimer: minimum error checking to focus on the main topic. 
#End Region

''' <summary>
''' Element Creation. 
''' </summary> 
<Transaction(TransactionMode.Manual)> _
Public Class ModelCreation
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

    ' Get the acess to the top most objects. 
    Dim rvtUIApp As UIApplication = commandData.Application
    Dim uiDoc As UIDocument = rvtUIApp.ActiveUIDocument
    _app = rvtUIApp.Application
    _doc = uiDoc.Document
    Using transaction As Transaction = New Transaction(_doc)
      transaction.Start("Create House")
      ' Let's make a simple "house" composed of four walls, a window 
      ' and a door. 
      CreateHouse()
      transaction.Commit()
    End Using
    Return Result.Succeeded

  End Function

  Sub CreateHouse()

    ' Simply create four walls with rectangular profile. 
    Dim walls As List(Of Wall) = CreateWalls()

    ' Add a door to the second wall 
    AddDoor(walls(0))

    ' Add windows to the rest of the walls. 
    For i As Integer = 1 To 3
      AddWindow(walls(i))
    Next

    ' (optional) add a roof over the walls' rectangular profile. 
    AddRoof(walls)

  End Sub

  ''' <summary>
  ''' There are five override methods for creating walls. 
  ''' We assume you are using metric template, where you have 
  ''' "Level 1" and "Level 2" 
  ''' cf. Developer Guide page 117 
  ''' </summary>
  Function CreateWalls() As List(Of Wall)

    ' Hard coding the size of the house for simplicity 
    Dim width As Double = MmToFeet(10000.0)
    Dim depth As Double = MmToFeet(5000.0)

    ' Get the levels we want to work on. 
    ' Note: hard coding for simplicity. Modify here you use a different template. 
    Dim level1 As Level = ElementFiltering.FindElement(_doc, GetType(Level), "Level 1")
    If level1 Is Nothing Then
      TaskDialog.Show("Create walls", "Cannot find (Level 1). Maybe you use a different template? Try with DefaultMetric.rte.")
      Return Nothing
    End If

    Dim level2 As Level = ElementFiltering.FindElement(_doc, GetType(Level), "Level 2")
    If level2 Is Nothing Then
      TaskDialog.Show("Create walls", "Cannot find (Level 2). Maybe you use a different template? Try with DefaultMetric.rte.")
      Return Nothing
    End If

    ' Set four corner of walls.
    ' 5th point is for combenience to loop through.  
    Dim dx As Double = width / 2.0
    Dim dy As Double = depth / 2.0

    Dim pts As New List(Of XYZ)(5)
    pts.Add(New XYZ(-dx, -dy, 0.0))
    pts.Add(New XYZ(dx, -dy, 0.0))
    pts.Add(New XYZ(dx, dy, 0.0))
    pts.Add(New XYZ(-dx, dy, 0.0))
    pts.Add(pts(0))

    ' Flag for structural wall or not. 
    Dim isStructural As Boolean = False

    ' Save walls we create. 
    Dim walls As New List(Of Wall)(4)

    ' Loop through list of points and define four walls. 
    For i As Integer = 0 To 3
      ' Define a base curve from two points. 
      Dim baseCurve As Line = Line.CreateBound(pts(i), pts(i + 1))
      ' Create a wall using the one of overloaded methods. 
      Dim aWall As Wall = Wall.Create(_doc, baseCurve, level1.Id, isStructural)
      ' Set the Top Constraint to Level 2 
      aWall.Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id)
      ' Save the wall.
      walls.Add(aWall)
    Next

    ' This is important. we need these lines to have shrinkwrap working. 
    _doc.Regenerate()
    _doc.AutoJoinElements()

    Return walls

  End Function

  ''' <summary>
  ''' Add a door to the center of the given wall. 
  ''' cf. Developer Guide p140. NewFamilyInstance() for Doors and Window. 
  ''' </summary>
  Sub AddDoor(ByVal hostWall As Wall)

    ' Hard coding the door type we will use. 
    ' E.g., "M_Single-Flush: 0915 x 2134mm 

    Const doorFamilyName As String = Util.Constant.DoorFamilyName
    Const doorTypeName As String = Util.Constant.DoorTypeName
    Const doorFamilyAndTypeName As String = doorFamilyName + ": " + doorTypeName

    ' Get the door type to use. 

    Dim doorType As FamilySymbol = _
    ElementFiltering.FindFamilyType(_doc, GetType(FamilySymbol), _
                                    doorFamilyName, doorTypeName, BuiltInCategory.OST_Doors)
    If doorType Is Nothing Then
      TaskDialog.Show( _
        "Add door", _
        "Cannot find (" + _
        doorFamilyAndTypeName + _
        "). Maybe you use a different template? Try with DefaultMetric.rte.")
    End If

    If Not doorType.IsActive Then
      doorType.Activate()
    End If
    ' Get the start and end points of the wall.

    Dim locCurve As LocationCurve = hostWall.Location
    Dim pt1 As XYZ = locCurve.Curve.GetEndPoint(0)
    Dim pt2 As XYZ = locCurve.Curve.GetEndPoint(1)
    ' Calculate the mid point. 
    Dim pt As XYZ = (pt1 + pt2) / 2.0

    ' we want to set the reference as a bottom of the wall or level1.

    Dim idLevel1 As ElementId = _
      hostWall.Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId
    Dim level1 As Level = _doc.GetElement(idLevel1)

    ' Finally, create a door. 

    Dim aDoor As FamilyInstance = _
      _doc.Create.NewFamilyInstance( _
        pt, doorType, hostWall, level1, _
        StructuralType.NonStructural)

  End Sub

  ''' <summary>
  ''' Add a window to the center of the wall given. 
  ''' cf. Developer Guide p140. NewFamilyInstance() for Doors and Window. 
  ''' Basically the same idea as a door except that we need to set sill hight. 
  ''' </summary>
  Sub AddWindow(ByVal hostWall As Wall)

    ' Hard coding the window type we will use. 
    ' E.g., "M_Fixed: 0915 x 1830mm 

    Const windowFamilyName As String = Util.Constant.WindowFamilyName
    Const windowTypeName As String = Util.Constant.WindowTypeName
    Const windowFamilyAndTypeName As String = windowFamilyName + ": " + windowTypeName
    Dim sillHeight As Double = MmToFeet(915)

    ' Get the door type to use. 

    Dim windowType As FamilySymbol = _
    ElementFiltering.FindFamilyType(_doc, GetType(FamilySymbol), _
        windowFamilyName, windowTypeName, BuiltInCategory.OST_Windows)

    If windowType Is Nothing Then
      TaskDialog.Show( _
        "Add door", _
        "Cannot find (" + _
        windowFamilyAndTypeName + _
        "). Maybe you use a different template? Try with DefaultMetric.rte.")
    End If

    If Not windowType.IsActive Then
      windowType.Activate()
    End If
    ' Get the start and end points of the wall. 

    Dim locCurve As LocationCurve = hostWall.Location
    Dim pt1 As XYZ = locCurve.Curve.GetEndPoint(0)
    Dim pt2 As XYZ = locCurve.Curve.GetEndPoint(1)
    ' Calculate the mid point. 
    Dim pt As XYZ = (pt1 + pt2) / 2.0

    ' we want to set the reference as a bottom of the wall or level1. 
    Dim idLevel1 As ElementId = hostWall.Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId
    Dim level1 As Level = _doc.GetElement(idLevel1)

    ' Finally create a window. 

    Dim aWindow As FamilyInstance = _doc.Create.NewFamilyInstance( _
    pt, windowType, hostWall, level1, StructuralType.NonStructural)

    ' Set the sill height 

    aWindow.Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(sillHeight)

  End Sub

  ''' <summary>
  ''' Add a roof over the rectangular profile of the walls we created earlier. 
  ''' </summary>
  Sub AddRoof(ByVal walls As List(Of Wall))

    ' Hard coding the roof type we will use. 
    ' E.g., "Basic Roof: Generic - 400mm"  

    Const roofFamilyName As String = "Basic Roof"
    Const roofTypeName As String = Util.Constant.RoofTypeName
    Const roofFamilyAndTypeName As String = roofFamilyName + ": " + roofTypeName

    ' Find the roof type

    Dim roofType As RoofType = _
    ElementFiltering.FindFamilyType(_doc, GetType(RoofType), _
                            roofFamilyName, roofTypeName)

    If roofType Is Nothing Then
      TaskDialog.Show( _
        "Add roof", _
        "Cannot find (" + _
        roofFamilyAndTypeName + _
        "). Maybe you use a different template? Try with DefaultMetric.rte.")
    End If

    ' Wall thickness to adjust the footprint of the walls
    ' to the outer most lines. 
    ' Note: this may not be the best way, 
    ' but we will live with this for this exercise. 

    'Dim wallThickness As Double = _
    '  walls(0).WallType.CompoundStructure.Layers.Item(0).Thickness ' 2011
    Dim wallThickness As Double = walls(0).Width ' 2012

    Dim dt As Double = wallThickness / 2.0
    Dim dts As New List(Of XYZ)(5)
    dts.Add(New XYZ(-dt, -dt, 0.0))
    dts.Add(New XYZ(dt, -dt, 0.0))
    dts.Add(New XYZ(dt, dt, 0.0))
    dts.Add(New XYZ(-dt, dt, 0.0))
    dts.Add(dts(0))

    ' Set the profile from four walls 

    Dim footPrint As New CurveArray()
    For i As Integer = 0 To 3
      Dim locCurve As LocationCurve = walls(i).Location
      Dim pt1 As XYZ = locCurve.Curve.GetEndPoint(0) + dts(i)
      Dim pt2 As XYZ = locCurve.Curve.GetEndPoint(1) + dts(i + 1)
      Dim line As Line = line.CreateBound(pt1, pt2)
      footPrint.Append(line)
    Next

    ' Get the level2 from the wall

    Dim idLevel2 As ElementId = _
        walls(0).Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId
    Dim level2 As Level = _doc.GetElement(idLevel2)

    ' Footprint to model curve mapping

    Dim mapping As ModelCurveArray = New ModelCurveArray()

    ' Create a roof.

    Dim aRoof As FootPrintRoof = _
      _doc.Create.NewFootPrintRoof(footPrint, level2, roofType, mapping)

    For Each modelCurve As ModelCurve In mapping
      aRoof.DefinesSlope(modelCurve) = True
      aRoof.SlopeAngle(modelCurve) = 0.5
    Next

  End Sub

End Class
