# FsToolkit.ErrorHandling.Sources

The `FsToolkit.ErrorHandling.Sources` package contains the source code of `FsToolkit.ErrorHandling` as F# files. The files compile inside your project, so your assembly does not reference `FsToolkit.ErrorHandling.dll`.

```xml
<ItemGroup>
  <PackageReference Include="FsToolkit.ErrorHandling.Sources" Version="x.y.z" PrivateAssets="all" />
</ItemGroup>
```

Set these properties in your project file:

- `FsToolkitErrorHandlingNamespace` sets the namespace of the source code. The default is `FsToolkit.ErrorHandling`.
- `FsToolkitErrorHandlingVisibility` sets the visibility of the types and modules: `internal` (the default) or `public`.

The [package README](https://github.com/demystifyfp/FsToolkit.ErrorHandling/blob/master/src/FsToolkit.ErrorHandling.Sources/README.md) gives the requirements and the limits.
