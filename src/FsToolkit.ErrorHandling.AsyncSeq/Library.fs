namespace FsToolkit.ErrorHandling

open FSharp.Control
open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncSeq =

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

    member this.Source(xs) = xs
