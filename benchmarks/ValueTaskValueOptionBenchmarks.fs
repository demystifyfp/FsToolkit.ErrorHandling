namespace Benchmarks

open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

/// Old ValueTaskValueOption implementations that always use task {} CE,
/// defeating ValueTask's allocation-free purpose when the input is already completed.
module OldValueTaskValueOption =
    let inline map ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        ValueTask<_ voption>(
            task {
                let! opt = ar
                return ValueOption.map f opt
            }
        )

    let inline bind ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        ValueTask<_ voption>(
            task {
                let! opt = ar

                let t =
                    match opt with
                    | ValueSome x -> f x
                    | ValueNone -> ValueTask<_ voption>(ValueNone)

                return! t
            }
        )

    let inline apply (f: ValueTask<('a -> 'b) voption>) (x: ValueTask<'a voption>) =
        ValueTask<_ voption>(
            task {
                let! fOpt = f

                match fOpt with
                | ValueSome f' ->
                    let! xOpt = x
                    return ValueOption.map f' xOpt
                | ValueNone -> return ValueNone
            }
        )

    let inline zip (x1: ValueTask<'a voption>) (x2: ValueTask<'b voption>) =
        ValueTask<('a * 'b) voption>(
            task {
                let! r1 = x1
                let! r2 = x2
                return ValueOption.zip r1 r2
            }
        )

    let inline defaultValue (value: 'value) (valueTaskValueOption: ValueTask<'value voption>) =
        ValueTask<'value>(
            task {
                let! opt = valueTaskValueOption
                return ValueOption.defaultValue value opt
            }
        )

    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (valueTaskValueOption: ValueTask<'value voption>)
        : ValueTask<'value> =
        ValueTask<'value>(
            task {
                let! opt = valueTaskValueOption
                return ValueOption.defaultWith defThunk opt
            }
        )

    let inline either
        ([<InlineIfLambda>] onValueSome: 'input -> ValueTask<'output>)
        ([<InlineIfLambda>] onValueNone: unit -> ValueTask<'output>)
        (input: ValueTask<'input voption>)
        : ValueTask<'output> =
        ValueTask<'output>(
            task {
                let! opt = input

                match opt with
                | ValueSome v -> return! onValueSome v
                | ValueNone -> return! onValueNone ()
            }
        )


[<MemoryDiagnoser>]
type ValueTaskValueOption_Map_Benchmarks() =
    let completedValueSome = ValueTask<int voption>(ValueSome 42)

    [<Benchmark(Baseline = true)>]
    member _.Old_Map() =
        (OldValueTaskValueOption.map (fun x -> x + 1) completedValueSome).Result

    [<Benchmark>]
    member _.New_Map() =
        (ValueTaskValueOption.map (fun x -> x + 1) completedValueSome).Result


[<MemoryDiagnoser>]
type ValueTaskValueOption_Bind_Benchmarks() =
    let completedValueSome = ValueTask<int voption>(ValueSome 42)

    [<Benchmark(Baseline = true)>]
    member _.Old_Bind() =
        (OldValueTaskValueOption.bind
            (fun x -> ValueTask<int voption>(ValueSome(x + 1)))
            completedValueSome)
            .Result

    [<Benchmark>]
    member _.New_Bind() =
        (ValueTaskValueOption.bind
            (fun x -> ValueTask<int voption>(ValueSome(x + 1)))
            completedValueSome)
            .Result


[<MemoryDiagnoser>]
type ValueTaskValueOption_Apply_Benchmarks() =
    let completedF = ValueTask<(int -> int) voption>(ValueSome(fun x -> x + 1))
    let completedX = ValueTask<int voption>(ValueSome 42)

    [<Benchmark(Baseline = true)>]
    member _.Old_Apply() =
        (OldValueTaskValueOption.apply completedF completedX).Result

    [<Benchmark>]
    member _.New_Apply() =
        (ValueTaskValueOption.apply completedF completedX).Result


[<MemoryDiagnoser>]
type ValueTaskValueOption_Zip_Benchmarks() =
    let completedX1 = ValueTask<int voption>(ValueSome 1)
    let completedX2 = ValueTask<int voption>(ValueSome 2)

    [<Benchmark(Baseline = true)>]
    member _.Old_Zip() =
        (OldValueTaskValueOption.zip completedX1 completedX2).Result

    [<Benchmark>]
    member _.New_Zip() =
        (ValueTaskValueOption.zip completedX1 completedX2).Result


[<MemoryDiagnoser>]
type ValueTaskValueOption_DefaultValue_Benchmarks() =
    let completedValueSome = ValueTask<int voption>(ValueSome 42)
    let completedValueNone = ValueTask<int voption>(ValueNone)

    [<Benchmark(Baseline = true)>]
    member _.Old_DefaultValue_Some() =
        (OldValueTaskValueOption.defaultValue 0 completedValueSome).Result

    [<Benchmark>]
    member _.New_DefaultValue_Some() =
        (ValueTaskValueOption.defaultValue 0 completedValueSome).Result

    [<Benchmark>]
    member _.Old_DefaultValue_None() =
        (OldValueTaskValueOption.defaultValue 0 completedValueNone).Result

    [<Benchmark>]
    member _.New_DefaultValue_None() =
        (ValueTaskValueOption.defaultValue 0 completedValueNone).Result


[<MemoryDiagnoser>]
type ValueTaskValueOption_Either_Benchmarks() =
    let completedValueSome = ValueTask<int voption>(ValueSome 42)

    [<Benchmark(Baseline = true)>]
    member _.Old_Either() =
        (OldValueTaskValueOption.either
            (fun v -> ValueTask<int>(v + 1))
            (fun () -> ValueTask<int>(0))
            completedValueSome)
            .Result

    [<Benchmark>]
    member _.New_Either() =
        (ValueTaskValueOption.either
            (fun v -> ValueTask<int>(v + 1))
            (fun () -> ValueTask<int>(0))
            completedValueSome)
            .Result
