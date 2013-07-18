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
' Written by M.Harada 
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
Imports IntroVb.Util
#End Region

#Region "Description"
' Revit Intro Lab 3 
'
' In this lab, we'll take a look how to filter element from the database. 
' Disclaimer: minimum error checking to focus on the main topic. 
#End Region

''' <summary>
''' ElementFiltering
''' </summary>
<Transaction(TransactionMode.Automatic)> _
Public Class ElementFiltering
  Implements IExternalCommand

  ' Member variables 
  Dim _app As Application
  Dim _doc As Document

  Public Function Execute( _
    ByVal commandData As ExternalCommandData, _
    ByRef message As String, _
    ByVal elements As ElementSet) _
    As Result _
    Implements IExternalCommand.Execute

    ' Get the access to the top most objects. 
    Dim uiApp As UIApplication = commandData.Application
    Dim uiDoc As UIDocument = uiApp.ActiveUIDocument
    _app = uiApp.Application
    _doc = uiDoc.Document

    ' (1) In eailer lab, CommandData command, we learned how to access to the wallType. i.e., 
    ' here we'll take a look at more on the topic of accessing to elements in the interal rvt project database. 
    ListFamilyTypes()

    ' (2) List instances of specific object class. 
    ListInstances()

    ' (3) Find a specific family type. 
    FindFamilyType()

    ' (4) Find specific instances, including filtering by parameters.
    FindInstance()

    ' (5) List all elements:
    ListAllElements()

    ' We are done. 
    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' List the family types 
  ''' </summary>
  Public Sub ListFamilyTypes()

    ' (1) Get a list of family types available in the current rvt project. 
    ' 
    ' For system family types, there are designated properties that 
    ' allow us to directly access to the types, e.g., rvtDoc.WallTypes 

    Dim s As String = String.Empty

    ' Demonstrate obsolete method use first

    'Dim wallTypes As WallTypeSet = _doc.WallTypes ' 2013
    Dim wallTypes As FilteredElementCollector = new FilteredElementCollector(_doc).OfClass(GetType(WallType))
    

    For Each wType As WallType In wallTypes
      s += wType.Kind.ToString + " : " + wType.Name + vbCr
    Next

    TaskDialog.Show(wallTypes.Count().ToString + " Wall Types:", s)

    ' (1.1) Same idea applies to other system family, such as Floors, Roofs. 

    s = String.Empty

    'Dim floorTypes As FloorTypeSet = _doc.FloorTypes
    Dim floorTypes As FilteredElementCollector = new FilteredElementCollector(_doc).OfClass(GetType(FloorType))

    For Each fType As FloorType In floorTypes
      ' Family name is not in the property for floor. so use BuiltInParameter here. 
      Dim param As Parameter = fType.Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM)
      If param IsNot Nothing Then
        s += param.AsString
      End If
      s += " : " + fType.Name + vbCr
    Next

    TaskDialog.Show(floorTypes.Count().ToString + " floor Types: ", s)

    ' (1.2a) Another approach is to use a filter. here is an example with wall type. 

    Dim wallTypeCollector1 = New FilteredElementCollector(_doc)
    wallTypeCollector1.WherePasses(New ElementClassFilter(GetType(WallType)))
    Dim wallTypes1 As IList(Of Element) = wallTypeCollector1.ToElements

    ' Using a helper function to display the result here. See code below. 
    ShowElementList(wallTypes1, "Wall Types (by Filter): ") ' use helper function. 

    ' (1.2b) The following are the same as two lines above. 
    ' These alternative forms are provided for convenience. 
    ' Using OfClass() 
    '
    'Dim wallTypeCollector2 = New FilteredElementCollector(_doc)
    'wallTypeCollector2.OfClass(GetType(WallType))

    ' (1.2c) The following are the same as above for convenience 
    ' Using short cut this time. 
    ' 
    'Dim wallTypeCollector3 = New FilteredElementCollector(_doc).OfClass(GetType(WallType))

    ' 
    ' (2) Listing for component family types.
    ' 
    ' For component family. it is slightly different. 
    ' There is no designate property in the document class. 
    ' You always need to use a filtering. 
    ' For example, doors and windows. 
    ' Remember for component family, you will need to check element type and category 

    Dim doorTypeCollector = New FilteredElementCollector(_doc)
    doorTypeCollector.OfClass(GetType(FamilySymbol))
    doorTypeCollector.OfCategory(BuiltInCategory.OST_Doors)
    Dim doorTypes As IList(Of Element) = doorTypeCollector.ToElements

    ShowElementList(doorTypes, "Door Types (by Filter): ")

  End Sub

  ''' <summary>
  ''' To get a list of instances of a specific family type, you will need to use filters. 
  ''' The same idea that we learned for family types applies for instances as well. 
  ''' </summary>
  Sub ListInstances()

    ' List all the wall instances 
    Dim wallCollector = _
        New FilteredElementCollector(_doc).OfClass(GetType(Wall))
    Dim wallList As IList(Of Element) = wallCollector.ToElements

    ShowElementList(wallList, "Wall Instances: ")

    ' List all the door instances 
    Dim doorCollector = New FilteredElementCollector(_doc). _
        OfClass(GetType(FamilyInstance))
    doorCollector.OfCategory(BuiltInCategory.OST_Doors)
    Dim doorList As IList(Of Element) = doorCollector.ToElements

    ShowElementList(doorList, "Door Instance: ")

  End Sub

  ''' <summary>
  ''' Looks at a way to get to the more specific family types with a given name. 
  ''' </summary> 
  Sub FindFamilyType()

    ' In this exercise, we will look for the following family types for wall and door 
    ' Hard coding for similicity.  modify here if you want to try out with other family types. 

    ' Constant to this function.         
    ' This is for wall. e.g., "Basic Wall: Generic - 200mm"
    Const wallFamilyName As String = Util.Constant.WallFamilyName
    Const wallTypeName As String = Util.Constant.WallTypeName
    Const wallFamilyAndTypeName As String = wallFamilyName + ": " + wallTypeName

    ' This is for door. e.g., "M_Single-Flush: 0915 x 2134mm 
    Const doorFamilyName As String = Util.Constant.DoorFamilyName
    Const doorTypeName As String = Util.Constant.DoorTypeName
    Const doorFamilyAndTypeName As String = doorFamilyName + ": " + doorTypeName

    ' Keep messages to the user in this function.
    Dim msg As String = "Find Family Type - All -: " + vbCr + vbCr

    ' (1) Get a specific system family type. e.g., wall type. 
    ' There are a few different ways to do this.

    ' (1.1) First version uses LINQ query.   

    Dim wallType1 As Element = FindFamilyType_Wall_v1(wallFamilyName, wallTypeName)

    ' Show the result.   
    msg += ShowFamilyTypeAndId("Find wall family type (using LINQ): ", _
                                    wallFamilyAndTypeName, wallType1) + vbCr

    ' (1.2) Another way is to use iterator.  (cf. look for example, Developer guide 87) 

    Dim wallType2 As Element = FindFamilyType_Wall_v2(wallFamilyName, wallTypeName)

    msg += ShowFamilyTypeAndId("Find wall family type (using iterator): ", _
                            wallFamilyAndTypeName, wallType2) + vbCr

    ' (1.3) The most efficient method is to use a parameter filter, since 
    ' this avoids mashalling and transporting all the data for the rejected 
    ' results from the internal Revit memory to the external .NET space:

    Dim wallType3 As ElementType = TryCast(FindFamilyType_Wall_v3(wallFamilyName, wallTypeName), ElementType)

    msg += ShowFamilyTypeAndId("Find wall family type (using parameter filter): ", wallFamilyAndTypeName, wallType2) & vbCr & vbLf

    ' (2) Get a specific component family type. e.g., door type.  
    ' 
    ' (2.1) Similar approach as (1.1) using LINQ.

    Dim doorType1 As Element = FindFamilyType_Door_v1(doorFamilyName, doorTypeName)

    msg += ShowFamilyTypeAndId("Find door type (using LINQ): ", _
                    doorFamilyAndTypeName, doorType1) + vbCr

    ' (2.2) Get a specific door type. the second approach.   
    ' another approach will be to look up from Family, then from Family.Symbols property.  
    ' This gets more complicated although it is logical approach.  

    Dim doorType2 As Element = FindFamilyType_Door_v2(doorFamilyName, doorTypeName)

    msg += ShowFamilyTypeAndId("Find door type (using Family): ", _
            doorFamilyAndTypeName, doorType2) + vbCr

    ' (3) Here is more generic form. Defining a more generalized function below.  
    '
    ' (3.1) For the wall type 

    Dim wallType4 As Element = _
    FindFamilyType(_doc, GetType(WallType), wallFamilyName, wallTypeName)

    msg += ShowFamilyTypeAndId("Find wall type (using generic function): ", _
                                    wallFamilyAndTypeName, wallType4) + vbCr

    ' (3.2) For the door type.  

    Dim doorType3 As Element = _
    FindFamilyType(_doc, GetType(FamilySymbol), doorFamilyName, doorTypeName, BuiltInCategory.OST_Doors)

    msg += ShowFamilyTypeAndId("Find door type (using generic function): ", _
                                    doorFamilyAndTypeName, doorType3) + vbCr

    ' (3.3) Simply return the first door type encountered: 

    Dim doorType4 As ElementType = GetFirstFamilySymbol(_doc, BuiltInCategory.OST_Doors)

    msg += ShowFamilyTypeAndId("Find first door type (using generic function): ", doorType4.Name, doorType4) + vbCrLf

    ' Finally, show the result all together
    TaskDialog.Show("Find family types", msg)

  End Sub

  ''' <summary>
  ''' Find a specific family type for a wall with a given family and type names. 
  ''' This version uses LINQ query. 
  ''' </summary>
  Function FindFamilyType_Wall_v1( _
                                 ByVal wallFamilyName As String, _
                                 ByVal wallTypeName As String) As Element

    ' Narrow down a collector with class. 
    Dim wallTypeCollector1 = New FilteredElementCollector(_doc)
    wallTypeCollector1.OfClass(GetType(WallType))

    ' LINQ query 
    Dim wallTypeElems1 = _
        From element In wallTypeCollector1 _
        Where element.Name.Equals(wallTypeName) _
        Select element

    ' Get the result. 
    Dim wallType1 As Element = Nothing ' Result will go here. 

    ' (1) directly accessing from the query result. 
    If wallTypeElems1.Count > 0 Then
      wallType1 = wallTypeElems1.First
    End If

    ' (2) If you want to get the result as a list of element, here is how.  
    'Dim wallTypeList1 As IList(Of Element) = wallTypeElems1.ToList()
    'If wallTypeList1.Count > 0 Then
    '    wallType1 = wallTypeList1(0) ' Found it. 
    'End If

    Return wallType1

  End Function

  ''' <summary>
  ''' Find a specific family type for a wall, which is a system family. 
  ''' This version uses iteration. (cf. look for example, Developer guide 87) 
  ''' </summary>
  Function FindFamilyType_Wall_v2( _
    ByVal wallFamilyName As String, ByVal wallTypeName As String) _
    As Element

    ' First, narrow down the collector by Class 
    Dim wallTypeCollector2 = New FilteredElementCollector(_doc).OfClass(GetType(WallType))

    ' Use iterator 
    Dim wallTypeItr As FilteredElementIterator = wallTypeCollector2.GetElementIterator
    wallTypeItr.Reset()
    Dim wallType2 As Element = Nothing
    While wallTypeItr.MoveNext
      Dim wType As WallType = wallTypeItr.Current
      ' We check two names for the match: type name and family name. 
      If (wType.Name = wallTypeName) And _
      (wType.Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString.Equals(wallFamilyName)) Then
        wallType2 = wType ' We found it. 
        Exit While
      End If
    End While

    Return wallType2

  End Function

  ''' <summary>
  ''' Find a specific family type for a wall, which is a system family. 
  ''' Most efficient way to find a named family symbol: use a parameter filter.
  ''' </summary>
  Public Function FindFamilyType_Wall_v3(ByVal wallFamilyName As String, ByVal wallTypeName As String) As Element

    Dim provider As New ParameterValueProvider(New ElementId(BuiltInParameter.DATUM_TEXT))

    Dim evaluator As FilterStringRuleEvaluator = New FilterStringEquals()

    Dim rule As FilterRule = New FilterStringRule(provider, evaluator, wallTypeName, True)

    Dim filter As New ElementParameterFilter(rule)

    Return New FilteredElementCollector(_doc).OfClass(GetType(WallType)).WherePasses(filter).FirstElement()

  End Function

  ''' <summary>
  ''' Find a specific family type for a door, which is a component family. 
  ''' This version uses LINQ. 
  ''' </summary>
  Function FindFamilyType_Door_v1( _
    ByVal doorFamilyName As String, ByVal doorTypeName As String) _
    As Element

    ' Narrow down the collection with class and category. 
    Dim doorFamilyCollector1 = New FilteredElementCollector(_doc)
    doorFamilyCollector1.OfClass(GetType(FamilySymbol))
    doorFamilyCollector1.OfCategory(BuiltInCategory.OST_Doors)

    ' Parse the collection for the given name
    ' Using LINQ query here.   
    Dim doorTypeElems = _
        From element In doorFamilyCollector1 _
        Where element.Name.Equals(doorTypeName) And _
        element.Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString.Equals(doorFamilyName) _
        Select element

    ' Get the result. 
    Dim doorType1 As Element = Nothing
    ' (1) Directly accessing from the query result 
    'If doorTypeElems.Count > 0 Then ' we should have only one with the given name. minimum error checking.   
    '    doorType1 = doorTypeElems(0) ' found it. 
    'End If

    ' (2) If we want to get the list of element, here is how.  
    Dim doorTypeList As IList(Of Element) = doorTypeElems.ToList()
    If doorTypeList.Count > 0 Then ' We should have only one with the given name. minimum error checking.   
      doorType1 = doorTypeList(0) ' Found it. 
    End If

    Return doorType1

  End Function

  ''' <summary>
  ''' Find a specific family type for a door. 
  ''' another approach will be to look up from Family, then from Family.Symbols property. 
  ''' This gets more complicated although it is logical approach. 
  ''' </summary>
  Function FindFamilyType_Door_v2( _
    ByVal doorFamilyName As String, ByVal doorTypeName As String) _
    As Element

    ' (1) Find the family with the given name. 

    Dim familyCollector = New FilteredElementCollector(_doc)
    familyCollector.OfClass(GetType(Family))

    ' Use the iterator 
    Dim doorFamily As Family = Nothing
    Dim familyItr As FilteredElementIterator = familyCollector.GetElementIterator
    'familyItr.Reset()
    While (familyItr.MoveNext)
      Dim fam As Family = familyItr.Current
      ' Check name and categoty 
      If (fam.Name = doorFamilyName) And _
      (fam.FamilyCategory.Id.IntegerValue = BuiltInCategory.OST_Doors) Then
        ' We found the family. 
        doorFamily = fam
        Exit While
      End If
    End While

    ' (2) Find the type with the given name. 

    Dim doorType2 As Element = Nothing ' id of door type we are looking for. 
    If doorFamily IsNot Nothing Then
      ' If we have a family, then proceed with finding a type under Symbols property.  
      Dim doorFamilySymbolSet As FamilySymbolSet = doorFamily.Symbols

      ' Iterate through the set of family symbols. 
      Dim doorTypeItr As FamilySymbolSetIterator = doorFamilySymbolSet.ForwardIterator
      While doorTypeItr.MoveNext
        Dim dType As FamilySymbol = doorTypeItr.Current
        If (dType.Name = doorTypeName) Then
          doorType2 = dType ' Found it.
          Exit While
        End If
      End While
    End If

    Return doorType2

  End Function

  ''' <summary>
  ''' Find specific instances, including filtering by parameters. 
  ''' </summary> 
  Sub FindInstance()

    ' Constant to this function. (we may want to change the value here.)         
    ' This is for wall. e.g., "Basic Wall: Generic - 200mm"
    Const wallFamilyName As String = Util.Constant.WallFamilyName
    Const wallTypeName As String = Util.Constant.WallTypeName
    Const wallFamilyAndTypeName As String = wallFamilyName + ": " + wallTypeName

    ' This is for door. e.g., "M_Single-Flush: 0915 x 2134mm 
    Const doorFamilyName As String = Util.Constant.DoorFamilyName
    Const doorTypeName As String = Util.Constant.DoorTypeName
    Const doorFamilyAndTypeName As String = doorFamilyName + ": " + doorTypeName


    ' (1) Find walls with a specific type
    ' 
    ' Find a specific family type. use the function we defined earlier.   
    Dim idWallType As ElementId = FindFamilyType(_doc, GetType(WallType), wallFamilyName, wallTypeName).Id
    ' Find instances of the given family type. 
    Dim walls As IList(Of Element) = FindInstancesOfType(GetType(Wall), idWallType)

    ' Show it
    Dim msgWalls As String = "Instances of wall with type: " + wallFamilyAndTypeName + vbCr
    ShowElementList(walls, msgWalls)


    ' (2) Find a specific door. same idea. 
    Dim idDoorType As ElementId = _
    FindFamilyType(_doc, GetType(FamilySymbol), doorFamilyName, doorTypeName, BuiltInCategory.OST_Doors).Id
    Dim doors As IList(Of Element) = _
    FindInstancesOfType(GetType(FamilyInstance), idDoorType, BuiltInCategory.OST_Doors)

    Dim msgDoors As String = "Instances of door with type: " + doorFamilyAndTypeName + vbCr
    ShowElementList(doors, msgDoors)

    ' (3) Apply the same idea to the supporting element, such as level. 
    ' In this case, we simply check the name. 
    ' This becomes handy when you are creating an object on a certain level,  
    ' for example, when we create a wall. 
    ' We will use this in the lab 5 when we create a simple house.  

    Dim level1 As Level = FindElement(_doc, GetType(Level), "Level 1")

    Dim msgLevel1 As String = "Level1: " + vbCr + ElementToString(level1) + vbCr
    TaskDialog.Show("Find instance", msgLevel1)

    ' (4) Finally, let's see how to use parameter filter 
    ' Let's try to get a wall whose length is larger than 60 feet. 

    Dim longWalls As IList(Of Element) = FindLongWalls()

    Dim msgLongWalls As String = "Long walls: " + vbCr
    ShowElementList(longWalls, msgLongWalls)

  End Sub

  ''' <summary>
  ''' Helper function: find a list of element with given class, family type and category (optional). 
  ''' </summary>
  Function FindInstancesOfType( _
    ByVal targetType As Type, _
    ByVal idType As ElementId, _
    Optional ByVal targetCategory As BuiltInCategory = Nothing) _
    As IList(Of Element)

    ' First, narrow down to the elements of the given type and category 

    Dim collector = New FilteredElementCollector(_doc).OfClass(targetType)
    If Not (targetCategory = Nothing) Then
      collector.OfCategory(targetCategory)
    End If

    ' Parse the collection for the given family type id.
    ' Using LINQ query here.
    Dim elems = _
        From element In collector _
        Where element.Parameter(BuiltInParameter.SYMBOL_ID_PARAM).AsElementId.Equals(idType) _
        Select element

    ' Put the result as a list of element fo accessibility. 
    Return elems.ToList()

  End Function

  ''' <summary>
  ''' Optional - example of parameter filter. 
  ''' Find walls whose length is longer than a certain length. e.g., 60 feet 
  '''     wall.parameter(length) > 60 feet 
  ''' This could get more complex than looping through in terms of writing a code. 
  ''' See page 87 of Developer guide. 
  ''' </summary> 
  Function FindLongWalls() As IList(Of Element)

    ' Constant for this function. 
    Const kWallLength As Double = 60.0 ' 60 feet. hard coding for simplicity. 

    ' First, narrow down to the elements of the given type and category 
    Dim collector = New FilteredElementCollector(_doc).OfClass(GetType(Wall))

    ' Define a filter by parameter 
    ' 1st arg - value provider 
    Dim lengthParam As BuiltInParameter = BuiltInParameter.CURVE_ELEM_LENGTH
    Dim iLengthParam As Integer = lengthParam
    Dim paramValueProvider = New ParameterValueProvider(New ElementId(iLengthParam))

    ' 2nd - evaluator 
    Dim evaluator As New FilterNumericGreater

    ' 3rd - rule value 
    Dim ruleVal As Double = kWallLength

    ' 4th - epsilon 
    Const eps As Double = 0.000001

    ' Define a rule 
    Dim filterRule = New FilterDoubleRule(paramValueProvider, evaluator, ruleVal, eps)

    ' Create a new filter 
    Dim paramFilter = New ElementParameterFilter(filterRule)

    ' Go through the filter 
    Dim elems As IList(Of Element) = collector.WherePasses(paramFilter).ToElements

    Return elems

  End Function

  ''' <summary>
  ''' List all elements in Revit database.
  ''' </summary>
  Sub ListAllElements()

    ' Create an output file:

    Dim filename As String = Path.Combine(Path.GetTempPath, "RevitElements.txt")
    Dim sw As New StreamWriter(filename)

    ' The Revit API does not expect an application
    ' ever to need to iterate over all elements.
    ' To do so, we need to use a trick: ask for all
    ' elements fulfilling a specific criteria and
    ' unite them with all elements NOT fulfilling
    ' the same criteria; an arbitrary criterion 
    ' could be chosen:

    Dim collector As FilteredElementCollector = New FilteredElementCollector(_doc).WhereElementIsElementType
    Dim collector2 As FilteredElementCollector = New FilteredElementCollector(_doc).WhereElementIsNotElementType
    collector.UnionWith(collector2)

    ' Loop over the elements and list their data:

    Dim s As String
    Dim e As Element
    For Each e In collector
      Dim line As String = "Id=" + e.Id.IntegerValue.ToString
      line += "; Class=" & e.GetType.Name

      ' The element category is not implemented for all classes,
      ' and may return null; for family elements, one can sometimes
      ' use the FamilyCategory property instead.

      s = String.Empty

      If Not Nothing Is e.Category Then
        s = e.Category.Name
      End If

      If 0 = s.Length AndAlso TypeOf e Is Family AndAlso Not Nothing Is DirectCast(e, Family).FamilyCategory Then
        s = DirectCast(e, Family).FamilyCategory.Name
      End If

      If 0 = s.Length Then
        s = "?"
      End If

      line += "; Category=" + s + "; Name=" + e.Name
      sw.WriteLine(line)
    Next
    sw.Close()
    TaskDialog.Show("List all elements", _
      String.Format("Element list has been written to '{0}'.", filename))
  End Sub

