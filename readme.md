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
