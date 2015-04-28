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
Imports IntroVb.Util.Constant
Imports IntroVb.Util
#End Region

''' <summary>
''' Create a new shared parameter, then set and retrieve its value.
''' In this example, we store a fire rating value on all doors.
''' Please also look at the FireRating Revit SDK sample.
''' </summary>
<Transaction(TransactionMode.Manual)> _
Class SharedParameter
  Implements IExternalCommand
  Const kSharedParamsGroupAPI As String = "API Parameters"
  Const kSharedParamsDefFireRating As String = "API FireRating"
  Const kSharedParamsPath As String = "C:\temp\SharedParams.txt"

  Public Function Execute(ByVal commandData As ExternalCommandData, ByRef message As String, ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute
    Dim uidoc As UIDocument = commandData.Application.ActiveUIDocument
    Dim app As Application = commandData.Application.Application
    Dim doc As Document = uidoc.Document

    ' Get the current shared params definition file
    Dim sharedParamsFile As DefinitionFile = GetSharedParamsFile(app)
    If sharedParamsFile Is Nothing Then
      message = "Error getting the shared params file."
      Return Result.Failed
    End If

    ' Get or create the shared params group
    Dim sharedParamsGroup As DefinitionGroup = GetOrCreateSharedParamsGroup(sharedParamsFile, kSharedParamsGroupAPI)
    If sharedParamsGroup Is Nothing Then
      message = "Error getting the shared params group."
      Return Result.Failed
    End If

    Dim cat As Category = doc.Settings.Categories.Item(BuiltInCategory.OST_Doors)

    ' Visibility of the new parameter:
    ' Category.AllowsBoundParameters property indicates if a category can 
    ' have shared or project parameters. If it is false, it may not be bound 
    ' to shared parameters using the BindingMap. Please note that non-user-visible 
    ' parameters can still be bound to these categories. 
    Dim visible As Boolean = cat.AllowsBoundParameters

    ' Get or create the shared params definition
    Dim fireRatingParamDef As Definition = GetOrCreateSharedParamsDefinition(sharedParamsGroup, ParameterType.Number, kSharedParamsDefFireRating, visible)
    If fireRatingParamDef Is Nothing Then
      message = "Error in creating shared parameter."
      Return Result.Failed
    End If

    ' Create the category set for binding and add the category
    ' we are interested in, doors or walls or whatever:
    Dim catSet As CategorySet = app.Create.NewCategorySet()
    Try
      catSet.Insert(cat)
    Catch generatedExceptionName As Exception
      message = String.Format("Error adding '{0}' category to parameters binding set.", cat.Name)
      Return Result.Failed
    End Try

    ' Bind the param
    Try
      Dim binding As Binding = app.Create.NewInstanceBinding(catSet)
      ' We could check if already bound, but looks like Insert will just ignore it in such case
      doc.ParameterBindings.Insert(fireRatingParamDef, binding)
    Catch ex As Exception
      message = ex.Message
      Return Result.Failed
    End Try

    Return Result.Succeeded
  End Function

  ''' <summary>
  ''' Helper to get shared parameters file.
  ''' </summary>
  Public Shared Function GetSharedParamsFile(ByVal app As Application) As DefinitionFile
    ' Get current shared params file name
    Dim sharedParamsFileName As String
    Try
      sharedParamsFileName = app.SharedParametersFilename
    Catch ex As Exception
      TaskDialog.Show("Get shared params file", "No shared params file set:" + ex.Message)
      Return Nothing
    End Try

    If 0 = sharedParamsFileName.Length Or Not System.IO.File.Exists(sharedParamsFileName) Then
      Dim stream As StreamWriter
      stream = New StreamWriter(kSharedParamsPath)
      stream.Close()
      app.SharedParametersFilename = kSharedParamsPath
      sharedParamsFileName = app.SharedParametersFilename
    End If

    ' Get the current file object and return it
    Dim sharedParametersFile As DefinitionFile
    Try
      sharedParametersFile = app.OpenSharedParameterFile()
    Catch ex As Exception
      TaskDialog.Show("Get shared params file", "Cannnot open shared params file:" + ex.Message)
      sharedParametersFile = Nothing
    End Try
    Return sharedParametersFile
  End Function

  Public Shared Function GetOrCreateSharedParamsGroup(ByVal sharedParametersFile As DefinitionFile, ByVal groupName As String) As DefinitionGroup
    Dim g As DefinitionGroup = sharedParametersFile.Groups.Item(groupName)
    If g Is Nothing Then
      Try
        g = sharedParametersFile.Groups.Create(groupName)
      Catch generatedExceptionName As Exception
        g = Nothing
      End Try
    End If
    Return g
  End Function

  Public Shared Function GetOrCreateSharedParamsDefinition(ByVal defGroup As DefinitionGroup, ByVal defType As ParameterType, ByVal defName As String, ByVal visible As Boolean) As Definition
    Dim definition As Definition = defGroup.Definitions.Item(defName)
    If definition Is Nothing Then
      Try

        ''Public Function Create(name As String, type As Autodesk.Revit.DB.ParameterType, visible As Boolean) 
        ''As Autodesk.Revit.DB.Definition' is obsolete: 
        'This method is deprecated in Revit 2015. 
        'Use Create(Autodesk.Revit.DB.ExternalDefinitonCreationOptions) instead'

        'definition = defGroup.Definitions.Create(defName, defType, visible)

        ' updated for Revit 2015
        Dim extDefCrOptions As ExternalDefinitionCreationOptions _
          = New ExternalDefinitionCreationOptions(defName, defType)

        extDefCrOptions.Visible = True
        definition = defGroup.Definitions.Create(extDefCrOptions)

      Catch generatedExceptionName As Exception
        definition = Nothing
      End Try
    End If
    Return definition
  End Function
