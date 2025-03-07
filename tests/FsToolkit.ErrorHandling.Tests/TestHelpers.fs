namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Async =

    open System

    /// An Async that never completes but can be cancelled
    let never<'a> : Async<'a> =
        let rec loop () =
            async {
                do! Async.Sleep(TimeSpan.FromHours 1)
                return! loop ()
            }

        loop ()

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
