# AGENTS.md

## Project Overview

Schematron.NET is a C# high-performance implementation of the [Schematron ISO/IEC standard](http://schematron.com/) for XML validation. It supports standalone Schematron schemas, schemas embedded inside W3C XML Schema (WXS), and SOAP web-service validation via a SoapExtension.

## Repository Structure

```
Schematron.slnx               # SDK-style XML solution (slnx format)
src/
  Directory.Build.props       # Shared MSBuild properties (authors, copyright, LangVersion, GitInfo)
  Schematron/                 # Core library (net48)
  Schematron.Tests/           # xUnit test project (net48)
  Schematron.WebServices/     # SOAP SoapExtension integration (net48, requires System.Web.Services)
VERSION                       # Base version file read by GitInfo (e.g. 0.6.0)
```

## Key Components

### Schematron (core)

| File | Purpose |
|------|---------|
| `Validator.cs` | Main entry point — loads schemas, validates XML/Schematron, collects errors |
| `Schema.cs` / `SchemaLoader.cs` | Represent and load a Schematron schema document |
| `SyncEvaluationContext.cs` | Synchronous evaluation engine; walks Phases → Patterns → Rules → Asserts/Reports |
| `EvaluationContextBase.cs` | Abstract base for evaluation strategies |
| `Formatters/` | Pluggable output formatters: `BooleanFormatter`, `LogFormatter`, `SimpleFormatter`, `XmlFormatter` |
| `Phase.cs`, `Pattern.cs`, `Rule.cs`, `Assert.cs`, `Report.cs` | Domain model for Schematron elements |
| `IMatchedNodes.cs` + implementations | Strategy for tracking already-matched nodes per pattern |

### Schematron.WebServices

`ValidationExtension` (a `SoapExtension`) and `ValidationAttribute` allow Schematron validation to be applied declaratively to SOAP web methods.

### Schematron.Tests

xUnit tests covering `Validator` behaviour with embedded XSD+Schematron schemas and standalone Schematron schemas.

## Build & Test

```
dotnet build Schematron.slnx
dotnet test  Schematron.slnx
```

## Conventions

- **Target framework**: `net48` for all projects (WebServices requires .NET Framework due to `System.Web.Services`).
- **Versioning**: Managed by the [GitInfo](https://github.com/devlooped/GitInfo) package; base version comes from the `VERSION` file at the repo root.
- **NuGet metadata**: Defined in `src/Directory.Build.props` and per-project `<PropertyGroup>`.
- **C# language version**: `latest` — use modern idioms (expression-bodied members, `var`, `string.IsNullOrEmpty`, `throw;`, pattern matching).
- **No custom build scripts**: Use standard `dotnet` CLI; no `build.cmd` / `build.proj`.
