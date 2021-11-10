namespace FsToolkit.ErrorHandling

#if !FABLE_COMPILER
[<RequireQualifiedAccess>]
module ValueOption =

    let ofOption (opt: 'a option) =
        match opt with
        | Some v -> ValueSome v
        | None -> ValueNone

    let toOption (vopt: 'a voption) =
        match vopt with
        | ValueSome v -> Some v
        | ValueNone -> None

    let traverseResult f vopt =
        match vopt with
        | ValueNone -> Ok ValueNone
        | ValueSome v -> f v |> Result.map ValueSome

    let sequenceResult opt = traverseResult id opt

    let inline tryParse< ^T when ^T: (static member TryParse : string * byref< ^T > -> bool) and ^T: (new : unit -> ^T)>
        valueToParse
        =
        let mutable output = new ^T()

        let parsed =
            (^T: (static member TryParse : string * byref< ^T > -> bool) (valueToParse, &output))

        match parsed with
        | true -> ValueSome output
        | _ -> ValueNone

    /// <summary>
    /// Takes two voptions and returns a tuple of the pair or none if either are none
    /// </summary>
    /// <param name="voption1">The input option</param>
    /// <param name="voption2">The input option</param>
    /// <returns></returns>
    let zip (voption1: 'a voption) (voption2: 'b voption) =
        match voption1, voption2 with
        | ValueSome v1, ValueSome v2 -> ValueSome(v1, v2)
        | _ -> ValueNone


    let ofResult =
        function
        | Ok v -> ValueSome v
        | Error _ -> ValueNone

#endif
