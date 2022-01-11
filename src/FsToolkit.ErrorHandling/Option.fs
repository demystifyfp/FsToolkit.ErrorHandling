namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Option =

    let inline ofValueOption (vopt: _ voption) =
        match vopt with
        | ValueSome v -> Some v
        | ValueNone -> None

    let inline toValueOption (opt: _ option) =
        match opt with
        | Some v -> ValueSome v
        | None -> ValueNone

    let inline traverseResult f opt =
        match opt with
        | None -> Ok None
        | Some v -> f v |> Result.map Some

    let inline sequenceResult opt = traverseResult id opt

#if !FABLE_COMPILER
    let inline tryParse< ^T when ^T: (static member TryParse : string * byref< ^T > -> bool) and ^T: (new : unit -> ^T)>
        valueToParse
        =
        let mutable output = new ^T()

        let parsed =
            (^T: (static member TryParse : string * byref< ^T > -> bool) (valueToParse, &output))

        match parsed with
        | true -> Some output
        | _ -> None

    let inline tryGetValue key dictionary =
        let mutable output = Unchecked.defaultof< ^Value>

        let parsed =
            (^Dictionary: (member TryGetValue : string * byref< ^Value > -> bool) (dictionary, key, &output))

        match parsed with
        | true -> Some output
        | false -> None
#endif

    /// <summary>
    /// Takes two options and returns a tuple of the pair or none if either are none
    /// </summary>
    /// <param name="option1">The input option</param>
    /// <param name="option2">The input option</param>
    /// <returns></returns>
    let inline zip (option1: 'a option) (option2: 'b option) =
        match option1, option2 with
        | Some v1, Some v2 -> Some(v1, v2)
        | _ -> None


    let inline ofResult (r: Result<_, _>) =
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
    let inline ofNull (value: 'nullableValue) =
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
    let inline bindNull (binder: 'value -> 'nullableValue) (option: Option<'value>) =
        match option with
        | Some x -> binder x |> ofNull
        | None -> None
