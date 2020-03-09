namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Seq =
  let rec private traverseResultM' (state : Result<_,_>) (f : _ -> Result<_,_>) (xs: seq<_>) =
    match xs |> Seq.tryHead with
    | None -> state
    | Some x -> 
      let r = result {
        let! y = f x
        let! ys = state
        return Seq.append ys (Seq.singleton y)
      }  
      match r with
      | Ok _ -> traverseResultM' r f (xs |> Seq.skip 1)
      | Error _ -> r

  let rec private traverseAsyncResultM' (state : Async<Result<_,_>>) (f : _ -> Async<Result<_,_>>) xs =
    match (xs |> Seq.tryHead) with
    | None -> state
    | Some x -> 
      async {
        let! r = asyncResult {
          let! ys = state
          let! y = f x
          return seq { yield! ys; yield y }
        }  
        match r with
        | Ok _ -> 
          return! traverseAsyncResultM' (Async.singleton r) f (xs |> Seq.skip 1)
        | Error _ -> return r
      }
  let traverseResultM f xs =
    traverseResultM' (Ok Seq.empty) f xs
  
  let sequenceResultM xs =
    traverseResultM id xs

  let traverseAsyncResultM f xs =
    traverseAsyncResultM' (AsyncResult.retn Seq.empty) f xs

  let sequenceAsyncResultM xs =
    traverseAsyncResultM id xs

  let rec private traverseResultA' state f xs =
    match (xs |> Seq.tryHead) with
    | None ->
      state
    | Some x ->
      let fR = 
        f x |> Result.mapError List.singleton
      match state, fR with
      | Ok ys, Ok y -> 
        traverseResultA' (Ok (Seq.append ys (Seq.singleton y))) f (xs |> Seq.skip 1)
      | Error errs, Error e -> 
        traverseResultA' (Error (errs @ e)) f (xs |> Seq.skip 1)
      | Ok _, Error e | Error e , Ok _  -> 
        traverseResultA' (Error e) f (xs |> Seq.skip 1)

  let traverseResultA (f: 'a -> Result<'b, 'd>) (xs: 'a seq): Result<seq<'b>, 'd list> =
    traverseResultA' (Ok Seq.empty) f xs

  let sequenceResultA xs =
    traverseResultA id xs

  let rec private traverseAsyncResultA' state f xs =
    match xs |> Seq.tryHead with
    | None -> state
    | Some x ->
      async {
        let! s = state
        let! fR = f x |> AsyncResult.mapError List.singleton
        match s, fR with
        | Ok ys, Ok y -> 
          return! traverseAsyncResultA' (AsyncResult.retn (Seq.append ys (Seq.singleton y))) f (xs |> Seq.skip(1))
        | Error errs, Error e -> 
          return! traverseAsyncResultA' (AsyncResult.returnError (errs @ e)) f (xs |> Seq.skip(1))
        | Ok _, Error e | Error e , Ok _  -> 
          return! traverseAsyncResultA' (AsyncResult.returnError e) f (xs |> Seq.skip(1))
      }

  let rec traverseValidationA' state f xs =
    match xs |> Seq.tryHead with
    | None -> state
    | Some x -> 
      let fR = f x
      match state, fR with
      | Ok ys, Ok y -> 
        traverseValidationA' (Ok (Seq.append ys (Seq.singleton y))) f (xs |> Seq.skip(1))
      | Error errs1, Error errs2 -> 
        traverseValidationA' (Error (errs2 @ errs1 )) f (xs |> Seq.skip(1))
      | Ok _, Error errs | Error errs, Ok _  -> 
        traverseValidationA' (Error errs) f (xs |> Seq.skip(1))

  let traverseValidationA f xs =
    traverseValidationA' (Ok Seq.empty) f xs

  let sequenceValidationA xs =
    traverseValidationA id xs

  let traverseAsyncResultA f xs =
    traverseAsyncResultA' (AsyncResult.retn Seq.empty) f xs

  let sequenceAsyncResultA xs =
    traverseAsyncResultA id xs
