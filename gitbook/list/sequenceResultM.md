# List.sequenceResultM

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Result<'a, 'b> list -> Result<'a list, 'b>
```

Note that `sequence` is the same as `traverse id`. See also [List.traverseResultM](traverseResultM.md).

This is monadic, stopping on the first error. Compare the example below with [sequenceResultA](sequenceResultA.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> 
    Error (sprintf "unable to parse '%s' to integer" str)

["1"; "2"; "3"]
|> List.map tryParseInt
|> List.sequenceResultM 
// Ok [1; 2; 3]

["1"; "foo"; "3"; "bar"]
|> List.map tryParseInt
|> List.sequenceResultM  
// Error "unable to parse 'foo' to integer"
```

### Example 2

```fsharp
// int -> Result<bool, string>
let isPrime (x : int) =
    if x < 2 then 
        sprintf "%i must be greater than 1" x |> Error
    elif 
        x = 2 then Ok true
    else
        let rec isPrime' (x : int) (i : int) =
            if i * i > x then Ok true
            elif x % i = 0 then Ok false
            else isPrime' x (i + 1)
        isPrime' x 2
  
// int list -> Result<bool, string list>      
let checkIfAllPrime (numbers : int list) =
    numbers
    |> List.map isPrime // Result<bool, string> list
    |> List.sequenceResultM // Result<bool list, string>
    |> Result.map (List.forall id) // shortened version of '|> Result.map (fun boolList -> boolList |> List.map (fun x -> x = true))';
    
let a = [1; 2; 3; 4; 5;] |> checkIfAllPrime
// Error ["1 must be greater than 1"]

let b = [1; 2; 3; 4; 5; 0;] |> checkIfAllPrime
// Error ["1 must be greater than 1"]

let a = [2; 3; 4; 5;] |> checkIfAllPrime
// Ok false

let a = [2; 3; 5;] |> checkIfAllPrime
// Ok true
```
