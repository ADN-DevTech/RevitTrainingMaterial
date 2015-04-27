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
// Ported from old rac labs by Jeremy Tammik
// 
#endregion // Copyright

#region Namespaces
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Util;
using System.IO;
#endregion

namespace IntroCs
{
  /// <summary>
  /// Create a new shared parameter, then set and retrieve its value.
  /// In this example, we store a fire rating value on all doors.
  /// Please also look at the FireRating Revit SDK sample.
  /// </summary>
  [Transaction( TransactionMode.Manual )]
  class SharedParameter : IExternalCommand
  {
    const string kSharedParamsGroupAPI = "API Parameters";
    const string kSharedParamsDefFireRating = "API FireRating";
    const string kSharedParamsPath = "C:\\temp\\SharedParams.txt";

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIDocument uidoc = commandData.Application.ActiveUIDocument;
      Application app = commandData.Application.Application;
      Document doc = uidoc.Document;

      // Get the current shared params definition file
      DefinitionFile sharedParamsFile = GetSharedParamsFile( app );
      if( null == sharedParamsFile )
      {
        message = "Error getting the shared params file.";
        return Result.Failed;
      }

      // Get or create the shared params group
      DefinitionGroup sharedParamsGroup = GetOrCreateSharedParamsGroup(
        sharedParamsFile, kSharedParamsGroupAPI );
      if( null == sharedParamsGroup )
      {
        message = "Error getting the shared params group.";
        return Result.Failed;
      }

      Category cat = doc.Settings.Categories.get_Item( BuiltInCategory.OST_Doors );

      // Visibility of the new parameter:
      // Category.AllowsBoundParameters property indicates if a category can 
      // have shared or project parameters. If it is false, it may not be bound 
      // to shared parameters using the BindingMap. Please note that non-user-visible 
      // parameters can still be bound to these categories. 
      bool visible = cat.AllowsBoundParameters;

      // Get or create the shared params definition
      Definition fireRatingParamDef = GetOrCreateSharedParamsDefinition(
        sharedParamsGroup, ParameterType.Number, kSharedParamsDefFireRating, visible );
      if( null == fireRatingParamDef )
      {
        message = "Error in creating shared parameter.";
        return Result.Failed;
      }

      // Create the category set for binding and add the category
      // we are interested in, doors or walls or whatever:
      CategorySet catSet = app.Create.NewCategorySet();
      try
      {
        catSet.Insert( cat );
      }
      catch( Exception )
      {
        message = string.Format(
          "Error adding '{0}' category to parameters binding set.",
          cat.Name );
        return Result.Failed;
      }

      using( Transaction transaction = new Transaction( doc ) )
      {
        transaction.Start( "Bind parameter" );
        // Bind the param
        try
        {
          Binding binding = app.Create.NewInstanceBinding( catSet );
          // We could check if already bound, but looks like Insert will just ignore it in such case
          doc.ParameterBindings.Insert( fireRatingParamDef, binding );
          transaction.Commit();
        }
        catch( Exception ex )
        {
          message = ex.Message;
          transaction.RollBack();
          return Result.Failed;
        }
      }

      return Result.Succeeded;
    }

    /// <summary>
    /// Helper to get shared parameters file.
    /// </summary>
    public static DefinitionFile GetSharedParamsFile( Application app )
    {
      // Get current shared params file name
      string sharedParamsFileName;
      try
      {
        sharedParamsFileName = app.SharedParametersFilename;
      }
      catch( Exception ex )
      {
        TaskDialog.Show( "Get shared params file", "No shared params file set:" + ex.Message );
        return null;
      }

      if( 0 == sharedParamsFileName.Length ||
        !System.IO.File.Exists( sharedParamsFileName ) )
      {
        StreamWriter stream;
        stream = new StreamWriter( kSharedParamsPath );
        stream.Close();
        app.SharedParametersFilename = kSharedParamsPath;
        sharedParamsFileName = app.SharedParametersFilename;
      }

      // Get the current file object and return it
      DefinitionFile sharedParametersFile;
      try
      {
        sharedParametersFile = app.OpenSharedParameterFile();
      }
      catch( Exception ex )
      {
        TaskDialog.Show( "Get shared params file", "Cannnot open shared params file:" + ex.Message );
        sharedParametersFile = null;
      }
      return sharedParametersFile;
    }

    public static DefinitionGroup GetOrCreateSharedParamsGroup(
     DefinitionFile sharedParametersFile,
     string groupName )
    {
      DefinitionGroup g = sharedParametersFile.Groups.get_Item( groupName );
      if( null == g )
      {
        try
        {
          g = sharedParametersFile.Groups.Create( groupName );
        }
        catch( Exception )
        {
          g = null;
        }
      }
      return g;
    }

