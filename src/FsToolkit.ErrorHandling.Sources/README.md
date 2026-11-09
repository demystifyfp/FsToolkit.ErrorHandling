# FsToolkit.ErrorHandling.Sources

This package contains the source code of [FsToolkit.ErrorHandling](https://www.nuget.org/packages/FsToolkit.ErrorHandling) as F# files.
The files compile inside your project. Your assembly does not get a reference to `FsToolkit.ErrorHandling.dll`.

Use this package when you cannot add a binary dependency, for example in a library that must not expose FsToolkit.ErrorHandling to its consumers.

The package contains only the source code of the `FsToolkit.ErrorHandling` package. It does not contain `FsToolkit.ErrorHandling.IcedTasks`, `FsToolkit.ErrorHandling.JobResult`, or `FsToolkit.ErrorHandling.AsyncSeq`.

## Requirements

- F# 9 or later (.NET SDK 9.0 or later).
- For `net9.0` or later, FSharp.Core 9.0.300 or later. The package depends on that version. The .NET SDK 9.0.300 or later gives it by default. With an earlier SDK 9.0 version, restore reports `NU1605` (a package downgrade). Then use SDK 9.0.300 or later, or add a direct `FSharp.Core` reference of version 9.0.300 or later.
- For `netstandard2.0` and `netstandard2.1`, FSharp.Core 6.0.4 or later.
- If your project targets `net9.0` or later, do not set `LangVersion` lower than `9.0`.
- Reference the package directly from each project that uses it. NuGet does not give content files to projects that reference your project.
- Use a `PackageReference`. Paket is not tested with this package.

## Install

Add the reference with `PrivateAssets="all"`. Then the package does not become a dependency of your own package.

```xml
<ItemGroup>
  <PackageReference Include="FsToolkit.ErrorHandling.Sources" Version="x.y.z" PrivateAssets="all" />
</ItemGroup>
```

If the `PackageReference` does not set `PrivateAssets="all"`, the build gives warning `FSTKSRC001`. A project that sets `IsPackable` to `false` does not get this warning. Without `PrivateAssets="all"`, a package of your project depends on `FsToolkit.ErrorHandling.Sources`. Each project that references your package then compiles the source files again.

## Configure

Set these properties in your project file:

| Property | Default | Values |
| --- | --- | --- |
| `FsToolkitErrorHandlingNamespace` | `FsToolkit.ErrorHandling` | Dotted F# identifiers, for example `Contoso.Internal.ErrorHandling`. Each segment must use only ASCII letters, digits, and `_`, and must not start with a digit. A segment must not be `_`, an F# keyword, or a reserved word. |
| `FsToolkitErrorHandlingVisibility` | `internal` | `internal` or `public`, in lowercase. |

The build fails with an error that names the property when a value is not valid.

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

The package replaces the namespace only in its `namespace`, `module`, and `open` lines. Comments keep the name `FsToolkit.ErrorHandling`.

## Compile order

The package files compile before the files of your project. If your project sets `CompileOrder="CompileFirst"` on its own `Compile` items, those files compile before the package files and cannot use them.

## Compiler warnings

Each source file has `#nowarn "57" "1182" "3180" "3261" "3390" "3513" "3517" "3559"` on the line after each `namespace` line and after its top-level `module` line. Thus strict project settings, such as `TreatWarningsAsErrors`, `Nullable`, and `GenerateDocumentationFile`, do not fail on the package files.

If you enable warning 3180 with `--warnon:3180`, the compiler reports FS3180 for the package files at `unknown(1,1)`. A `#nowarn` cannot suppress a warning without a source location. If you must enable warning 3180, add `FS3180` to `WarningsNotAsErrors`.

With F# 9, `--warnon:3517` reports FS3517 at the places where your code calls functions of this package. The binary `FsToolkit.ErrorHandling` package gives the same warnings. F# 10 does not report them.

## Fable

The files that the `FsToolkit.ErrorHandling` project does not compile for Fable (the `Task` files) are in `#if !FABLE_COMPILER` blocks. Fable is not tested with this package.

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
