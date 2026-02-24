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

#if !FABLE_COMPILER
    open System.Collections.Generic
    open System.Threading.Tasks

    /// Creates a simple IAsyncEnumerable<'T> from a list, for use in tests.
    let toAsyncEnumerable (items: 'T list) : IAsyncEnumerable<'T> =
        let arr = List.toArray items

        { new IAsyncEnumerable<'T> with
            member _.GetAsyncEnumerator(_ct) =
                let mutable index = -1

                { new IAsyncEnumerator<'T> with
                    member _.MoveNextAsync() =
                        index <- index + 1
                        ValueTask.FromResult(index < arr.Length)

                    member _.Current = arr.[index]
                    member _.DisposeAsync() = ValueTask()
                }
        }
#endif

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
