# FsToolkit.ErrorHandling

**FsToolkit.ErrorHandling** is a utility library to work with the `Result` type in F#, and allows you to do clear, simple and powerful error handling.

The library provides utility functions like `map`, `bind`, `apply`, `traverse`, `sequence` as well as computation expressions and infix operators to work with `Result<'a, 'b>`, `Result<'a option, 'b>`, `Async<Result<'a, 'b>>`, `Async<Result<'a option, 'b>>`, and `Result<'a, 'b list>`.

It was inspired by [Chessie](https://github.com/fsprojects/Chessie) and Cvdm.ErrorHandling (the latter has now been merged into FsToolkit.ErrorHandling).

FsToolkit.ErrorHandling targets .NET Standard 2.0 and .NET Framework 4.6.1 and supports Fable.

## Documentation

The documentation is [available here](https://demystifyfp.gitbook.io/fstoolkit-errorhandling).

## Further material

* The main resource as to learning this style of programming [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)
* However Result isn't a panacea, see what pitfalls and where you shouldn't use `Result`. [In defense of Exceptions: Throw (away) your Result](https://skillsmatter.com/skillscasts/17243-in-defense-of-exceptions-throw-away-your-result)


## Builds

GitHub Actions |
:---: |
[![GitHub Actions](https://github.com/demystifyfp/FsToolkit.ErrorHandling/workflows/Build%20master/badge.svg)](https://github.com/demystifyfp/FsToolkit.ErrorHandling/actions?query=branch%3Amaster) |
[![Build History](https://buildstats.info/github/chart/demystifyfp/FsToolkit.ErrorHandling)](https://github.com/demystifyfp/FsToolkit.ErrorHandling/actions?query=branch%3Amaster) |


### NuGet

| Package name | Badge |
| --- | --- |
| FsToolkit.ErrorHandling | [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling)
| FsToolkit.ErrorHandling.TaskResult | [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.TaskResult.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.TaskResult)
| FsToolkit.ErrorHandling.JobResult | [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.JobResult.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.JobResult)
| FsToolkit.ErrorHandling.AsyncSeq | [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.AsyncSeq.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.AsyncSeq)


### Developing locally

#### Requirements

* [.NET Core SDK](https://www.microsoft.com/net/download/)
  * v3.1.200 or higher
  * Install from [here](https://dotnet.microsoft.com/download/dotnet-core/3.1).
* [Node](https://nodejs.org/en/)
  * v10.15.0 or LTS
  * Not required but recommend that you use [NVM](https://github.com/nvm-sh/nvm) to easily manage multiple versions of Node


#### Compiling

```
> build.cmd <optional buildtarget> // on windows
$ ./build.sh  <optional buildtarget>// on unix
```

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

## Sponsor(s):

<a href="https://www.ajira.tech"><img src="./Ajira-logo.png" alt="Ajira Technologies, India" width="200" /></a>