#Region "Helper Functions"
  '====================================================================
  ' Helper Functions 
  '====================================================================
  ''' <summary>
  ''' Helper function: find an element of the given type, name, and category(optional) 
  ''' You can use this, for example, to find a specific wall and window family with the given name. 
  ''' e.g., 
  ''' FindFamilyType(_doc, GetType(WallType), "Basic Wall", "Generic - 200mm") 
  ''' FindFamilyType(_doc, GetType(FamilySymbol), "M_Single-Flush", "0915 x 2134mm", BuiltInCategory.OST_Doors) 
  ''' </summary>
  Public Shared Function FindFamilyType( _
    ByVal rvtDoc As Document, _
    ByVal targetType As Type, ByVal targetFamilyName As String, _
    ByVal targetTypeName As String, _
    Optional ByVal targetCategory As BuiltInCategory = Nothing) _
    As Element

    ' First, narrow down to the elements of the given type and category 
    Dim collector = New FilteredElementCollector(rvtDoc).OfClass(targetType)
    If Not (targetCategory = Nothing) Then
      collector.OfCategory(targetCategory)
    End If

    ' Parse the collection for the given names
    ' Using LINQ query here.
    Dim targetElems = _
        From element In collector _
        Where element.Name.Equals(targetTypeName) And _
        element.Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM). _
        AsString.Equals(targetFamilyName) _
        Select element

    ' Put the result as a list of element fo accessibility. 
    Dim elems As IList(Of Element) = targetElems.ToList()

    ' Return the result. 
    If elems.Count > 0 Then
      Return elems(0)
    End If

    Return Nothing

  End Function

  ''' <summary>
  ''' Return all elements of the requested class,
  ''' i.e. System.Type, matching the given built-in
  ''' category in the given document.
  ''' </summary>
  Public Shared Function GetElementsOfType( _
    ByVal doc As Document, _
    ByVal type As Type, _
    ByVal bic As BuiltInCategory) _
    As FilteredElementCollector

    Dim collector As New FilteredElementCollector(doc)

    collector.OfCategory(bic)
    collector.OfClass(type)

    Return collector
  End Function

  ''' <summary>
  ''' Return all family symbols in the given document
  ''' matching the given built-in category.
  ''' Todo: Compare this with the FamilySymbolFilter class.
  ''' </summary>
  Public Shared Function GetFamilySymbols( _
    ByVal doc As Document, _
    ByVal bic As BuiltInCategory) _
    As FilteredElementCollector

    Return GetElementsOfType(doc, _
      GetType(FamilySymbol), bic)
  End Function

  ''' <summary>
  ''' Return the first family symbol found in the given document
  ''' matching the given built-in category or null if none is found.
  ''' </summary>
  Public Shared Function GetFirstFamilySymbol(
    ByVal doc As Document, _
    ByVal bic As BuiltInCategory) _
    As FamilySymbol

    Dim s As FamilySymbol = GetFamilySymbols(doc, bic).FirstElement()

    Debug.Assert(s IsNot Nothing, String.Format(
      "expected at least one {0} symbol in project",
      bic.ToString()))

    Return s
  End Function

  ''' <summary>
  ''' Helper function: find a list of element with given Class, Name and Category (optional). 
  ''' </summary>  
  Public Shared Function FindElements( _
    ByVal rvtDoc As Document, _
    ByVal targetType As Type, ByVal targetName As String, _
    Optional ByVal targetCategory As BuiltInCategory = Nothing) _
    As IList(Of Element)

    ' First, narrow down to the elements of the given type and category 
    Dim collector = New FilteredElementCollector(rvtDoc).OfClass(targetType)
    If Not (targetCategory = Nothing) Then
      collector.OfCategory(targetCategory)
    End If

    ' Parse the collection for the given names
    ' Using LINQ query here.
    Dim elems = _
        From element In collector _
        Where element.Name.Equals(targetName) _
        Select element

    ' Put the result as a list of element for accessibility. 
    Return elems.ToList()

  End Function

  ''' <summary>
  ''' Helper function: searches elements with given Class, Name and Category (optional), 
  ''' and returns the first in the elements found. 
  ''' This gets handy when trying to find, for example, Level. 
  ''' e.g., FindElement(_doc, GetType(Level), "Level 1") 
  ''' </summary>
  Public Shared Function FindElement( _
    ByVal rvtDoc As Document, _
    ByVal targetType As Type, ByVal targetName As String, _
    Optional ByVal targetCategory As BuiltInCategory = Nothing) _
    As Element

    ' Find a list of elements using the overloaded method. 
    Dim elems As IList(Of Element) = FindElements(rvtDoc, targetType, targetName, targetCategory)

    ' Return the first one from the result. 
    If elems.Count > 0 Then
      Return elems(0)
    End If

    Return Nothing

  End Function

  ''' <summary>
  ''' Helper function: to show the result of finding a family type. 
  ''' </summary>  
  Function ShowFamilyTypeAndId( _
    ByVal header As String, ByVal familyAndTypeName As String, _
    ByVal familyType As ElementType) _
    As String

    ' Show the result.
    Dim msg As String = header + vbCr + familyAndTypeName + " >> Id = "

    If familyType IsNot Nothing Then
      msg += familyType.Id.ToString + vbCr
    End If

    ' Uncomment this if you want to show each result. 
    'TaskDialog.Show("Show family type and id", msg)

    Return msg

  End Function

  ''' <summary>
  ''' Helper function to display info from a list of elements passed onto. 
  ''' </summary> 
  Sub ShowElementList( _
    ByVal elems As IList(Of Element), ByVal header As String)

    Dim s As String = String.Empty
    s += " - Class - Category - Name (or Family: Type Name) - Id - " + vbCr
    For Each e As Element In elems
      s += ElementToString(e)
    Next
    TaskDialog.Show(header + "(" + elems.Count.ToString() + "):", s)

  End Sub

  ''' <summary>
  ''' Helper function: summarize an element information as a line of text, 
  ''' which is composed of: class, category, name and id. 
  ''' name will be "Family: Type" if a given element is ElementType. 
  ''' Intended for quick viewing of list of element, for example. 
  ''' </summary> 
  Function ElementToString(ByVal e As Element) As String

    If e Is Nothing Then
      Return "none"
    End If

    Dim name As String = ""

    If TypeOf e Is ElementType Then
      Dim param As Parameter = e.Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)
      If param IsNot Nothing Then
        name = param.AsString
      End If
    Else
      name = e.Name
    End If

    Return e.GetType.Name + "; " + e.Category.Name + "; " _
      + name + "; " + e.Id.IntegerValue.ToString + vbCr

  End Function

#End Region

End Class
