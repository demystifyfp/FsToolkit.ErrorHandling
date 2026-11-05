# FsToolkit.ErrorHandling.Sources

This package contains the source code of [FsToolkit.ErrorHandling](https://www.nuget.org/packages/FsToolkit.ErrorHandling) as F# files.
The files compile inside your project. Your assembly does not get a reference to `FsToolkit.ErrorHandling.dll`.

Use this package when you cannot add a binary dependency, for example in a library that must not expose FsToolkit.ErrorHandling to its consumers.

The package contains only the source code of the `FsToolkit.ErrorHandling` package. It does not contain `FsToolkit.ErrorHandling.IcedTasks`, `FsToolkit.ErrorHandling.JobResult`, or `FsToolkit.ErrorHandling.AsyncSeq`.

## Requirements

- F# 9 or later (.NET SDK 9.0 or later).
- If your project targets `net9.0` or later, do not set `LangVersion` lower than `9.0`.
- Reference the package directly from each project that uses it. NuGet does not give content files to projects that reference your project.

## Install

Add the reference with `PrivateAssets="all"`. Then the package does not become a dependency of your own package.

```xml
<ItemGroup>
  <PackageReference Include="FsToolkit.ErrorHandling.Sources" Version="x.y.z" PrivateAssets="all" />
</ItemGroup>
```

## Configure

Set these properties in your project file:

| Property | Default | Values |
| --- | --- | --- |
| `FsToolkitErrorHandlingNamespace` | `FsToolkit.ErrorHandling` | A dotted F# identifier, for example `Contoso.Internal.ErrorHandling`. |
| `FsToolkitErrorHandlingVisibility` | `internal` | `internal` or `public`. |

```xml
<PropertyGroup>
  <FsToolkitErrorHandlingNamespace>Contoso.Internal.ErrorHandling</FsToolkitErrorHandlingNamespace>
  <FsToolkitErrorHandlingVisibility>internal</FsToolkitErrorHandlingVisibility>
</PropertyGroup>
```

Then open the namespace that you set:

```fsharp
open Contoso.Internal.ErrorHandling
```

### Visibility

With `internal`, the types and modules are not visible outside your assembly. Your public API cannot use them. For example, a public function cannot return `Validation<'ok, 'error>`. The compiler gives error FS0410.

Use `public` when your public API must expose these types. Then the types become part of your assembly's public API.

### Namespace

Your project can have a module with the same full name as a module of this package. For example, your project can have `module Async` in `namespace FsToolkit.ErrorHandling`. The compiler then gives error FS0248. Set `FsToolkitErrorHandlingNamespace` to a different namespace to prevent this.

## Compiler warnings

Each source file has `#nowarn "57" "1182" "3180" "3261" "3390" "3513" "3517" "3559"` on the line after its first `namespace` or `module` line. Thus strict project settings, such as `TreatWarningsAsErrors`, `Nullable`, and `GenerateDocumentationFile`, do not fail on the package files.

If you enable warning 3180 with `--warnon:3180`, the compiler reports FS3180 for the package files at `unknown(1,1)`. A `#nowarn` cannot suppress a warning without a source location. If you must enable warning 3180, add `FS3180` to `WarningsNotAsErrors`.

With F# 9, `--warnon:3517` reports FS3517 at the places where your code calls functions of this package. The binary `FsToolkit.ErrorHandling` package gives the same warnings. F# 10 does not report them.

## .NET SDK 9 and rebuilds

With .NET SDK 9, a rebuild of a project with more than one target framework does not compile the package files. `dotnet build --no-incremental` and `dotnet build -t:Rebuild` are rebuilds. The build then fails, for example with error FS0039. Use `dotnet clean` and then `dotnet build`. .NET SDK 10 does not have this problem.

## netstandard2.0

On `netstandard2.0`, the source code uses `Microsoft.Bcl.AsyncInterfaces`. This package has a dependency on it, and your project gets it.

With `PrivateAssets="all"`, this dependency does not go to the consumers of your package. If your project targets `netstandard2.0` and you publish it as a package, add a direct reference to `Microsoft.Bcl.AsyncInterfaces`:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="6.0.0" />
</ItemGroup>
```

## Documentation

The documentation is [available here](https://demystifyfp.gitbook.io/fstoolkit-errorhandling).
