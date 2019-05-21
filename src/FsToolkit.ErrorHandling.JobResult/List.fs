namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =
  open Hopac
  let rec private traverseJobResultM' (state : Job<Result<_,_>>) (f : _ -> Job<Result<_,_>>) xs =
    match xs with
    | [] -> state
    | x :: xs -> 
      job {
        let! r = jobResult {
          let! ys = state
          let! y = f x
          return ys @ [y]
        }  
        match r with
        | Ok _ -> 
          return! traverseJobResultM' (Job.singleton r) f xs
        | Error _ -> return r
      }

  let traverseJobResultM f xs =
    traverseJobResultM' (JobResult.retn []) f xs

  let sequenceJobResultM xs =
    traverseJobResultM id xs


  let rec private traverseJobResultA' state f xs =
    match xs with
    | [] -> state
    | x :: xs ->
      job {
        let! s = state
        let! fR = f x |> JobResult.mapError List.singleton
        match s, fR with
        | Ok ys, Ok y -> 
          return! traverseJobResultA' (JobResult.retn (ys @ [y])) f xs
        | Error errs, Error e -> 
          return! traverseJobResultA' (JobResult.returnError (errs @ e)) f xs
        | Ok _, Error e | Error e , Ok _  -> 
          return! traverseJobResultA' (JobResult.returnError  e) f xs
      }


  let traverseJobResultA f xs =
    traverseJobResultA' (JobResult.retn []) f xs

  let sequenceJobResultA xs =
    traverseJobResultA id xs