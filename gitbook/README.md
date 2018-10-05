# FsToolkit.ErrorHandling

**FsToolkit.ErrorHandling** is a utility library to work with Result type in F# to do error handling. 

It provides utility functions like `map`, `bind`, `apply`, `traverse`, `sequence`, computation expressions and infix operators to work with `Result`, `Result<'a option, 'b>`, `Async<Result<'a, 'b>>`, `Async<Result<'a option, 'b>>` & `Result<'a, 'b list>` types.

It was inspired by the [Chessie](https://github.com/fsprojects/Chessie) and [Cvdm.ErrorHandling](https://github.com/cmeeren/Cvdm.ErrorHandling) libraries. 

[![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling)

## Note:

This library assumes that are you familiar with the standard functions - map, apply, bind, traverse & sequence and the problem these functions solve. In case, if you are not aware of it, do check out [this excellent tutorial](https://fsharpforfunandprofit.com/series/map-and-bind-and-apply-oh-my.html) by Scott Wlaschin on this subject.

