#load "Result.fs"
open FsToolkit.ErrorHandling


let toR x =
  match x with
  | 1 -> Ok 1
  | 2 -> Ok 2
  | x -> Error x

Result.traverseListM toR [3;1;2;1]

