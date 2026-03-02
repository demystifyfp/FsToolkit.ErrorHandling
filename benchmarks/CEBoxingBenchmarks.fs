module CEBoxingBenchmarks

open System.Threading.Tasks
open BenchmarkDotNet.Attributes

// ─── Test disposable types ────────────────────────────────────────────────────
// Struct disposable: isNull (box x) always returns false but boxes the struct
// Class disposable:  isNull (box x) is equivalent to obj.ReferenceEquals(x, null)

[<Struct>]
type StructAsyncDisposable(value: int) =
    interface System.IAsyncDisposable with
        member _.DisposeAsync() = ValueTask()

type ClassAsyncDisposable(value: int) =
    interface System.IAsyncDisposable with
        member _.DisposeAsync() = ValueTask()

// ─── Old null check: isNull (box resource) ────────────────────────────────────
// For value types: boxes the struct onto the heap on every call = allocation
// For reference types: no boxing, but less idiomatic
module private OldNullCheck =
    let inline checkStruct (resource: StructAsyncDisposable) = isNull (box resource)
    let inline checkClass (resource: ClassAsyncDisposable) = isNull (box resource)

// ─── New null check: obj.ReferenceEquals(resource, null) ──────────────────────
// For value types: no boxing — F# compiler handles without heap allocation
// For reference types: same semantics, same speed
module private NewNullCheck =
    let inline checkStruct (resource: StructAsyncDisposable) =
        obj.ReferenceEquals(resource, null)

    let inline checkClass (resource: ClassAsyncDisposable) =
        obj.ReferenceEquals(resource, null)

// ─── Benchmark: Struct resource (value type) ─────────────────────────────────
// This is where the fix matters most: struct boxing → heap allocation per call

[<MemoryDiagnoser>]
type BoxingNullCheck_Struct_Benchmarks() =
    let resource = StructAsyncDisposable(42)

    [<Benchmark(Baseline = true)>]
    member _.Old_IsNullBox() = OldNullCheck.checkStruct resource

    [<Benchmark>]
    member _.New_ReferenceEquals() = NewNullCheck.checkStruct resource

// ─── Benchmark: Class resource (reference type) ───────────────────────────────
// For reference types both checks are equivalent (no boxing occurs either way)
// This validates the fix doesn't regress the common class-based usage

[<MemoryDiagnoser>]
type BoxingNullCheck_Class_Benchmarks() =
    let resource = ClassAsyncDisposable(42)

    [<Benchmark(Baseline = true)>]
    member _.Old_IsNullBox() = OldNullCheck.checkClass resource

    [<Benchmark>]
    member _.New_ReferenceEquals() = NewNullCheck.checkClass resource

// ─── Benchmark: Repeated null checks (realistic loop scenario) ────────────────
// Simulates a CE builder calling Using/TryFinallyAsync multiple times

[<MemoryDiagnoser>]
type BoxingNullCheck_Repeated_Benchmarks() =
    [<Params(1, 10, 100, 1000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_IsNullBox_Struct() =
        let resource = StructAsyncDisposable(42)
        let mutable result = false
        for _ in 1 .. this.N do
            result <- OldNullCheck.checkStruct resource
        result

    [<Benchmark>]
    member this.New_ReferenceEquals_Struct() =
        let resource = StructAsyncDisposable(42)
        let mutable result = false
        for _ in 1 .. this.N do
            result <- NewNullCheck.checkStruct resource
        result
