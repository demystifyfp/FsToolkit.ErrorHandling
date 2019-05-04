namespace FsToolkit.ErrorHandling
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FSharp.Control.Tasks.V2.ContextInsensitive

[<RequireQualifiedAccess>]
module List =
    let private traverseTaskResultM' (f : 'c -> Task<Result<'a,'b>>) (xs : 'c list) =
        let mutable state = Ok []
        let mutable index = 0
        let xs = xs |> List.toArray
        task {
            while state |> Result.isOk && index < xs.Length do
                let! r = xs |> Array.item index |> f
                index <- index + 1 
                match (r, state) with
                | Ok y, Ok ys ->
                    state <- Ok (y :: ys)
                | Error e, _ ->
                    state <- Error e
                | _, _ ->
                    ()
            return 
                state
                |> Result.map List.rev
        }

    let traverseTaskResultM f xs =
        traverseTaskResultM' f xs

    let sequenceTaskResultM xs =
        traverseTaskResultM id xs

    let private traverseTaskResultA' (f : 'c -> Task<Result<'a,'b>>) (xs : 'c list) =
        let mutable state = Ok []
        
        task {
            for x in xs do
                let! r = f x 
                match (r, state) with
                | Ok y, Ok ys ->
                    state <- Ok (y :: ys)
                | Error e, Error errs ->
                    state <- Error (e :: errs)
                | Ok _, Error e ->
                    state <- Error e
                | Error e , Ok _  -> 
                    state <- Error [e]
            return
                state
                |> Result.eitherMap List.rev List.rev
        }

    let traverseTaskResultA f xs =
        traverseTaskResultA' f xs

    let sequenceTaskResultA xs =
        traverseTaskResultA id xs