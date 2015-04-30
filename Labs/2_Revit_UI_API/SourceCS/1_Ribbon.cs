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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Util;
#endregion

namespace UiCs
{
  /// <summary>
  /// Ribbon UI. 
  /// we'll be using commands we defined in Revit Intro labs. alternatively, 
  /// you can use your own. Any command will do for this ribbon exercise. 
  /// cf. Developer Guide, Section 3.8: Ribbon Panels and Controls. (pp 46). 
  /// </summary>
  public class UIRibbon : IExternalApplication
  {
    /// <summary>
    /// This is both the assembly name and the namespace 
    /// of the external command provider.
    /// </summary>
    const string _introLabName = "IntroCs";
    const string _uiLabName = "UiCs";
    const string _dllExtension = ".dll";

    /// <summary>
    /// Name of subdirectory containing images.
    /// </summary>
    const string _imageFolderName = "Images";

    /// <summary>
    /// Location of managed dll where we have defined the commands.
    /// </summary>
    string _introLabPath;

    /// <summary>
    /// Location of images for icons.
    /// </summary>
    string _imageFolder;

    /// <summary>
    /// Starting at the given directory, search upwards for 
    /// a subdirectory with the given target name located
    /// in some parent directory. 
    /// </summary>
    /// <param name="path">Starting directory, e.g. GetDirectoryName( GetExecutingAssembly().Location ).</param>
    /// <param name="target">Target subdirectory name, e.g. "Images".</param>
    /// <returns>The full path of the target directory if found, else null.</returns>
    string FindFolderInParents(string path, string target)
    {
      Debug.Assert(Directory.Exists(path),
        "expected an existing directory to start search in");

      string s;

      do
      {
        s = Path.Combine(path, target);
        if (Directory.Exists(s))
        {
          return s;
        }
        path = Path.GetDirectoryName(path);
      } while (null != path);

      return null;
    }

    /// <summary>
    /// Load a new icon bitmap from our image folder.
    /// </summary>
    BitmapImage NewBitmapImage(string imageName)
    {
      return new BitmapImage(new Uri(
        Path.Combine(_imageFolder, imageName)));
    }

    /// <summary>
    /// OnShutdown() - called when Revit ends. 
    /// </summary>
    public Result OnShutdown(UIControlledApplication app)
    {
      return Result.Succeeded;
    }

    /// <summary>
    /// OnStartup() - called when Revit starts. 
    /// </summary>
    public Result OnStartup(UIControlledApplication app)
    {
      // External application directory:

      string dir = Path.GetDirectoryName(
        System.Reflection.Assembly
        .GetExecutingAssembly().Location);

      // External command path:

      _introLabPath = Path.Combine(dir, _introLabName + _dllExtension);

      if (!File.Exists(_introLabPath))
      {
        TaskDialog.Show("UIRibbon", "External command assembly not found: " + _introLabPath);
        return Result.Failed;
      }

      // Image path:

      _imageFolder = FindFolderInParents(dir, _imageFolderName);

      if (null == _imageFolder
        || !Directory.Exists(_imageFolder))
      {
        TaskDialog.Show(
          "UIRibbon",
          string.Format(
            "No image folder named '{0}' found in the parent directories of '{1}.",
            _imageFolderName, dir));

        return Result.Failed;
      }

      // Show what kind of custom buttons and controls 
      // we can add to the Add-Ins tab 

      AddRibbonSampler(app);

      // Add one for UI Labs, too. 

      AddUILabsButtons(app);

      return Result.Succeeded;
    }

