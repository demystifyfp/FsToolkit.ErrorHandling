module IcedBuilderProbes

open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask
open RuntimeAsyncTestHelpers

type Marker = class end

type private R = Result<int, string>
type private V = Validation<int, string>

let private onDefaultScheduler () =
    isNull SynchronizationContext.Current
    && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)

let private cold (calls: int ref) (value: 'T) : CancellationToken -> Task<'T> =
    fun _ ->
        calls.Value <-
            calls.Value
            + 1

        Task.FromResult value

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeCancellableTaskResult (gate: Task<unit>) (onDefault: bool ref) =
    cancellableTaskResult {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: R)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundCancellableTaskResult (gate: Task<unit>) (onDefault: bool ref) =
    backgroundCancellableTaskResult {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: R)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeCancellableValueTaskResult (gate: Task<unit>) (onDefault: bool ref) =
    cancellableValueTaskResult {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: R)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundCancellableValueTaskResult (gate: Task<unit>) (onDefault: bool ref) =
    backgroundCancellableValueTaskResult {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: R)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeCancellableTaskValidation (gate: Task<unit>) (onDefault: bool ref) =
    cancellableTaskValidation {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: V)
        and! y = after gate (Ok 2: V)
        return x + y
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeCancellableTaskOption (gate: Task<unit>) (onDefault: bool ref) =
    cancellableTaskOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundCancellableTaskOption (gate: Task<unit>) (onDefault: bool ref) =
    backgroundCancellableTaskOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeCancellableValueTaskOption (gate: Task<unit>) (onDefault: bool ref) =
    cancellableValueTaskOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundCancellableValueTaskOption (gate: Task<unit>) (onDefault: bool ref) =
    backgroundCancellableValueTaskOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeValueTaskValueOption (gate: Task<unit>) (onDefault: bool ref) : ValueTask<int voption> =
    valueTaskValueOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (ValueSome 1)
        return x
    }

let private render running = renderTask running

