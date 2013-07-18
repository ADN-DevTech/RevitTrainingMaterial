#Region "Copyright"
'
' Copyright (C) 2010-2013 by Autodesk, Inc.
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
' Created by Saikat Bhattacharaya and added to Revit API Labs by Jeremy Tammik
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
'Imports Autodesk.Revit.DB.ExtensibleStorage ' need for extensible storage 
Imports IntroVb.Util.Constant
Imports IntroVb.Util
#End Region

''' <summary>
''' Revit Intro Lab 6.
''' This lab demonstrates the new extensible storage functionality. 
''' In this example, we store the location of a wall socket into 
''' extensible data stored on the wall.
''' Please also look at the ExtensibleStorageManager Revit SDK sample.
''' </summary>
<Transaction(TransactionMode.Manual)> _
Friend Class ExtensibleStorage
  Implements IExternalCommand

  ''' <summary>
  ''' The schema specific GUID.
  ''' </summary>
  Private _guid As Guid = New Guid("87aaad89-6f1b-45e1-9397-2985e1560a02")

  ''' <summary>
  ''' Allow only walls to be selected.
  ''' </summary>
  Private Class WallSelectionFilter
    Implements ISelectionFilter

    Public Function AllowElement(ByVal e As Element) As Boolean Implements ISelectionFilter.AllowElement
      Return TypeOf e Is Wall
    End Function

    Public Function AllowReference(ByVal r As Reference, ByVal p As XYZ) As Boolean Implements ISelectionFilter.AllowReference
      Return True
    End Function

  End Class

  Public Function Execute( _
    ByVal commandData As ExternalCommandData, _
    ByRef message As String, _
    ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute

    Dim uiDoc As UIDocument = commandData.Application.ActiveUIDocument
    Dim doc As Document = uiDoc.Document

    ' Create transaction for working with schema

    Dim trans As New Transaction(doc, "Extensible Storage")
    trans.Start()

    ' Select a wall element

    Dim wall As Wall = Nothing
    Try
      Dim r As Reference = uiDoc.Selection.PickObject(ObjectType.Element, New WallSelectionFilter)
      wall = TryCast(doc.GetElement(r), Wall)
    Catch exception1 As Autodesk.Revit.Exceptions.OperationCanceledException
      message = "Nothing selected; please select a wall to attach extensible data to."
      Return Result.Failed
    End Try

    Debug.Assert(wall IsNot Nothing, "expected a wall to be selected")

    If wall Is Nothing Then
      message = "Please select a wall to attach extensible data to."
      Return Result.Failed
    End If

    ' Create a schema builder

    Dim builder As New SchemaBuilder(_guid)

    ' Set read and write access levels 

    builder.SetReadAccessLevel(AccessLevel.Public)
    builder.SetWriteAccessLevel(AccessLevel.Public)

    ' Note: if this was set as vendor or application access, 
    ' we would have been additionally required to use SetVendorId

    ' Set name to this schema builder

    builder.SetSchemaName("WallSocketLocation")
    builder.SetDocumentation("Data store for socket related info in a wall")

    ' Create field1

    Dim fieldBuilder1 As FieldBuilder = _
      builder.AddSimpleField("SocketLocation", GetType(XYZ)).SetUnitType(UnitType.UT_Length)

    ' Set unit type

    fieldBuilder1.SetUnitType(UnitType.UT_Length)

    ' Add documentation (optional)

    ' Create field2

    Dim fieldBuilder2 As FieldBuilder = _
      builder.AddSimpleField("SocketNumber", GetType(String))

    'fieldBuilder2.SetUnitType(UnitType.UT_Custom);

    ' Register the schema object

    Dim schema As Schema = builder.Finish

    ' Create an entity (object) for this schema (class)

    Dim ent As New Entity(schema)
    Dim socketLocation As Field = schema.GetField("SocketLocation")
    ent.Set(Of XYZ)(socketLocation, New XYZ(2, 0, 0), DisplayUnitType.DUT_METERS)

    Dim socketNumber As Field = schema.GetField("SocketNumber")
    ent.Set(Of String)(socketNumber, "200")

    wall.SetEntity(ent)

    ' Now create another entity (object) for this schema (class)

    Dim ent2 As New Entity(schema)
    Dim socketNumber1 As Field = schema.GetField("SocketNumber")
    ent2.Set(Of String)(socketNumber1, "400")
    wall.SetEntity(ent2)

    ' Note: this will replace the previous entity on the wall 

    ' List all schemas in the document

    Dim s As String = String.Empty
    Dim schemas As IList(Of Schema) = schema.ListSchemas
    Dim sch As Schema
    For Each sch In schemas
      s += vbCrLf + "Schema name: " + sch.SchemaName
    Next
    TaskDialog.Show("Schema details", s)

    ' List all Fields for our schema

    s = String.Empty
    Dim fields As IList(Of Field) = schema.Lookup(_guid).ListFields
    Dim fld As Field
    For Each fld In fields
      s += vbCrLf + "Field name: " + fld.FieldName
    Next
    TaskDialog.Show("Field details", s)

    ' Extract the value for the field we created

    Dim wallSchemaEnt As Entity = wall.GetEntity(schema.Lookup(_guid))

    Dim wallSocketPos As XYZ = wallSchemaEnt.Get(Of XYZ)( _
      schema.Lookup(_guid).GetField("SocketLocation"), _
      DisplayUnitType.DUT_METERS)

    s = "SocketLocation: " + Format.PointString(wallSocketPos)

    Dim wallSocketNumber As String = wallSchemaEnt.Get(Of String)( _
      schema.Lookup(_guid).GetField("SocketNumber"))

    s += vbCrLf + "SocketNumber: " + wallSocketNumber

    TaskDialog.Show("Field Values", s)

    trans.Commit()

    Return Result.Succeeded
  End Function
End Class
