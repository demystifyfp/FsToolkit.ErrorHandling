[<RequireQualifiedAccess>]
module FsToolkit.ErrorHandling.Seq

let sequenceResultM (xs: seq<Result<'t, 'e>>) : Result<'t[], 'e> =
    if isNull xs then
        nullArg (nameof xs)

    let acc = ResizeArray()
    let mutable err = Unchecked.defaultof<_>
    let mutable ok = true
    use e = xs.GetEnumerator()

    while ok
          && e.MoveNext() do
        match e.Current with
        | Ok r -> acc.Add r
        | Error e ->
            ok <- false
            err <- e

    if ok then Ok(acc.ToArray()) else Error err

let sequenceResultA (xs: seq<Result<'t, 'e>>) : Result<'t[], 'e[]> =
    if isNull xs then
        nullArg (nameof xs)

    let oks = ResizeArray()
    let errs = ResizeArray()

    for x in xs do
        match x with
        | Ok r -> oks.Add r
        | Error e -> errs.Add e

    match errs.ToArray() with
    | [||] -> Ok(oks.ToArray())
    | errs -> Error errs