let private probe
    name
    builder
    method
    background
    expected
    (start: Task<unit> -> bool ref -> Task<'T>)
    =
    {
        Builder = name
        Value =
            (box builder
             |> nonNull)
        Method = method
        Background = background
        Expected = sprintf "%A" (expected: 'T)
        Start = fun gate onDefault -> render (start gate onDefault)
    }

let private onToken (start: Task<unit> -> bool ref -> CancellationToken -> Task<'T>) =
    fun gate onDefault -> start gate onDefault CancellationToken.None

let private onTokenValue (start: Task<unit> -> bool ref -> CancellationToken -> ValueTask<'T>) =
    fun gate onDefault -> (start gate onDefault CancellationToken.None).AsTask()

let all: BuilderProbe list = [
    probe
        "cancellableTaskResult"
        cancellableTaskResult
        (nameof probeCancellableTaskResult)
        false
        (Ok 1: R)
        (onToken probeCancellableTaskResult)
    probe
        "backgroundCancellableTaskResult"
        backgroundCancellableTaskResult
        (nameof probeBackgroundCancellableTaskResult)
        true
        (Ok 1: R)
        (onToken probeBackgroundCancellableTaskResult)
    probe
        "cancellableValueTaskResult"
        cancellableValueTaskResult
        (nameof probeCancellableValueTaskResult)
        false
        (Ok 1: R)
        (onTokenValue probeCancellableValueTaskResult)
    probe
        "backgroundCancellableValueTaskResult"
        backgroundCancellableValueTaskResult
        (nameof probeBackgroundCancellableValueTaskResult)
        true
        (Ok 1: R)
        (onTokenValue probeBackgroundCancellableValueTaskResult)
    probe
        "cancellableTaskValidation"
        cancellableTaskValidation
        (nameof probeCancellableTaskValidation)
        false
        (Ok 3: V)
        (onToken probeCancellableTaskValidation)
    probe
        "cancellableTaskOption"
        cancellableTaskOption
        (nameof probeCancellableTaskOption)
        false
        (Some 1)
        (onToken probeCancellableTaskOption)
    probe
        "backgroundCancellableTaskOption"
        backgroundCancellableTaskOption
        (nameof probeBackgroundCancellableTaskOption)
        true
        (Some 1)
        (onToken probeBackgroundCancellableTaskOption)
    probe
        "cancellableValueTaskOption"
        cancellableValueTaskOption
        (nameof probeCancellableValueTaskOption)
        false
        (Some 1)
        (onTokenValue probeCancellableValueTaskOption)
    probe
        "backgroundCancellableValueTaskOption"
        backgroundCancellableValueTaskOption
        (nameof probeBackgroundCancellableValueTaskOption)
        true
        (Some 1)
        (onTokenValue probeBackgroundCancellableValueTaskOption)
    probe
        "valueTaskValueOption"
        valueTaskValueOption
        (nameof probeValueTaskValueOption)
        false
        (ValueSome 1)
        (fun gate onDefault -> (probeValueTaskValueOption gate onDefault).AsTask())
]

/// Each start gets a token, a flag that the body sets, a gate before the cold bind, and a count of cold source calls.
[<NoEquality; NoComparison>]
type CancelProbe = {
    Builder: string
    Start: CancellationToken -> bool ref -> Task<unit> -> int ref -> Task
}

let private cancelProbe
    name
    (start: CancellationToken -> bool ref -> Task<unit> -> int ref -> Task<'T>)
    =
    {
        Builder = name
        Start = fun ct reached gate calls -> start ct reached gate calls :> Task
    }

let private valueStart
    (start: CancellationToken -> bool ref -> Task<unit> -> int ref -> ValueTask<'T>)
    =
    fun ct reached gate calls -> (start ct reached gate calls).AsTask()

let private startWith (ct: CancellationToken) (computation: CancellationToken -> 'T) =
    computation ct

let cancellation: CancelProbe list = [
    cancelProbe "cancellableTaskResult"
    <| fun ct reached gate calls ->
        cancellableTaskResult {
            reached.Value <- true
            do! gate
            let! x = cold calls (Ok 1: R)
            return x
        }
        |> startWith ct
    cancelProbe "backgroundCancellableTaskResult"
    <| fun ct reached gate calls ->
        backgroundCancellableTaskResult {
            reached.Value <- true
            do! gate
            let! x = cold calls (Ok 1: R)
            return x
        }
        |> startWith ct
    cancelProbe "cancellableValueTaskResult"
    <| valueStart (fun ct reached gate calls ->
        cancellableValueTaskResult {
            reached.Value <- true
            do! gate
            let! x = cold calls (Ok 1: R)
            return x
        }
        |> startWith ct
    )
    cancelProbe "backgroundCancellableValueTaskResult"
    <| valueStart (fun ct reached gate calls ->
        backgroundCancellableValueTaskResult {
            reached.Value <- true
            do! gate
            let! x = cold calls (Ok 1: R)
            return x
        }
        |> startWith ct
    )
    cancelProbe "cancellableTaskValidation"
    <| fun ct reached gate calls ->
        cancellableTaskValidation {
            reached.Value <- true
            do! gate
            let! x = cold calls (Ok 1: V)
            return x
        }
        |> startWith ct
    cancelProbe "cancellableTaskOption"
    <| fun ct reached gate calls ->
        cancellableTaskOption {
            reached.Value <- true
            do! gate
            let! x = cold calls (Some 1)
            return x
        }
        |> startWith ct
    cancelProbe "backgroundCancellableTaskOption"
    <| fun ct reached gate calls ->
        backgroundCancellableTaskOption {
            reached.Value <- true
            do! gate
            let! x = cold calls (Some 1)
            return x
        }
        |> startWith ct
    cancelProbe "cancellableValueTaskOption"
    <| valueStart (fun ct reached gate calls ->
        cancellableValueTaskOption {
            reached.Value <- true
            do! gate
            let! x = cold calls (Some 1)
            return x
        }
        |> startWith ct
    )
    cancelProbe "backgroundCancellableValueTaskOption"
    <| valueStart (fun ct reached gate calls ->
        backgroundCancellableValueTaskOption {
            reached.Value <- true
            do! gate
            let! x = cold calls (Some 1)
            return x
        }
        |> startWith ct
    )
]

let private asTask (start: CancellationToken -> ValueTask<'T>) : CancellationToken -> Task<'T> =
    fun ct -> (start ct).AsTask()

let private rendered (start: CancellationToken -> Task<'T>) : CancellationToken -> Task<string> =
    fun ct -> render (start ct)

/// Each entry binds a pooled ValueTask<int> and adds one to its value.
let pooled: (string * string * (ValueTask<int> -> Task<string>)) list = [
    "cancellableTaskResult",
    sprintf "%A" (Ok 2: R),
    fun source ->
        rendered (cancellableTaskResult { let! x = source in return x + 1 }) CancellationToken.None
    "backgroundCancellableTaskResult",
    sprintf "%A" (Ok 2: R),
    fun source ->
        rendered
            (backgroundCancellableTaskResult { let! x = source in return x + 1 })
            CancellationToken.None
    "cancellableValueTaskResult",
    sprintf "%A" (Ok 2: R),
    fun source ->
        rendered
            (asTask (cancellableValueTaskResult { let! x = source in return x + 1 }))
            CancellationToken.None
    "backgroundCancellableValueTaskResult",
    sprintf "%A" (Ok 2: R),
    fun source ->
        rendered
            (asTask (backgroundCancellableValueTaskResult { let! x = source in return x + 1 }))
            CancellationToken.None
    "cancellableTaskValidation",
    sprintf "%A" (Ok 2: V),
    fun source ->
        rendered
            (cancellableTaskValidation { let! x = source in return x + 1 })
            CancellationToken.None
    "cancellableTaskOption",
    sprintf "%A" (Some 2),
    fun source ->
        rendered (cancellableTaskOption { let! x = source in return x + 1 }) CancellationToken.None
    "backgroundCancellableTaskOption",
    sprintf "%A" (Some 2),
    fun source ->
        rendered
            (backgroundCancellableTaskOption { let! x = source in return x + 1 })
            CancellationToken.None
    "cancellableValueTaskOption",
    sprintf "%A" (Some 2),
    fun source ->
        rendered
            (asTask (cancellableValueTaskOption { let! x = source in return x + 1 }))
            CancellationToken.None
    "backgroundCancellableValueTaskOption",
    sprintf "%A" (Some 2),
    fun source ->
        rendered
            (asTask (backgroundCancellableValueTaskOption { let! x = source in return x + 1 }))
            CancellationToken.None
    "valueTaskValueOption",
    sprintf "%A" (ValueSome 2),
    fun source -> render ((valueTaskValueOption { let! x = source in return x + 1 }).AsTask())
]

/// Each entry builds one cold computation whose body counts its runs.
let reusable: (string * (int ref -> CancellationToken -> Task<string>)) list = [
    "cancellableTaskResult",
    fun runs ->
        rendered (
            cancellableTaskResult {
                runs.Value <-
                    runs.Value
                    + 1

                do! Task.Yield()
                return runs.Value
            }
        )
    "backgroundCancellableTaskResult",
    fun runs ->
        rendered (
            backgroundCancellableTaskResult {
                runs.Value <-
                    runs.Value
                    + 1

                do! Task.Yield()
                return runs.Value
            }
        )
    "cancellableValueTaskResult",
    fun runs ->
        rendered (
            asTask (
                cancellableValueTaskResult {
                    runs.Value <-
                        runs.Value
                        + 1

                    do! Task.Yield()
                    return runs.Value
                }
            )
        )
    "backgroundCancellableValueTaskResult",
    fun runs ->
        rendered (
            asTask (
                backgroundCancellableValueTaskResult {
                    runs.Value <-
                        runs.Value
                        + 1

                    do! Task.Yield()
                    return runs.Value
                }
            )
        )
    "cancellableTaskValidation",
    fun runs ->
        rendered (
            cancellableTaskValidation {
                runs.Value <-
                    runs.Value
                    + 1

                do! Task.Yield()
                return runs.Value
            }
        )
    "cancellableTaskOption",
    fun runs ->
        rendered (
            cancellableTaskOption {
                runs.Value <-
                    runs.Value
                    + 1

                do! Task.Yield()
                return runs.Value
            }
        )
    "backgroundCancellableTaskOption",
    fun runs ->
        rendered (
            backgroundCancellableTaskOption {
                runs.Value <-
                    runs.Value
                    + 1

                do! Task.Yield()
                return runs.Value
            }
        )
    "cancellableValueTaskOption",
    fun runs ->
        rendered (
            asTask (
                cancellableValueTaskOption {
                    runs.Value <-
                        runs.Value
                        + 1

                    do! Task.Yield()
                    return runs.Value
                }
            )
        )
    "backgroundCancellableValueTaskOption",
    fun runs ->
        rendered (
            asTask (
                backgroundCancellableValueTaskOption {
                    runs.Value <-
                        runs.Value
                        + 1

                    do! Task.Yield()
                    return runs.Value
                }
            )
        )
]

let private asyncDisposable
    (disposals: int ref)
    (started: TaskCompletionSource<unit>)
    (disposed: Task)
    =
    { new System.IAsyncDisposable with
        member _.DisposeAsync() =
            disposals.Value <-
                disposals.Value
                + 1

            started.TrySetResult(())
            |> ignore

            ValueTask disposed
    }

let private runWith (ct: CancellationToken) (computation: CancellationToken -> Task<'T>) =
    computation ct

let private runValueWith (ct: CancellationToken) (computation: CancellationToken -> ValueTask<'T>) =
    (computation ct).AsTask()

/// Shapes that FS3918 rejected in earlier candidates: a let! in a for loop with a mutable, a let! in a while loop, and a use whose DisposeAsync suspends after a suspension.
[<NoEquality; NoComparison>]
type Regression = {
    Builder: string
    Expected: string * string * string
    ForLoop: Task<unit> -> Task<string>
    WhileLoop: Task<unit> -> Task<string>
    SuspendedUse: Task<unit> -> TaskCompletionSource<unit> -> Task -> int ref -> Task<string>
}

let regressions: Regression list = [
    {
        Builder = "cancellableTaskResult"
        Expected = sprintf "%A" (Ok 3: R), sprintf "%A" (Ok 2: R), sprintf "%A" (Ok 3: R)
        ForLoop =
            fun gate ->
                cancellableTaskResult {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Ok x: R)
                        total <- total + y

                    return total
                }
                |> runWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                cancellableTaskResult {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Ok 1: R)
                        count <- count + y

                    return count
                }
                |> runWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                cancellableTaskResult {
                    let! a = after gate (Ok 1: R)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Ok 2: R)
                    return a + b
                }
                |> runWith CancellationToken.None
                |> render
    }
    {
        Builder = "backgroundCancellableTaskResult"
        Expected = sprintf "%A" (Ok 3: R), sprintf "%A" (Ok 2: R), sprintf "%A" (Ok 3: R)
        ForLoop =
            fun gate ->
                backgroundCancellableTaskResult {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Ok x: R)
                        total <- total + y

                    return total
                }
                |> runWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                backgroundCancellableTaskResult {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Ok 1: R)
                        count <- count + y

                    return count
                }
                |> runWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                backgroundCancellableTaskResult {
                    let! a = after gate (Ok 1: R)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Ok 2: R)
                    return a + b
                }
                |> runWith CancellationToken.None
                |> render
    }
    {
        Builder = "cancellableValueTaskResult"
        Expected = sprintf "%A" (Ok 3: R), sprintf "%A" (Ok 2: R), sprintf "%A" (Ok 3: R)
        ForLoop =
            fun gate ->
                cancellableValueTaskResult {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Ok x: R)
                        total <- total + y

                    return total
                }
                |> runValueWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                cancellableValueTaskResult {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Ok 1: R)
                        count <- count + y

                    return count
                }
                |> runValueWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                cancellableValueTaskResult {
                    let! a = after gate (Ok 1: R)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Ok 2: R)
                    return a + b
                }
                |> runValueWith CancellationToken.None
                |> render
    }
    {
        Builder = "backgroundCancellableValueTaskResult"
        Expected = sprintf "%A" (Ok 3: R), sprintf "%A" (Ok 2: R), sprintf "%A" (Ok 3: R)
        ForLoop =
            fun gate ->
                backgroundCancellableValueTaskResult {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Ok x: R)
                        total <- total + y

                    return total
                }
                |> runValueWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                backgroundCancellableValueTaskResult {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Ok 1: R)
                        count <- count + y

                    return count
                }
                |> runValueWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                backgroundCancellableValueTaskResult {
                    let! a = after gate (Ok 1: R)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Ok 2: R)
                    return a + b
                }
                |> runValueWith CancellationToken.None
                |> render
    }
    {
        Builder = "cancellableTaskValidation"
        Expected = sprintf "%A" (Ok 3: V), sprintf "%A" (Ok 2: V), sprintf "%A" (Ok 3: V)
        ForLoop =
            fun gate ->
                cancellableTaskValidation {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Ok x: V)
                        total <- total + y

                    return total
                }
                |> runWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                cancellableTaskValidation {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Ok 1: V)
                        count <- count + y

                    return count
                }
                |> runWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                cancellableTaskValidation {
                    let! a = after gate (Ok 1: V)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Ok 2: V)
                    return a + b
                }
                |> runWith CancellationToken.None
                |> render
    }
    {
        Builder = "cancellableTaskOption"
        Expected = sprintf "%A" (Some 3), sprintf "%A" (Some 2), sprintf "%A" (Some 3)
        ForLoop =
            fun gate ->
                cancellableTaskOption {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Some x)
                        total <- total + y

                    return total
                }
                |> runWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                cancellableTaskOption {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Some 1)
                        count <- count + y

                    return count
                }
                |> runWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                cancellableTaskOption {
                    let! a = after gate (Some 1)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Some 2)
                    return a + b
                }
                |> runWith CancellationToken.None
                |> render
    }
    {
        Builder = "backgroundCancellableTaskOption"
        Expected = sprintf "%A" (Some 3), sprintf "%A" (Some 2), sprintf "%A" (Some 3)
        ForLoop =
            fun gate ->
                backgroundCancellableTaskOption {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Some x)
                        total <- total + y

                    return total
                }
                |> runWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                backgroundCancellableTaskOption {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Some 1)
                        count <- count + y

                    return count
                }
                |> runWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                backgroundCancellableTaskOption {
                    let! a = after gate (Some 1)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Some 2)
                    return a + b
                }
                |> runWith CancellationToken.None
                |> render
    }
    {
        Builder = "cancellableValueTaskOption"
        Expected = sprintf "%A" (Some 3), sprintf "%A" (Some 2), sprintf "%A" (Some 3)
        ForLoop =
            fun gate ->
                cancellableValueTaskOption {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Some x)
                        total <- total + y

                    return total
                }
                |> runValueWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                cancellableValueTaskOption {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Some 1)
                        count <- count + y

                    return count
                }
                |> runValueWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                cancellableValueTaskOption {
                    let! a = after gate (Some 1)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Some 2)
                    return a + b
                }
                |> runValueWith CancellationToken.None
                |> render
    }
    {
        Builder = "backgroundCancellableValueTaskOption"
        Expected = sprintf "%A" (Some 3), sprintf "%A" (Some 2), sprintf "%A" (Some 3)
        ForLoop =
            fun gate ->
                backgroundCancellableValueTaskOption {
                    let mutable total = 0

                    for x in
                        [
                            1
                            2
                        ] do
                        let! y = after gate (Some x)
                        total <- total + y

                    return total
                }
                |> runValueWith CancellationToken.None
                |> render
        WhileLoop =
            fun gate ->
                backgroundCancellableValueTaskOption {
                    let mutable count = 0

                    while count < 2 do
                        let! y = after gate (Some 1)
                        count <- count + y

                    return count
                }
                |> runValueWith CancellationToken.None
                |> render
        SuspendedUse =
            fun gate started disposed disposals ->
                backgroundCancellableValueTaskOption {
                    let! a = after gate (Some 1)
                    use _resource = asyncDisposable disposals started disposed
                    let! b = after gate (Some 2)
                    return a + b
                }
                |> runValueWith CancellationToken.None
                |> render
    }
]

