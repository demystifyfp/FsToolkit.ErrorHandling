namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Option =

    let ofValueOption (vopt: _ voption) =
        match vopt with
        | ValueSome v -> Some v
        | ValueNone -> None

    let toValueOption (opt: _ option) =
        match opt with
        | Some v -> ValueSome v
        | None -> ValueNone

    let traverseResult f opt =
        match opt with
        | None -> Ok None
        | Some v -> f v |> Result.map Some

    let sequenceResult opt = traverseResult id opt

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
    let zip (option1: 'a option) (option2: 'b option) =
        match option1, option2 with
        | Some v1, Some v2 -> Some(v1, v2)
        | _ -> None


    let ofResult =
        function
        | Ok v -> Some v
        | Error _ -> None
