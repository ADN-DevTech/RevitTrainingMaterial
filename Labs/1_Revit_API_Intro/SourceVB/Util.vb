#Region "Copyright"
'
' Copyright (C) 2010-2014 by Autodesk, Inc.
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
#End Region

' Instruction: 
' To switch between Imperial vs. Matric constants. 
' either define a the following pre-processor directive: 
'#Const USE_IMPERIAL_FAMILY_NAMES_AND_TYPES = True
' or add "Not" in the #If statement. 
'#If Not USE_IMPERIAL_FAMILY_NAMES_AND_TYPES 
' In .Net, you can also use Conditional attribute. 

Imports Autodesk.Revit.DB ' for XYZ


Namespace Util
  Public Class Format
    Public Shared Function RealString(ByVal a As Double) As String
      Return a.ToString("0.##")
    End Function

    Public Shared Function PointString(ByVal p As XYZ) As String
      Return String.Format("({0},{1},{2})", RealString(p.X), RealString(p.Y), RealString(p.Z))
    End Function
  End Class

  ''' <summary>
  ''' Define some string constants for family and type 
  ''' names which vary between imperial and metric content.
  ''' Please note that a professional application is 
  ''' language independent, i.e. avoid use of strings 
  ''' like this or switches them automatically.
  ''' </summary>
  Public Class Constant
    ''' <summary>
    ''' Conversion factor to convert millimetres to feet.
    ''' </summary>
    Const _mmToFeet As Double = 0.0032808399

    Public Shared Function MmToFeet(ByVal mmValue As Double) As Double
      Return mmValue * _mmToFeet
    End Function

#If USE_IMPERIAL_FAMILY_NAMES_AND_TYPES Then

    ' Imperial family names and types:

    Public Const DoorFamilyName As String = "Single-Flush"
    ' "M_Single-Flush" 
    Public Const DoorTypeName As String = "30"" x 80"""
    ' "0915 x 2134mm" 
    Public Const DoorTypeName2 As String = "30"" x 80"""
    ' "0762 x 2032mm"
    Public Const RoofTypeName As String = "Generic - 9"""
    ' "Generic - 400mm"
    Public Const WallFamilyName As String = "Basic Wall"
    Public Const WallTypeName As String = "Generic - 8"""
    ' "Generic - 200mm" 
    Public Const WindowFamilyName As String = "Fixed"
    ' "M_Fixed" 
    Public Const WindowTypeName As String = "16"" x 24"""
    ' "0915 x 1830mm" 
#Else

    ' Metric family names and types:

    Public Const DoorFamilyName As String = "M_Single-Flush"
    Public Const DoorTypeName As String = "0915 x 2134mm"
    Public Const DoorTypeName2 As String = "0762 x 2032mm"

    Public Const RoofTypeName As String = "Generic - 400mm"

    Public Const WallFamilyName As String = "Basic Wall"
    Public Const WallTypeName As String = "Generic - 200mm"

    Public Const WindowFamilyName As String = "M_Fixed"
    Public Const WindowTypeName As String = "0915 x 1830mm"

#End If

  End Class
End Namespace