    /// <summary>
    /// Create our own ribbon panel with verious buttons 
    /// for our exercise. We re-use commands defined in the
    /// Revit Intro Labs here. Cf. Section 3.8 (pp 46) of 
    /// the Developers Guide. 
    /// </summary>
    public void AddRibbonSampler(UIControlledApplication app)
    {
      // (1) create a ribbon tab and ribbon panel 

      app.CreateRibbonTab("Ribbon Sampler");

      RibbonPanel panel = app.CreateRibbonPanel("Ribbon Sampler", "Ribbon Sampler");

      // Below are samplers of ribbon items. Uncomment 
      // functions of your interest to see how it looks like 

      // (2.1) add a simple push button for Hello World 

      AddPushButton(panel);

      // (2.2) add split buttons for "Command Data", "DB Element" and "Element Filtering" 

      AddSplitButton(panel);

      // (2.3) add pulldown buttons for "Command Data", "DB Element" and "Element Filtering" 

      AddPulldownButton(panel);

      // (2.4) add radio/toggle buttons for "Command Data", "DB Element" and "Element Filtering" 
      // we put it on the slide-out below. 
      //AddRadioButton(panel);
      //panel.AddSeparator();

      // (2.5) add text box - TBD: this is used with the conjunction with event. Probably too complex for day one training. 
      //  for now, without event. 
      // we put it on the slide-out below. 
      //AddTextBox(panel);
      //panel.AddSeparator();

      // (2.6) combo box - TBD: this is used with the conjunction with event. Probably too complex for day one training. 
      // For now, without event. show two groups: Element Bascis (3 push buttons) and Modification/Creation (2 push button)  

      AddComboBox(panel);

      // (2.7) stacked items - 1. hello world push button, 2. pulldown element bscis (command data, DB element, element filtering) 
      // 3. pulldown modification/creation(element modification, model creation). 

      AddStackedButtons_Complex(panel);

      // (2.8) slide out - if you don't have enough space, you can add additional space below the panel. 
      // anything which comes after this will be on the slide out. 

      panel.AddSlideOut();

      // (2.4) radio button - what it is 

      AddRadioButton(panel);

      // (2.5) text box - what it is 

      AddTextBox(panel);
    }

    /// <summary>
    /// We create our own buttons for UI Labs, too. 
    /// cf. Section 3.8 (pp 46) of Developer Guide. 
    /// </summary>
    public void AddUILabsButtons(UIControlledApplication app)
    {
      // Create a ribbon panel 

      RibbonPanel panel = app.CreateRibbonPanel("UI Labs");

      // (3) adding buttons for the current labs itself. 
      // You may modify this AFTER each command are defined in each lab. 
      //AddUILabsCommandButtons_Template(panel) ' dummy 

      AddUILabsCommandButtons(panel);

      // (4) This is for Lab4 event and dynamic update. 

      AddUILabsCommandButtons2(panel);
    }

    /// <summary>
    /// Simple push button for "Hello World" 
    /// </summary>
    public void AddPushButton(RibbonPanel panel)
    {
      // Set the information about the command we will be assigning to the button 

      PushButtonData pushButtonDataHello 
        = new PushButtonData(
          "PushButtonHello", 
          "Hello World", 
          _introLabPath,
          _introLabName + ".HelloWorld" ); // could also use typeof(HelloWorld).FullName here

      // Add a button to the panel 

      PushButton pushButtonHello = panel.AddItem(pushButtonDataHello) as PushButton;

      // Add an icon 
      // Make sure you reference WindowsBase and PresentationCore, and import System.Windows.Media.Imaging namespace. 

      pushButtonHello.LargeImage = NewBitmapImage("ImgHelloWorld.png");

      // Add a tooltip 

      pushButtonHello.ToolTip = "simple push button";
    }

    /// <summary>
    /// Split button for "Command Data", "DB Element" and "Element Filtering" 
    /// </summary>
    public void AddSplitButton(RibbonPanel panel)
    {
      // Create three push buttons for split button drop down 

      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("SplitCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData");
      pushButtonData1.LargeImage = NewBitmapImage("ImgHelloWorld.png");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("SplitDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement");
      pushButtonData2.LargeImage = NewBitmapImage("ImgHelloWorld.png");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("SplitElementFiltering", "ElementFiltering", _introLabPath, _introLabName + ".ElementFiltering");
      pushButtonData3.LargeImage = NewBitmapImage("ImgHelloWorld.png");

      // Make a split button now 
      SplitButtonData splitBtnData = new SplitButtonData("SplitButton", "Split Button");
      SplitButton splitBtn = panel.AddItem(splitBtnData) as SplitButton;
      splitBtn.AddPushButton(pushButtonData1);
      splitBtn.AddPushButton(pushButtonData2);
      splitBtn.AddPushButton(pushButtonData3);
    }

