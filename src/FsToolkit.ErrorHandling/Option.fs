namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Option =

    let inline bind ([<InlineIfLambdaAttribute>] f) x =
        match x with
        | Some v -> f v
        | None -> None

    let inline map ([<InlineIfLambdaAttribute>] f) x =
        match x with
        | Some v -> Some(f v)
        | None -> None

    let inline ofValueOption (vopt: 'value voption) : 'value option =
        match vopt with
        | ValueSome v -> Some v
        | ValueNone -> None

    let inline toValueOption (opt: 'value option) : 'value voption =
        match opt with
        | Some v -> ValueSome v
        | None -> ValueNone

    let inline traverseResult
        ([<InlineIfLambda>] binder: 'input -> Result<'okOutput, 'error>)
        (input: option<'input>)
        : Result<'okOutput option, 'error> =
        match input with
        | None -> Ok None
        | Some v ->
            binder v
            |> Result.map Some

    let inline sequenceResult (opt: Result<'ok, 'error> option) : Result<'ok option, 'error> =
        traverseResult id opt

#if !FABLE_COMPILER
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
    /// Takes two options and returns a tuple of the pair or none if either are none
    /// </summary>
    /// <param name="left">The input option</param>
    /// <param name="right">The input option</param>
    /// <returns></returns>
    let inline zip (left: 'left option) (right: 'right option) : ('left * 'right) option =
        match left, right with
        | Some v1, Some v2 -> Some(v1, v2)
        | _ -> None


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

    let inline ofPair (input: bool * 'a) =
        match input with
        | true, x -> Some x
        | false, _ -> None
