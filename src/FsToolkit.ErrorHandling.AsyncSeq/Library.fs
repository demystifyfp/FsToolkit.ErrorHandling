namespace FsToolkit.ErrorHandling

open FSharp.Control
open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncSeqCE =

    type private AsyncSeq<'t> = FSharp.Control.AsyncSeq<'t>

    type AsyncResultBuilder with
        member inline this.While
            (
                [<InlineIfLambda>] guard: unit -> Async<option<Result<'T, 'TError>>>,
                [<InlineIfLambda>] computation: 'T -> Async<Result<unit, 'TError>>
            ) : Async<Result<unit, 'TError>> =
            async {
                match! guard () with
                | Some (Ok x) ->
                    let mutable whileAsync = Unchecked.defaultof<_>

                    whileAsync <-
                        fun x ->
                            this.Bind(
                                computation x,
                                (fun () -> async {
                                    match! guard () with
                                    | Some (Ok x) -> return! whileAsync x
                                    | Some (Error e) -> return Error e
                                    | None -> return! this.Zero()
                                })
                            )

                    return! whileAsync x
                | Some (Error e) -> return Error e
                | None -> return! this.Zero()
            }


        member this.For
            (
                xs: AsyncSeq<Result<'T, 'TError>>,
                binder: 'T -> Async<Result<unit, 'TError>>
            ) : Async<Result<unit, 'TError>> =
            this.Using(xs.GetEnumerator(), (fun enum -> this.While(enum.MoveNext, binder)))

        member this.For(xs: AsyncSeq<'T>, binder: 'T -> Async<Result<unit, 'TError>>) : Async<Result<unit, 'TError>> =
            this.Using(
                xs.GetEnumerator(),
                fun enum ->
                    let moveNext = enum.MoveNext >> Async.map (Option.map Ok)

                    this.While(moveNext, binder)
            )

        member this.Source(xs: AsyncSeq<_>) = xs
