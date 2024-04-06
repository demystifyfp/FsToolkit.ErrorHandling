# Seq.sequenceResultA

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
seq<Result<'a, 'b>> -> Result<'a[], 'b[]>
```

This is applicative, collecting all errors. Compare the example below with [sequenceResultM](sequenceResultM.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error $"unable to parse '{str}' to integer"

["1"; "2"; "3"]
|> Seq.map tryParseInt
|> Seq.sequenceResultA
// Ok [| 1; 2; 3 |]

["1"; "foo"; "3"; "bar"]
|> Seq.map tryParseInt
|> Seq.sequenceResultA
// Error [| "unable to parse 'foo' to integer" 
//          "unable to parse 'bar' to integer" |]
```

### Example 2

```fsharp
// int -> Result<bool, string>
let isPrime (x: int) =
    if x < 2 then Error $"{x} must be greater than 1"
    elif x = 2 then Ok true
    else
        let rec isPrime' (x : int) (i : int) =
            if i * i > x then Ok true
            elif x % i = 0 then Ok false
            else isPrime' x (i + 1)
        isPrime' x 2
  
// seq<int> -> Result<bool, string[]>      
let checkIfAllPrime (numbers: seq<int>) =
    seq { for x in numbers -> isPrime x } // Result<bool, string> seq
    |> Seq.sequenceResultA // Result<bool[], string[]>
    |> Result.map (Seq.forall id) // shortened version of '|> Result.map (fun results -> results |> Array.forall (fun x -> x = true))'
    
let a = [| 1; 2; 3; 4; 5 |] |> checkIfAllPrime
// Error [| "1 must be greater than 1" |]

let b = [ 1; 2; 3; 4; 5; 0 ] |> checkIfAllPrime
// Error [| "1 must be greater than 1"; "0 must be greater than 1" |]

let a = seq { 2; 3; 4; 5 } |> checkIfAllPrime
// Ok false

let a = seq { 2; 3; 5 } |> checkIfAllPrime
// Ok true
```
