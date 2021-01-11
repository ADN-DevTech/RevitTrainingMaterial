#region Copyright
//
// Copyright (C) 2009-2021 by Autodesk, Inc.
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
#endregion // Copyright

#region Namespaces
using System;
using System.Diagnostics;
using WinForms = System.Windows.Forms;
#endregion // Namespaces

namespace FamilyCs
{
  public class Util
  {
    #region Formatting and message handlers
    public const string Caption = "Revit Family API Labs";

    /// <summary>
    /// MessageBox wrapper for informational message.
    /// </summary>
    public static void InfoMsg(string msg)
    {
      Debug.WriteLine(msg);
      WinForms.MessageBox.Show(msg, Caption, WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
    }

    /// <summary>
    /// MessageBox wrapper for error message.
    /// </summary>
    public static void ErrorMsg(string msg)
    {
      WinForms.MessageBox.Show(msg, Caption, WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
    }
    #endregion // Formatting and message handlers
  }
}
