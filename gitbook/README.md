# FsToolkit.ErrorHandling

**FsToolkit.ErrorHandling** is a utility library to work with the `Result` type in F#, and allows you to do clear, simple and powerful error handling. 

The library provides utility functions like `map`, `bind`, `apply`, `traverse`, `sequence` as well as computation expressions and infix operators to work with `Result<'a, 'b>`, `Result<'a option, 'b>`, `Async<Result<'a, 'b>>`, `Async<Result<'a option, 'b>>`, and `Result<'a, 'b list>`.

With adding a reference to `FsToolkit.ErrorHandling.TaskResult` you will also have access to computation expressions and infix operators related to `Task<Result<'a, 'b>>`.

It was inspired by [Chessie](https://github.com/fsprojects/Chessie) and Cvdm.ErrorHandling (the latter has now been merged into FsToolkit.ErrorHandling).

FsToolkit.ErrorHandling targets .NET Standard 2.0 and .NET Framework 4.6.1 and supports Fable.
[DUB](https://img.shields.io/dub/l/vibe-d.svg)



### Nuget

| Package name | Badge |
| --- | --- |
| FsToolkit.ErrorHandling | [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling)
| FsToolkit.ErrorHandling.TaskResult | [![NuGet](https://img.shields.io/nuget/v/FsToolkit.ErrorHandling.TaskResult.svg)](https://www.nuget.org/packages/FsToolkit.ErrorHandling.TaskResult)


## Note

This documentation assumes some familiarity with standard functions like `map`, `apply`, `bind`, `traverse` and `sequence` and the problem these functions solve. If this is new to you, check out Scott Wlaschin’s [excellent tutorial](https://fsharpforfunandprofit.com/series/map-and-bind-and-apply-oh-my.html) on the subject.

A motivating example
--------------------

This example of composing a login flow shows one example of how this library can aid in clear, simple, and powerful error handling, using just a computation expression and a few helper functions. (The library has many more helper functions and computation expressions as well as infix operators; see the rest of the documentation for details.)

```fsharp
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

[![Ajira Technologies, India](https://raw.githubusercontent.com/demystifyfp/FsToolkit.ErrorHandling/master/Ajira-logo.png)](https://www.ajira.tech)
