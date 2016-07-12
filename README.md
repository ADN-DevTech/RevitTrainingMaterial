# Revit API Labs Training Material

This repository contains the ADN Revit API Labs, the material we use for our two-day hands-on Revit API training classes.

It includes both the labs themselves, consisting of sample source code exercises in both C# and VB for you to fill in, corresponding instruction documents for both languages, and an accompanying slide deck.

The labs cover three main areas: 

- Introduction for getting started with the Revit API, database, elements and properties
- User interface programming to create an external application and custom ribbon
- Family API for programmatic family generation

Tip: For easiest set-up of the labs exercises, extract to the following folder:

    C:\Revit API Training


## Labs

Training Labs &ndash; a set of hands-on exercises during the class.

## Presentation

Slide deck used for the training.

## Sample Drawing

Sample .RVT Revit models used for labs.

## Note about Build Warning

If a Revit Application is built with "Any CPU" build configuration,
you will receive a warning similar to the following when and the
RevitAPI and RevitAPIUI assemblies are referenced:

There was a mismatch between the processor architecture of the project
being built "MSIL" and the processor architecture of the reference
"RevitAPI, Version=XXXX.0.0.0, Culture=neutral, processorArchitecture=x86",
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


## About this material

* The material provided here is used for our two-day classroom trainings.
  You can also use it for self-learning.

* This introduces you to the fundamentals of the Revit API to get
  you started. It does not provide a complete coverage of the entire
  Revit API or .NET Framework.

* Materials are in both C# and VB.NET. Labs exercises are provided
  in two languages. The accompanying PowerPoint presentation is mixed.

* Disclaimer: We are aware that this material is not completely free of errors.
  We correct them as we encounter them.
Please help us by forking this repository, fixing the issues you note and submitting a pull request for us to integrate your fixes back into the main repository. Thank you!
We hope this will still be useful for you to get
started with Revit API programming.

Good luck!

AEC workgroup
Developer Technical Services
Autodesk Developer Network

July 2016

## Copyright

(C) Copyright 2009-2016 by Autodesk, Inc.

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


## License

This sample is licensed under the terms of the [MIT License](http://www.apache.org/licenses/LICENSE-2.0).
Please see the [LICENSE](LICENSE) file for full details.
