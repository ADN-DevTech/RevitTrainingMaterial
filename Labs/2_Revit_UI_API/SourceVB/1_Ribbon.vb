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

' Additionl references needed to UI Labs - Ribbon:
'   - WindowsBase 
'   - PresentationCore 
'   - IntroVb2 (From our Revit Intro Labs)

'Imports System
'Imports System.Windows.Media.Imaging  ' for bitmap images. you will need to reference WindowsBase and PresentationCore  
'Imports Autodesk.Revit.DB
'Imports Autodesk.Revit.UI
'Imports Autodesk.Revit.ApplicationServices  ' Application class
'Imports Autodesk.Revit.Attributes ' specific this if you want to save typing for attributes. e.g., 
'Imports Autodesk.Revit.UI.Selection ' for selection 
'Imports IntroVb ' we'll be using commands we defined in Revit Intro labs. alternatively, you can use your own. Any command will do for this ribbon exercise.
#End Region

''' <summary>
''' Ribbon UI. 
''' we'll be using commands we defined in Revit Intro labs. alternatively, 
''' you can use your own. Any command will do for this ribbon exercise. 
''' cf. Developer Guide, Section 3.8: Ribbon Panels and Controls. (pp 46). 
''' </summary>
<Transaction(TransactionMode.Automatic)> _
Public Class UIRibbon
  Implements IExternalApplication

  ''' <summary>
  ''' This is both the assembly name and the namespace 
  ''' of the external command provider.
  ''' </summary>
  Private Const _dllExtension As String = ".dll"
  Private Const _introLabName As String = "IntroVb"
  Private Const _uiLabName As String = "UiVb"

  ''' <summary>
  ''' Name of subdirectory containing images.
  ''' </summary>
  Private Const _imageFolderName As String = "Images"

  ''' <summary>
  ''' Location of images for icons. 
  ''' </summary>
  Private _imageFolder As String

  ''' <summary>
  ''' Location of managed dll where we have defined the commands.
  ''' </summary>
  Private _introLabPath As String

  ''' <summary>
  ''' Starting at the given directory, search upwards for 
  ''' a subdirectory with the given target name located
  ''' in some parent directory. 
  ''' </summary>
  ''' <param name="path">Starting directory, e.g. GetDirectoryName( GetExecutingAssembly().Location ).</param>
  ''' <param name="target">Target subdirectory name, e.g. "Images".</param>
  ''' <returns>The full path of the target directory if found, else null.</returns>
  Private Function FindFolderInParents(ByVal path As String, ByVal target As String) As String
    Debug.Assert(Directory.Exists(path), _
      "expected an existing directory to start search in")
    Do
      Dim s As String = System.IO.Path.Combine(path, target)
      If Directory.Exists(s) Then
        Return s
      End If
      path = System.IO.Path.GetDirectoryName(path)
    Loop While (path IsNot Nothing)
    Return Nothing
  End Function

  ''' <summary>
  ''' Load a new icon bitmap from our image folder.
  ''' </summary>
  Function NewBitmapImage(ByVal imageName As String) As BitmapImage
    Return New BitmapImage(New Uri(Path.Combine(Me._imageFolder, imageName)))
  End Function

  ''' <summary>
  ''' OnShutdown() - called when Revit ends. 
  ''' </summary>
  Public Function OnShutdown(ByVal app As UIControlledApplication) As Result _
      Implements IExternalApplication.OnShutdown

    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' OnStartup() - called when Revit starts. 
  ''' </summary>
  Public Function OnStartup(ByVal app As UIControlledApplication) As Result _
    Implements IExternalApplication.OnStartup

    ' External application directory:

    Dim dir As String = Path.GetDirectoryName( _
        System.Reflection.Assembly.GetExecutingAssembly.Location)

    _introLabPath = Path.Combine(dir, _introLabName + _dllExtension)

    ' External command path:

    If Not File.Exists(_introLabPath) Then
      TaskDialog.Show("UIRibbon", "External command assembly not found: " + _introLabPath)
      Return Result.Failed
    End If

    ' Image path:

    _imageFolder = FindFolderInParents(dir, _imageFolderName)

    If _imageFolder Is Nothing Or Not Directory.Exists(_imageFolder) Then
      TaskDialog.Show( _
          "UIRibbon", _
          String.Format( _
            "No image folder named '{0}' found in the parent directories of '{1}.", _
            _imageFolderName, dir))
      Return Result.Failed
    End If

    ' Show what kind of custom buttons and controls 
    ' we can add to the Add-Ins tab 

    AddRibbonSampler(app)

    ' Add one for UI Labs, too. 

    AddUILabsButtons(app)

    Return Result.Succeeded

  End Function

  ''' <summary>
  ''' Create our own ribbon panel with verious buttons 
  ''' for our exercise. We re-use commands defined in the
  ''' Revit Intro Labs here. Cf. Section 3.8 (pp 46) of 
  ''' the Developers Guide. 
  ''' </summary>
  Sub AddRibbonSampler(ByVal app As UIControlledApplication)

    ' (1) create a ribbon tab and panel 

    app.CreateRibbonTab("Ribbon Sampler")

    Dim panel As RibbonPanel = app.CreateRibbonPanel("Ribbon Sampler", "Ribbon Sampler")

    ' Below are samplers of ribbon items. Uncomment 
    ' functions of your interest to see how it looks like  

    ' (2.1) add a simple push button for Hello World 
    AddPushButton(panel)
    'panel.AddSeparator()

    ' (2.2) add split buttons for "Command Data", "DB Element" and "Element Filtering"
    AddSplitButton(panel)
    'panel.AddSeparator()

    ' (2.3) add pulldown buttons for "Command Data", "DB Element" and "Element Filtering"
    AddPulldownButton(panel)
    'panel.AddSeparator()

    ' (2.4) add radio/toggle buttons for "Command Data", "DB Element" and "Element Filtering" 
    ' We put it on the slide-out below. 
    'AddRadioButton(panel)
    'panel.AddSeparator()

    ' (2.5) add text box - TBD: this is used with the conjunction with event. Probably too complex for day one training. 
    ' For now, without event. 
    ' We put it on the slide-out below. 
    'AddTextBox(panel)
    'panel.AddSeparator()

    ' (2.6) combo box - TBD: this is used with the conjunction with event. Probably too complex for day one training. 
    '  For now, without event. show two groups: Element Bascis (3 push buttons) and Modification/Creation (2 push button)  
    AddComboBox(panel)
    'panel.AddSeparator()

    ' (2.7) stacked items - 1. hello world push button, 2. pulldown element bscis (command data, DB element, element filtering)
    ' 3. pulldown modification/creation(element modification, model creation). 
    ' 
    'AddStackedButtons_Simple(panel) ' simple push button stack. 
    'panel.AddSeparator()
    AddStackedButtons_Complex(panel)
    'panel.AddSeparator()

    '
    ' (2.8) slide out - if you don't have enough space, you can add additional space below the panel. 
    '  anything which comes after this will be on the slide out. 
    panel.AddSlideOut()
    'panel.AddSeparator()

    ' (2.4) radio button - what it is 
    AddRadioButton(panel)
    ' (2.5) text box - what it is 
    AddTextBox(panel)

  End Sub

  ''' <summary>
  ''' We create our own buttons for UI Labs, too. 
  ''' cf. Section 3.8 (pp 46) of Developer Guide. 
  ''' </summary>
  Sub AddUILabsButtons(ByVal app As UIControlledApplication)

    ' Create a ribbon panel 
    Dim panel As RibbonPanel = app.CreateRibbonPanel("UI Labs")

    ' (3) Adding buttons for the current labs itself. 
    ' You may modify this AFTER each command are defined in each lab. 
    'AddUILabsCommandButtons_Template(panel) ' dummy 
    AddUILabsCommandButtons(panel) ' after subsequence labs are done 

    ' (4) This is for Lab4 event and dynamic update.  
    AddUILabsCommandButtons2(panel)

  End Sub


  ''' <summary>
  ''' Simple push button for "Hello World" 
  ''' </summary>
  Sub AddPushButton(ByVal panel As RibbonPanel)

    ' Set the information about the command we will be assigning to the button 
    Dim pushButtonDataHello As New PushButtonData("PushButtonHello", "Hello World", _introLabPath, _introLabName + ".HelloWorld")
    ' Add a button to the panel 
    Dim pushButtonHello As PushButton = panel.AddItem(pushButtonDataHello)
    ' Add an icon 
    ' Make sure you reference WindowsBase and PresentationCore, and import System.Windows.Media.Imaging namespace.
    pushButtonHello.LargeImage = NewBitmapImage("ImgHelloWorld.png")
    ' Add a tooltip
    pushButtonHello.ToolTip = "simple push button"

  End Sub

  ''' <summary>
  ''' Split button for "Command Data", "DB Element" and "Element Filtering" 
  ''' </summary>
  Sub AddSplitButton(ByVal panel As RibbonPanel)

    ' Create three push buttons for split button drop down
    ' #1 
    Dim pushButtonData1 As New PushButtonData("SplitCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData")
    pushButtonData1.LargeImage = NewBitmapImage("ImgHelloWorld.png")

    ' #2 
        Dim pushButtonData2 As New PushButtonData("SplitDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement")
    pushButtonData2.LargeImage = NewBitmapImage("ImgHelloWorld.png")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("SplitElementFiltering", "ElementFiltering", _introLabPath, _introLabName + ".ElementFiltering")
    pushButtonData3.LargeImage = NewBitmapImage("ImgHelloWorld.png")

    ' Make a split button now 
    Dim splitBtnData As New SplitButtonData("SplitButton", "Split Button")
    Dim splitBtn As SplitButton = panel.AddItem(splitBtnData)
    splitBtn.AddPushButton(pushButtonData1)
    splitBtn.AddPushButton(pushButtonData2)
    splitBtn.AddPushButton(pushButtonData3)

  End Sub

  ''' <summary>
  ''' Pulldown button for "Command Data", "DB Element" and "Element Filtering"
  ''' </summary>
  Sub AddPulldownButton(ByVal panel As RibbonPanel)

    ' Create three push buttons for pulldown button drop down
    ' #1 
    Dim pushButtonData1 As New PushButtonData("PulldownCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData")
    pushButtonData1.LargeImage = NewBitmapImage("Basics.ico")

    ' #2 
        Dim pushButtonData2 As New PushButtonData("PulldownDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement")
    pushButtonData2.LargeImage = NewBitmapImage("Basics.ico")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("PulldownElementFiltering", "Filtering", _introLabPath, _introLabName + ".ElementFiltering")
    pushButtonData3.LargeImage = NewBitmapImage("Basics.ico")

    ' Make a pulldown button now 
    Dim pulldownBtnData As New PulldownButtonData("PulldownButton", "Pulldown")
    Dim pulldownBtn As PulldownButton = panel.AddItem(pulldownBtnData)
    pulldownBtn.AddPushButton(pushButtonData1)
    pulldownBtn.AddPushButton(pushButtonData2)
    pulldownBtn.AddPushButton(pushButtonData3)

  End Sub

  ''' <summary>
  ''' Radio/toggle button for "Command Data", "DB Element" and "Element Filtering"
  ''' </summary>
  Sub AddRadioButton(ByVal panel As RibbonPanel)

    ' Create three toggle buttons for radio button group
    ' #1 
    Dim toggleButtonData1 As New ToggleButtonData("RadioCommandData", "Command" + vbCr + "Data", _introLabPath, _introLabName + ".CommandData")
    toggleButtonData1.LargeImage = NewBitmapImage("Basics.ico")

    ' #2 
        Dim toggleButtonData2 As New ToggleButtonData("RadioDbElement", "DB" + vbCr + "Element", _introLabPath, _introLabName + ".DBElement")
    toggleButtonData2.LargeImage = NewBitmapImage("Basics.ico")

    ' #3  
    Dim toggleButtonData3 As New ToggleButtonData("RadioElementFiltering", "Filtering", _introLabPath, _introLabName + ".ElementFiltering")
    toggleButtonData3.LargeImage = NewBitmapImage("Basics.ico")

    ' Make a radio button group now 
    Dim radioBtnGroupData As New RadioButtonGroupData("RadioButton")
    Dim radioBtnGroup As RadioButtonGroup = panel.AddItem(radioBtnGroupData)
    radioBtnGroup.AddItem(toggleButtonData1)
    radioBtnGroup.AddItem(toggleButtonData2)
    radioBtnGroup.AddItem(toggleButtonData3)

  End Sub

  ''' <summary>
  ''' Text box 
  ''' Text box used in conjunction with event. We'll come to this later. 
  ''' For now, just shows how to make a text box. 
  ''' </summary>
  Sub AddTextBox(ByVal panel As RibbonPanel)

    ' Fill the text gox information
    Dim txtBoxData As New TextBoxData("TextBox")
    txtBoxData.Image = NewBitmapImage("Basics.ico")
    txtBoxData.Name = "Text Box"
    txtBoxData.ToolTip = "Enter text here"
    txtBoxData.LongDescription = "<p>This is Revit UI Labs.</p><p>Ribbon Lab</p>"
    txtBoxData.ToolTipImage = NewBitmapImage("ImgHelloWorld.png")

    ' Create the text box item on the panel 
    Dim txtBox As TextBox = panel.AddItem(txtBoxData)
    txtBox.PromptText = "Enter a comment"
    txtBox.ShowImageAsButton = True
    txtBox.Width = 180
    'txtBox.ItemText = "my text box"
    'txtBox.Name ' this is read only. 

    ' p51. we'll talk about event in Lab4.  
    AddHandler txtBox.EnterPressed, New EventHandler(Of TextBoxEnterPressedEventArgs)(AddressOf txtBox_EnterPressed)

  End Sub

  ''' <summary>
  ''' Event handler for the above text box 
  ''' </summary>
  Sub txtBox_EnterPressed(ByVal sender As Object, ByVal e As TextBoxEnterPressedEventArgs)
    ' Cast sender to TextBox to retrieve text value
    Dim txtBox As TextBox = sender
    TaskDialog.Show("TextBox Input", "This is what you typed in: " + txtBox.Value.ToString())
  End Sub

  ''' <summary>
  ''' Combo box - 5 items in 2 groups. 
  ''' Combo box is used in conjunction with event. We'll come back later. 
  ''' For now, just demonstrates how to make a combo box. 
  ''' </summary>
  Sub AddComboBox(ByVal panel As RibbonPanel)

    ' Create five combo box members with two groups 
    ' #1 
    Dim comboBoxMemberData1 As New ComboBoxMemberData("ComboCommandData", "Command Data")
    comboBoxMemberData1.Image = NewBitmapImage("Basics.ico")
    comboBoxMemberData1.GroupName = "DB Basics"

    ' #2 
    Dim comboBoxMemberData2 As New ComboBoxMemberData("ComboDbElement", "DB Element")
    comboBoxMemberData2.Image = NewBitmapImage("Basics.ico")
    comboBoxMemberData2.GroupName = "DB Basics"

    ' #3  
    Dim comboBoxMemberData3 As New ComboBoxMemberData("ComboElementFiltering", "Filtering")
    comboBoxMemberData3.Image = NewBitmapImage("Basics.ico")
    comboBoxMemberData3.GroupName = "DB Basics"

    ' #4
    Dim comboBoxMemberData4 As New ComboBoxMemberData("ComboElementModification", "Modify")
    comboBoxMemberData4.Image = NewBitmapImage("Basics.ico")
    comboBoxMemberData4.GroupName = "Modeling"

    ' #5
    Dim comboBoxMemberData5 As New ComboBoxMemberData("ComboModelCreation", "Create")
    comboBoxMemberData5.Image = NewBitmapImage("Basics.ico")
    comboBoxMemberData5.GroupName = "Modeling"


    ' Make a combo box now 
    Dim comboBxData As New ComboBoxData("ComboBox")
    Dim comboBx As ComboBox = panel.AddItem(comboBxData)
    comboBx.ToolTip = "Select an Option"
    comboBx.LongDescription = "select a command you want to run"
    comboBx.AddItem(comboBoxMemberData1)
    comboBx.AddItem(comboBoxMemberData2)
    comboBx.AddItem(comboBoxMemberData3)
    comboBx.AddItem(comboBoxMemberData4)
    comboBx.AddItem(comboBoxMemberData5)

    AddHandler comboBx.CurrentChanged, New EventHandler(Of ComboBoxCurrentChangedEventArgs)(AddressOf comboBx_CurrentChanged)

  End Sub

  ''' <summary>
  ''' Event handler for the above combo box 
  ''' </summary>
  Sub comboBx_CurrentChanged(ByVal sender As Object, ByVal e As ComboBoxCurrentChangedEventArgs)
    ' Cast sender as TextBox to retrieve text value
    Dim combodata As ComboBox = TryCast(sender, ComboBox)
    Dim member As ComboBoxMember = combodata.Current
    TaskDialog.Show("Combobox Selection", "Your new selection: " + member.ItemText)
  End Sub

  ''' <summary>
  ''' Stacked Buttons - combination of: push button, dropdown button, combo box and text box. 
  ''' (no radio button group, split buttons). 
  ''' Here we stack three push buttons for "Command Data", "DB Element" and "Element Filtering". 
  ''' </summary>
  Sub AddStackedButtons_Simple(ByVal panel As RibbonPanel)

    ' Create three push buttons to stack up 
    ' #1 
    Dim pushButtonData1 As New PushButtonData("StackSimpleCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData")
    pushButtonData1.Image = NewBitmapImage("ImgHelloWorldSmall.png")

    ' #2 
        Dim pushButtonData2 As New PushButtonData("StackSimpleDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement")
    pushButtonData2.Image = NewBitmapImage("ImgHelloWorldSmall.png")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("StackSimpleElementFiltering", "Element Filtering", _introLabPath, _introLabName + ".ElementFiltering")
    pushButtonData3.Image = NewBitmapImage("ImgHelloWorldSmall.png")


    ' Put them on stack  
    Dim stackedButtons As IList(Of RibbonItem) = panel.AddStackedItems(pushButtonData1, pushButtonData2, pushButtonData3)

  End Sub

  ''' <summary>
  ''' Stacked Buttons - combination of: push button, dropdown button, combo box and text box. 
  ''' (no radio button group, split buttons). 
  ''' Here we define 6 buttons, make grouping of 1, 3, 2 items, and stack them in three layer: 
  ''' (1) simple push button with "Hello World" 
  ''' (2) pull down with 3 items: "Command Data", "DB Element" and "Element Filtering". 
  ''' (3) pull down with 2 items: "Element Modification" and "Model Creation" 
  ''' </summary>
  Sub AddStackedButtons_Complex(ByVal panel As RibbonPanel)

    ' Create six push buttons to group for pull down and stack up 

    ' #0 
    Dim pushButtonData0 As New PushButtonData("StackComplexHelloWorld", "Hello World", _introLabPath, _introLabName + ".HelloWorld")
    pushButtonData0.Image = NewBitmapImage("Basics.ico")

    ' #1 
    Dim pushButtonData1 As New PushButtonData("StackComplexCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData")
    pushButtonData1.Image = NewBitmapImage("Basics.ico")

    ' #2 
        Dim pushButtonData2 As New PushButtonData("StackComplexDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement")
    'pushButtonData2.Image = NewBitmapImage( "ImgHelloWorldSmall.png")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("StackComplexElementFiltering", "Filtering", _introLabPath, _introLabName + ".ElementFiltering")
    'pushButtonData3.Image = NewBitmapImage( "ImgHelloWorldSmall.png")

    ' #4 
    Dim pushButtonData4 As New PushButtonData("StackComplexElementModification", "Modify", _introLabPath, _introLabName + ".ElementModification")
    'pushButtonData4.Image = NewBitmapImage( "ImgHelloWorldSmall.png")

    ' #5  
    Dim pushButtonData5 As New PushButtonData("StackComplexModelCreation", "Create", _introLabPath, _introLabName + ".ModelCreation")
    'pushButtonData5.Image = NewBitmapImage( "ImgHelloWorldSmall.png")

    ' Make two sets of pull down 

    Dim pulldownBtnData1 As New PulldownButtonData("StackComplePulldownButton1", "DB Basics")
    Dim pulldownBtnData2 As New PulldownButtonData("StackComplePulldownButton2", "Modeling")

    ' Create three item stack. 
    Dim stackedItems As IList(Of RibbonItem) = panel.AddStackedItems(pushButtonData0, pulldownBtnData1, pulldownBtnData2)
    'Dim pulldownBtn1 As PulldownButton = stackedItems(0) ' the first is simple bush button. 
    Dim pulldownBtn2 As PulldownButton = stackedItems(1)
    Dim pulldownBtn3 As PulldownButton = stackedItems(2)

    pulldownBtn2.Image = NewBitmapImage("Basics.ico")
    pulldownBtn3.Image = NewBitmapImage("House.ico")

    ' Add each sub items 
    Dim button1 As PushButton = pulldownBtn2.AddPushButton(pushButtonData1)
    Dim button2 As PushButton = pulldownBtn2.AddPushButton(pushButtonData2)
    Dim button3 As PushButton = pulldownBtn2.AddPushButton(pushButtonData3)
    Dim button4 As PushButton = pulldownBtn3.AddPushButton(pushButtonData4)
    Dim button5 As PushButton = pulldownBtn3.AddPushButton(pushButtonData5)

    ' Note: we need to set the image later.  If we do in button bata, it won't show in the Ribbon. 
    button1.Image = NewBitmapImage("Basics.ico")
    button2.Image = NewBitmapImage("Basics.ico")
    button3.Image = NewBitmapImage("Basics.ico")
    button4.Image = NewBitmapImage("Basics.ico")
    button5.Image = NewBitmapImage("Basics.ico")

  End Sub

  ''' <summary>
  ''' Add buttons for the commands we define in this labs. 
  ''' Here we stack three push buttons and repeat it as we get more. 
  ''' This is a template to use during the Ribbon lab exercise prior to going to following labs. 
  ''' </summary>
  Sub AddUILabsCommandButtons_Template(ByVal panel As RibbonPanel)

    ' Get the location of this dll. 
    Dim assembly As String = [GetType]().Assembly.Location

    ' Create three push buttons to stack up 
    ' #1 
    Dim pushButtonData1 As New PushButtonData("UILabsCommand1", "Command1", assembly, _uiLabName + ".Command1")
    pushButtonData1.Image = NewBitmapImage("ImgHelloWorldSmall.png")

    ' #2 
    Dim pushButtonData2 As New PushButtonData("UILabsCommand2", "Command2", assembly, _uiLabName + ".Command2")
    pushButtonData2.Image = NewBitmapImage("ImgHelloWorldSmall.png")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("UILabsCommand3", "Command3", assembly, _uiLabName + ".Command3")
    pushButtonData3.Image = NewBitmapImage("ImgHelloWorldSmall.png")


    ' Put them on stack  
    Dim stackedButtons As IList(Of RibbonItem) = panel.AddStackedItems(pushButtonData1, pushButtonData2, pushButtonData3)

  End Sub

  ''' <summary>
  ''' Add buttons for the commands we define in this labs. 
  ''' Here we stack three push buttons and repeat it as we get more. 
  ''' </summary>
  Sub AddUILabsCommandButtons(ByVal panel As RibbonPanel)

    ' Get the location of this dll. 
    Dim assembly As String = [GetType]().Assembly.Location

    ' Create three push buttons to stack up 
    ' #1 
    Dim pushButtonData1 As New PushButtonData("UILabsSelection", "Pick Sampler", assembly, _uiLabName + ".UISelection")
    pushButtonData1.Image = NewBitmapImage("basics.ico")

    ' #2 
    Dim pushButtonData2 As New PushButtonData("UILabsCreateHouse", "Create House Pick", assembly, _uiLabName + ".UICreateHouse")
    pushButtonData2.Image = NewBitmapImage("House.ico")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("UILabsTaskDialog", "Dialog Sampler", assembly, _uiLabName + ".UITaskDialog")
    pushButtonData3.Image = NewBitmapImage("basics.ico")

    ' #4
    Dim pushButtonData4 As New PushButtonData("UILabsCreateHouseDialog", "Create House Dialog", assembly, _uiLabName + ".UICreateHouseDialog")
    pushButtonData4.Image = NewBitmapImage("House.ico")

    ' #5  
    ' Make three sets of pull down 

    Dim pulldownBtnData1 As New PulldownButtonData("UILabsPulldownButton1", "Selection")
    Dim pulldownBtnData2 As New PulldownButtonData("UILabsPulldownButton2", "Task Dialog")

    ' create three item stack. 
    Dim stackedItems As IList(Of RibbonItem) = panel.AddStackedItems(pulldownBtnData1, pulldownBtnData2)
    Dim pulldownBtn1 As PulldownButton = stackedItems(0)
    Dim pulldownBtn2 As PulldownButton = stackedItems(1)

    pulldownBtn1.Image = NewBitmapImage("Basics.ico")
    pulldownBtn2.Image = NewBitmapImage("Basics.ico")

    ' Add each sub items 
    Dim button1 As PushButton = pulldownBtn1.AddPushButton(pushButtonData1)
    Dim button2 As PushButton = pulldownBtn1.AddPushButton(pushButtonData2)
    Dim button3 As PushButton = pulldownBtn2.AddPushButton(pushButtonData3)
    Dim button4 As PushButton = pulldownBtn2.AddPushButton(pushButtonData4)

    ' Note: we need to set the image later.  if we do in button bata, it won't show in the Ribbon. 
    button1.Image = NewBitmapImage("Basics.ico")
    button2.Image = NewBitmapImage("Basics.ico")
    button3.Image = NewBitmapImage("Basics.ico")
    button4.Image = NewBitmapImage("Basics.ico")

  End Sub

  ''' <summary>
  ''' Add buttons for the commands we define in this labs. 
  ''' Here we stack 2 x 2-push buttons and repeat it as we get more. 
  ''' TBD: still thinking which version is better ... 
  ''' </summary>
  Sub AddUILabsCommandButtons_v2(ByVal panel As RibbonPanel)

    ' Get the location of this dll. 
    Dim assembly As String = [GetType]().Assembly.Location

    ' Create push buttons to stack up 
    ' #1 
    Dim pushButtonData1 As New PushButtonData("UILabsSelection", "Pick Sampler", assembly, _uiLabName + ".UISelection")
    pushButtonData1.Image = NewBitmapImage("basics.ico")

    ' #2 
    Dim pushButtonData2 As New PushButtonData("UILabsCreateHouseUI", "Create House Pick", assembly, _uiLabName + ".CreateHouseUI")
    pushButtonData2.Image = NewBitmapImage("basics.ico")

    ' #3  
    Dim pushButtonData3 As New PushButtonData("UILabsTaskDialog", "Dialog Sampler", assembly, _uiLabName + ".UITaskDialog")
    pushButtonData3.Image = NewBitmapImage("basics.ico")

    ' #4
    Dim pushButtonData4 As New PushButtonData("UILabsCreateHouseDialog", "Create House Dialog", assembly, _uiLabName + ".CreateHouseDialog")
    pushButtonData4.Image = NewBitmapImage("basics.ico")

    ' create 2 x 2-item stack. 
    'Dim stackedItems As IList(Of RibbonItem) = panel.AddStackedItems(pulldownBtnData1, pulldownBtnData2, pulldownBtnData3)
    Dim stackedItems1 As IList(Of RibbonItem) = panel.AddStackedItems(pushButtonData1, pushButtonData2)
    Dim stackedItems2 As IList(Of RibbonItem) = panel.AddStackedItems(pushButtonData3, pushButtonData4)

  End Sub

  ''' <summary>
  ''' Control buttons for Event and Dynamic Model Update 
  ''' </summary>
  Sub AddUILabsCommandButtons2(ByVal panel As RibbonPanel)

    ' Get the location of this dll. 
    Dim assembly As String = [GetType]().Assembly.Location

    ' Create three toggle buttons for radio button group
    ' #1 
    Dim toggleButtonData1 As New ToggleButtonData("UILabsEventOn", "Event" + vbCr + "Off", assembly, _uiLabName + ".UIEventOff")
    toggleButtonData1.LargeImage = NewBitmapImage("Basics.ico")

    ' #2 
    Dim toggleButtonData2 As New ToggleButtonData("UILabsEventOff", "Event" + vbCr + "On", assembly, _uiLabName + ".UIEventOn")
    toggleButtonData2.LargeImage = NewBitmapImage("Basics.ico")

    ' Create three toggle buttons for radio button group
    ' #3 
    Dim toggleButtonData3 As New ToggleButtonData("UILabsDynUpdateOn", "Center" + vbCr + "Off", assembly, _uiLabName + ".UIDynamicModelUpdateOff")
    toggleButtonData3.LargeImage = NewBitmapImage("Families.ico")

    ' #4 
    Dim toggleButtonData4 As New ToggleButtonData("UILabsDynUpdateOff", "Center" + vbCr + "On", assembly, _uiLabName + ".UIDynamicModelUpdateOn")
    toggleButtonData4.LargeImage = NewBitmapImage("Families.ico")

    ' Make event pn/off radio button group 
    Dim radioBtnGroupData1 As New RadioButtonGroupData("EventNotification")
    Dim radioBtnGroup1 As RadioButtonGroup = panel.AddItem(radioBtnGroupData1)
    radioBtnGroup1.AddItem(toggleButtonData1)
    radioBtnGroup1.AddItem(toggleButtonData2)

    ' Make dyn update on/off radio button group 
    Dim radioBtnGroupData2 As New RadioButtonGroupData("WindowDoorCenter")
    Dim radioBtnGroup2 As RadioButtonGroup = panel.AddItem(radioBtnGroupData2)
    radioBtnGroup2.AddItem(toggleButtonData3)
    radioBtnGroup2.AddItem(toggleButtonData4)

  End Sub

End Class

#Region "Helper Classes"
'=============================================================================
' Helper Classes 
'=============================================================================
''' <summary>
''' This lab uses Revit Intro Labs. 
''' If you prefer to use a dummy command instead, you can do so. 
''' Providing a command template here. 
''' </summary>
<Transaction(TransactionMode.Automatic)> _
Public Class DummyCommand1
  Implements IExternalCommand

  Public Function Execute(ByVal commandData As ExternalCommandData, _
                          ByRef message As String, _
                          ByVal elements As ElementSet) _
                          As Result _
                          Implements IExternalCommand.Execute

    ' Write your command here 
    TaskDialog.Show("Dummy command", "You have called Command1")

    Return Result.Succeeded
  End Function

End Class

#End Region