    /// <summary>
    /// Pulldown button for "Command Data", "DB Element" and "Element Filtering"
    /// </summary>
    public void AddPulldownButton(RibbonPanel panel)
    {
      // Create three push buttons for pulldown button drop down 

      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("PulldownCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData");
      pushButtonData1.LargeImage = NewBitmapImage("Basics.ico");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("PulldownDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement");
      pushButtonData2.LargeImage = NewBitmapImage("Basics.ico");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("PulldownElementFiltering", "Filtering", _introLabPath, _introLabName + ".ElementFiltering");
      pushButtonData3.LargeImage = NewBitmapImage("Basics.ico");

      // Make a pulldown button now 
      PulldownButtonData pulldownBtnData = new PulldownButtonData("PulldownButton", "Pulldown");
      PulldownButton pulldownBtn = panel.AddItem(pulldownBtnData) as PulldownButton;
      pulldownBtn.AddPushButton(pushButtonData1);
      pulldownBtn.AddPushButton(pushButtonData2);
      pulldownBtn.AddPushButton(pushButtonData3);
    }

    /// <summary>
    /// Radio/toggle button for "Command Data", "DB Element" and "Element Filtering"
    /// </summary>
    public void AddRadioButton(RibbonPanel panel)
    {
      // Create three toggle buttons for radio button group 

      // #1 
      ToggleButtonData toggleButtonData1 = new ToggleButtonData("RadioCommandData", "Command" + "\n Data", _introLabPath, _introLabName + ".CommandData");
      toggleButtonData1.LargeImage = NewBitmapImage("Basics.ico");

      // #2 
      ToggleButtonData toggleButtonData2 = new ToggleButtonData("RadioDbElement", "DB" + "\n Element", _introLabPath, _introLabName + ".DBElement");
      toggleButtonData2.LargeImage = NewBitmapImage("Basics.ico");

      // #3 
      ToggleButtonData toggleButtonData3 = new ToggleButtonData("RadioElementFiltering", "Filtering", _introLabPath, _introLabName + ".ElementFiltering");
      toggleButtonData3.LargeImage = NewBitmapImage("Basics.ico");

      // Make a radio button group now 
      RadioButtonGroupData radioBtnGroupData = new RadioButtonGroupData("RadioButton");
      RadioButtonGroup radioBtnGroup = panel.AddItem(radioBtnGroupData) as RadioButtonGroup;
      radioBtnGroup.AddItem(toggleButtonData1);
      radioBtnGroup.AddItem(toggleButtonData2);
      radioBtnGroup.AddItem(toggleButtonData3);
    }

    /// <summary>
    /// Text box 
    /// Text box used in conjunction with event. We'll come to this later. 
    /// For now, just shows how to make a text box. 
    /// </summary>
    public void AddTextBox(RibbonPanel panel)
    {
      // Fill the text box information 
      TextBoxData txtBoxData = new TextBoxData("TextBox");
      txtBoxData.Image = NewBitmapImage("Basics.ico");
      txtBoxData.Name = "Text Box";
      txtBoxData.ToolTip = "Enter text here";
      txtBoxData.LongDescription = "<p>This is Revit UI Labs.</p><p>Ribbon Lab</p>";
      txtBoxData.ToolTipImage = NewBitmapImage("ImgHelloWorld.png");

      // Create the text box item on the panel 
      TextBox txtBox = panel.AddItem(txtBoxData) as TextBox;
      txtBox.PromptText = "Enter a comment";
      txtBox.ShowImageAsButton = true;

      txtBox.EnterPressed += new EventHandler<Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs>(txtBox_EnterPressed);
      txtBox.Width = 180;
    }

