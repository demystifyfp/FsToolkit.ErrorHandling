module ArrayTraverse

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Configs
open Microsoft.FSharp.Core.CompilerServices

module Original =

    let inline traverseResultM
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let results = ResizeArray<'okOutput>(xs.Length)
        let mutable index = 0
        let mutable error: 'error voption = ValueNone

        while error.IsNone
              && index < xs.Length do
            match f xs[index] with
            | Ok value ->
                results.Add value
                index <- index + 1
            | Error e -> error <- ValueSome e

        match error with
        | ValueNone -> Ok(results.ToArray())
        | ValueSome e -> Error e

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

    let inline traverseResultM1
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        if xs.Length = 0 then
            Ok Array.empty
        else
            let oks = ResizeArray<'okOutput>(xs.Length)
            let mutable ok = true
            let mutable error = Unchecked.defaultof<_>
            let mutable i = 0

            while i < xs.Length
                  && ok do
                match f xs[i] with
                | Ok value ->
                    oks.Add value
                    i <- i + 1
                | Error e ->
                    error <- e
                    ok <- false

            if ok then Ok(oks.ToArray()) else Error error

    let inline traverseResultM2
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        match xs with
        | [||] -> Ok Array.empty
        | [| x |] ->
            match f x with
            | Ok value -> Ok [| value |]
            | Error e -> Error e
        | [| x1; x2 |] ->
            match f x1 with
            | Error e -> Error e
            | Ok value ->
                match f x2 with
                | Error e -> Error e
                | Ok value2 ->
                    Ok [|
                        value
                        value2
                    |]
        | _ ->
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

    let inline traverseResultA1
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        if xs.Length = 0 then
            Ok Array.empty
        else
            let oks = ResizeArray<'okOutput>(xs.Length)
            let mutable errors = ArrayCollector()
            let mutable ok = true

            for x in xs do
                match f x with
                | Ok value when ok -> oks.Add value
                | Ok _ -> ()
                | Error e ->
                    // TOCONSIDER while benchmark does not let it win, arguably this is a better capacity to allocate
                    // let buf = ResizeArray<'error>(xs.Length - oks.Count)
                    errors.Add e
                    ok <- false

            if ok then Ok(oks.ToArray()) else Error(errors.Close())

    let inline traverseResultAAC
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let oks = ResizeArray<'okOutput>(xs.Length)
        let mutable ok = true
        let mutable errors = ArrayCollector()

        for x in xs do
            match f x with
            | Ok value when ok -> oks.Add value
            | Ok _ -> ()
            | Error e ->
                errors.Add e
                ok <- false

        if ok then Ok(oks.ToArray()) else Error(errors.Close())

    let inline traverseResultA2
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        if xs.Length = 0 then
            Ok Array.empty
        elif xs.Length > 2 then
            traverseResultAAC f xs
        else
            let mutable oks = ArrayCollector()
            let mutable errors = ArrayCollector()
            let mutable ok = true

            for x in xs do
                match f x with
                | Ok value when ok -> oks.Add value
                | Ok _ -> ()
                | Error e ->
                    errors.Add e
                    ok <- false

            if ok then Ok(oks.Close()) else Error(errors.Close())

[<MemoryDiagnoser>]
[<GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)>]
type ArrayTraverseBenchmarks() =
    let mutable inputs = [||]

    let toResult x = if x < 0 then Error x else Ok(x + 1)
    let toAsyncResult x = async { return toResult x }

    [<Params(0, 1, 2, 3, 10, 100, 1000)>]
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
