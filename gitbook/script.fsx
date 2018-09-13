#I @"./../src/FsToolkit.ErrorHandling"
#load "Create.fs"
#load "Result.fs"
#load "ResultCE.fs"
#load "Validation.fs"
#load "ValidationOp.fs"

open System
open FsToolkit.ErrorHandling
let add a b = a + b
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error (sprintf "unable to parse '%s' to integer" str)


Result.map2 add (tryParseInt "40") (tryParseInt "2")

type Latitude = private Latitude of double with
  // double
  member this.Value =
    let (Latitude lat) = this
    lat
  // double -> Result<Latitude, string>
  static member TryCreate (lat : double) =
    if lat > -180. && lat < 180. then
      Ok (Latitude lat)
    else
      sprintf "%A is a invalid latitude value" lat |> Error 

type Longitude = private Longitude of double with
  // double
  member this.Value =
    let (Longitude lng) = this
    lng
  // double -> Result<Longitude, string>
  static member TryCreate (lng : double) =
    if lng > -90. && lng < 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 


type Location = {
  Latitude : Latitude
  Longitude : Longitude
}


let location lat lng =
  {Latitude = lat; Longitude = lng}