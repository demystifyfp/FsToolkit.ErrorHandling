namespace FsToolkit.ErrorHandling

open System

[<AutoOpen>]
module ResultOptionCE =

    type ResultOptionBuilder() =
        member inline _.Return value = ResultOption.retn value

        member inline _.ReturnFrom value : Result<'ok option, 'error> = value

        member inline _.Zero() =
            option.Zero()
            |> Ok

        member inline _.Bind
            (
                resultOpt: Result<'okInput option, 'error>,
                [<InlineIfLambda>] binder: 'okInput -> Result<'okOutput option, 'error>
            ) : Result<'okOutput option, 'error> =
            ResultOption.bind binder resultOpt

        member inline _.Combine
            (
                resultOpt: Result<unit option, 'error>,
                [<InlineIfLambda>] binder: unit -> Result<'ok option, 'error>
            ) : Result<'ok option, 'error> =
            ResultOption.bind binder resultOpt

        member inline _.Delay([<InlineIfLambda>] delayer: unit -> Result<'ok option, 'error>) =
            delayer

        member inline _.Run
            ([<InlineIfLambda>] generator: unit -> Result<'ok option, 'error>)
            : Result<'ok option, 'error> =
            generator ()

        member inline this.TryWith
            (
                [<InlineIfLambda>] generator: unit -> Result<'T option, 'TError>,
                [<InlineIfLambda>] handler: exn -> Result<'T option, 'TError>
            ) : Result<'T option, 'TError> =
            try
                this.Run generator
            with e ->
                handler e

        member inline this.TryFinally
            (
                [<InlineIfLambda>] generator: unit -> Result<'ok option, 'error>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Result<'ok option, 'error> =
            try
                this.Run generator
            finally
                compensation ()

        member inline this.Using
            (resource: 'disposable :> IDisposable, binder: 'disposable -> Result<'ok option, 'error>) : Result<
                                                                                                            'ok option,
                                                                                                            'error
                                                                                                         >
            =
            this.TryFinally(
                (fun () -> binder resource),
                (fun () ->
                    if not (obj.ReferenceEquals(resource, null)) then
                        resource.Dispose()
                )
            )

        member inline this.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                [<InlineIfLambda>] generator: unit -> Result<unit option, 'error>
            ) : Result<unit option, 'error> =

            let mutable doContinue = true
            let mutable result = Ok(Some())

            while doContinue
                  && guard () do
                match generator () with
                | Ok option ->
                    match option with
                    | Some _ -> ()
                    | None ->
                        doContinue <- false
                        result <- Ok None
                | Error e ->
                    doContinue <- false
                    result <- Error e

            result

        member inline this.For
            (sequence: #seq<'T>, [<InlineIfLambda>] binder: 'T -> Result<unit option, 'TError>)
            : Result<unit option, 'TError> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum ->
                    this.While(
                        (fun () -> enum.MoveNext()),
                        this.Delay(fun () -> binder enum.Current)
                    )
            )

        member inline _.BindReturn
            (resultOpt: Result<'T option, 'TError>, [<InlineIfLambda>] binder: 'T -> 'U)
            : Result<'U option, 'TError> =
            ResultOption.map binder resultOpt

        member inline _.Source(result: Result<'ok option, 'error>) : Result<'ok option, 'error> =
            result

    let resultOption = ResultOptionBuilder()

[<AutoOpen>]
module ResultOptionCEExtensions =

    type ResultOptionBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<'value>) : #seq<'value> = s

        member inline _.Source(result: Result<'ok, 'error>) : Result<'ok option, 'error> =
            ResultOption.ofResult result

        member inline _.Source(option: 'T option) : Result<'T option, 'error> =
            ResultOption.ofOption option

        member inline _.Source(choice: Choice<'T, 'Error>) : Result<'T option, 'Error> =
            ResultOption.ofChoice choice
