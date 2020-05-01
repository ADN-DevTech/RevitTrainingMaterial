#region Copyright
//
// Copyright (C) 2009-2020 by Autodesk, Inc.
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
// Created by Saikat Bhattacharaya and added to Revit API Labs by Jeremy Tammik
// 
#endregion // Copyright

#region Namespaces
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage; // needed for Extensible Storage 
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Util;
#endregion

namespace IntroCs
{
  /// <summary>
  /// Revit Intro Lab 6.
  /// This lab demonstrates the extensible storage functionality. 
  /// In this example, we store the location of a wall socket into 
  /// extensible data stored on the wall.
  /// Please also look at the ExtensibleStorageManager Revit SDK sample.
  /// </summary>
  [Transaction(TransactionMode.Manual)]
  class ExtensibleStorage : IExternalCommand
  {
    /// <summary>
    /// The schema specific GUID. 
    /// </summary>
    Guid _guid = new Guid("87aaad89-6f1b-45e1-9397-2985e1560a02");

    /// <summary>
    /// Allow only walls to be selected.
    /// </summary>
    class WallSelectionFilter : ISelectionFilter
    {
      public bool AllowElement(Element e)
      {
        return e is Wall;
      }

      public bool AllowReference(Reference r, XYZ p)
      {
        return true;
      }
    }

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIDocument uiDoc = commandData.Application.ActiveUIDocument;
      Document doc = uiDoc.Document;

      // Create transaction for working with schema

      Transaction trans = new Transaction(doc, "Extensible Storage");
      trans.Start();

      // Select a wall element

      Wall wall = null;

      try
      {
        Reference r = uiDoc.Selection.PickObject(ObjectType.Element,
          new WallSelectionFilter());

        wall = doc.GetElement(r) as Wall;
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException)
      {
        message = "Nothing selected; please select a wall to attach extensible data to.";
        return Result.Failed;
      }

      Debug.Assert(null != wall, "expected a wall to be selected");

      if (null == wall)
      {
        message = "Please select a wall to attach extensible data to.";
        return Result.Failed;
      }

      // Create a schema builder

      SchemaBuilder builder = new SchemaBuilder(_guid);

      // Set read and write access levels

      builder.SetReadAccessLevel(AccessLevel.Public);
      builder.SetWriteAccessLevel(AccessLevel.Public);

      // Note: if this was set as vendor or application access, 
      // we would have been additionally required to use SetVendorId

      // Set name to this schema builder

      builder.SetSchemaName("WallSocketLocation");
      builder.SetDocumentation("Data store for socket related info in a wall");

      // Create field1

      FieldBuilder fieldBuilder1 =
        builder.AddSimpleField("SocketLocation", typeof(XYZ));

      // Set unit type

      fieldBuilder1.SetUnitType(UnitType.UT_Length);

      // Add documentation (optional)

      // Create field2

      FieldBuilder fieldBuilder2 =
        builder.AddSimpleField("SocketNumber", typeof(string));

      //fieldBuilder2.SetUnitType(UnitType.UT_Custom);

      // Register the schema object

      Schema schema = builder.Finish();

      // Create an entity (object) for this schema (class)

      Entity ent = new Entity(schema);
      Field socketLocation = schema.GetField("SocketLocation");
      ent.Set<XYZ>(socketLocation, new XYZ(2, 0, 0), DisplayUnitType.DUT_METERS);

      Field socketNumber = schema.GetField("SocketNumber");
      ent.Set<string>(socketNumber, "200");

      wall.SetEntity(ent);

      // Now create another entity (object) for this schema (class)

      Entity ent2 = new Entity(schema);
      Field socketNumber1 = schema.GetField("SocketNumber");
      ent2.Set<String>(socketNumber1, "400");
      wall.SetEntity(ent2);

      // Note: this will replace the previous entity on the wall 

      // List all schemas in the document

      string s = string.Empty;
      IList<Schema> schemas = Schema.ListSchemas();
      foreach (Schema sch in schemas)
      {
        s += "\r\nSchema Name: " + sch.SchemaName;
      }
      TaskDialog.Show("Schema details", s);

      // List all Fields for our schema

      s = string.Empty;
      Schema ourSchema = Schema.Lookup(_guid);
      IList<Field> fields = ourSchema.ListFields();
      foreach (Field fld in fields)
      {
        s += "\r\nField Name: " + fld.FieldName;
      }
      TaskDialog.Show("Field details", s);

      // Extract the value for the field we created

      Entity wallSchemaEnt = wall.GetEntity(Schema.Lookup(_guid));

      XYZ wallSocketPos = wallSchemaEnt.Get<XYZ>(
        Schema.Lookup(_guid).GetField("SocketLocation"),
        DisplayUnitType.DUT_METERS);

      s = "SocketLocation: " + Format.PointString(wallSocketPos);

      string wallSocketNumber = wallSchemaEnt.Get<String>(
        Schema.Lookup(_guid).GetField("SocketNumber"));

      s += "\r\nSocketNumber: " + wallSocketNumber;

      TaskDialog.Show("Field values", s);

      trans.Commit();

      return Result.Succeeded;
    }
  }
}
