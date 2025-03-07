namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Async =

    open System

#if FABLE_COMPILER && FABLE_COMPILER_JAVASCRIPT
    open Fable.Core

    /// An Async that never completes but can be cancelled
    let never<'a> : Async<'a> =
        Fable.Core.JS.Constructors.Promise.Create(fun _ _ -> ())
        |> Async.AwaitPromise
#else
    /// An Async that never completes but can be cancelled
    let never<'a> : Async<'a> =
        let granularity = TimeSpan.FromSeconds 3.

        let rec loop () =
            async {
                do! Async.Sleep(granularity)
                return! loop ()
            }

        loop ()
#endif

module TestHelpers =
    let makeDisposable (callback) =
        { new System.IDisposable with
            member this.Dispose() = callback ()
        }

    let makeAsyncDisposable (callback) =
        { new System.IAsyncDisposable with
            member this.DisposeAsync() = callback ()
        }

type MemoryStreamNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    System.IO.MemoryStream | null
#else
    System.IO.MemoryStream
#endif

type UriNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    System.Uri | null
#else
    System.Uri
#endif