    public static Definition GetOrCreateSharedParamsDefinition(
      DefinitionGroup defGroup,
      ParameterType defType,
      string defName,
      bool visible )
    {
      Definition definition = defGroup.Definitions.get_Item( defName );
      if( null == definition )
      {
        try
        {
          //definition = defGroup.Definitions.Create(defName, defType, visible);

          // 'Autodesk.Revit.DB.Definitions.Create(string, Autodesk.Revit.DB.ParameterType, bool)' is obsolete: 
          // 'This method is deprecated in Revit 2015. Use Create(Autodesk.Revit.DB.ExternalDefinitonCreationOptions) instead'

          // Modified code for Revit 2015
          // and fixed typo in class name in Revit 2016

          ExternalDefinitionCreationOptions opt
            = new ExternalDefinitionCreationOptions(
              defName, defType );

          opt.Visible = true;
          definition = defGroup.Definitions.Create( opt );

        }
        catch( Exception )
        {
          definition = null;
        }
      }
      return definition;
    }
  }

  [Transaction( TransactionMode.Automatic )]
  public class PerDocParameter : IExternalCommand
  {
    public const string kParamGroupName = "Per-doc Params";
    public const string kParamNameVisible = "Visible per-doc Integer";
    public const string kParamNameInvisible = "Invisible per-doc Integer";

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIDocument uiDoc = commandData.Application.ActiveUIDocument;
      Application app = commandData.Application.Application;
      Document doc = uiDoc.Document;

      // get the current shared params definition file
      DefinitionFile sharedParamsFile = SharedParameter.GetSharedParamsFile( app );
      if( null == sharedParamsFile )
      {
        TaskDialog.Show( "Per document parameter", "Error getting the shared params file." );
        return Result.Failed;
      }
      // get or create the shared params group
      DefinitionGroup sharedParamsGroup = SharedParameter.GetOrCreateSharedParamsGroup( sharedParamsFile, kParamGroupName );
      if( null == sharedParamsGroup )
      {
        TaskDialog.Show( "Per document parameter", "Error getting the shared params group." );
        return Result.Failed;
      }
      // visible param
      Definition docParamDefVisible = SharedParameter.GetOrCreateSharedParamsDefinition( sharedParamsGroup, ParameterType.Integer, kParamNameVisible, true );
      if( null == docParamDefVisible )
      {
        TaskDialog.Show( "Per document parameter", "Error creating visible per-doc parameter." );
        return Result.Failed;
      }
      // invisible param
      Definition docParamDefInvisible = SharedParameter.GetOrCreateSharedParamsDefinition( sharedParamsGroup, ParameterType.Integer, kParamNameInvisible, false );
      if( null == docParamDefInvisible )
      {
        TaskDialog.Show( "Per document parameter", "Error creating invisible per-doc parameter." );
        return Result.Failed;
      }
      // bind the param
      try
      {
        CategorySet catSet = app.Create.NewCategorySet();
        catSet.Insert( doc.Settings.Categories.get_Item( BuiltInCategory.OST_ProjectInformation ) );
        Binding binding = app.Create.NewInstanceBinding( catSet );
        doc.ParameterBindings.Insert( docParamDefVisible, binding );
        doc.ParameterBindings.Insert( docParamDefInvisible, binding );
      }
      catch( Exception e )
      {
        TaskDialog.Show( "Per document parameter", "Error binding shared parameter: " + e.Message );
        return Result.Failed;
      }
      // set the initial values
      // get the singleton project info element
      Element projInfoElem = GetProjectInfoElem( doc );

      if( null == projInfoElem )
      {
        TaskDialog.Show( "Per document parameter", "No project info elem found. Aborting command..." );
        return Result.Failed;
      }
      // for simplicity, access params by name rather than by GUID:
      //projInfoElem.get_Parameter(kParamNameVisible).Set(55);
      //projInfoElem.get_Parameter(kParamNameInvisible).Set(0);

      // 'Autodesk.Revit.DB.Element.get_Parameter(string)' is obsolete: 
      // 'This property is obsolete in Revit 2015, as more than one parameter can have the same name on a given element. 
      // Use Element.Parameters to obtain a complete list of parameters on this Element, 
      // or Element.GetParameters(String) to get a list of all parameters by name, 
      // or Element.LookupParameter(String) to return the first available parameter with the given name.

      // modified code for Revit 2015
      projInfoElem.LookupParameter( kParamNameVisible ).Set( 55 );
      projInfoElem.LookupParameter( kParamNameVisible ).Set( 0 );


      return Result.Succeeded;
    }

    /// <summary>
    /// Return the one and only project information element using Revit 2009 filtering
    /// by searching for the "Project Information" category. Only one such element exists.
    /// </summary>
    public static Element GetProjectInfoElem( Document doc )
    {
      FilteredElementCollector collector = new FilteredElementCollector( doc );
      collector.OfCategory( BuiltInCategory.OST_ProjectInformation );
      IList<Element> elems = collector.ToElements();

      Debug.Assert( elems.Count == 1, "There should be exactly one of this object in the project" );

      return elems[0];
    }
  }
}