/// Each entry builds one cold computation that waits on a gate, then gives the token of its invocation to a cold source.
let interleaved
    : (string * (Task<unit> -> CancellationToken ref -> CancellationToken -> Task<string>)) list = [
    "cancellableTaskResult",
    fun gate seen ->
        let computation =
            cancellableTaskResult {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Ok 1: R)

                return value
            }

        fun ct ->
            runWith ct computation
            |> render
    "backgroundCancellableTaskResult",
    fun gate seen ->
        let computation =
            backgroundCancellableTaskResult {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Ok 1: R)

                return value
            }

        fun ct ->
            runWith ct computation
            |> render
    "cancellableValueTaskResult",
    fun gate seen ->
        let computation =
            cancellableValueTaskResult {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Ok 1: R)

                return value
            }

        fun ct ->
            runValueWith ct computation
            |> render
    "backgroundCancellableValueTaskResult",
    fun gate seen ->
        let computation =
            backgroundCancellableValueTaskResult {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Ok 1: R)

                return value
            }

        fun ct ->
            runValueWith ct computation
            |> render
    "cancellableTaskValidation",
    fun gate seen ->
        let computation =
            cancellableTaskValidation {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Ok 1: V)

                return value
            }

        fun ct ->
            runWith ct computation
            |> render
    "cancellableTaskOption",
    fun gate seen ->
        let computation =
            cancellableTaskOption {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Some 1)

                return value
            }

        fun ct ->
            runWith ct computation
            |> render
    "backgroundCancellableTaskOption",
    fun gate seen ->
        let computation =
            backgroundCancellableTaskOption {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Some 1)

                return value
            }

        fun ct ->
            runWith ct computation
            |> render
    "cancellableValueTaskOption",
    fun gate seen ->
        let computation =
            cancellableValueTaskOption {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Some 1)

                return value
            }

        fun ct ->
            runValueWith ct computation
            |> render
    "backgroundCancellableValueTaskOption",
    fun gate seen ->
        let computation =
            backgroundCancellableValueTaskOption {
                do! gate

                let! value =
                    fun (ct: CancellationToken) ->
                        seen.Value <- ct
                        Task.FromResult(Some 1)

                return value
            }

        fun ct ->
            runValueWith ct computation
            |> render
]

