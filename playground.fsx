#r "nuget: FsToolkit.ErrorHandling.TaskResult"

let inline id x = x

open FsToolkit.ErrorHandling

Result.ofChoice

module Operators =

    let inline bindM builder m ([<InlineIfLambda>] f) =
        (^M: (member Bind: 'd -> ('e -> 'c) -> 'c) (builder, m, f))

    let inline bind2M builder m1 m2 f =
        (^M: (member Bind2: 'a -> 'b -> ('e * 'f -> 'c) -> 'c) (builder, m1, m2, f))

    let inline delayM builder x =
        (^M: (member Delay: (unit -> 'a) -> 'c) (builder, x))

    /// Inject a value into the monadic type
    let inline returnM builder x =
        (^M: (member Return: 'b -> 'c) (builder, x))

    let inline returnFromM builder x =
        (^M: (member ReturnFrom: 'b -> 'c) (builder, x))

    let inline bindReturnM builder m ([<InlineIfLambda>] f) =
        (^M: (member BindReturn: 'd -> ('e -> 'c) -> 'f) (builder, m, f))

    let inline bind2ReturnM builder m1 m2 ([<InlineIfLambda>] f) =
        (^M: (member Bind2Return: 'a -> 'b -> ('c * 'd -> 'e) -> 'f) (builder, m1, m2, f))

    let inline mergeSourcesM builder m1 m2 =
        (^M: (member MergeSources: 'a * 'b -> 'c) (builder, m1, m2))

    let inline mergeSources3M builder m1 m2 m3 =
        (^M: (member MergeSources3: 'a * 'b * 'c -> 'd) (builder, m1, m2, m3))

    let inline liftM builder m ([<InlineIfLambda>] f) =
        bindM builder m
        <| fun m1 -> returnM builder (f m1)

    let inline lift2M builder1 builder2 m1 m2 ([<InlineIfLambda>] f) =
        bindM builder1 m1
        <| fun m1' ->
            liftM builder2 m2
            <| fun m2' -> (f m1' m2')

    /// Sequential application
    let inline applyM (builder1: ^M1) (builder2: ^M2) f m =
        lift2M builder1 builder2 f m
        <| fun f x -> f x

    /// Sequential application
    let inline zipM (builder1: ^M1) (builder2: ^M2) m1 m2 =
        lift2M builder1 builder2 m1 m2
        <| fun m1' m2' -> (m1', m2')


module Async =
    let inline bind ([<InlineIfLambda>] f) x = Operators.bindM async x f
    let inline delay x = Operators.delayM async x
    let inline singleton x = Operators.returnM async x
    let inline returnFrom x = Operators.returnFromM async x
    let inline bindReturn ([<InlineIfLambda>] f) x = Operators.liftM async x f
    let inline map ([<InlineIfLambda>] f) x = bindReturn f x
    let inline mergeSources x y = Operators.zipM async async x y
    let inline zip x y = mergeSources x y
    let inline apply f x = Operators.applyM async async f x

    let inline flatten x = bind id x

module Result =
    open FsToolkit.ErrorHandling

    let inline either ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) =
        function
        | Ok x -> okF x
        | Error e -> errorF e

    let inline bind ([<InlineIfLambda>] f) x = Operators.bindM result x f
    let inline delay x = Operators.delayM result x
    let inline singleton x = Operators.returnM result x
    let inline returnFrom x = Operators.returnFromM result x
    let inline bindReturn ([<InlineIfLambda>] f) x = Operators.bindReturnM result x f
    let inline map ([<InlineIfLambda>] f) x = bindReturn f x
    let inline mergeSources x y = Operators.mergeSourcesM result x y
    let inline zip x y = mergeSources x y

    let inline apply f x =
        mergeSources f x
        |> bindReturn (fun (f', x') -> f' x')

    let inline sequence builder f x =
        x
        |> either
            (fun x -> Operators.bindReturnM builder x f)
            (fun x ->
                x
                |> Result.Error
                |> Operators.returnM builder
            )

    let inline sequenceLift builder f x =
        x
        |> either
            (fun x -> Operators.liftM builder x f)
            (fun x ->
                x
                |> Result.Error
                |> Operators.returnM builder
            )

    let inline sequenceAsync x = sequenceLift async Ok x

    let inline sequenceAsyncResult x = sequenceLift async id x

    let inline flatten x = bind id x

module AsyncResult =

    type AsyncResultBuilder() =
        member inline _.Return x = Async.singleton (Result.singleton x)

        member inline _.Bind(m, f: 'a -> Async<Result<'b, 'c>>) =
            m
            |> Async.bind (fun x ->
                Result.map f x
                |> Result.sequenceAsyncResult
            )

        member inline _.Source(x: Async<Result<'a, 'b>>) = x

    [<AutoOpen>]
    module AsyncResultBuilderExt =
        open System.Threading.Tasks

        type AsyncResultBuilder with

            member inline _.Source(x: Async<'a>) = Async.map Ok x

            member inline _.Source(x: Task<'a>) =
                task {
                    let! x = x
                    return Ok x
                }
                |> Async.AwaitTask

            member inline _.Source(x: Result<_, _>) = Async.singleton x

    let asyncResult = AsyncResultBuilder()

    let inline bind ([<InlineIfLambda>] f) x = Operators.bindM asyncResult x f
    let inline singleton x = Operators.returnM asyncResult x

    let example () =
        asyncResult {
            let! x = singleton 1
            let! y = singleton 2
            let! z = Async.singleton 3
            let! a = Result.singleton 4

            return
                x
                + y
                + z
                + a
        }


module DisposableOptionThings =
    open System
    open System.Threading.Tasks

    [<RequireQualifiedAccess>]
    [<DefaultAugmentation(false)>]
    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    [<StructuralEquality; StructuralComparison>]
    type DisposableOption<'a when 'a :> IDisposable> =
        | None
        | Some of 'a

        interface IDisposable with
            member this.Dispose() =
                match this with
                | None -> ()
                | Some x -> x.Dispose()

        static member inline OfObj<'a when 'a :> IDisposable>(x: 'a) =
            if
                box x
                |> isNull
            then
                None
            else
                Some x

        static member inline ToOption(x: DisposableOption<'a>) =
            match x with
            | None -> Option.None
            | Some x -> Option.Some x

        static member inline ToValueOption(x: DisposableOption<'a>) =
            match x with
            | None -> ValueOption.None
            | Some x -> ValueOption.Some x

        static member inline OfOption(x: 'a Option) =
            match x with
            | Option.None -> None
            | Option.Some x -> Some x

        static member inline OfValueOption(x: 'a ValueOption) =
            match x with
            | ValueNone -> None
            | ValueSome x -> Some x

        static member inline op_Implicit(x: 'a) = DisposableOption.OfObj x

        static member inline op_Implicit(x: 'a DisposableOption) = DisposableOption.ToOption x

        static member inline op_Implicit(x: 'a DisposableOption) = DisposableOption.ToValueOption x

        static member inline op_Implicit(x: 'a Option) = DisposableOption.OfOption x

        static member inline op_Imp licit (x: 'a ValueOption) = DisposableOption.OfValueOption x


    [<RequireQualifiedAccess>]
    module DisposableOption =
        let inline bind f x =
            match x with
            | DisposableOption.Some x -> f x
            | DisposableOption.None -> None

        let inline map f x =
            match x with
            | DisposableOption.Some x -> Some(f x)
            | DisposableOption.None -> None

        let inline iter f x =
            match x with
            | DisposableOption.Some x -> f x
            | DisposableOption.None -> ()


    [<RequireQualifiedAccess>]
    type AsyncDisposableOption<'a when 'a :> IAsyncDisposable> =
        | Some of 'a
        | None

        interface IAsyncDisposable with
            member this.DisposeAsync() =
                match this with
                | Some x -> x.DisposeAsync()
                | None -> ValueTask()

        static member inline ofObj(x: 'a) =
            if
                box x
                |> isNull
            then
                None
            else
                Some x

        member inline x.toOption() =
            match x with
            | Some x -> Option.Some x
            | None -> Option.None

        member inline x.toValueOption() =
            match x with
            | Some x -> ValueOption.Some x
            | None -> ValueOption.None

        static member inline ofOption(x: 'a Option) =
            match x with
            | Option.Some x -> Some x
            | Option.None -> None

        static member inline ofValueOption(x: 'a ValueOption) =
            match x with
            | ValueOption.ValueSome x -> Some x
            | ValueOption.ValueNone -> None

        static member inline op_Implicit(x: 'a) = AsyncDisposableOption.ofObj x

        static member inline op_Implicit(x: 'a AsyncDisposableOption) = x.toOption ()

        static member inline op_Implicit(x: 'a AsyncDisposableOption) = x.toValueOption ()

        static member inline op_Implicit(x: 'a Option) = AsyncDisposableOption.ofOption x

        static member inline op_Implicit(x: 'a ValueOption) = AsyncDisposableOption.ofValueOption x

module Examples =
    open DisposableOptionThings
    open System.Diagnostics

    let inline implicitConv (x: ^T) : ^U =
        ((^T or ^U): (static member op_Implicit: ^T -> ^U) (x))

    let inline (!>) x = implicitConv x
    let inline (|!>) x f = f (!>x)

    let activitySource = new ActivitySource("Playground.App")

    let example () =
        use a =
            activitySource.StartActivity("lol")
            |> DisposableOption.OfObj

        a
        |!> Option.iter (fun a ->
            a.AddTag("hello", "world")
            |> ignore
        )

        ()
