namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =
    open Hopac

    let rec private traverseJobResultM' (state: Job<Result<_, _>>) (f: _ -> Job<Result<_, _>>) xs =
        match xs with
        | [] ->
            state
            |> JobResult.map List.rev
        | x :: xs -> job {
            let! r = jobResult {
                let! ys = state
                let! y = f x
                return y :: ys
            }

            match r with
            | Ok _ -> return! traverseJobResultM' (Job.singleton r) f xs
            | Error _ -> return r
          }

    let traverseJobResultM f xs =
        traverseJobResultM' (JobResult.retn []) f xs

    let sequenceJobResultM xs = traverseJobResultM id xs


    let rec private traverseJobResultA' state (f : _ -> Job<Result<_,_>>) xs =
        match xs with
        | [] ->
            state
            |> JobResult.eitherMap List.rev List.rev
        | x :: xs -> job {
            let! s = state

            let! fR = f x


            match s, fR with
            | Ok ys, Ok y -> return! traverseJobResultA' (JobResult.retn (y :: ys)) f xs
            | Error errs, Error e ->
                return! traverseJobResultA' (JobResult.returnError (e :: errs)) f xs
            | Ok _, Error e -> return! traverseJobResultA' (JobResult.returnError [e]) f xs
            | Error e, Ok _ -> return! traverseJobResultA' (JobResult.returnError e) f xs
          }


    let traverseJobResultA f xs =
        traverseJobResultA' (JobResult.retn []) f xs

    let sequenceJobResultA xs = traverseJobResultA id xs
