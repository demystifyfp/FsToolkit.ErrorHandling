namespace FsToolkit.ErrorHandling

/// <summary>
/// Operators for working with the <c>Option</c> type.
/// </summary>
[<RequireQualifiedAccess>]
module Option =

    /// <summary>
    /// Binds a function to an option, applying the function to the value if the option is <c>Some</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/bind</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the value.</param>
    /// <param name="input">The input option.</param>
    /// <typeparam name="'TInput">The input type of the option.</typeparam>
    /// <typeparam name="'TOutput">The output type of the option.</typeparam>
    /// <returns>The result of applying the function to the value, or <c>None</c> if the option is <c>None</c>.</returns>
    let inline bind
        ([<InlineIfLambda>] mapper: 'TInput -> 'TOutput option)
        (input: 'TInput option)
        : 'TOutput option =
        match input with
        | Some v -> mapper v
        | None -> None

    /// <summary>
    /// Applies a mapper function to the value inside an option, returning a new option with the mapped value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the value inside the option.</param>
    /// <param name="input">The input option.</param>
    /// <returns>An option with the mapped value if the input option is <c>Some</c>, otherwise <c>None</c>.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'TInput -> 'TOutput)
        (input: 'TInput option)
        : 'TOutput option =
        match input with
        | Some v -> Some(mapper v)
        | None -> None

    /// <summary>
    /// Applies a mapper function to the values inside two options, returning a new option with the mapped value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map2</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the values inside the options.</param>
    /// <param name="input1">The first input option.</param>
    /// <param name="input2">The second input option.</param>
    /// <returns>An option with the mapped value if both input options are <c>Some</c>, otherwise <c>None</c>.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'TInput1 -> 'TInput2 -> 'TOutput)
        (input1: 'TInput1 option)
        (input2: 'TInput2 option)
        : 'TOutput option =
        match (input1, input2) with
        | Some x, Some y -> Some(mapper x y)
        | _ -> None

    /// <summary>
    /// Applies a mapper function to the values inside three options, returning a new option with the mapped value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map3</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the values inside the options.</param>
    /// <param name="input1">The first input option.</param>
    /// <param name="input2">The second input option.</param>
    /// <param name="input3">The third input option.</param>
    /// <returns>An option with the mapped value if all input options are <c>Some</c>, otherwise <c>None</c>.</returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'TInput1 -> 'TInput2 -> 'TInput3 -> 'TOutput)
        (input1: 'TInput1 option)
        (input2: 'TInput2 option)
        (input3: 'TInput3 option)
        : 'TOutput option =
        match (input1, input2, input3) with
        | Some x, Some y, Some z -> Some(mapper x y z)
        | _ -> None

    /// <summary>
    /// Ignores the value of an option and returns a unit option.
    /// </summary>
    /// <param name="opt">The option to ignore.</param>
    /// <returns>A unit option.</returns>
    let inline ignore<'T> (opt: 'T option) : unit option =
        match opt with
        | Some _ -> Some()
        | None -> None

    /// <summary>
    /// Converts a value option to a regular option.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/transforms/ofvalueoption</href>
    /// </summary>
    /// <param name="vopt">The value option to convert.</param>
    /// <returns>The converted regular option.</returns>
    let inline ofValueOption (vopt: 'value voption) : 'value option =
        match vopt with
        | ValueSome v -> Some v
        | ValueNone -> None

    /// <summary>
    /// Converts an option value to a value option.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/transforms/tovalueoption</href>
    /// </summary>
    /// <param name="opt">The option value to convert.</param>
    /// <returns>A value option.</returns>
    let inline toValueOption (opt: 'value option) : 'value voption =
        match opt with
        | Some v -> ValueSome v
        | None -> ValueNone

    /// <summary>
    /// Traverses an option value and applies a function to its inner value, returning a result.
    /// </summary>
    /// <param name="binder">The function to apply to the inner value of the option.</param>
    /// <param name="input">The option value to traverse.</param>
    /// <returns>A result containing either an option with the transformed value or an error.</returns>
    let inline traverseResult
        ([<InlineIfLambda>] binder: 'input -> Result<'okOutput, 'error>)
        (input: option<'input>)
        : Result<'okOutput option, 'error> =
        match input with
        | None -> Ok None
        | Some v ->
            binder v
            |> Result.map Some

    /// <summary>
    /// Flips around the option and result structures of an option that contains a result.
    /// </summary>
    /// <param name="opt">The option containing a result.</param>
    /// <returns>A result containing an option.</returns>
    let inline sequenceResult (opt: Result<'ok, 'error> option) : Result<'ok option, 'error> =
        traverseResult id opt

#if !FABLE_COMPILER
    /// <summary>
    /// Tries to parse a string value into a specified type using the TryParse method of the type.
    /// </summary>
    /// <typeparam name="^value">The type to parse the string value into, if it has a <c>TryParse</c> function</typeparam>
    /// <param name="valueToParse">The string value to parse.</param>
    /// <returns>An option containing the parsed value if successful, or None if parsing fails.</returns>
    let inline tryParse< ^value
        when ^value: (static member TryParse: string * byref< ^value > -> bool)>
        (valueToParse: string)
        : ^value option =
        let mutable output = Unchecked.defaultof< ^value>

        let parsed =
            (^value: (static member TryParse: string * byref< ^value > -> bool) (valueToParse,
                                                                                 &output))

        match parsed with
        | true -> Some output
        | _ -> None

    /// <summary>
    /// Tries to get the value associated with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="dictionary">The dictionary to search for the key.</param>
    /// <typeparam name="^Dictionary">The type of the dictionary, assuming it has a <c>TryGetValue</c> member accessible</typeparam>
    /// <returns>
    /// An option containing the value associated with the key if it exists in the dictionary,
    /// or None if the key does not exist.
    /// </returns>
    let inline tryGetValue (key: 'key) (dictionary: ^Dictionary) : ^value option =
        let mutable output = Unchecked.defaultof< ^value>

        let parsed =
            (^Dictionary: (member TryGetValue: 'key * byref< ^value > -> bool) (dictionary,
                                                                                key,
                                                                                &output))

        match parsed with
        | true -> Some output
        | false -> None
#endif

    /// <summary>
    /// Takes two options and returns a tuple of the pair or <c>None</c> if either are <c>None</c>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/zip</href>
    /// </summary>
    /// <param name="left">The input option</param>
    /// <param name="right">The input option</param>
    /// <returns></returns>
    let inline zip (left: 'left option) (right: 'right option) : ('left * 'right) option =
        match left, right with
        | Some v1, Some v2 -> Some(v1, v2)
        | _ -> None

    /// <summary>
    /// Converts a <c>Result</c> to an <c>option</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/transforms/ofresult</href>
    /// </summary>
    /// <param name="r">The result to convert.</param>
    /// <returns>An option containing the value if the result is <c>Ok</c>, or <c>None</c> if the result is <c>Error</c></returns>
    let inline ofResult (r: Result<'ok, 'error>) : 'ok option =
        match r with
        | Ok v -> Some v
        | Error _ -> None

    /// <summary>
    /// Convert a potentially null value to an option.
    ///
    /// This is different from <see cref="FSharp.Core.Option.ofObj">Option.ofObj</see> where it doesn't require the value to be constrained to null.
    /// This is beneficial where third party APIs may generate a record type using reflection and it can be null.
    /// See <a href="https://latkin.org/blog/2015/05/18/null-checking-considerations-in-f-its-harder-than-you-think/">Null-checking considerations in F#</a> for more details.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/transforms/ofnull</href>
    /// </summary>
    /// <param name="value">The potentially null value</param>
    /// <returns>An option</returns>
    /// <seealso cref="FSharp.Core.Option.ofObj"/>
    let inline ofNull (value: 'nullableValue) : 'nullableValue option =
        if System.Object.ReferenceEquals(value, null) then
            None
        else
            Some value

    /// <summary>
    ///
    /// <c>bindNull binder option</c> evaluates to <c>match option with None -> None | Some x -> binder x |> Option.ofNull</c>
    ///
    /// Automatically onverts the result of binder that is pontentially null into an option.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/bindnull</href>
    /// </summary>
    /// <param name="binder">A function that takes the value of type 'value from an option and transforms it into
    /// a value of type 'nullableValue.</param>
    /// <param name="option">The input option</param>
    /// <typeparam name="'value"></typeparam>
    /// <typeparam name="'nullableValue"></typeparam>
    /// <returns>An option of the output type of the binder.</returns>
    /// <seealso cref="ofNull"/>
    let inline bindNull
        ([<InlineIfLambda>] binder: 'value -> 'nullableValue)
        (option: Option<'value>)
        : 'nullableValue option =
        match option with
        | Some x ->
            binder x
            |> ofNull
        | None -> None

    /// <summary>
    /// Returns result of running <paramref name="onSome"/> if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/either</href>
    /// </summary>
    /// <param name="onSome">The function to run if <paramref name="input"/> is <c>Some</c></param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c></param>
    /// <param name="input">The input option.</param>
    /// <returns>
    /// The result of running <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.
    /// </returns>
    let inline either
        ([<InlineIfLambda>] onSome: 'a -> 'output)
        ([<InlineIfLambda>] onNone: unit -> 'output)
        (input: 'a option)
        : 'output =
        match input with
        | Some x -> onSome x
        | None -> onNone ()

    /// <summary>
    /// If the option is <c>Some</c>, executes the function on the <c>Some</c> value and passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/teefunctions#teesome</href>
    /// </summary>
    /// <param name="f">The function to execute on the <c>Some</c> value.</param>
    /// <param name="opt">The input option.</param>
    /// <returns>The input option</returns>
    let inline teeSome ([<InlineIfLambda>] f: 'T -> unit) (opt: 'T option) : 'T option =
        match opt with
        | Some x -> f x
        | None -> ()

        opt

    /// <summary>
    /// If the option is <c>None</c>, executes the function and passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/teefunctions#teenone</href>
    /// </summary>
    /// <param name="f">The function to execute if the input is <c>None</c>.</param>
    /// <param name="opt">The input option.</param>
    /// <returns>The input option</returns>
    let inline teeNone (f: unit -> unit) (opt: 'T option) : 'T option =
        match opt with
        | Some _ -> ()
        | None -> f ()

        opt

    /// <summary>
    /// If the result is <c>Some</c> and the predicate returns true, executes the function
    /// on the <c>Some</c> value and passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/teefunctions#teeif</href>
    /// </summary>
    /// <param name="predicate">The predicate to execute on the <c>Some</c> value.</param>
    /// <param name="f">The function to execute on the <c>Some</c> value if the predicate proves true</param>
    /// <param name="opt">The input option.</param>
    /// <returns>The input option</returns>
    let inline teeIf
        ([<InlineIfLambda>] predicate: 'T -> bool)
        ([<InlineIfLambda>] f: 'T -> unit)
        (opt: 'T option)
        : 'T option =
        match opt with
        | Some x ->
            if predicate x then
                f x
        | None -> ()

        opt

#if !FABLE_COMPILER
    open System.Threading.Tasks

    /// <summary>
    /// Converts an <c>Option&lt;Task&lt;_&gt;&gt;</c> to an <c>Task&lt;Option&lt;_&gt;&gt;</c><br />
    ///
    /// Documentation is found here: <see href="https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/sequencetask" />
    /// </summary>
    let inline sequenceTask (optTask: Option<Task<'T>>) : Task<Option<'T>> =
        task {
            match optTask with
            | Some tsk ->
                let! x = tsk
                return Some x
            | None -> return None
        }

    /// <summary>
    /// Maps a <c>Task</c> function over an <c>option</c>, returning a <c>Task&lt;'T option&gt;</c><br/>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/traversetask</href>
    /// </summary>
    /// <param name="f">The function to map over the <c>option</c>.</param>
    /// <param name="opt">The <c>option</c> to map over.</param>
    /// <returns>A <c>Task&lt;'T option&gt;</c> with the mapped value.</returns>
    let inline traverseTask
        ([<InlineIfLambda>] f: 'T -> Task<'T>)
        (opt: Option<'T>)
        : Task<Option<'T>> =
        sequenceTask ((map f) opt)

    /// <summary>
    /// Converts an <c>Task&lt;Result&lt;'ok,'error&gt;&gt; option</c> to an <c>Task&lt;Result&lt;'ok option,'error&gt;&gt;</c><br />
    ///
    /// Documentation is found here: <see href="https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/sequencetaskresult" />
    /// </summary>
    let inline sequenceTaskResult
        (optTaskResult: Task<Result<'T, 'E>> option)
        : Task<Result<'T option, 'E>> =
        task {
            match optTaskResult with
            | Some taskRes ->
                let! xRes = taskRes

                return
                    xRes
                    |> Result.map Some
            | None -> return Ok None
        }

    /// <summary>
    /// Maps a <c>TaskResult</c> function over an <c>option</c>, returning a <c>Task&lt;Result&lt;'U option, 'E&gt;&gt;</c>.<br />
    ///
    /// Documentation is found here: <see href="https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/traversetaskresult" />
    /// </summary>
    /// <param name="f">The function to map over the <c>option</c>.</param>
    /// <param name="opt">The <c>option</c> to map over.</param>
    /// <returns>A <c>Task&lt;Result&lt;'U option, 'E&gt;&gt;</c> with the mapped value.</returns>
    let inline traverseTaskResult
        ([<InlineIfLambda>] f: 'T -> Task<Result<'U, 'E>>)
        (opt: 'T option)
        : Task<Result<'U option, 'E>> =
        sequenceTaskResult ((map f) opt)
#endif

    /// <summary>
    /// Converts a Option<Async<_>> to an Async<Option<_>>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/sequenceasync</href>
    /// </summary>
    let inline sequenceAsync (optAsync: Option<Async<'T>>) : Async<Option<'T>> =
        async {
            match optAsync with
            | Some asnc ->
                let! x = asnc
                return Some x
            | None -> return None
        }

    /// <summary>
    /// Maps an Async function over an Option, returning an Async Option.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/traverseasync</href>
    /// </summary>
    /// <param name="f">The function to map over the Option.</param>
    /// <param name="opt">The Option to map over.</param>
    /// <returns>An Async Option with the mapped value.</returns>
    let inline traverseAsync
        ([<InlineIfLambda>] f: 'T -> Async<'T>)
        (opt: Option<'T>)
        : Async<Option<'T>> =
        sequenceAsync ((map f) opt)

    /// <summary>
    /// Converts a Async<Result<'ok,'error>> option to an Async<Result<'ok option,'error>>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/sequenceasyncresult</href>
    /// </summary>
    let inline sequenceAsyncResult
        (optAsyncResult: Async<Result<'T, 'E>> option)
        : Async<Result<'T option, 'E>> =
        async {
            match optAsyncResult with
            | Some asncRes ->
                let! xRes = asncRes

                return
                    xRes
                    |> Result.map Some
            | None -> return Ok None
        }

    /// <summary>
    /// Maps an AsyncResult function over an option, returning an AsyncResult option.
    ///
    /// /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/traverseasyncresult</href>
    /// </summary>
    /// <param name="f">The function to map over the Option.</param>
    /// <param name="opt">The Option to map over.</param>
    /// <returns>An AsyncResult Option with the mapped value.</returns>
    let inline traverseAsyncResult
        ([<InlineIfLambda>] f: 'T -> Async<Result<'U, 'E>>)
        (opt: 'T option)
        : Async<Result<'U option, 'E>> =
        sequenceAsyncResult ((map f) opt)

    /// <summary>
    /// Creates an option from a boolean value and a value of type 'a.
    /// If the boolean value is true, returns <c>Some</c> value.
    /// If the boolean value is false, returns <c>None</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/transforms/ofpair</href>
    /// </summary>
    /// <param name="input">A tuple containing a boolean value and a value of type 'a.</param>
    /// <returns>An option value.</returns>
    let inline ofPair (input: bool * 'a) =
        match input with
        | true, x -> Some x
        | false, _ -> None
