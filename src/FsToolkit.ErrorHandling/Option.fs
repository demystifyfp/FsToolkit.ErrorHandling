namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Option =

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
#endif

    /// <summary>
    /// Takes two options and returns a tuple of the pair or none if either are none
    /// </summary>
    /// <param name="option1">The input option</param>
    /// <param name="option2">The input option</param>
    /// <returns></returns>
    let zip (option1: option<'a>) (option2: option<'b>) =
        match option1, option2 with
        | Some v1, Some v2 -> Some(v1, v2)
        | _ -> None

    let ofResult =
        function
        | Ok v -> Some v
        | Error _ -> None
