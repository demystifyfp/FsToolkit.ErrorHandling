namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open System
open System.Threading
open System.Threading.Tasks
open Expecto
open IcedTasks
open FsToolkit.ErrorHandling

// The let! and use! lines compile only if F# infers the type of the bound value from the source.
module BindInferenceTests =

    type Resource() =
        let mutable disposed = 0
        member _.Value = 1
        member _.Disposed = disposed

        interface IDisposable with
            member _.Dispose() =
                disposed <-
                    disposed
                    + 1

    type AsyncResource() =
        let mutable disposed = 0
        member _.Value = 1
        member _.Disposed = disposed

        interface IAsyncDisposable with
            member _.DisposeAsync() =
                disposed <-
                    disposed
                    + 1

                ValueTask()

    let inline private runWith ([<InlineIfLambda>] computation: CancellationToken -> 'Awaitable) =
        computation CancellationToken.None

    [<Tests>]
    let bindInferenceTests =
        testList "Bind inference" [
            testList "cancellableTaskResult" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskResult {
                                let! a =
                                    Task.FromResult((Ok(new Resource()): Result<Resource, string>))

                                let! b =
                                    ValueTask<Result<Resource, string>>(
                                        (Ok(new Resource()): Result<Resource, string>)
                                    )

                                let! c =
                                    async { return (Ok(new Resource()): Result<Resource, string>) }

                                let! d = (Ok(new Resource()): Result<Resource, string>)

                                let! e =
                                    cancellableTask {
                                        return (Ok(new Resource()): Result<Resource, string>)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableTaskResult {
                                use! r = Task.FromResult((Ok(resource): Result<Resource, string>))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableTaskResult {
                                use! r = Task.FromResult((Ok(resource): Result<Resource, string>))
                                let! stop = Task.FromResult((Error "stop": Result<int, string>))

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual (Error "stop") ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            cancellableTaskResult {
                                use! r =
                                    Task.FromResult((Ok(resource): Result<AsyncResource, string>))

                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskResult {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Ok(
                                                a.Value
                                                + b.Value
                                            )
                                            : Result<int, string>)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Ok 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskResult { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Ok 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            cancellableTaskResult {
                                use! r = Task.FromResult((Error "stop": Result<Resource, string>))
                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Error "stop") ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
            testList "backgroundCancellableTaskResult" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            backgroundCancellableTaskResult {
                                let! a =
                                    Task.FromResult((Ok(new Resource()): Result<Resource, string>))

                                let! b =
                                    ValueTask<Result<Resource, string>>(
                                        (Ok(new Resource()): Result<Resource, string>)
                                    )

                                let! c =
                                    async { return (Ok(new Resource()): Result<Resource, string>) }

                                let! d = (Ok(new Resource()): Result<Resource, string>)

                                let! e =
                                    cancellableTask {
                                        return (Ok(new Resource()): Result<Resource, string>)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            backgroundCancellableTaskResult {
                                use! r = Task.FromResult((Ok(resource): Result<Resource, string>))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            backgroundCancellableTaskResult {
                                use! r = Task.FromResult((Ok(resource): Result<Resource, string>))
                                let! stop = Task.FromResult((Error "stop": Result<int, string>))

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual (Error "stop") ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            backgroundCancellableTaskResult {
                                use! r =
                                    Task.FromResult((Ok(resource): Result<AsyncResource, string>))

                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            backgroundCancellableTaskResult {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Ok(
                                                a.Value
                                                + b.Value
                                            )
                                            : Result<int, string>)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Ok 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            backgroundCancellableTaskResult { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Ok 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            backgroundCancellableTaskResult {
                                use! r = Task.FromResult((Error "stop": Result<Resource, string>))
                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Error "stop") ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
            testList "cancellableValueTaskResult" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableValueTaskResult {
                                let! a =
                                    Task.FromResult((Ok(new Resource()): Result<Resource, string>))

                                let! b =
                                    ValueTask<Result<Resource, string>>(
                                        (Ok(new Resource()): Result<Resource, string>)
                                    )

                                let! c =
                                    async { return (Ok(new Resource()): Result<Resource, string>) }

                                let! d = (Ok(new Resource()): Result<Resource, string>)

                                let! e =
                                    cancellableTask {
                                        return (Ok(new Resource()): Result<Resource, string>)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableValueTaskResult {
                                use! r = Task.FromResult((Ok(resource): Result<Resource, string>))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableValueTaskResult {
                                use! r = Task.FromResult((Ok(resource): Result<Resource, string>))
                                let! stop = Task.FromResult((Error "stop": Result<int, string>))

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual (Error "stop") ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            cancellableValueTaskResult {
                                use! r =
                                    Task.FromResult((Ok(resource): Result<AsyncResource, string>))

                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableValueTaskResult {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult(
                                            (Ok(new Resource()): Result<Resource, string>)
                                        )

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Ok(
                                                a.Value
                                                + b.Value
                                            )
                                            : Result<int, string>)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Ok 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableValueTaskResult { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Ok 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            cancellableValueTaskResult {
                                use! r = Task.FromResult((Error "stop": Result<Resource, string>))
                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Error "stop") ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
            testList "cancellableTaskValidation" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskValidation {
                                let! a =
                                    Task.FromResult(
                                        (Validation.ok (new Resource())
                                        : Validation<Resource, string>)
                                    )

                                let! b =
                                    ValueTask<Validation<Resource, string>>(
                                        (Validation.ok (new Resource())
                                        : Validation<Resource, string>)
                                    )

                                let! c =
                                    async {
                                        return
                                            (Validation.ok (new Resource())
                                            : Validation<Resource, string>)
                                    }

                                let! d =
                                    (Validation.ok (new Resource()): Validation<Resource, string>)

                                let! e =
                                    cancellableTask {
                                        return
                                            (Validation.ok (new Resource())
                                            : Validation<Resource, string>)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult(
                                            (Validation.ok (new Resource())
                                            : Validation<Resource, string>)
                                        )

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult(
                                            (Validation.ok (new Resource())
                                            : Validation<Resource, string>)
                                        )

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableTaskValidation {
                                use! r =
                                    Task.FromResult(
                                        (Validation.ok (resource): Validation<Resource, string>)
                                    )

                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableTaskValidation {
                                use! r =
                                    Task.FromResult(
                                        (Validation.ok (resource): Validation<Resource, string>)
                                    )

                                let! stop =
                                    Task.FromResult(
                                        (Validation.error "stop": Validation<int, string>)
                                    )

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual (Error [ "stop" ]) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            cancellableTaskValidation {
                                use! r =
                                    Task.FromResult(
                                        (Validation.ok (resource): Validation<AsyncResource, string>)
                                    )

                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Ok 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskValidation {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult(
                                            (Validation.ok (new Resource())
                                            : Validation<Resource, string>)
                                        )

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Validation.ok (
                                                a.Value
                                                + b.Value
                                            )
                                            : Validation<int, string>)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Ok 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskValidation { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Ok 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            cancellableTaskValidation {
                                use! r =
                                    Task.FromResult(
                                        (Validation.error "stop": Validation<Resource, string>)
                                    )

                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Error [ "stop" ]) ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
            testList "cancellableTaskOption" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskOption {
                                let! a = Task.FromResult((Some(new Resource()): Resource option))

                                let! b =
                                    ValueTask<Resource option>(
                                        (Some(new Resource()): Resource option)
                                    )

                                let! c = async { return (Some(new Resource()): Resource option) }
                                let! d = (Some(new Resource()): Resource option)

                                let! e =
                                    cancellableTask {
                                        return (Some(new Resource()): Resource option)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableTaskOption {
                                use! r = Task.FromResult((Some(resource): Resource option))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableTaskOption {
                                use! r = Task.FromResult((Some(resource): Resource option))
                                let! stop = Task.FromResult((None: int option))

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual None ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            cancellableTaskOption {
                                use! r = Task.FromResult((Some(resource): AsyncResource option))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskOption {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Some(
                                                a.Value
                                                + b.Value
                                            )
                                            : int option)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Some 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableTaskOption { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Some 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            cancellableTaskOption {
                                use! r = Task.FromResult((None: Resource option))
                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual None ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
            testList "backgroundCancellableTaskOption" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            backgroundCancellableTaskOption {
                                let! a = Task.FromResult((Some(new Resource()): Resource option))

                                let! b =
                                    ValueTask<Resource option>(
                                        (Some(new Resource()): Resource option)
                                    )

                                let! c = async { return (Some(new Resource()): Resource option) }
                                let! d = (Some(new Resource()): Resource option)

                                let! e =
                                    cancellableTask {
                                        return (Some(new Resource()): Resource option)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            backgroundCancellableTaskOption {
                                use! r = Task.FromResult((Some(resource): Resource option))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            backgroundCancellableTaskOption {
                                use! r = Task.FromResult((Some(resource): Resource option))
                                let! stop = Task.FromResult((None: int option))

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual None ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            backgroundCancellableTaskOption {
                                use! r = Task.FromResult((Some(resource): AsyncResource option))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            backgroundCancellableTaskOption {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Some(
                                                a.Value
                                                + b.Value
                                            )
                                            : int option)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Some 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            backgroundCancellableTaskOption { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Some 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            backgroundCancellableTaskOption {
                                use! r = Task.FromResult((None: Resource option))
                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual None ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
            testList "cancellableValueTaskOption" [
                testCaseTask "let! infers the type of each source"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableValueTaskOption {
                                let! a = Task.FromResult((Some(new Resource()): Resource option))

                                let! b =
                                    ValueTask<Resource option>(
                                        (Some(new Resource()): Resource option)
                                    )

                                let! c = async { return (Some(new Resource()): Resource option) }
                                let! d = (Some(new Resource()): Resource option)

                                let! e =
                                    cancellableTask {
                                        return (Some(new Resource()): Resource option)
                                    }

                                let! f =
                                    fun () ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                let! g =
                                    fun (_: CancellationToken) ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                let! h = Task.FromResult(new Resource())
                                let! i = ValueTask<Resource>(new Resource())
                                let! j = async { return new Resource() }
                                let! k = cancellableTask { return new Resource() }
                                let! l = fun () -> Task.FromResult(new Resource())

                                return
                                    a.Value
                                    + b.Value
                                    + c.Value
                                    + d.Value
                                    + e.Value
                                    + f.Value
                                    + g.Value
                                    + h.Value
                                    + i.Value
                                    + j.Value
                                    + k.Value
                                    + l.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 12) ""
                    }
                testCaseTask "use! disposes an IDisposable"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableValueTaskOption {
                                use! r = Task.FromResult((Some(resource): Resource option))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IDisposable when a later bind stops the computation"
                <| fun () ->
                    task {
                        let resource = new Resource()

                        let! actual =
                            cancellableValueTaskOption {
                                use! r = Task.FromResult((Some(resource): Resource option))
                                let! stop = Task.FromResult((None: int option))

                                return
                                    r.Value
                                    + stop
                            }
                            |> runWith

                        Expect.equal actual None ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "use! disposes an IAsyncDisposable"
                <| fun () ->
                    task {
                        let resource = AsyncResource()

                        let! actual =
                            cancellableValueTaskOption {
                                use! r = Task.FromResult((Some(resource): AsyncResource option))
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual (Some 1) ""
                        Expect.equal resource.Disposed 1 "The resource must be disposed once"
                    }
                testCaseTask "let! and return! take lambdas with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableValueTaskOption {
                                let! a = fun ct -> Task.FromResult(new Resource())

                                let! b =
                                    fun _ ->
                                        Task.FromResult((Some(new Resource()): Resource option))

                                return!
                                    fun _ ->
                                        Task.FromResult(
                                            (Some(
                                                a.Value
                                                + b.Value
                                            )
                                            : int option)
                                        )
                            }
                            |> runWith

                        Expect.equal actual (Some 2) ""
                    }
                testCaseTask "return! takes a plain lambda with an unannotated parameter"
                <| fun () ->
                    task {
                        let! actual =
                            cancellableValueTaskOption { return! fun _ -> Task.FromResult 3 }
                            |> runWith

                        Expect.equal actual (Some 3) ""
                    }
                testCaseTask "use! of a stopped source does not run the rest of the computation"
                <| fun () ->
                    task {
                        let ran = ref false

                        let! actual =
                            cancellableValueTaskOption {
                                use! r = Task.FromResult((None: Resource option))
                                ran.Value <- true
                                return r.Value
                            }
                            |> runWith

                        Expect.equal actual None ""
                        Expect.isFalse ran.Value "The computation must stop at the use! line"
                    }
            ]
        ]