    /// <summary>
    /// Event handler for the above text box 
    /// </summary>
    void txtBox_EnterPressed(object sender, Autodesk.Revit.UI.Events.TextBoxEnterPressedEventArgs e)
    {
      // Cast sender to TextBox to retrieve text value
      TextBox textBox = sender as TextBox;
      TaskDialog.Show("TextBox Input", "This is what you typed in: " + textBox.Value.ToString());
    }

    /// <summary>
    /// Combo box - 5 items in 2 groups. 
    /// Combo box is used in conjunction with event. We'll come back later. 
    /// For now, just demonstrates how to make a combo box. 
    /// </summary>
    public void AddComboBox(RibbonPanel panel)
    {
      // Create five combo box members with two groups 

      // #1 
      ComboBoxMemberData comboBoxMemberData1 = new ComboBoxMemberData("ComboCommandData", "Command Data");
      comboBoxMemberData1.Image = NewBitmapImage("Basics.ico");
      comboBoxMemberData1.GroupName = "DB Basics";

      // #2 
      ComboBoxMemberData comboBoxMemberData2 = new ComboBoxMemberData("ComboDbElement", "DB Element");
      comboBoxMemberData2.Image = NewBitmapImage("Basics.ico");
      comboBoxMemberData2.GroupName = "DB Basics";

      // #3 
      ComboBoxMemberData comboBoxMemberData3 = new ComboBoxMemberData("ComboElementFiltering", "Filtering");
      comboBoxMemberData3.Image = NewBitmapImage("Basics.ico");
      comboBoxMemberData3.GroupName = "DB Basics";

      // #4 
      ComboBoxMemberData comboBoxMemberData4 = new ComboBoxMemberData("ComboElementModification", "Modify");
      comboBoxMemberData4.Image = NewBitmapImage("Basics.ico");
      comboBoxMemberData4.GroupName = "Modeling";

      // #5 
      ComboBoxMemberData comboBoxMemberData5 = new ComboBoxMemberData("ComboModelCreation", "Create");
      comboBoxMemberData5.Image = NewBitmapImage("Basics.ico");
      comboBoxMemberData5.GroupName = "Modeling";

      // Make a combo box now 
      ComboBoxData comboBxData = new ComboBoxData("ComboBox");
      ComboBox comboBx = panel.AddItem(comboBxData) as ComboBox;
      comboBx.ToolTip = "Select an Option";
      comboBx.LongDescription = "select a command you want to run";
      comboBx.AddItem(comboBoxMemberData1);
      comboBx.AddItem(comboBoxMemberData2);
      comboBx.AddItem(comboBoxMemberData3);
      comboBx.AddItem(comboBoxMemberData4);
      comboBx.AddItem(comboBoxMemberData5);

      comboBx.CurrentChanged += new EventHandler<Autodesk.Revit.UI.Events.ComboBoxCurrentChangedEventArgs>(comboBx_CurrentChanged);
    }

    /// <summary>
    /// Event handler for the above combo box 
    /// </summary>    
    void comboBx_CurrentChanged(object sender, Autodesk.Revit.UI.Events.ComboBoxCurrentChangedEventArgs e)
    {
      // Cast sender as TextBox to retrieve text value
      ComboBox combodata = sender as ComboBox;
      ComboBoxMember member = combodata.Current;
      TaskDialog.Show("Combobox Selection", "Your new selection: " + member.ItemText);
    }

    /// <summary>
    /// Stacked Buttons - combination of: push button, dropdown button, combo box and text box. 
    /// (no radio button group, split buttons). 
    /// Here we stack three push buttons for "Command Data", "DB Element" and "Element Filtering". 
    /// </summary>
    public void AddStackedButtons_Simple(RibbonPanel panel)
    {
      // Create three push buttons to stack up 
      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("StackSimpleCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData");
      pushButtonData1.Image = NewBitmapImage("ImgHelloWorldSmall.png");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("StackSimpleDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement");
      pushButtonData2.Image = NewBitmapImage("ImgHelloWorldSmall.png");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("StackSimpleElementFiltering", "Element Filtering", _introLabPath, _introLabName + ".ElementFiltering");
      pushButtonData3.Image = NewBitmapImage("ImgHelloWorldSmall.png");

      // Put them on stack 
      IList<RibbonItem> stackedButtons = panel.AddStackedItems(pushButtonData1, pushButtonData2, pushButtonData3);
    }

