#Region "Copyright"
'
' Copyright (C) 2009-2021 by Autodesk, Inc.
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
Imports IntroVb.Util.Constant
#End Region

#Region "Description"
' Revit Intro Lab 4 
'
' In this lab, you will learn how to modify elements.
' There are two places to look at when you want to modify an element. 
' (1) at each element level, such as by modifying each properties, parameters and location. 
' (2) use transformation utility methods, such as move, rotate and mirror.  
' 
' for #2, ElementTransformUtils.MoveElement, RotateElement, etc., see pp113 of developer guide. 
' 
' Disclaimer: minimum error checking to focus on the main topic. 
' 
#End Region

''' <summary>
''' Element Modification 
''' </summary>  
<Transaction(TransactionMode.Manual)> _
Public Class ElementModification
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
    Dim rvtUIApp As UIApplication = commandData.Application
    Dim uiDoc As UIDocument = rvtUIApp.ActiveUIDocument
    _app = rvtUIApp.Application
    _doc = uiDoc.Document

    ' Select an object on a screen. (We'll come back to the selection in the UI Lab later.) 
    Dim ref As Reference = uiDoc.Selection.PickObject(ObjectType.Element, "Pick a wall, please")
    ' We have picked something. 
    Dim e As Element = _doc.GetElement(ref)

    Using transaction As Transaction = New Transaction(_doc)
      transaction.Start("Modify Element")
      ' (1) element level modification     
      ' Modify element's properties, parameters, location. 

      ModifyElementPropertiesWall(e)
      'ModifyElementPropertiesDoor(e)
      _doc.Regenerate()

      ' Select an object on a screen. (We'll come back to the selection in the UI Lab later.) 
      Dim ref2 As Reference = uiDoc.Selection.PickObject(ObjectType.Element, "Pick another element")
      ' We have picked something. 
      Dim elem2 As Element = _doc.GetElement(ref2)

      ' (2) you can also use transformation utility to move and rotate.
      ModifyElementByDocumentMethods(elem2)
      transaction.Commit()
    End Using
    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' A sampler function to demonstrate how to modify an element through its properties. 
  ''' Using a wall as an example here. 
  ''' </summary> 
  Sub ModifyElementPropertiesWall(ByVal e As Element)

    ' Constant to this function.         
    ' This is for wall. e.g., "Basic Wall: Exterior - Brick on CMU"
    ' You can modify this to fit your need.   

    Const wallFamilyName As String = Util.Constant.WallFamilyName
    Const wallTypeName As String = "Exterior - Brick on CMU"
    Const wallFamilyAndTypeName As String = wallFamilyName + ": " + wallTypeName

    ' For simplicity, we assume we can only modify a wall 
    If Not (TypeOf e Is Wall) Then
      TaskDialog.Show( _
        "Modify element properties - wall", _
        "Sorry, I only know how to modify a wall. Please select a wall.")
      Return
    End If

    Dim aWall As Wall = e

    'Keep the message to the user. 
    Dim msg As String = "Wall changed: " + vbCr + vbCr

    ' (1) change its family type to a different one.  
    '  You can enhance this to import symbol if you want. 

    Dim newWallType As Element = _
    ElementFiltering.FindFamilyType(_doc, GetType(WallType), wallFamilyName, wallTypeName)

    If newWallType IsNot Nothing Then
      aWall.WallType = newWallType
      msg += "Wall type to: " + wallFamilyAndTypeName + vbCr
      'TaskDialog.Show( _
      '  "Modify element properties - wall", _
      '  msg)  
    End If

    ' (2) change its parameters. 
    ' As a way of exercise, let's constrain top of the wall to the level1 and set an offset. 

    ' Find the level 1 using the helper function we defined in the lab3. 
    Dim level1 As Level = ElementFiltering.FindElement(_doc, GetType(Level), "Level 1")
    If level1 IsNot Nothing Then
      ' Top Constraint 
      aWall.Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level1.Id)
      msg += "Top Constraint to: Level 1" + vbCr
    End If

    ' Hard coding for simplicity here. 
    Dim topOffset As Double = MmToFeet(5000.0)
    ' Top Offset Double 
    aWall.Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(topOffset)
    ' Structural Usage = Bearing(1)  
    'aWall.Parameter(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM).Set(1) ' This is read only.  
    ' Comments - String  
    aWall.Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("Modified by API")

    msg += "Top Offset to: 5000.0" + vbCr
    msg += "Structural Usage to: Bearing" + vbCr
    msg += "Comments added: Modified by API" + vbCr
    'TaskDialog.Show("Modify element properties - wall", msg)

    ' (3) Optional: change its location, using location curve 
    ' LocationCurve also has move and rotation methods.  
    ' Note: constaints affect the result.
    ' Effect will be more visible with disjoined wall.
    ' To test this, you may want to draw a single standing wall, and run this command. 

    Dim wallLocation As LocationCurve = aWall.Location

    Dim pt1 As XYZ = wallLocation.Curve.GetEndPoint(0)
    Dim pt2 As XYZ = wallLocation.Curve.GetEndPoint(1)

    ' Hard coding the displacement value for simility here. 
    Dim dt As Double = MmToFeet(1000.0)
    Dim newPt1 = New XYZ(pt1.X - dt, pt1.Y - dt, pt1.Z)
    Dim newPt2 = New XYZ(pt2.X - dt, pt2.Y - dt, pt2.Z)

    ' Create a new line bound. 
    Dim newWallLine As Line = Line.CreateBound(newPt1, newPt2)

    ' Finally change the curve.
    wallLocation.Curve = newWallLine

    msg += "Location: start point moved -1000.0 in X-direction" + vbCr

    ' Message to the user. 
    TaskDialog.Show("Modify element properties - wall", msg)

  End Sub

  ''' <summary>
  ''' A sampler function to demonstrate how to modify an element through its properties. 
  ''' Using a door as an example here. 
  ''' </summary> 
  Sub ModifyElementPropertiesDoor(ByVal e As Element)

    ' Constant to this function.         
    ' This is for a door. e.g., "M_Single-Flush: 0762 x 2032mm"
    ' You can modify this to fit your need.   

    Const doorFamilyName As String = Util.Constant.DoorFamilyName
    Const doorTypeName As String = "0762 x 2032mm"
    Const doorFamilyAndTypeName As String = doorFamilyName + ": " + doorTypeName

    ' For simplicity, we assume we can only modify a door 
    If Not (TypeOf e Is FamilyInstance) Then
      TaskDialog.Show( _
        "Modify element properties - door", _
        "Sorry, I only know how to modify a door. Please select a door.")
      Return
    End If
    Dim aDoor As FamilyInstance = e

    Dim msg As String = "Door changed: " + vbCr + vbCr  'keep the message to the user. 

    ' (1) change its family type to a different one.  

    Dim newDoorType As Element = _
    ElementFiltering.FindFamilyType(_doc, GetType(FamilySymbol), _
        doorFamilyName, doorTypeName, BuiltInCategory.OST_Doors)

    If newDoorType IsNot Nothing Then
      aDoor.Symbol = newDoorType
      msg += "Door type to: " + doorFamilyAndTypeName + vbCr
      'TaskDialog.Show("Modify element properties - door", msg)
    End If

    ' (2) change its parameters. 
    ' leave this as your exercise. 


    ' message to the user. 
    TaskDialog.Show("Modify element properties - door", msg)

  End Sub

  ''' <summary>
  ''' A sampler function that demonstrates how to modify an element through document methods. 
  ''' </summary> 
  Sub ModifyElementByDocumentMethods(ByVal e As Element)

    Dim msg As String = "The element changed: " + vbCr + vbCr  'keep the message to the user. 

    ' Try move 
    Dim dt As Double = MmToFeet(1000.0)
    ' Hard cording for simplicity. 
    Dim v As XYZ = New XYZ(dt, dt, 0.0)
    ElementTransformUtils.MoveElement(e.Document, e.Id, v) ' 2012

    msg += "move by (1000, 1000, 0)" + vbCr

    ' Try rotate: 15 degree around z-axis. 
    Dim pt1 = XYZ.Zero
    Dim pt2 = XYZ.BasisZ
    Dim axis As Line = Line.CreateBound(pt1, pt2)
    ElementTransformUtils.RotateElement(e.Document, e.Id, axis, Math.PI / 12.0) ' 2012

    msg += "rotate by 15 degree around Z-axis" + vbCr

    ' Message to the user. 
    TaskDialog.Show("Modify element by document methods", msg)

  End Sub

End Class
