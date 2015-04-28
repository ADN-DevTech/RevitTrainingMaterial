#region Copyright
//
// Copyright (C) 2010-2014 by Autodesk, Inc.
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
#endregion // Copyright

#region Namespaces
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices; // This is for Revit Application 
#endregion

#region Description
// Revit Intro Lab - 1 
// 
// In this lab, you will learn how to "hook" your add-on program to Revit. 
// This command defines a minimum external command. 
// 
// Explain about addin manifest. How to create GUID. 
// Hello World in VB.NET is from page 367 of Developer Guide. 
#endregion

namespace IntroCs
{
  /// <summary>
  /// Hello World #1 - A minimum Revit external command. 
  /// </summary>
  [Autodesk.Revit.Attributes.Transaction( Autodesk.Revit.Attributes.TransactionMode.Manual )]
  public class HelloWorld : Autodesk.Revit.UI.IExternalCommand
  {
    public Autodesk.Revit.UI.Result Execute(
      Autodesk.Revit.UI.ExternalCommandData commandData,
      ref string message,
      Autodesk.Revit.DB.ElementSet elements )
    {
      Autodesk.Revit.UI.TaskDialog.Show(
        "My Dialog Title",
        "Hello World!" );

      return Autodesk.Revit.UI.Result.Succeeded;
    }
  }

  /// <summary>
  /// Hello World #2 - simplified without full namespace 
  /// and use ReadOnly attribute.
  /// </summary>
  [Transaction( TransactionMode.ReadOnly )]
  public class HelloWorldSimple : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      TaskDialog.Show(
        "My Dialog Title",
        "Hello World Simple!" );

      return Result.Succeeded;
    }
  }

  /// <summary>
  /// Hello World #3 - minimum external application 
  /// Difference: IExternalApplication instead of IExternalCommand. 
  /// In addin manifest, use addin type "Application" and Name instead of Text tag. 
  /// </summary>
  public class HelloWorldApp : IExternalApplication
  {
    // OnStartup() - called when Revit starts. 

    public Result OnStartup( UIControlledApplication app )
    {
      TaskDialog.Show( "My Dialog Title", "Hello World from App!" );

      return Result.Succeeded;
    }

    // OnShutdown() - called when Revit ends. 

    public Result OnShutdown( UIControlledApplication app )
    {
      return Result.Succeeded;
    }
  }

  /// <summary>
  /// Command Arguments 
  /// Take a look at the command arguments. 
  /// commandData is the topmost object and 
  /// provides the entry point to the Revit model. 
  /// </summary>
  [Transaction( TransactionMode.ReadOnly )]
  public class CommandData : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      // The first argument, commandData, provides access to the top most object model. 
      // You will get the necessary information from commandData. 
      // To see what's in there, print out a few data accessed from commandData 
      // 
      // Exercise: Place a break point at commandData and drill down the data. 

      UIApplication uiApp = commandData.Application;
      Application rvtApp = uiApp.Application;
      UIDocument uiDoc = uiApp.ActiveUIDocument;
      Document rvtDoc = uiDoc.Document;

      // Print out a few information that you can get from commandData 
      string versionName = rvtApp.VersionName;
      string documentTitle = rvtDoc.Title;

      TaskDialog.Show(
        "Revit Intro Lab",
        "Version Name = " + versionName
        + "\nDocument Title = " + documentTitle );

      // Print out a list of wall types available in the current rvt project:

      //WallTypeSet wallTypes = rvtDoc.WallTypes; // 2013, deprecated in 2014

      FilteredElementCollector wallTypes // 2014
        = new FilteredElementCollector( rvtDoc )
          .OfClass( typeof( WallType ) );

      string s = "";
      foreach( WallType wallType in wallTypes )
      {
        s += wallType.Name + "\r\n";
      }

      // Show the result:

      TaskDialog.Show(
        "Revit Intro Lab",
        "Wall Types (in main instruction):\n\n" + s );

      // 2nd and 3rd arguments are when the command fails. 
      // 2nd - set a message to the user. 
      // 3rd - set elements to highlight. 

      return Result.Succeeded;
    }
  }
}
