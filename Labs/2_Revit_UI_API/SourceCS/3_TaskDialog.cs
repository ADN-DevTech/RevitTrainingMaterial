#region Copyright
//
// Copyright (C) 2009-2015 by Autodesk, Inc.
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
//
// Migrated to C# by Saikat Bhattacharya
// 
#endregion // Copyright

#region Namespaces
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Util;
#endregion

namespace UiCs
{
  /// <summary>
  /// Task Dialog 
  /// 
  /// cf. Developer Guide, Section 3.9 Revit-style Task Dialogs (pp55) 
  /// Appexdix G. API User Interface Guidelines (pp381), Task Dialog (pp404) 
  /// </summary>
  [Transaction(TransactionMode.ReadOnly)]
  public class UITaskDialog : IExternalCommand
  {
    // Member variables 
    UIApplication _uiApp;
    UIDocument _uiDoc;

    public Result Execute( 
      ExternalCommandData commandData, 
      ref string message, 
      ElementSet elements )
    {
      // Get the access to the top most objects. (we may not use them all in this specific lab.) 
      _uiApp = commandData.Application;
      _uiDoc = _uiApp.ActiveUIDocument;

      // (1) static TaskDialog.Show() 
      // We have been using this already. let's see what else we can do with it. 
      //ShowTaskDialogStatic(); 

      // (2) use an instance of TaskDialog 
      // This way has more option to customize 
      // Let's see what we can do. 
      
      ShowTaskDialogInstance( true );

      return Result.Succeeded;
    }

    /// <summary>
    /// Task Dialog static sampler 
    /// There are three overloads for static Show(). 
    /// </summary>
    public void ShowTaskDialogStatic()
    {
      // (1) simplest of all. title and main instruction. has default [Close] button at lower right corner. 
      TaskDialog.Show( "Task Dialog Static 1", "Main message" );

      // (2) this version accepts command buttons in addition to above. 
      // Here we add [Yes] [No] [Cancel} 
      
      TaskDialogResult res2 = default( TaskDialogResult );
      res2 = TaskDialog.Show( "Task Dialog Static 2", "Main message", ( TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel ) );

      // What did the user pressed? 
      TaskDialog.Show( "Show task dialog", "You pressed: " + res2.ToString() );

      // (3) this version accepts default button in addition to above. 
      // Here we set [No] as a default (just for testing purposes). 
      
      TaskDialogResult res3 = default( TaskDialogResult );
      TaskDialogResult defaultButton = TaskDialogResult.No;
      res3 = TaskDialog.Show( "Task Dialog Static 3", "Main message", ( TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel ), defaultButton );

      // What did the user press? 

      TaskDialog.Show("Show task dialog", "You pressed: " + res3.ToString());
    }

