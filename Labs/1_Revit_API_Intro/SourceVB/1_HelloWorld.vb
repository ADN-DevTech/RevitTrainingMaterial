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
' If you explicitely set them in each .vb file, you will need to specify full name spaces. 

'Imports System
'Imports Autodesk.Revit.DB
'Imports Autodesk.Revit.UI
'Imports Autodesk.Revit.Attributes ' Specify this if you want to save typing for attributes. 
'Imports Autodesk.Revit.ApplicationServices  ' This is for Revit Application Services 
#End Region

#Region "Description"
' Revit Intro Lab - 1 
'
' In this lab, you will learn how to "hook" your add-on program to Revit. 
' This command defines a minimum external command.
' 
' Explain about addin manifest. How to create GUID. 
' Hello World in VB.NET is from page 367 of Developer Guide. 
#End Region

''' <summary>
''' Hello World #1 - A minimum Revit external command. 
''' </summary>
<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)> _
Public Class HelloWorld
  Implements Autodesk.Revit.UI.IExternalCommand

  Public Function Execute( _
    ByVal commandData As Autodesk.Revit.UI.ExternalCommandData, _
    ByRef message As String, _
    ByVal elements As Autodesk.Revit.DB.ElementSet) _
    As Autodesk.Revit.UI.Result _
    Implements Autodesk.Revit.UI.IExternalCommand.Execute

    Autodesk.Revit.UI.TaskDialog.Show("My Dialog Title", "Hello World!")

    Return Autodesk.Revit.UI.Result.Succeeded

  End Function

End Class

''' <summary>
''' Hello World #2 - simplified without full namespace
''' and use ReadOnly attribute.   
''' </summary>
<Transaction(TransactionMode.ReadOnly)> _
Public Class HelloWorldSimple
  Implements IExternalCommand

  Public Function Execute( _
    ByVal commandData As ExternalCommandData, _
    ByRef message As String, _
    ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute

    TaskDialog.Show("My Dialog Title", "Hello World Simple!")
    Return Result.Succeeded

  End Function

End Class

''' <summary>
''' Hello World #3 - minimum external application 
''' difference: IExternalApplication instead of IExternalCommand. in addin manifest. 
''' Use addin type "Application", use <Name/> instead of <Text/>. 
''' </summary>
Public Class HelloWorldApp
  Implements IExternalApplication

  ' OnShutdown() - called when Revit ends. 

  Public Function OnShutdown(ByVal app As UIControlledApplication) _
    As Result _
    Implements IExternalApplication.OnShutdown

    Return Result.Succeeded

  End Function

  ' OnStartup() - called when Revit starts. 

  Public Function OnStartup(ByVal app As UIControlledApplication) _
    As Result _
    Implements IExternalApplication.OnStartup

    TaskDialog.Show("My Dialog Title", "Hello World from App!")

    Return Result.Succeeded

  End Function
End Class

''' <summary>
''' Command Arguments
''' Take a look at the command arguments. commandData is the top most
''' object and the entry point to the Revit model. 
''' </summary>

<Transaction(TransactionMode.ReadOnly)> _
Public Class CommandData
  Implements IExternalCommand

  Public Function Execute( _
    ByVal commandData As ExternalCommandData, _
    ByRef message As String, _
    ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute

    ' The first argument, commandData, is the top most in the object model.
    ' You will get the necessary information from commandData. 
    ' To see what's in there, print out a few data accessed from commandData 
    ' 
    ' Exercise: Place a break point at commandData and drill down the data. 

    Dim uiApp As UIApplication = commandData.Application
    Dim rvtApp As Application = uiApp.Application
    Dim uiDoc As UIDocument = uiApp.ActiveUIDocument
    Dim rvtDoc As Document = uiDoc.Document

    ' Print out a few information that you can get from commandData 
    Dim versionName As String = rvtApp.VersionName
    Dim documentTitle As String = rvtDoc.Title

    TaskDialog.Show( _
        "Revit Intro Lab", _
        "Version Name = " + versionName _
        + vbCr + "Document Title = " + documentTitle)

    ' Print out a list of wall types available in the current rvt project. 

    'Dim wallTypes As WallTypeSet = rvtDoc.WallTypes ' 2013, deprecated in 2014

    Dim wallTypes As FilteredElementCollector _
      = New FilteredElementCollector(rvtDoc) _
        .OfClass(GetType(WallType))

    Dim s As String = ""

    For Each wallType As WallType In wallTypes
      s += wallType.Name + vbCr
    Next

    ' Show the result:

    TaskDialog.Show(
      "Revit Intro Lab",
      "Wall Types (in main instruction):" + vbCr + vbCr + s)

    ' 2nd and 3rd arguments are when the command fails.  
    ' 2nd - set a message to the user.   
    ' 3rd - set elements to highlight. 

    Return Result.Succeeded

  End Function
End Class