/// The do! after the let! makes the guarded source compile to Bind, not BindReturn.
let bindCancellation: CancelProbe list = [
    cancelProbe "cancellableTaskResult"
    <| fun ct reached gate calls ->
        cancellableTaskResult {
            reached.Value <- true
            do! gate
            let! value = cold calls (Ok 1: R)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    cancelProbe "backgroundCancellableTaskResult"
    <| fun ct reached gate calls ->
        backgroundCancellableTaskResult {
            reached.Value <- true
            do! gate
            let! value = cold calls (Ok 1: R)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    cancelProbe "cancellableValueTaskResult"
    <| valueStart (fun ct reached gate calls ->
        cancellableValueTaskResult {
            reached.Value <- true
            do! gate
            let! value = cold calls (Ok 1: R)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    )
    cancelProbe "backgroundCancellableValueTaskResult"
    <| valueStart (fun ct reached gate calls ->
        backgroundCancellableValueTaskResult {
            reached.Value <- true
            do! gate
            let! value = cold calls (Ok 1: R)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    )
    cancelProbe "cancellableTaskValidation"
    <| fun ct reached gate calls ->
        cancellableTaskValidation {
            reached.Value <- true
            do! gate
            let! value = cold calls (Ok 1: V)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    cancelProbe "cancellableTaskOption"
    <| fun ct reached gate calls ->
        cancellableTaskOption {
            reached.Value <- true
            do! gate
            let! value = cold calls (Some 1)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    cancelProbe "backgroundCancellableTaskOption"
    <| fun ct reached gate calls ->
        backgroundCancellableTaskOption {
            reached.Value <- true
            do! gate
            let! value = cold calls (Some 1)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    cancelProbe "cancellableValueTaskOption"
    <| valueStart (fun ct reached gate calls ->
        cancellableValueTaskOption {
            reached.Value <- true
            do! gate
            let! value = cold calls (Some 1)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    )
    cancelProbe "backgroundCancellableValueTaskOption"
    <| valueStart (fun ct reached gate calls ->
        backgroundCancellableValueTaskOption {
            reached.Value <- true
            do! gate
            let! value = cold calls (Some 1)
            do! Task.CompletedTask
            return value
        }
        |> startWith ct
    )
]

/// Merges two token-taking validation factories with and!.
let mergeValidation
    (left: CancellationToken -> Task<V>)
    (right: CancellationToken -> Task<V>)
    (ct: CancellationToken)
    : Task<V> =
    cancellableTaskValidation {
        let! a = left
        and! b = right
        return a + b
    }
    |> startWith ct

let private coldAwaiter () = Task.FromResult(Ok 42: R).GetAwaiter()

/// Each entry binds a factory that returns a TaskAwaiter of a Result, with return! and with let!.
let coldAwaiters: (string * (unit -> Task<string>) * (unit -> Task<string>)) list = [
    "cancellableTaskResult",
    (fun () ->
        cancellableTaskResult { return! coldAwaiter }
        |> runWith CancellationToken.None
        |> render
    ),
    (fun () ->
        cancellableTaskResult {
            let! x = coldAwaiter
            return x
        }
        |> runWith CancellationToken.None
        |> render
    )
    "backgroundCancellableTaskResult",
    (fun () ->
        backgroundCancellableTaskResult { return! coldAwaiter }
        |> runWith CancellationToken.None
        |> render
    ),
    (fun () ->
        backgroundCancellableTaskResult {
            let! x = coldAwaiter
            return x
        }
        |> runWith CancellationToken.None
        |> render
    )
    "cancellableValueTaskResult",
    (fun () ->
        cancellableValueTaskResult { return! coldAwaiter }
        |> runValueWith CancellationToken.None
        |> render
    ),
    (fun () ->
        cancellableValueTaskResult {
            let! x = coldAwaiter
            return x
        }
        |> runValueWith CancellationToken.None
        |> render
    )
    "backgroundCancellableValueTaskResult",
    (fun () ->
        backgroundCancellableValueTaskResult { return! coldAwaiter }
        |> runValueWith CancellationToken.None
        |> render
    ),
    (fun () ->
        backgroundCancellableValueTaskResult {
            let! x = coldAwaiter
            return x
        }
        |> runValueWith CancellationToken.None
        |> render
    )
]

let private cancelingSequence (cts: CancellationTokenSource) (moves: int ref) =
    { new System.Collections.Generic.IAsyncEnumerable<int> with
        member _.GetAsyncEnumerator(_) =
            { new System.Collections.Generic.IAsyncEnumerator<int> with
                member _.MoveNextAsync() =
                    moves.Value <-
                        moves.Value
                        + 1

                    if moves.Value = 1 then
                        cts.Cancel()
                        ValueTask<bool>(true)
                    else
                        ValueTask<bool>(false)

                member _.Current = 1
                member _.DisposeAsync() = ValueTask()
            }
    }

/// The first MoveNextAsync cancels the invocation token and gives an item.
let asyncSequenceCancellation
    : (string * (CancellationTokenSource -> int ref -> int ref -> Task<string>)) list = [
    "cancellableTaskResult",
    fun cts moves calls ->
        cancellableTaskResult {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runWith cts.Token
        |> render
    "backgroundCancellableTaskResult",
    fun cts moves calls ->
        backgroundCancellableTaskResult {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runWith cts.Token
        |> render
    "cancellableValueTaskResult",
    fun cts moves calls ->
        cancellableValueTaskResult {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runValueWith cts.Token
        |> render
    "backgroundCancellableValueTaskResult",
    fun cts moves calls ->
        backgroundCancellableValueTaskResult {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runValueWith cts.Token
        |> render
    "cancellableTaskValidation",
    fun cts moves calls ->
        cancellableTaskValidation {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runWith cts.Token
        |> render
    "cancellableTaskOption",
    fun cts moves calls ->
        cancellableTaskOption {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runWith cts.Token
        |> render
    "backgroundCancellableTaskOption",
    fun cts moves calls ->
        backgroundCancellableTaskOption {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runWith cts.Token
        |> render
    "cancellableValueTaskOption",
    fun cts moves calls ->
        cancellableValueTaskOption {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runValueWith cts.Token
        |> render
    "backgroundCancellableValueTaskOption",
    fun cts moves calls ->
        backgroundCancellableValueTaskOption {
            for _ in cancelingSequence cts moves do
                calls.Value <-
                    calls.Value
                    + 1
        }
        |> runValueWith cts.Token
        |> render
]

/// Each background builder body records that it started, then returns one.
let queued: (string * (CancellationToken -> (unit -> unit) -> Task<string>)) list = [
    "backgroundCancellableTaskResult",
    fun ct started ->
        backgroundCancellableTaskResult {
            started ()
            return 1
        }
        |> runWith ct
        |> render
    "backgroundCancellableValueTaskResult",
    fun ct started ->
        backgroundCancellableValueTaskResult {
            started ()
            return 1
        }
        |> runValueWith ct
        |> render
    "backgroundCancellableTaskOption",
    fun ct started ->
        backgroundCancellableTaskOption {
            started ()
            return 1
        }
        |> runWith ct
        |> render
    "backgroundCancellableValueTaskOption",
    fun ct started ->
        backgroundCancellableValueTaskOption {
            started ()
            return 1
        }
        |> runValueWith ct
        |> render
]

/// The return! of the cold source after the do! compiles to ReturnFrom, not Bind or BindReturn.
let returnFromCancellation: CancelProbe list = [
    cancelProbe "cancellableTaskResult"
    <| fun ct reached gate calls ->
        cancellableTaskResult {
            reached.Value <- true
            do! gate
            return! cold calls (Ok 1: R)
        }
        |> startWith ct
    cancelProbe "backgroundCancellableTaskResult"
    <| fun ct reached gate calls ->
        backgroundCancellableTaskResult {
            reached.Value <- true
            do! gate
            return! cold calls (Ok 1: R)
        }
        |> startWith ct
    cancelProbe "cancellableValueTaskResult"
    <| valueStart (fun ct reached gate calls ->
        cancellableValueTaskResult {
            reached.Value <- true
            do! gate
            return! cold calls (Ok 1: R)
        }
        |> startWith ct
    )
    cancelProbe "backgroundCancellableValueTaskResult"
    <| valueStart (fun ct reached gate calls ->
        backgroundCancellableValueTaskResult {
            reached.Value <- true
            do! gate
            return! cold calls (Ok 1: R)
        }
        |> startWith ct
    )
    cancelProbe "cancellableTaskValidation"
    <| fun ct reached gate calls ->
        cancellableTaskValidation {
            reached.Value <- true
            do! gate
            return! cold calls (Ok 1: V)
        }
        |> startWith ct
    cancelProbe "cancellableTaskOption"
    <| fun ct reached gate calls ->
        cancellableTaskOption {
            reached.Value <- true
            do! gate
            return! cold calls (Some 1)
        }
        |> startWith ct
    cancelProbe "backgroundCancellableTaskOption"
    <| fun ct reached gate calls ->
        backgroundCancellableTaskOption {
            reached.Value <- true
            do! gate
            return! cold calls (Some 1)
        }
        |> startWith ct
    cancelProbe "cancellableValueTaskOption"
    <| valueStart (fun ct reached gate calls ->
        cancellableValueTaskOption {
            reached.Value <- true
            do! gate
            return! cold calls (Some 1)
        }
        |> startWith ct
    )
    cancelProbe "backgroundCancellableValueTaskOption"
    <| valueStart (fun ct reached gate calls ->
        backgroundCancellableValueTaskOption {
            reached.Value <- true
            do! gate
            return! cold calls (Some 1)
        }
        |> startWith ct
    )
]

/// Each entry cancels its own token inside a loop without a bind: a while loop, then a for loop over a list.
let synchronousLoopCancellation: (string * string * (CancellationTokenSource -> Task<string>)) list = [
    "cancellableTaskResult",
    sprintf "%A" (Ok 9: R),
    fun cts ->
        cancellableTaskResult {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runWith cts.Token
        |> render
    "backgroundCancellableTaskResult",
    sprintf "%A" (Ok 9: R),
    fun cts ->
        backgroundCancellableTaskResult {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runWith cts.Token
        |> render
    "cancellableValueTaskResult",
    sprintf "%A" (Ok 9: R),
    fun cts ->
        cancellableValueTaskResult {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runValueWith cts.Token
        |> render
    "backgroundCancellableValueTaskResult",
    sprintf "%A" (Ok 9: R),
    fun cts ->
        backgroundCancellableValueTaskResult {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runValueWith cts.Token
        |> render
    "cancellableTaskValidation",
    sprintf "%A" (Ok 9: V),
    fun cts ->
        cancellableTaskValidation {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runWith cts.Token
        |> render
    "cancellableTaskOption",
    sprintf "%A" (Some 9),
    fun cts ->
        cancellableTaskOption {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runWith cts.Token
        |> render
    "backgroundCancellableTaskOption",
    sprintf "%A" (Some 9),
    fun cts ->
        backgroundCancellableTaskOption {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runWith cts.Token
        |> render
    "cancellableValueTaskOption",
    sprintf "%A" (Some 9),
    fun cts ->
        cancellableValueTaskOption {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runValueWith cts.Token
        |> render
    "backgroundCancellableValueTaskOption",
    sprintf "%A" (Some 9),
    fun cts ->
        backgroundCancellableValueTaskOption {
            let mutable count = 0

            while count < 3 do
                if count = 1 then
                    cts.Cancel()

                count <- count + 1

            for x in
                [
                    1
                    2
                    3
                ] do
                count <- count + x

            return count
        }
        |> runValueWith cts.Token
        |> render
]

let typedValidation (gate: Task<unit>) : Task<Validation<int, string>> =
    let computation: CancellableTaskValidation<int, string> =
        cancellableTaskValidation {
            let! () = gate.ConfigureAwait(false)
            do! Task.Yield()
            let! x = (Ok 1: Validation<int, string>)
            return x + 1
        }

    computation CancellationToken.None