    /// <summary>
    /// Task Dialog - create an instance of task dialog gives you more options. 
    /// cf. Developer guide, Figure 223 (on pp 405) has a image of all the components visible. 
    /// This function is to visulize what kind of contents you can add with TaskDialog. 
    /// Note: actual interpretation of 
    /// </summary>
    public void ShowTaskDialogInstance( bool stepByStep )
    {
      // (0) create an instance of task dialog to set more options. 
      TaskDialog myDialog = new TaskDialog( "Revit UI Labs - Task Dialog Options" );
      if( stepByStep ) myDialog.Show();

      // (1) set the main area. these appear at the upper portion of the dialog. 
       
      myDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
      // or TaskDialogIcon.TaskDialogIconNone. 
      if( stepByStep ) myDialog.Show();

      myDialog.MainInstruction = "Main instruction: This is Revit UI Lab 3 Task Dialog";
      if( stepByStep ) myDialog.Show();

      myDialog.MainContent = "Main content: You can add detailed description here.";
      if( stepByStep ) myDialog.Show();

      // (2) set the bottom area 
       
      myDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel;
      myDialog.DefaultButton = TaskDialogResult.Yes;
      if( stepByStep ) myDialog.Show();

      myDialog.ExpandedContent = "Expanded content: the visibility of this portion is controled by Show/Hide button.";
      if( stepByStep ) myDialog.Show();

      myDialog.VerificationText = "Verification: Do not show this message again comes here";
      if( stepByStep ) myDialog.Show();

      myDialog.FooterText = "Footer: <a href=\"http://www.autodesk.com/developrevit\">Revit Developer Center</a>";
      if( stepByStep ) myDialog.Show();

      // (4) add command links. you can add up to four links 
       
      myDialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink1, "Command Link 1", "description 1" );
      if( stepByStep ) myDialog.Show();
      myDialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink2, "Command Link 2", "description 2" );
      if( stepByStep ) myDialog.Show();
      myDialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink3, "Command Link 3", "you can add up to four command links" );
      if( stepByStep ) myDialog.Show();
      myDialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink4, "Command Link 4", "Can also have URLs e.g. Revit Product Online Help" );
      //if (stepByStep) myDialog.Show(); 

      // Show it. 
      TaskDialogResult res = myDialog.Show();
      if( TaskDialogResult.CommandLink4 == res )
      {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        // process.StartInfo.FileName = "http://docs.autodesk.com/REVIT/2011/ENU/landing.html";
        //process.StartInfo.FileName = "http://wikihelp.autodesk.com/Revit/enu/2012";
        process.StartInfo.FileName = "http://wikihelp.autodesk.com/Revit/enu/2013";
        process.Start();
      }

      TaskDialog.Show("Show task dialog", "The last action was: " + res.ToString());
    }
  }

  /// <summary>
  /// Create House with Dialog added 
  /// 
  /// Show a task dialog and ask the user if he/she wants to create a house interactively or automatically. 
  /// </summary> 
  [Transaction(TransactionMode.Manual)]
  public class UICreateHouseDialog : IExternalCommand
  {
    // Member variables 
    UIApplication _uiApp;
    UIDocument _uiDoc;

    public Result Execute( 
      ExternalCommandData commandData, 
      ref string message, 
      ElementSet elements )
    {
      // Get access to the top most objects. (we may not use them all in this specific lab.) 
      _uiApp = commandData.Application;
      _uiDoc = _uiApp.ActiveUIDocument;

      // (1) create an instance of task dialog to set more options. 
       
      TaskDialog houseDialog = new TaskDialog( "Revit UI Labs - Create House Dialog" );
      houseDialog.MainInstruction = "Create a house";
      houseDialog.MainContent = "There are two options to create a house.";
      houseDialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink1, "Interactive", "You will pick two corners of rectangular footprint of a house, and choose where you want to add a front door." );
      houseDialog.AddCommandLink( TaskDialogCommandLinkId.CommandLink2, "Automatic", "This is will automatically place a house with a default settings." );
      houseDialog.CommonButtons = TaskDialogCommonButtons.Cancel;
      houseDialog.DefaultButton = TaskDialogResult.CommandLink1;

      // Show the dialog to the user. 
       
      TaskDialogResult res = houseDialog.Show();

      //TaskDialog.Show( "Create house dialog", "The last action was: " + res.ToString()); 

      // (2) pause the result and create a house with the method that use has chosen. 
      // 
      // Create a house interactively. 
      if( res == TaskDialogResult.CommandLink1 )
      {
        UICreateHouse.CreateHouseInteractive( _uiDoc );
        return Result.Succeeded;
      }

      // Create a house automatically with the default settings. 
      if( res == TaskDialogResult.CommandLink2 )
      {
        IntroCs.ModelCreationExport.CreateHouse( _uiDoc.Document );
        return Result.Succeeded;
      }

      // Request canceled. 
      if( res == TaskDialogResult.Cancel )
      {
        return Result.Cancelled;
      }
      return Result.Succeeded;
    }
  }
}
