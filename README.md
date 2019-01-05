# FsToolkit.ErrorHandling

**FsToolkit.ErrorHandling** is a utility library to work with Result type in F# to do error handling. 

It provides utility functions like `map`, `bind`, `apply`, `traverse`, `sequence`, computation expressions and infix operators to work with `Result`, `Result<'a option, 'b>`, `Async<Result<'a, 'b>>`, `Async<Result<'a option, 'b>>` & `Result<'a, 'b list>` types.

It was inspired by the [Chessie](https://github.com/fsprojects/Chessie) and [Cvdm.ErrorHandling](https://github.com/cmeeren/Cvdm.ErrorHandling) libraries. 

Supports .NET Core, Fable & .NET Framework 4.6.1 and above.

The documentation is available [here](https://demystifyfp.gitbook.io/fstoolkit-errorhandling).

[![Build Status](https://img.shields.io/travis/demystifyfp/FsToolkit.ErrorHandling/master.svg)](https://travis-ci.org/demystifyfp/FsToolkit.ErrorHandling) [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling) ![DUB](https://img.shields.io/dub/l/vibe-d.svg)


## Note:

This library assumes that are you familiar with the standard functions - map, apply, bind, traverse & sequence and the problem these functions solve. In case, if you are not aware of it, do check out [this excellent tutorial](https://fsharpforfunandprofit.com/series/map-and-bind-and-apply-oh-my.html) by Scott Wlaschin on this subject.

## Sponsor(s):

<a href="https://www.ajira.tech"><img src="./Ajira-logo.png" alt="Ajira Technologies, India" width="200" /></a>