    /// <summary>
    /// Stacked Buttons - combination of: push button, dropdown button, combo box and text box. 
    /// (no radio button group, split buttons). 
    /// Here we define 6 buttons, make grouping of 1, 3, 2 items, and stack them in three layer: 
    /// (1) simple push button with "Hello World" 
    /// (2) pull down with 3 items: "Command Data", "DB Element" and "Element Filtering". 
    /// (3) pull down with 2 items: "Element Modification" and "Model Creation" 
    /// </summary>
    public void AddStackedButtons_Complex(RibbonPanel panel)
    {
      // Create six push buttons to group for pull down and stack up 

      // #0 
      PushButtonData pushButtonData0 = new PushButtonData("StackComplexHelloWorld", "Hello World", _introLabPath, _introLabName + ".HelloWorld");
      pushButtonData0.Image = NewBitmapImage("Basics.ico");

      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("StackComplexCommandData", "Command Data", _introLabPath, _introLabName + ".CommandData");
      pushButtonData1.Image = NewBitmapImage("Basics.ico");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("StackComplexDbElement", "DB Element", _introLabPath, _introLabName + ".DBElement");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("StackComplexElementFiltering", "Filtering", _introLabPath, _introLabName + ".ElementFiltering");

      // #4 
      PushButtonData pushButtonData4 = new PushButtonData("StackComplexElementModification", "Modify", _introLabPath, _introLabName + ".ElementModification");

      // #5 
      PushButtonData pushButtonData5 = new PushButtonData("StackComplexModelCreation", "Create", _introLabPath, _introLabName + ".ModelCreation");

      // Make two sets of pull down 

      PulldownButtonData pulldownBtnData1 = new PulldownButtonData("StackComplePulldownButton1", "DB Basics");
      PulldownButtonData pulldownBtnData2 = new PulldownButtonData("StackComplePulldownButton2", "Modeling");

      // Create three item stack. 
      IList<RibbonItem> stackedItems = panel.AddStackedItems(pushButtonData0, pulldownBtnData1, pulldownBtnData2);
      PulldownButton pulldownBtn2 = stackedItems[1] as PulldownButton;
      PulldownButton pulldownBtn3 = stackedItems[2] as PulldownButton;

      pulldownBtn2.Image = NewBitmapImage("Basics.ico");
      pulldownBtn3.Image = NewBitmapImage("House.ico");

      // Add each sub items 
      PushButton button1 = pulldownBtn2.AddPushButton(pushButtonData1);
      PushButton button2 = pulldownBtn2.AddPushButton(pushButtonData2);
      PushButton button3 = pulldownBtn2.AddPushButton(pushButtonData3);
      PushButton button4 = pulldownBtn3.AddPushButton(pushButtonData4);
      PushButton button5 = pulldownBtn3.AddPushButton(pushButtonData5);

      // Note: we need to set the image later. if we do in button data, it won't show in the Ribbon. 
      button1.Image = NewBitmapImage("Basics.ico");
      button2.Image = NewBitmapImage("Basics.ico");
      button3.Image = NewBitmapImage("Basics.ico");
      button4.Image = NewBitmapImage("Basics.ico");

      button5.Image = NewBitmapImage("Basics.ico");
    }

