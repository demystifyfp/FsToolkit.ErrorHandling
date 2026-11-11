module ArrayTraverse

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Configs
open Microsoft.FSharp.Core.CompilerServices
open FsToolkit.ErrorHandling

module Original =

    let inline traverseResultM
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let results = ResizeArray<'okOutput>(xs.Length)
        let mutable index = 0
        let mutable error = Unchecked.defaultof<'error>
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | Ok value ->
                results.Add value
                index <- index + 1
            | Error e ->
                error <- e
                ok <- false

        if ok then Ok(results.ToArray()) else Error error

    let inline traverseResultA
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let oks = ResizeArray<'okOutput>(xs.Length)
        let mutable errors: ResizeArray<'error> option = None
        let mutable ok = true

        for x in xs do
            match f x with
            | Ok value when ok -> oks.Add value
            | Ok _ -> ()
            | Error e ->
                let errorBuffer =
                    match errors with
                    | Some errors -> errors
                    | None ->
                        let buffer = ResizeArray<'error>()
                        errors <- Some buffer
                        buffer

                errorBuffer.Add e
                ok <- false

        if ok then
            Ok(oks.ToArray())
        else
            match errors with
            | Some errors -> Error(errors.ToArray())
            | None -> Error [||]

module private Candidates =

    let traverseResultM1' state f xs =
        match xs with
        | [||] -> state
        | xs -> Seq.traverseResultM' state f xs

    let traverseResultM1 f xs = traverseResultM1' (Ok Array.empty) f xs

    let inline traverseResultM2
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let mutable results = ArrayCollector<'okOutput>()
        let mutable error = Unchecked.defaultof<'error>
        let mutable index = 0
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | Ok value ->
                results.Add value
                index <- index + 1
            | Error e ->
                error <- e
                ok <- false

        if ok then Ok(results.Close()) else Error error

    let inline traverseResultM3
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let mutable results = ArrayCollector<'okOutput>()
        let mutable error = Unchecked.defaultof<'error>
        let mutable ok = true

        for x in xs do
            match f x with
            | Ok value ->
                if ok then
                    results.Add value
            | Error e ->
                error <- e
                ok <- false

        if ok then Ok(results.Close()) else Error error

    let inline traverseResultM4
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let mutable error = Unchecked.defaultof<'error>
        let mutable ok = true

        let res = [|
            for x in xs do
                match f x with
                | Ok value ->
                    if ok then
                        value
                | Error e ->
                    error <- e
                    ok <- false
        |]

        if ok then Ok res else Error error

    let inline traverseResultA1'
        (state: Result<'a[], 'b[]>)
        ([<InlineIfLambda>] f: 'c -> Result<'a, 'b>)
        (xs: 'c[])
        : Result<'a[], 'b[]> =
        match xs with
        | [||] -> state
        | xs -> Seq.traverseResultA' state f xs

    let traverseResultA1 f xs = traverseResultA1' (Ok Array.empty) f xs

    let inline traverseResultA2'
        (state: Result<'a[], 'b[]>)
        ([<InlineIfLambda>] f: 'c -> Result<'a, 'b>)
        (xs: 'c[])
        : Result<'a[], 'b[]> =
#if FABLE_COMPILER
        traverseResultA1' state f xs
#else
        match xs with
        | [||] -> state
        | xs ->
            match state with
            | Error failuresToDate ->
                let mutable errs = ArrayCollector()
                errs.AddMany failuresToDate

                for x in xs do
                    match f x with
                    | Error e -> errs.Add e
                    | Ok _ -> () // as the initial state was failure, oks are irrelevant

                Error(errs.Close())
            | Ok initialSuccesses ->

                match xs with
                | [| x |] ->
                    match f x with
                    | Ok r ->
                        let mutable oks = ArrayCollector()
                        oks.AddMany initialSuccesses
                        oks.Add r
                        Ok(oks.Close())
                    | Error e -> Error [| e |]
                | xs ->

                    let mutable oks = ArrayCollector()
                    oks.AddMany initialSuccesses
                    let mutable errs = ArrayCollector()
                    let mutable ok = true

                    for x in xs do
                        match f x with
                        | Ok r when ok -> oks.Add r
                        | Ok _ -> () // no point saving results we won't use given the end result will be Error
                        | Error e ->
                            errs.Add e
                            ok <- false

                    if ok then Ok(oks.Close()) else Error(errs.Close())
#endif
    let traverseResultA2 f xs = traverseResultA2' (Ok Array.empty) f xs

[<MemoryDiagnoser>]
[<GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)>]
type ArrayTraverseBenchmarks() =
    let mutable inputs = [||]

    let toResult x = if x < 0 then Error x else Ok(x + 1)
    let toAsyncResult x = async { return toResult x }

    [<Params(0, 1, 5, 50)>]
    member val Size = 0 with get, set

    [<Params(false, true)>]
    member val HasErrors = false with get, set

    [<GlobalSetup>]
    member this.Setup() =
        inputs <-
            Array.init
                this.Size
                (fun i ->
                    if
                        this.HasErrors
                        && i = this.Size
                               / 2
                    then
                        -1
                    else
                        i
                )

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM() =
        Original.traverseResultM toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM1() =
        Candidates.traverseResultM1 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM2() =
        Candidates.traverseResultM2 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM3() =
        Candidates.traverseResultM3 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM4() =
        Candidates.traverseResultM4 toResult inputs

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA() =
        Original.traverseResultA toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA1() =
        Candidates.traverseResultA1 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA2() =
        Candidates.traverseResultA2 toResult inputs
