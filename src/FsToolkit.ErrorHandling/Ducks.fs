namespace FsToolkit.ErrorHandling.Ducks

open System
open System.Collections.Generic
open System.Threading.Tasks
open System.Runtime.CompilerServices
open Microsoft.FSharp.Core.CompilerServices

type Disposable<'Disposable when 'Disposable: (member Dispose: unit -> unit)> = 'Disposable

type Disposable =
    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    static member inline Dispose<'Disposable when Disposable<'Disposable>>(d: 'Disposable) =
        d.Dispose()

[<AutoOpen>]
module LowerPriorityDisposable =
    type Disposable with

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        static member inline Dispose(d: #IDisposable) = d.Dispose()


type AsyncDisposable<'AsyncDisposable
    when 'AsyncDisposable: (member DisposeAsync: unit -> ValueTask)> = 'AsyncDisposable

type AsyncDisposable =
    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    static member inline DisposeAsync<'AsyncDisposable when AsyncDisposable<'AsyncDisposable>>
        (d: 'AsyncDisposable)
        =
        d.DisposeAsync()

[<AutoOpen>]
module LowerPriorityAsyncDisposable =
    type AsyncDisposable with

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        static member inline DisposeAsync(d: #IAsyncDisposable) = d.DisposeAsync()


type Enumerator<'Enumerator, 'Element
    when 'Enumerator: (member Reset: unit -> unit)
    and 'Enumerator: (member Current: 'Element)
    and 'Enumerator: (member MoveNext: unit -> bool)> = 'Enumerator

type Enumerator =

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    static member inline Reset(e: #IEnumerator<'Element>) = e.Reset()
    /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
    static member inline Current(e: #IEnumerator<'Element>) = e.Current

    /// <summary>Advances the enumerator to the next element of the collection.</summary>
    static member inline MoveNext(e: #IEnumerator<'Element>) =
        let mutable e = e
        e.MoveNext()

[<AutoOpen>]
module LowerPriorityEnumerator =

    type Enumerator with

        /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
        [<NoEagerConstraintApplication>]
        static member inline Reset<'Enumerator, 'Element when Enumerator<'Enumerator, 'Element>>
            (e: 'Enumerator)
            =
            e.Reset()

        /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
        [<NoEagerConstraintApplication>]
        static member inline Current<'Enumerator, 'Element when Enumerator<'Enumerator, 'Element>>
            (e: 'Enumerator)
            =
            e.Current

        /// <summary>Advances the enumerator to the next element of the collection.</summary>
        [<NoEagerConstraintApplication>]
        static member inline MoveNext<'Enumerator, 'Element when Enumerator<'Enumerator, 'Element>>
            (e: 'Enumerator)
            =
            e.MoveNext()


type Enumerable<'Enumerable, 'Enumerator, 'Element
    when 'Enumerable: (member GetEnumerator: unit -> Enumerator<'Enumerator, 'Element>)> =
    'Enumerable

type Enumerable =

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    static member inline GetEnumerator(e: #IEnumerable<'Element>) = e.GetEnumerator()


[<AutoOpen>]
module LowerPriorityEnumerable =

    type Enumerable with

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        [<NoEagerConstraintApplication>]
        static member inline GetEnumerator<'Enumerable, 'Enumerator, 'Element
            when Enumerable<'Enumerable, 'Enumerator, 'Element>>
            (e: 'Enumerable)
            =
            e.GetEnumerator()


/// <summary>A structure that looks like an Awaiter</summary>
type Awaiter<'Awaiter, 'TResult
    when 'Awaiter :> ICriticalNotifyCompletion
    and 'Awaiter: (member get_IsCompleted: unit -> bool)
    and 'Awaiter: (member GetResult: unit -> 'TResult)> = 'Awaiter

/// <summary>Functions for Awaiters</summary>
type Awaiter =
    /// <summary>Gets a value that indicates whether the asynchronous task has completed</summary>
    static member inline IsCompleted<'Awaiter, 'TResult when Awaiter<'Awaiter, 'TResult>>
        (x: 'Awaiter)
        =
        x.get_IsCompleted ()

    /// <summary>Ends the wait for the completion of the asynchronous task.</summary>
    static member inline GetResult<'Awaiter, 'TResult when Awaiter<'Awaiter, 'TResult>>
        (x: 'Awaiter)
        =
        x.GetResult()


// [<AutoOpen>]
// module LowerPriorityAwaiter =
//     type Awaiter with

//         /// <summary>Gets a value that indicates whether the asynchronous task has completed</summary>
//         static member inline IsCompleted(x: TaskAwaiter<'T>) = x.IsCompleted

//         /// <summary>Ends the wait for the completion of the asynchronous task.</summary>
//         static member inline GetResult(x: TaskAwaiter<'T>) = x.GetResult()


/// <summary>A structure looks like an Awaitable</summary>
type Awaitable<'Awaitable, 'Awaiter, 'TResult
    when 'Awaitable: (member GetAwaiter: unit -> Awaiter<'Awaiter, 'TResult>)> = 'Awaitable

/// <summary>Functions for Awaitables</summary>
type Awaitable =

    /// <summary>Creates an awaiter for this value.</summary>
    static member inline GetAwaiter(x: Task<'T>) = x.GetAwaiter()

[<AutoOpen>]
module LowerPriorityAwaitable =
    type Awaitable with

        /// <summary>Creates an awaiter for this value.</summary>
        [<NoEagerConstraintApplication>]
        static member inline GetAwaiter<'Awaitable, 'Awaiter, 'TResult
            when Awaitable<'Awaitable, 'Awaiter, 'TResult>>
            (x: 'Awaitable)
            =
            x.GetAwaiter()

module Examples =

    module Disposable =
        type ImplicitDisposable() =
            member _.Dispose() = ()

        type ExplicitDisposable() =
            interface IDisposable with
                member _.Dispose() = ()

        let useCase1 () =
            let d = ImplicitDisposable()
            Disposable.Dispose d

        let useCase2 () =
            let d = new ExplicitDisposable()
            Disposable.Dispose d

    module AsyncDisposable =
        type ImplicitAsyncDisposable() =
            member _.DisposeAsync() = ValueTask()

        type ExplicitAsyncDisposable() =
            interface IAsyncDisposable with
                member _.DisposeAsync() = ValueTask()

        let useCase1 () =
            let d = ImplicitAsyncDisposable()
            AsyncDisposable.DisposeAsync d

        let useCase2 () =
            let d = new ExplicitAsyncDisposable()
            AsyncDisposable.DisposeAsync d

    module Enumerable =
        type ImplicitEnumerator() =
            member _.Current = ()
            member _.MoveNext() = true
            member _.Reset() = ()

        type ExplicitEnumerator<'T>() =
            interface IEnumerator<'T> with

                member _.Reset() = ()
                member _.Current = Unchecked.defaultof<'T>
                member _.Current = Unchecked.defaultof<'T> :> obj
                member _.MoveNext() = true
                member _.Dispose() = ()

        type ImplicitEnumerable() =
            member _.GetEnumerator() = ImplicitEnumerator()

        type ExplicitEnumerable<'T>() =
            interface IEnumerable<'T> with
                member _.GetEnumerator() = Unchecked.defaultof<IEnumerator<'T>>

                member _.GetEnumerator() =
                    Unchecked.defaultof<IEnumerator<'T>> :> System.Collections.IEnumerator

        let useCase1 () =
            let e = ImplicitEnumerable()
            Enumerable.GetEnumerator e

        let useCase2 () =
            let e = new ExplicitEnumerable<int32>()
            Enumerable.GetEnumerator e

        let useCase3 () =
            let e = []
            Enumerable.GetEnumerator e

    module Enumerator =
        let useCase1 () =
            let e = Enumerable.useCase1 ()
            Enumerator.Reset e

            if Enumerator.MoveNext e then
                Enumerator.Current e
                |> ignore

        let useCase2 () =
            let e = Enumerable.useCase2 ()
            Enumerator.Reset e

            if Enumerator.MoveNext e then
                Enumerator.Current e
                |> ignore

        let useCase3 () =
            let e = Enumerable.useCase3 ()
            Enumerator.Reset e

            if Enumerator.MoveNext e then
                Enumerator.Current e
                |> ignore


    module Awaitable =

        type ImplicitAwaiter() =
            member _.IsCompleted = true
            member _.GetResult() = ()

            interface ICriticalNotifyCompletion with
                member this.OnCompleted(continuation: Action) : unit = failwith "Not Implemented"

                member this.UnsafeOnCompleted(continuation: Action) : unit =
                    failwith "Not Implemented"

        type ImplicitAwaitable() =
            member _.GetAwaiter() = ImplicitAwaiter()

        let ex1 () = Task.Yield()
        let ex2 () = Task.FromResult 1
        let ex3 () = Task.FromResult() :> Task

        let useCase1 () =
            let t = ex1 ()
            Awaitable.GetAwaiter t

        let useCase2 () =
            let t = ex2 ()
            Awaitable.GetAwaiter t

        let useCase3 () =
            let t = ex3 ()
            Awaitable.GetAwaiter t

        let useCase4 () =
            let t = ImplicitAwaitable()
            Awaitable.GetAwaiter t

    module Awaiter =
        let useCase1 () =
            let a = Awaitable.useCase1 ()

            if Awaiter.IsCompleted a then
                Awaiter.GetResult a

        let useCase2 () =
            let a = Awaitable.useCase2 ()

            if Awaiter.IsCompleted a then
                Awaiter.GetResult a
                |> ignore

        let useCase3 () =
            let a = Awaitable.useCase3 ()

            if Awaiter.IsCompleted a then
                Awaiter.GetResult a

        let useCase4 () =
            let a = Awaitable.useCase4 ()

            if Awaiter.IsCompleted a then
                Awaiter.GetResult a

