namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =
    open Hopac

    // O(n): ResizeArray accumulation — avoids cons+List.rev (extra O(n) copy)
    let traverseJobResultM
        (f: 'okInput -> Job<Result<'okOutput, 'error>>)
        (xs: 'okInput list)
        : Job<Result<'okOutput list, 'error>> =
        let oks = ResizeArray()

        let rec loop current =
            match current with
            | [] -> Job.result (Ok(List.ofSeq oks))
            | x :: rest ->
                job {
                    let! r = f x

                    match r with
                    | Ok v ->
                        oks.Add v
                        return! loop rest
                    | Error e -> return Error e
                }

        loop xs

    let sequenceJobResultM xs = traverseJobResultM id xs


    // O(n): applicative — collects all errors in a single pass, no List.rev copy
    let traverseJobResultA
        (f: 'okInput -> Job<Result<'okOutput, 'error>>)
        (xs: 'okInput list)
        : Job<Result<'okOutput list, 'error list>> =
        let oks = ResizeArray()
        let errs = ResizeArray()

        let rec loop current =
            match current with
            | [] ->
                if errs.Count = 0 then
                    Job.result (Ok(List.ofSeq oks))
                else
                    Job.result (Error(List.ofSeq errs))
            | x :: rest ->
                job {
                    let! r = f x

                    match r with
                    | Ok v when errs.Count = 0 ->
                        oks.Add v
                        return! loop rest
                    | Ok _ -> return! loop rest
                    | Error e ->
                        errs.Add e
                        return! loop rest
                }

        loop xs

    let sequenceJobResultA xs = traverseJobResultA id xs
