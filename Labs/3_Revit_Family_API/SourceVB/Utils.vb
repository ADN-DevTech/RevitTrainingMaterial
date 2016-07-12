#Region "Copyright"
''
'' Copyright (C) 2009-2016 by Autodesk, Inc.
''
'' Permission to use, copy, modify, and distribute this software in
'' object code form for any purpose and without fee is hereby granted,
'' provided that the above copyright notice appears in all copies and
'' that both that copyright notice and the limited warranty and
'' restricted rights notice below appear in all supporting
'' documentation.
''
'' AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
'' AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
'' MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
'' DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
'' UNINTERRUPTED OR ERROR FREE.
''
'' Use, duplication, or disclosure by the U.S. Government is subject to
'' restrictions set forth in FAR 52.227-19 (Commercial Computer
'' Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
'' (Rights in Technical Data and Computer Software), as applicable.
''
#End Region

#Region "Imports"
'' Import the following name spaces in the project properties/references. 
'' Note: VB.NET has a slighly different way of recognizing name spaces than C#. 
'' if you explicitely set them in each .vb file, you will need to specify full name spaces. 

'Imports System.Linq  
'Imports Autodesk.Revit
'Imports Autodesk.Revit.DB
'Imports Autodesk.Revit.UI
'Imports Autodesk.Revit.ApplicationServices

#End Region

#Region "Description"
''  Family API how to 
'' 
#End Region

Public Class Utils

    ''  Helper function: find a list of element with the given Class, Name and Category (optional). 
    ''
    Public Shared Function FindElements(ByVal rvtDoc As Document, _
                          ByVal targetType As Type, ByVal targetName As String, _
                          Optional ByVal targetCategory As BuiltInCategory = Nothing) As IList(Of Element)

        ''  first, narrow down to the elements of the given type and category 
        Dim collector = New FilteredElementCollector(rvtDoc).OfClass(targetType)
        If Not (targetCategory = Nothing) Then
            collector.OfCategory(targetCategory)
        End If

        ''  parse the collection for the given names
        ''  using LINQ query here.
        Dim elems = _
            From element In collector _
            Where element.Name.Equals(targetName) _
            Select element

        ''  put the result as a list of element for accessibility. 
        Return elems.ToList()

    End Function

    ''  Helper function: searches elements with given Class, Name and Category (optional),  
    ''  and returns the first in the elements found. 
    ''  This gets handy when trying to find, for example, Level and View 
    ''  e.g., FindElement(m_rvtDoc, GetType(Level), "Level 1")
    ''
    Public Shared Function FindElement(ByVal rvtDoc As Document, _
                         ByVal targetType As Type, ByVal targetName As String, _
                         Optional ByVal targetCategory As BuiltInCategory = Nothing) As Element

        ''  find a list of elements using the overloaded method. 
        Dim elems As IList(Of Element) = FindElements(rvtDoc, targetType, targetName, targetCategory)

        ''  return the first one from the result. 
        If elems.Count > 0 Then
            Return elems(0)
        End If

        Return Nothing

    End Function

    '' ============================================
    ''   helper function: given a solid, find a planar face with the given normal (version 2)
    ''   this is a slightly enhanced version which checks if the face is on the given reference plane.
    '' ============================================
    ''  FindFace2 is not used.  
    Public Shared Function FindFace2(ByVal aSolid As Extrusion, ByVal normal As XYZ, ByVal refPlane As ReferencePlane) As PlanarFace

        '' get the geometry object of the given element
        ''
        Dim op As New Options
        op.ComputeReferences = True
        Dim geomElem As GeometryElement = aSolid.Geometry(op)

        '' loop through the array and find a face with the given normal
        ''
        For Each geomObj As GeometryObject In geomElem

            If TypeOf geomObj Is Solid Then  ''  solid is what we are interested in.

                Dim pSolid As Solid = geomObj
                Dim faces As FaceArray = pSolid.Faces

                For Each pFace As Face In faces
                    Dim pPlanarFace As PlanarFace = pFace
                    If Not (pPlanarFace Is Nothing) Then
                        ''  check to see if they have same normal
            If pPlanarFace.FaceNormal.IsAlmostEqualTo(normal) Then

              ' '' additionally, we want to check if the face is on the reference plane
              ' ''
              'Dim p0 As XYZ = refPlane.BubbleEnd
              'Dim p1 As XYZ = refPlane.FreeEnd
              'Dim pCurve As Line = _rvtApp.Create.NewLineBound(p0, p1)
              'Dim res As SetComparisonResult = pPlanarFace.Intersect(pCurve)
              'If res = SetComparisonResult.Subset Then
              '    Return (pPlanarFace) '' we found the face
              'End If

              '' get a point on the face. Any point will do.
              Dim pEdge As Edge = pPlanarFace.EdgeLoops.Item(0).Item(0)
              Dim pt As XYZ = pEdge.Evaluate(0.0)
              Dim res As Boolean = IsPointOnPlane(pt, refPlane)

            End If
                    End If
                Next

            ElseIf TypeOf geomObj Is GeometryInstance Then
                '' will come back later as needed.

            ElseIf TypeOf geomObj Is Curve Then
                '' will come nack later as needed.

            ElseIf TypeOf geomObj Is Mesh Then
                '' will come back later as needed.

            Else
                '' what else do we have?

            End If
        Next

        '' if we come here, we did not find any.
        Return Nothing

    End Function

    Public Shared Function FindFace(ByVal aSolid As Extrusion, ByVal normal As XYZ, Optional ByVal refPlane As ReferencePlane = Nothing) As PlanarFace

        '' get the geometry object of the given element
        ''
        Dim op As New Options
        op.ComputeReferences = True
        Dim geomElem As GeometryElement = aSolid.Geometry(op)

        '' loop through the array and find a face with the given normal
        ''
        For Each geomObj As GeometryObject In geomElem

            If TypeOf geomObj Is Solid Then  ''  solid is what we are interested in.

                Dim pSolid As Solid = geomObj
                Dim faces As FaceArray = pSolid.Faces

                For Each pFace As Face In faces
                    Dim pPlanarFace As PlanarFace = pFace
                    If Not (pPlanarFace Is Nothing) Then
                        ''  check to see if they have same normal
            If pPlanarFace.FaceNormal.IsAlmostEqualTo(normal) Then

              If refPlane Is Nothing Then
                Return pPlanarFace  '' we found the face. 
              Else
                ''  additionally, we want to check if the face is on the reference plane
                ''  get a point on the face. Any point will do.
                Dim pEdge As Edge = pPlanarFace.EdgeLoops.Item(0).Item(0)
                Dim pt As XYZ = pEdge.Evaluate(0.0)
                ''  is the point on the reference plane? 
                Dim res As Boolean = IsPointOnPlane(pt, refPlane)
                If res Then
                  Return pPlanarFace  '' we found the face 
                End If
              End If

            End If
                    End If
                Next

            ElseIf TypeOf geomObj Is GeometryInstance Then
                '' will come back later as needed.

            ElseIf TypeOf geomObj Is Curve Then
                '' will come nack later as needed.

            ElseIf TypeOf geomObj Is Mesh Then
                '' will come back later as needed.

            Else
                '' what else do we have?

            End If
        Next

        '' if we come here, we did not find any.
        Return Nothing

    End Function

    ''  test if a given point lies on the given reference plane. 
    ''  linear equation of plane: ax + by + cz = d 
    ''  the normal is orthogonal to any vector on the plane: 
    ''      n.(p1 - p0) = 0 
    ''
    Public Shared Function IsPointOnPlane(ByVal p1 As XYZ, ByVal plane As ReferencePlane)

        ''  get the plane equation 
        Dim n As XYZ = plane.Normal
    Dim p0 As XYZ = plane.GetPlane().Origin

        Dim dt As Double = n.DotProduct(p1 - p0)

        If IsAlmostEqual(dt, 0.0) Then Return True
        Return False

    End Function

    ''  compare two double values and judges if it is "almost equal".
    ''  Note: you may need to adjust the tolorance to fit your needs. 
    ''
    Public Shared Function IsAlmostEqual(ByVal val1 As Double, ByVal val2 As Double)

        Const tol As Double = 0.0001 '' hard coding the tolerance here. 

        If Math.Abs(val1 - val2) < tol Then Return True
        Return False

    End Function

    ''=============================================
    ''  Helper Functions 
    ''=============================================

    ''   convert millimeter to feet
    '' 
    Public Shared Function mmToFeet(ByVal mmVal As Double) As Double

        Return mmVal / 304.8 '' * 0.00328;

    End Function



End Class
