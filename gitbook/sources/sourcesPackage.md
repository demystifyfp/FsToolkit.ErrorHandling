# FsToolkit.ErrorHandling.Sources

This package contains the source code of [FsToolkit.ErrorHandling](https://www.nuget.org/packages/FsToolkit.ErrorHandling) as F# files.
The files compile inside your project. Your assembly does not get a reference to `FsToolkit.ErrorHandling.dll`.

Use this package when you cannot add a binary dependency, for example in a library that must not expose FsToolkit.ErrorHandling to its consumers.

The package contains only the source code of the `FsToolkit.ErrorHandling` package. It does not contain `FsToolkit.ErrorHandling.IcedTasks`, `FsToolkit.ErrorHandling.JobResult`, or `FsToolkit.ErrorHandling.AsyncSeq`.

## Requirements

- F# 10 or later (.NET SDK 10.0 or later).
- For `net9.0`, FSharp.Core 9.0.300 or later. For `net10.0`, FSharp.Core 10.0.100 or later. For `net11.0` or later, FSharp.Core 11.0.101 or later. The package depends on these versions. For each target framework that it can build, the .NET SDK 10.0 or later gives a sufficient `FSharp.Core` version by default.
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

If the `PackageReference` does not set `PrivateAssets="all"`, the build gives warning `FSTKSRC001`. A project that sets `IsPackable` to `false` does not get this warning. Without `PrivateAssets="all"`, a package of your project depends on `FsToolkit.ErrorHandling.Sources`, and the projects that reference your package get its `FSharp.Core` minimum version. These projects do not compile the source files, because NuGet does not give content files to them.

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

With `internal`, the types and modules are not visible outside your assembly. Your public API cannot use them. For example, a public function cannot return a `TaskResultBuilder`. The compiler gives error FS0410. A public `inline` function cannot call a function of this package. The compiler gives error FS1113. Type abbreviations, such as `Validation<'ok, 'error>`, are not affected, because the compiler replaces each abbreviation with the type that it names.

Use `public` when your public API must expose these types. Then the types become part of your assembly's public API.

### Namespace

Your project can have a module with the same full name as a module of this package. For example, your project can have `module Async` in `namespace FsToolkit.ErrorHandling`. The compiler then gives error FS0248. Set `FsToolkitErrorHandlingNamespace` to a different namespace to prevent this.

The package replaces the namespace only in its `namespace`, `module`, and `open` lines. Comments keep the name `FsToolkit.ErrorHandling`.

## Compile order

The package files compile before the files of your project. If your project sets `CompileOrder="CompileFirst"` on its own `Compile` items, those files compile before the package files and cannot use them.

## Compiler warnings

Each source file has `#nowarn "52" "57" "1182" "3180" "3261" "3390" "3513" "3517" "3559"` on the line after each `namespace` line and after its top-level `module` line. Thus strict project settings, such as `TreatWarningsAsErrors`, `Nullable`, and `GenerateDocumentationFile`, do not fail on the package files.

If you enable warning 3180 with `--warnon:3180`, the compiler reports FS3180 for the package files at `unknown(1,1)`. A `#nowarn` cannot suppress a warning without a source location. If you must enable warning 3180, add `FS3180` to `WarningsNotAsErrors`.

With .NET SDK 10.0.115, `--warnon:3517` reports FS3517 at the places where your code calls functions of this package. The binary `FsToolkit.ErrorHandling` package gives the same warnings. SDK 10.0.404 and 11.0.100 do not report them.

## Fable

The files that the `FsToolkit.ErrorHandling` project does not compile for Fable (the `Task` files) are in `#if !FABLE_COMPILER` blocks. Fable is not tested with this package.

## WebSharper

WebSharper compiles F# to JavaScript. To translate client code that calls `FsToolkit.ErrorHandling`, WebSharper needs a proxy project. A proxy project compiles the source code of `FsToolkit.ErrorHandling` with WebSharper. This package supplies that source code.

The CI of FsToolkit.ErrorHandling builds a proxy project in this way and runs the FsToolkit.ErrorHandling tests with it in Node.js. The projects are in [tests/FsToolkit.ErrorHandling.WebSharper](https://github.com/demystifyfp/FsToolkit.ErrorHandling/tree/master/tests/FsToolkit.ErrorHandling.WebSharper).

### Proxy project

Make a proxy project, for example `FsToolkit.ErrorHandling.WebSharper.fsproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <WebSharperProject>Proxy</WebSharperProject>
    <DefineConstants>$(DefineConstants);FABLE_COMPILER</DefineConstants>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <None Include="wsconfig.json" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FsToolkit.ErrorHandling.Sources" Version="$(FsToolkitErrorHandlingVersion)" PrivateAssets="all" />
    <PackageReference Include="WebSharper" Version="10.1.10.687" />
    <PackageReference Include="WebSharper.FSharp" Version="10.1.10.687" />
  </ItemGroup>
</Project>
```

Put this `wsconfig.json` file in the folder of the proxy project:

```json
{
  "project": "proxy",
  "proxyTargetName": "FsToolkit.ErrorHandling"
}
```

These settings are necessary:

- `"proxyTargetName": "FsToolkit.ErrorHandling"` tells WebSharper that the proxy translates the types of the `FsToolkit.ErrorHandling` assembly. The proxy assembly does not contain the .NET types.
- `FABLE_COMPILER` removes the `Task` files. WebSharper cannot translate them.
- Keep the default `FsToolkitErrorHandlingNamespace`. WebSharper finds the proxy types by their full names, so the namespace must be `FsToolkit.ErrorHandling`.
- `IsPackable` `false` and `PrivateAssets="all"` each prevent warning `FSTKSRC001`.

`FsToolkitErrorHandlingVisibility` can keep its default, `internal`. The tests pass with `internal` and with `public`.

Use the WebSharper version of your client projects.

### Client projects

Reference the binary package and the proxy project from each project whose client code calls `FsToolkit.ErrorHandling`:

```xml
<ItemGroup>
  <PackageReference Include="FsToolkit.ErrorHandling" Version="$(FsToolkitErrorHandlingVersion)" />
  <ProjectReference Include="../FsToolkit.ErrorHandling.WebSharper/FsToolkit.ErrorHandling.WebSharper.fsproj" />
</ItemGroup>
```

WebSharper compiles the client code against the binary package and gets the JavaScript from the proxy. Server code runs the binary package.

### Versions

`FsToolkit.ErrorHandling` and `FsToolkit.ErrorHandling.Sources` must have the same version. If the versions are different, the JavaScript can behave differently from the .NET code, or WebSharper cannot find a function. Set the version one time, in `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <FsToolkitErrorHandlingVersion>x.y.z</FsToolkitErrorHandlingVersion>
  </PropertyGroup>
</Project>
```

This `Directory.Build.targets` file stops the restore and the build when a reference to one of the two packages has a different version. It reads the `Version` of `PackageReference` items and of the `PackageVersion` items of central package management. It does not read `VersionOverride`.

```xml
<Project>
  <Target Name="FsToolkitErrorHandlingVersionCheck" BeforeTargets="CollectPackageReferences">
    <ItemGroup>
      <_FsToolkitErrorHandlingVersion Include="@(PackageReference)" Condition="('%(PackageReference.Identity)' == 'FsToolkit.ErrorHandling' or '%(PackageReference.Identity)' == 'FsToolkit.ErrorHandling.Sources') and '%(PackageReference.Version)' != ''" />
      <_FsToolkitErrorHandlingVersion Include="@(PackageVersion)" Condition="'%(PackageVersion.Identity)' == 'FsToolkit.ErrorHandling' or '%(PackageVersion.Identity)' == 'FsToolkit.ErrorHandling.Sources'" />
    </ItemGroup>
    <Error Condition="'@(_FsToolkitErrorHandlingVersion)' != '' and '%(_FsToolkitErrorHandlingVersion.Version)' != '$(FsToolkitErrorHandlingVersion)'"
           Text="%(_FsToolkitErrorHandlingVersion.Identity) has version %(_FsToolkitErrorHandlingVersion.Version). FsToolkit.ErrorHandling and FsToolkit.ErrorHandling.Sources must have the same version, $(FsToolkitErrorHandlingVersion)." />
  </Target>
</Project>
```

Paket does not make `PackageReference` items, so the check does not see a Paket reference. If Paket references `FsToolkit.ErrorHandling`, set the same version in `paket.dependencies` yourself.

### Known WebSharper problems

With WebSharper 10.1.10, some code compiles to JavaScript that does not behave as on .NET:

- `use` and `try`/`finally` in the `result`, `resultOption`, `validation`, and `voption` computation expressions, and `Validation.apply`, compile to JavaScript that uses an undeclared variable.
- `ValueOption.tryGetValue` calls `TryGetValue` as a JavaScript method. The object that `dict` makes does not have that method.
- WebSharper compiles `()` to `null` in some places and to `undefined` in others. `=` then gives `false` for two `unit` values.

[WebSharperPending.fs](https://github.com/demystifyfp/FsToolkit.ErrorHandling/blob/master/tests/FsToolkit.ErrorHandling.WebSharper/TestAdapter/WebSharperPending.fs) lists each test that fails only with WebSharper.

## netstandard2.0

On `netstandard2.0`, the source code uses `Microsoft.Bcl.AsyncInterfaces`. This package has a dependency on it, and your project gets it.

With `PrivateAssets="all"`, this dependency does not go to the consumers of your package. If your project targets `netstandard2.0` and you publish it as a package, add a direct reference to `Microsoft.Bcl.AsyncInterfaces`:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="6.0.0" />
</ItemGroup>
```