    /// <summary>
    /// Add buttons for the commands we define in this labs. 
    /// Here we stack three push buttons and repeat it as we get more. 
    /// This is a template to use during the Ribbon lab exercise prior to going to following labs. 
    /// </summary>
    public void AddUILabsCommandButtons_Template(RibbonPanel panel)
    {
      // Get the location of this dll. 
      string assembly = GetType().Assembly.Location;

      // Create three push buttons to stack up 
      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("UILabsCommand1", "Command1", assembly, _uiLabName + ".Command1");
      pushButtonData1.Image = NewBitmapImage("ImgHelloWorldSmall.png");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("UILabsCommand2", "Command2", assembly, _uiLabName + ".Command2");
      pushButtonData2.Image = NewBitmapImage("ImgHelloWorldSmall.png");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("UILabsCommand3", "Command3", assembly, _uiLabName + ".Command3");
      pushButtonData3.Image = NewBitmapImage("ImgHelloWorldSmall.png");

      // Put them on stack 

      IList<RibbonItem> stackedButtons = panel.AddStackedItems(pushButtonData1, pushButtonData2, pushButtonData3);
    }

    /// <summary>
    /// Add buttons for the commands we define in this labs. 
    /// Here we stack three push buttons and repeat it as we get more. 
    /// </summary>
    public void AddUILabsCommandButtons(RibbonPanel panel)
    {
      // Get the location of this dll. 
      string assembly = GetType().Assembly.Location;

      // Create three push buttons to stack up 
      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("UILabsSelection", "Pick Sampler", assembly, _uiLabName + ".UISelection");
      pushButtonData1.Image = NewBitmapImage("basics.ico");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("UILabsCreateHouse", "Create House Pick", assembly, _uiLabName + ".UICreateHouse");
      pushButtonData2.Image = NewBitmapImage("House.ico");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("UILabsTaskDialog", "Dialog Sampler", assembly, _uiLabName + ".UITaskDialog");
      pushButtonData3.Image = NewBitmapImage("basics.ico");

      // #4 
      PushButtonData pushButtonData4 = new PushButtonData("UILabsCreateHouseDialog", "Create House Dialog", assembly, _uiLabName + ".UICreateHouseDialog");
      pushButtonData4.Image = NewBitmapImage("House.ico");

      // #5 
      // Make three sets of pull down 
      PulldownButtonData pulldownBtnData1 = new PulldownButtonData("UILabsPulldownButton1", "Selection");
      PulldownButtonData pulldownBtnData2 = new PulldownButtonData("UILabsPulldownButton2", "Task Dialog");

      // Create three item stack. 
      IList<RibbonItem> stackedItems = panel.AddStackedItems(pulldownBtnData1, pulldownBtnData2);
      PulldownButton pulldownBtn1 = stackedItems[0] as PulldownButton;
      PulldownButton pulldownBtn2 = stackedItems[1] as PulldownButton;

      pulldownBtn1.Image = NewBitmapImage("Basics.ico");
      pulldownBtn2.Image = NewBitmapImage("Basics.ico");

      // Add each sub items 
      PushButton button1 = pulldownBtn1.AddPushButton(pushButtonData1);
      PushButton button2 = pulldownBtn1.AddPushButton(pushButtonData2);
      PushButton button3 = pulldownBtn2.AddPushButton(pushButtonData3);
      PushButton button4 = pulldownBtn2.AddPushButton(pushButtonData4);

      // Note: we need to set the image later. if we do in button data, it won't show in the Ribbon. 
      button1.Image = NewBitmapImage("Basics.ico");
      button2.Image = NewBitmapImage("Basics.ico");
      button3.Image = NewBitmapImage("Basics.ico");
      button4.Image = NewBitmapImage("Basics.ico");
    }

