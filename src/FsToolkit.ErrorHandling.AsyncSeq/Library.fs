namespace FsToolkit.ErrorHandling

open FSharp.Control
open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncSeqCE =

  type private AsyncSeq<'t> = FSharp.Control.AsyncSeq<'t>

  type AsyncResultBuilder with
    member this.While(guard : unit -> Async<option<Result<'T,'TError>>>, computation : 'T -> Async<Result<unit, 'TError>>) : Async<Result<unit, 'TError>> =
      async {
        match! guard () with
        | Some (Ok x) ->
          return! this.Bind(computation x, fun () -> this.While(guard, computation))
        | Some (Error e) ->
          return Error e
        | None ->
          return! this.Zero()
      }

    member this.For(xs : AsyncSeq<Result<'T,'TError>>, binder: 'T -> Async<Result<unit, 'TError>>) : Async<Result<unit, 'TError>> =
      this.Using(xs.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext, binder))

    member this.For(xs : AsyncSeq<'T>, binder: 'T -> Async<Result<unit, 'TError>>) : Async<Result<unit, 'TError>> =
      this.Using(xs.GetEnumerator (), fun enum ->
        let moveNext = enum.MoveNext >> Async.map(Option.map Ok)
        this.While(moveNext, binder))

    // member this.Source(xs : #seq<_>) = xs
    member this.Source(xs : AsyncSeq<_>) = xs

// [<AutoOpen>]
// // Having members as extensions gives them lower priority in
// // overload resolution and allows skipping more type annotations.
// module AsyncSeqCEExtensions =

//    type AsyncResultBuilder with

//     member inline this.Source(xs : #seq<_>) = xs


    // /// <summary>
    // /// Needed to allow `for..in` and `for..do` functionality
    // /// </summary>
    // member inline __.Source(s: #seq<_>) = s

    // /// <summary>
    // /// Method lets us transform data types into our internal representation.
    // /// </summary>
    // member inline __.Source(r: Option<'t>) = Job.singleton r
    // /// <summary>
    // /// Method lets us transform data types into our internal representation.
    // /// </summary>
    // member inline __.Source(a: Job<'t>) = a |> Job.map Some
    // /// <summary>
    // /// Method lets us transform data types into our internal representation.
    // /// </summary>
    // member inline __.Source(a: Async<'t>) = a |> Job.fromAsync |> Job.map Some
    // /// <summary>
    // /// Method lets us transform data types into our internal representation.
    // /// </summary>
    // member inline __.Source(a: Task<'t>) = a |> Job.awaitTask |> Job.map Some

