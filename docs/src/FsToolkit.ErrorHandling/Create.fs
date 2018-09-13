namespace FsToolkit.ErrorHandling

module Create =
  let inline tryCreate (x : ^a) : Result< ^b,'c> =
    let tryCreate' x = (^b : (static member TryCreate : ^a -> Result< ^b, 'c>) x)
    tryCreate' x

  let inline tryCreate2 fieldName (x : ^a) : Result< ^b, (string * 'c)> =
    let tryCreate' x = (^b : (static member TryCreate : ^a -> Result< ^b, 'c>) x)
    tryCreate' x
    |> Result.mapError (fun z -> (fieldName, z))