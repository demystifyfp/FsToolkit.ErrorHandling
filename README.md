# FsToolkit.ErrorHandling

**FsToolkit.ErrorHandling** is a utility library to work with the `Result` type in F#, and allows you to do clear, simple and powerful error handling.

The library provides utility functions like `map`, `bind`, `apply`, `traverse`, `sequence` as well as computation expressions and infix operators to work with `Result<'a, 'b>`, `Result<'a option, 'b>`, `Async<Result<'a, 'b>>`, `Async<Result<'a option, 'b>>`, and `Result<'a, 'b list>`.

It was inspired by [Chessie](https://github.com/fsprojects/Chessie) and Cvdm.ErrorHandling (the latter has now been merged into FsToolkit.ErrorHandling).

FsToolkit.ErrorHandling targets .NET Standard 2.0, .NET Standard2.1 and supports Fable.

## Documentation

The documentation is [available here](https://demystifyfp.gitbook.io/fstoolkit-errorhandling).

## Further material

* The main resource as to learning this style of programming [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)
* However Result isn't a panacea, see what pitfalls and where you shouldn't use `Result`. ["Don't Throw Exceptions, Use A Result Pattern" is BAD ADVICE!](https://www.youtube.com/watch?v=txU58-Q03JA)

## Builds

GitHub Actions |
:---: |
[![GitHub Actions](https://github.com/demystifyfp/FsToolkit.ErrorHandling/workflows/Build%20master/badge.svg)](https://github.com/demystifyfp/FsToolkit.ErrorHandling/actions?query=branch%3Amaster) |
[![Build History](https://buildstats.info/github/chart/demystifyfp/FsToolkit.ErrorHandling?branch=master)](https://github.com/demystifyfp/FsToolkit.ErrorHandling/actions?query=branch%3Amaster) |

### NuGet

| Package name | Release | Prelease
| --- | --- | --- |
| FsToolkit.ErrorHandling | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling)](https://www.nuget.org/packages/FsToolkit.ErrorHandling) | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling?includePreReleases=true)](https://www.nuget.org/packages/FsToolkit.ErrorHandling/absoluteLatest)
| FsToolkit.ErrorHandling.TaskResult | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.TaskResult)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.TaskResult) | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.TaskResult?includePreReleases=true)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.TaskResult/absoluteLatest)
| FsToolkit.ErrorHandling.JobResult | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.JobResult)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.JobResult) | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.JobResult?includePreReleases=true)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.JobResult/absoluteLatest)
| FsToolkit.ErrorHandling.AsyncSeq | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.AsyncSeq)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.AsyncSeq) | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.AsyncSeq?includePreReleases=true)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.AsyncSeq/absoluteLatest)
| FsToolkit.ErrorHandling.IcedTasks | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.IcedTasks)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.IcedTasks) | [![NuGet](https://buildstats.info/nuget/FsToolkit.ErrorHandling.IcedTasks?includePreReleases=true)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.IcedTasks/absoluteLatest)

## Developing locally

### Devcontainer 
This repository has a devcontainer setup for VSCode. For more information see:
- [VSCode](https://code.visualstudio.com/docs/devcontainers/containers)

### Local Setup

* [.NET Core SDK](https://www.microsoft.com/net/download/)
  * [v6.x](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
  * [v7.x](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
  * [v8.x](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

#### Optional 

To test fable builds locally you will need:

* [Node](https://nodejs.org/en/)
  * v18.0.0 or Higher
  * Not required but recommend that you use [NVM](https://github.com/nvm-sh/nvm) to easily manage multiple versions of Node
* [Python](https://www.python.org/downloads/)
  * v3.10.0 or higher
  * Required for Fable-Python


#### Compiling

```bash
> build.cmd <optional buildtarget> // on windows
$ ./build.sh  <optional buildtarget>// on unix
```

Without specifying a build target, the default target is `DotnetPack`, which will run tests for all projects on dotnet and then pack the projects into nuget packages. For additional notable targets see below.

##### Build Targets

- `Clean` - Will clean all projects `bin` and `obj` folders
- `DotnetTest` - Will run tests for `dotnet` projects
- `NpmTest` - Will run tests for `fable-javascript` projects
- `PythonTest` - Will run tests for `fable-python` projects
- `RunTests` - Will run tests for `dotnet`, `fable-javascript` and `fable-python` projects
- `FormatCode` - Will run `fantomas` to format the codebase

This is not an exhaustive list. Additional targets can be found in the `./build/build.fs` file.


A motivating example
--------------------

This example of composing a login flow shows one example of how this library can aid in clear, simple, and powerful error handling, using just a computation expression and a few helper functions. (The library has many more helper functions and computation expressions as well as infix operators; see [the documentation](https://demystifyfp.gitbook.io/fstoolkit-errorhandling) for details.)

```f#
// Given the following functions:
//   tryGetUser: string -> Async<User option>
//   isPwdValid: string -> User -> bool
//   authorize: User -> Async<Result<unit, AuthError>>
//   createAuthToken: User -> Result<AuthToken, TokenError>

type LoginError = InvalidUser | InvalidPwd | Unauthorized of AuthError | TokenErr of TokenError

let login (username: string) (password: string) : Async<Result<AuthToken, LoginError>> =
  asyncResult {
    // requireSome unwraps a Some value or gives the specified error if None
    let! user = username |> tryGetUser |> AsyncResult.requireSome InvalidUser

    // requireTrue gives the specified error if false
    do! user |> isPwdValid password |> Result.requireTrue InvalidPwd

    // Error value is wrapped/transformed (Unauthorized has signature AuthError -> LoginError)
    do! user |> authorize |> AsyncResult.mapError Unauthorized

    // Same as above, but synchronous, so we use the built-in mapError
    return! user |> createAuthToken |> Result.mapError TokenErr
  }
```

## Sponsor(s)

<a href="https://www.ajira.tech"><img src="./Ajira-logo.png" alt="Ajira Technologies, India" width="200" /></a>
