ADN Revit API Training Material
===============================

This folder contains the materials used by the Autodesk Developer Network ADN for Revit API Training.

For easiest setup for the labs exercises, extract this folder under `C:\Revit 2014 API Training`.


Labs
----

Training Labs - a set of hands-on exercises that you work on during the class.


Presentation
------------

The slide set presented during the training.


Sample Drawing
--------------

Sample Revit models used for lab exercises.


Special Note about Build Warning
--------------------------------

If a Revit Application is built with "Any CPU" build configuration,
you will receive a warning similar to the following when the
RevitAPI and RevitAPIUI assemblies are referenced:

There was a mismatch between the processor architecture of the project
being built "MSIL" and the processor architecture of the reference
"RevitAPI, Version=2014.0.0.0, Culture=neutral, processorArchitecture=x86",
"AMD64". This mismatch may cause runtime failures. Please consider changing
the targeted processor architecture of your project through the
Configuration Manager so as to align the processor architectures between
your project and references, or take a dependency on references with a
processor architecture that matches the targeted processor architecture
of your project.

To overcome the above warning, we modified the lab .csproj and .vbproj
project files by adding the following lines:

    <PropertyGroup>
      <ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
        None
      </ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch>
    </PropertyGroup>

For more information on this, please look at this blog post on the
[processor architecture mismatch warning](http://thebuildingcoder.typepad.com/blog/2013/06/processor-architecture-mismatch-warning.html).


About these materials
---------------------

* Materials provided here are from our two day classroom trainings.
  You can also use this for self-learning.

* This is to introduce you to the fundamentals of Revit API and not
  meant to provide a complete coverage of Revit API nor the .NET Framework.

* Materials are in C# and VB.NET. All lab exercises are provided
  in both languages. The Powerpoint presentation is mixed.

* Disclaimer: We are aware that materials is not free of errors.
  They are corrected as we encounter them. Hopefully this will
  still be useful for you to get started with Revit API programming.


Off we go!
----------

Good luck!

AEC workgroup - Developer Technical Services - Autodesk Developer Network

March 2013


Copyright
---------

(C) Copyright 2012-2013 by Autodesk, Inc.

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