    /// <summary>
    /// Add buttons for the commands we define in this labs. 
    /// Here we stack 2 x 2-push buttons and repeat it as we get more. 
    /// TBD: still thinking which version is better ... 
    /// </summary>
    public void AddUILabsCommandButtons_v2(RibbonPanel panel)
    {
      // Get the location of this dll. 
      string assembly = GetType().Assembly.Location;

      // Create push buttons to stack up 
      // #1 
      PushButtonData pushButtonData1 = new PushButtonData("UILabsSelection", "Pick Sampler", assembly, _uiLabName + ".UISelection");
      pushButtonData1.Image = NewBitmapImage("basics.ico");

      // #2 
      PushButtonData pushButtonData2 = new PushButtonData("UILabsCreateHouseUI", "Create House Pick", assembly, _uiLabName + ".CreateHouseUI");
      pushButtonData2.Image = NewBitmapImage("basics.ico");

      // #3 
      PushButtonData pushButtonData3 = new PushButtonData("UILabsTaskDialog", "Dialog Sampler", assembly, _uiLabName + ".UITaskDialog");
      pushButtonData3.Image = NewBitmapImage("basics.ico");

      // #4 
      PushButtonData pushButtonData4 = new PushButtonData("UILabsCreateHouseDialog", "Create House Dialog", assembly, _uiLabName + ".CreateHouseDialog");
      pushButtonData4.Image = NewBitmapImage("basics.ico");

      // Create 2 x 2-item stack. 
      IList<RibbonItem> stackedItems1 = panel.AddStackedItems(pushButtonData1, pushButtonData2);

      IList<RibbonItem> stackedItems2 = panel.AddStackedItems(pushButtonData3, pushButtonData4);
    }

    /// <summary>
    /// Control buttons for Event and Dynamic Model Update 
    /// </summary>
    public void AddUILabsCommandButtons2(RibbonPanel panel)
    {
      // Get the location of this dll. 
      string assembly = GetType().Assembly.Location;

      // Create three toggle buttons for radio button group 
      // #1 
      ToggleButtonData toggleButtonData1 = new ToggleButtonData("UILabsEventOn", "Event" + "\n Off", assembly, _uiLabName + ".UIEventOff");
      toggleButtonData1.LargeImage = NewBitmapImage("Basics.ico");

      // #2 
      ToggleButtonData toggleButtonData2 = new ToggleButtonData("UILabsEventOff", "Event" + "\n On", assembly, _uiLabName + ".UIEventOn");
      toggleButtonData2.LargeImage = NewBitmapImage("Basics.ico");

      // Create three toggle buttons for radio button group 
      // #3 
      ToggleButtonData toggleButtonData3 = new ToggleButtonData("UILabsDynUpdateOn", "Center" + "\n Off", assembly, _uiLabName + ".UIDynamicModelUpdateOff");
      toggleButtonData3.LargeImage = NewBitmapImage("Families.ico");

      // #4 
      ToggleButtonData toggleButtonData4 = new ToggleButtonData("UILabsDynUpdateOff", "Center" + "\n On", assembly, _uiLabName + ".UIDynamicModelUpdateOn");
      toggleButtonData4.LargeImage = NewBitmapImage("Families.ico");

      // Make event pn/off radio button group 
      RadioButtonGroupData radioBtnGroupData1 = new RadioButtonGroupData("EventNotification");
      RadioButtonGroup radioBtnGroup1 = panel.AddItem(radioBtnGroupData1) as RadioButtonGroup;
      radioBtnGroup1.AddItem(toggleButtonData1);
      radioBtnGroup1.AddItem(toggleButtonData2);

      // Make dyn update on/off radio button group 
      RadioButtonGroupData radioBtnGroupData2 = new RadioButtonGroupData("WindowDoorCenter");
      RadioButtonGroup radioBtnGroup2 = panel.AddItem(radioBtnGroupData2) as RadioButtonGroup;
      radioBtnGroup2.AddItem(toggleButtonData3);

      radioBtnGroup2.AddItem(toggleButtonData4);
    }
  }

  #region Helper Classes
  /// <summary>
  /// This lab uses Revit Intro Labs. 
  /// If you prefer to use a dummy command instead, you can do so. 
  /// Providing a command template here. 
  /// </summary>
  [Transaction(TransactionMode.ReadOnly)]
  public class DummyCommand1 : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      // Write your command implementation here 

      TaskDialog.Show("Dummy command", "You have called Command1");

      return Result.Succeeded;
    }
  }
  #endregion
}