End Class

<Transaction(TransactionMode.Manual)> _
Public Class PerDocParameter
  Implements IExternalCommand
  Public Const kParamGroupName As String = "Per-doc Params"
  Public Const kParamNameVisible As String = "Visible per-doc Integer"
  Public Const kParamNameInvisible As String = "Invisible per-doc Integer"

  Public Function Execute(ByVal commandData As ExternalCommandData, ByRef message As String, ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute
    Dim uiDoc As UIDocument = commandData.Application.ActiveUIDocument
    Dim app As Application = commandData.Application.Application
    Dim doc As Document = uiDoc.Document

    ' get the current shared params definition file
    Dim sharedParamsFile As DefinitionFile = SharedParameter.GetSharedParamsFile(app)
    If sharedParamsFile Is Nothing Then
      TaskDialog.Show("Per document parameter", "Error getting the shared params file.")
      Return Result.Failed
    End If
    ' get or create the shared params group
    Dim sharedParamsGroup As DefinitionGroup = SharedParameter.GetOrCreateSharedParamsGroup(sharedParamsFile, kParamGroupName)
    If sharedParamsGroup Is Nothing Then
      TaskDialog.Show("Per document parameter", "Error getting the shared params group.")
      Return Result.Failed
    End If
    ' visible param
    Dim docParamDefVisible As Definition = SharedParameter.GetOrCreateSharedParamsDefinition(sharedParamsGroup, ParameterType.[Integer], kParamNameVisible, True)
    If docParamDefVisible Is Nothing Then
      TaskDialog.Show("Per document parameter", "Error creating visible per-doc parameter.")
      Return Result.Failed
    End If
    ' invisible param
    Dim docParamDefInvisible As Definition = SharedParameter.GetOrCreateSharedParamsDefinition(sharedParamsGroup, ParameterType.[Integer], kParamNameInvisible, False)
    If docParamDefInvisible Is Nothing Then
      TaskDialog.Show("Per document parameter", "Error creating invisible per-doc parameter.")
      Return Result.Failed
    End If
    ' bind the param
    Try
      Dim catSet As CategorySet = app.Create.NewCategorySet()
      catSet.Insert(doc.Settings.Categories.Item(BuiltInCategory.OST_ProjectInformation))
      Dim binding As Binding = app.Create.NewInstanceBinding(catSet)
      doc.ParameterBindings.Insert(docParamDefVisible, binding)
      doc.ParameterBindings.Insert(docParamDefInvisible, binding)
    Catch e As Exception
      TaskDialog.Show("Per document parameter", "Error binding shared parameter: " + e.Message)
      Return Result.Failed
    End Try
    ' set the initial values
    ' get the singleton project info element
    Dim projInfoElem As Element = GetProjectInfoElem(doc)

    If projInfoElem Is Nothing Then
      TaskDialog.Show("Per document parameter", "No project info elem found. Aborting command...")
      Return Result.Failed
    End If
    ' for simplicity, access params by name rather than by GUID:

    '' 'Get' accessor of 'Public ReadOnly Property Parameter(paramName As String) As Autodesk.Revit.DB.Parameter' is obsolete: 
    'This property is obsolete in Revit 2015

    'projInfoElem.Parameter(kParamNameVisible).Set(55)  ' Revit 2014 or earlier
    'projInfoElem.Parameter(kParamNameInvisible).Set(0) ' Revit 2014 or earlier

    projInfoElem.LookupParameter(kParamNameVisible).Set(55)
    projInfoElem.LookupParameter(kParamNameInvisible).Set(0)

    Return Result.Succeeded
  End Function

  ''' <summary>
  ''' Return the one and only project information element using Revit 2009 filtering
  ''' by searching for the "Project Information" category. Only one such element exists.
  ''' </summary>
  Public Shared Function GetProjectInfoElem(ByVal doc As Document) As Element
    Dim collector As New FilteredElementCollector(doc)
    collector.OfCategory(BuiltInCategory.OST_ProjectInformation)
    Dim elems As IList(Of Element) = collector.ToElements()

    Debug.Assert(elems.Count = 1, "There should be exactly one of this object in the project")

    Return elems(0)
  End Function
End Class
