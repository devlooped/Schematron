# Schematron.NET

[![Version](https://img.shields.io/nuget/vpre/Schematron.svg?color=royalblue)](https://www.nuget.org/packages/Schematron)
[![Downloads](https://img.shields.io/nuget/dt/Schematron.svg?color=darkmagenta)](https://www.nuget.org/packages/Schematron)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/devlooped/Schematron/blob/main/license.txt)
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/Schematron)

<!-- #description -->
A .NET implementation of Schematron for validating XML with standalone `.sch` schemas or
Schematron rules embedded in W3C XML Schema. The library supports both the current ISO
Schematron namespace and the legacy ASCC namespace, with a compact public API centered on
`Validator` and `Schema`.
<!-- #description -->

<!-- include https://github.com/devlooped/.github/raw/main/osmf.md -->
## Open Source Maintenance Fee

To ensure the long-term sustainability of this project, users of this package who generate 
revenue must pay an [Open Source Maintenance Fee](https://opensourcemaintenancefee.org). 
While the source code is freely available under the terms of the [License](license.txt), 
this package and other aspects of the project require [adherence to the Maintenance Fee](osmfeula.txt).

To pay the Maintenance Fee, [become a Sponsor](https://github.com/sponsors/devlooped) at the proper 
OSMF tier. A single fee covers all of [Devlooped packages](https://www.nuget.org/profiles/Devlooped).

<!-- https://github.com/devlooped/.github/raw/main/osmf.md -->

<!-- #content -->
## Installation

```shell
dotnet add package Schematron
```

The package targets `netstandard2.0` and `net8.0`.

## Usage

The package exposes two primary entry points:

- `Validator` loads Schematron and XML Schema definitions and validates XML documents.
- `Schema` loads and inspects Schematron documents programmatically.

### One-shot validation with a standalone Schematron schema

```csharp
using Schematron;

var validator = new Validator();
validator.AddSchema("order.sch");

try
{
    validator.Validate("order.xml");
    Console.WriteLine("Document is valid.");
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message);
}
```

`Validate` returns the loaded `IXPathNavigable` document on success and throws
`ValidationException` when XML Schema or Schematron validation fails.

### Validating XSD plus embedded Schematron

When the schema is a W3C XML Schema document with embedded Schematron, `Validator` runs
both validations in one pass:

```csharp
using Schematron;

var validator = new Validator(OutputFormatting.XML);

// Use the overload with the target namespace when the XSD imports or includes other schemas.
validator.AddSchema("http://example.com/po-schematron", "po-schema.xsd");

try
{
    validator.Validate("purchase-order.xml");
}
catch (ValidationException ex)
{
    Console.WriteLine(ex.Message); // XML formatted output
}
```

### Schematron-only validation for in-memory XML

If you already have an `IXPathNavigable`, use `ValidateSchematron` to skip XML Schema
validation and evaluate only the loaded Schematron rules:

```csharp
using System.Xml.XPath;
using Schematron;

var validator = new Validator();
validator.AddSchema("rules.sch");

var document = new XPathDocument("order.xml");
validator.ValidateSchematron(document);
```

### Selecting the phase, formatter, and return type

`Validator` lets you control the active phase, output format, and result type:

```csharp
using Schematron;
using Schematron.Formatters;

var validator = new Validator(OutputFormatting.XML, NavigableType.XmlDocument)
{
    Phase = "paymentInfo",
    Formatter = new XmlFormatter(),
};

validator.AddSchema("purchase-order.sch");
var result = validator.Validate("purchase-order.xml");
```

Use `Phase.All` (`#ALL`) to evaluate every pattern regardless of phase activation.

### Loading and inspecting a schema

Use `Schema` directly when you want to inspect a Schematron document without validating
an instance document yet:

```csharp
using Schematron;

var schema = new Schema();
schema.Load("rules.sch");

Console.WriteLine(schema.Title);
Console.WriteLine(schema.SchematronEdition);
Console.WriteLine(schema.DefaultPhase);
Console.WriteLine(schema.IsLibrary);
Console.WriteLine(schema.Patterns.Count);
Console.WriteLine(schema.Lets.Contains("maxAge"));
```

This is useful for tooling, analyzers, test helpers, or apps that need to inspect declared
phases, patterns, diagnostics, parameters, and `let` bindings.

## Core API

Schematron keeps the public surface intentionally small:

- `Validator` is the main entry point for loading schemas and validating XML.
- `Schema` loads and inspects Schematron documents programmatically.
- `OutputFormatting` selects the built-in output style: `Default`/`Log`, `Simple`,
  `Boolean`, or `XML`.
- `Validator.Phase`, `Validator.Formatter`, and `Validator.ReturnType` let you choose
  the active phase, custom output formatter, and returned document type.
- `BadSchemaException` signals invalid schema input, while `ValidationException`
  signals XML or Schematron validation failures.

## Supported features

The package currently supports the main scenarios expected for a v1 release, including:

- Standalone `.sch` schemas and Schematron embedded in W3C XML Schema
- ISO Schematron namespace (`http://purl.oclc.org/dsdl/schematron`) and legacy ASCC namespace compatibility
- ISO Schematron 2025 features such as `<library>`, phase `@when`, rule `@visit-each`, rule flags, and schema parameters
- Schema-, pattern-, and rule-level `let` bindings
- Diagnostics, severity metadata, abstract patterns/rules, and groups
<!-- #content -->

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Khamza Davletov](https://avatars.githubusercontent.com/u/13615108?u=11b0038e255cdf9d1940fbb9ae9d1d57115697ab&v=4&s=39 "Khamza Davletov")](https://github.com/khamza85)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://avatars.githubusercontent.com/u/67574?u=3991fb983e1c399edf39aebc00a9f9cd425703bd&v=4&s=39 "Kori Francis")](https://github.com/kfrancis)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![mccaffers](https://avatars.githubusercontent.com/u/16667079?u=110034edf51097a5ee82cb6a94ae5483568e3469&v=4&s=39 "mccaffers")](https://github.com/mccaffers)
[![Seika Logiciel](https://avatars.githubusercontent.com/u/2564602?v=4&s=39 "Seika Logiciel")](https://github.com/SeikaLogiciel)
[![Andrew Grant](https://avatars.githubusercontent.com/devlooped-user?s=39 "Andrew Grant")](https://github.com/wizardness)
[![eska-gmbh](https://avatars.githubusercontent.com/devlooped-team?s=39 "eska-gmbh")](https://github.com/eska-gmbh)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
