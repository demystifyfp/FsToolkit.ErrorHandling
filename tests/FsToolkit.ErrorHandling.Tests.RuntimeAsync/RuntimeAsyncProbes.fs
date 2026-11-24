module RuntimeAsyncProbes

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask
open RuntimeAsyncTestHelpers

type Marker = class end

type private R = Result<int, string>

[<MethodImpl(MethodImplOptions.NoInlining)>]
let letTaskResult gate : Task<Result<int, string>> =
    taskResult {
        let! x = after gate (Ok 1: R)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let letAsyncResult gate : Task<Result<int, string>> =
    taskResult {
        let! x = Async.AwaitTask(after gate (Ok 1: R))
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let letResult (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        do! gate
        let! x = (Ok 1: R)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let doTask (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        do! (gate :> Task)
        return 1
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let letPlainAwaitable gate : Task<Result<int, string>> =
    taskResult {
        let! x = (after gate 1).ConfigureAwait(false)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let useBang gate : Task<Result<int, string>> =
    taskResult {
        use! _resource =
            after
                gate
                (Ok
                    { new IDisposable with
                        member _.Dispose() = ()
                    }
                : Result<IDisposable, string>)

        return 1
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let andTaskResults gate : Task<Result<int, string>> =
    taskResult {
        let! a = after gate (Ok 1: R)
        and! b = after gate (Ok 2: R)
        return a + b
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let andPlains gate : Task<Result<int, string>> =
    taskResult {
        let! a = after gate 1
        and! b = ValueTask<int>(after gate 2)
        return a + b
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let useAsyncDisposable (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        use _resource =
            { new IAsyncDisposable with
                member _.DisposeAsync() = ValueTask(gate :> Task)
            }

        do! gate
        return 1
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let tryWithAwaitInHandler (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        try
            do! gate
            return raise (InvalidOperationException "body")
        with _ ->
            let! x = after gate 1
            return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let forSeq (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        let mutable total = 0

        for x in [ 1 ] do
            let! y = after gate (Ok x: R)
            total <- total + y

        return total
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let forAsyncSeq (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        let mutable total = 0

        for x in gatedAsyncSeq (Log()) gate [ 1 ] do
            total <- total + x

        return total
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let whileLoop (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        let mutable total = 0

        while total < 1 do
            let! y = after gate (Ok 1: R)
            total <- total + y

        return total
    }

let all: Probe list = [
    {
        Name = "let! of Task<Result>"
        Method = nameof letTaskResult
        Expected = 1
        Start = letTaskResult
    }
    {
        Name = "let! of Async<Result>"
        Method = nameof letAsyncResult
        Expected = 1
        Start = letAsyncResult
    }
    {
        Name = "let! of Result"
        Method = nameof letResult
        Expected = 1
        Start = letResult
    }
    {
        Name = "do! of Task"
        Method = nameof doTask
        Expected = 1
        Start = doTask
    }
    {
        Name = "let! of an awaitable of 'T"
        Method = nameof letPlainAwaitable
        Expected = 1
        Start = letPlainAwaitable
    }
    {
        Name = "use!"
        Method = nameof useBang
        Expected = 1
        Start = useBang
    }
    {
        Name = "and! of two Task<Result>"
        Method = nameof andTaskResults
        Expected = 3
        Start = andTaskResults
    }
    {
        Name = "and! of Task<'T> and ValueTask<'T>"
        Method = nameof andPlains
        Expected = 3
        Start = andPlains
    }
    {
        Name = "use of IAsyncDisposable"
        Method = nameof useAsyncDisposable
        Expected = 1
        Start = useAsyncDisposable
    }
    {
        Name = "try/with with let! in the handler"
        Method = nameof tryWithAwaitInHandler
        Expected = 1
        Start = tryWithAwaitInHandler
    }
    {
        Name = "for over seq"
        Method = nameof forSeq
        Expected = 1
        Start = forSeq
    }
    {
        Name = "for over IAsyncEnumerable"
        Method = nameof forAsyncSeq
        Expected = 1
        Start = forAsyncSeq
    }
    {
        Name = "while"
        Method = nameof whileLoop
        Expected = 1
        Start = whileLoop
    }
]
