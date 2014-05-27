Revit 2015 API Training
=======================

This repository contains material used for Revit 2015 API Training. 

Tip: For easiest set up for the labs exercises, extract to the following folder:

    C:\Revit 2015 API Training


Labs
----
Training Labs - a set of hands-on exercises during the class.

Presentation 
------------
Slide deck used for the training.

Sample Drawing
--------------
Sample .RVT Revit models used for labs. 

Special Note about build warning:
---------------------------------
If a Revit Application is built with "Any CPU" build configuration, 
you will receive a warning similar to the following when and the 
RevitAPI and RevitAPIUI assemblies are referenced:

There was a mismatch between the processor architecture of the project 
being built "MSIL" and the processor architecture of the reference 
"RevitAPI, Version=2015.0.0.0, Culture=neutral, processorArchitecture=x86", 
"AMD64". This mismatch may cause runtime failures. Please consider changing 
the targeted processor architecture of your project through the 
Configuration Manager so as to align the processor architectures between 
your project and references, or take a dependency on references with a 
processor architecture that matches the targeted processor architecture 
of your project.

To overcome the above warning, we modified the Labs .csproj and .vbproj
files to add the following lines.

<PropertyGroup>
  <ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
    None
  </ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
</PropertyGroup>

For more information on this, please look at this blog post:

http://thebuildingcoder.typepad.com/blog/2013/06/processor-architecture-mismatch-warning.html


About this material
-------------------

* Materials provided here are from our two day classroom trainings. 
  You can also use this for self-learning. 

* This is to introduce you to the fundamentals of Revit API to get 
  you started. (Not meant to provide a complete coverage of 
  Revit API nor .NET Framework.) 

* Materials are in C# and VB.NET. Labs exercises are provided 
  in two languages. Powerpoint presentation is mixed. 

* Disclaimer: We are aware that materials is not free of errors. 
  We intend to correct them as we encounter. We hope this will 
  be still useful for you to get started with Revit API programming. 

Good luck!  

AEC workgroup  
Developer Technical Services  
Autodesk Developer Network  

May 2014 


Copyright
---------

(C) Copyright 2013-2014 by Autodesk, Inc. 

Permission to use, copy, modify, and distribute this software in
object code form for any purpose and without fee is hereby granted, 
provided that the above copyright notice appears in all copies and 
that both that copyright notice and the limited warranty and
restricted rights notice below appear in all supporting 
documentation.

AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
UNINTERRUPTED OR ERROR FREE.
