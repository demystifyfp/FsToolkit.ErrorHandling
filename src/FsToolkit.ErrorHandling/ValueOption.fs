namespace FsToolkit.ErrorHandling

#if !FABLE_COMPILER
[<RequireQualifiedAccess>]
module ValueOption =

    let inline ofOption (opt: 'a option) =
        match opt with
        | Some v -> ValueSome v
        | None -> ValueNone

    let inline toOption (vopt: 'a voption) =
        match vopt with
        | ValueSome v -> Some v
        | ValueNone -> None

    let inline traverseResult f vopt =
        match vopt with
        | ValueNone -> Ok ValueNone
        | ValueSome v -> f v |> Result.map ValueSome

    let inline sequenceResult opt = traverseResult id opt

    let inline tryParse< ^T when ^T: (static member TryParse : string * byref< ^T > -> bool) and ^T: (new : unit -> ^T)>
        valueToParse
        =
        let mutable output = new ^T()

        let parsed =
            (^T: (static member TryParse : string * byref< ^T > -> bool) (valueToParse, &output))

        match parsed with
        | true -> ValueSome output
        | _ -> ValueNone

    let inline tryGetValue key dictionary =
        let mutable output = Unchecked.defaultof< ^Value>

        let parsed =
            (^Dictionary: (member TryGetValue : string * byref< ^Value > -> bool) (dictionary, key, &output))

        match parsed with
        | true -> ValueSome output
        | false -> ValueNone

    /// <summary>
    /// Takes two voptions and returns a tuple of the pair or none if either are none
    /// </summary>
    /// <param name="voption1">The input option</param>
    /// <param name="voption2">The input option</param>
    /// <returns></returns>
    let inline zip (voption1: 'a voption) (voption2: 'b voption) =
        match voption1, voption2 with
        | ValueSome v1, ValueSome v2 -> ValueSome(v1, v2)
        | _ -> ValueNone


    let inline ofResult (result: Result<_, _>) =
        match result with
        | Ok v -> ValueSome v
        | Error _ -> ValueNone


    /// <summary>
    /// Convert a potentially null value to an ValueOption.
    ///
    /// This is different from <see cref="FSharp.Core.ValueOption.ofObj">ValueOption.ofObj</see> where it doesn't require the value to be constrained to null.
    /// This is beneficial where third party APIs may generate a record type using reflection and it can be null.
    /// See <a href="https://latkin.org/blog/2015/05/18/null-checking-considerations-in-f-its-harder-than-you-think/">Null-checking considerations in F#</a> for more details.
    /// </summary>
    /// <param name="value">The potentially null value</param>
    /// <returns>An ValueOption</returns>
    /// <seealso cref="FSharp.Core.ValueOption.ofObj"/>
    let inline ofNull (value: 'nullableValue) =
        if System.Object.ReferenceEquals(value, null) then
            ValueNone
        else
            ValueSome value


    /// <summary>
    ///
    /// <c>bindNull binder voption</c> evaluates to <c>match voption with ValueNone -> ValueNone | ValueSome x -> binder x |> ValueOption.ofNull</c>
    ///
    /// Automatically onverts the result of binder that is pontentially null into an Valueoption.
    /// </summary>
    /// <param name="binder">A function that takes the value of type 'value from an voption and transforms it into
    /// a value of type 'nullableValue.</param>
    /// <param name="voption">The input voption</param>
    /// <typeparam name="'value"></typeparam>
    /// <typeparam name="'nullableValue"></typeparam>
    /// <returns>A voption of the output type of the binder.</returns>
    /// <seealso cref="ofNull"/>
    let inline bindNull (binder: 'value -> 'nullableValue) (voption: ValueOption<'value>) =
        match voption with
        | ValueSome x -> binder x |> ofNull
        | ValueNone -> ValueNone

#endif
