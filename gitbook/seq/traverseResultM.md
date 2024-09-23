# Seq.traverseResultM

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('a -> Result<'b,'c>) -> 'a seq -> Result<'b seq, 'c>
```

Note that `traverse` is the same as `map >> sequence`. See also [Seq.sequenceResultM](sequenceResultM.md).

This is monadic, stopping on the first error. Compare the example below with [traverseResultA](traverseResultA.md).

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
|> Seq.traverseResultM tryParseInt 
// Ok [1; 2; 3]

["1"; "foo"; "3"; "bar"]
|> Seq.traverseResultM tryParseInt 
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
  
// int seq -> Result<bool, string seq>      
let checkIfAllPrime (numbers : int seq) =
    numbers
    |> Seq.traverseResultM isPrime // Result<bool seq, string>
    |> Result.map (Seq.forall id) // shortened version of '|> Result.map (fun boolSeq -> boolSeq |> Seq.map (fun x -> x = true))';
    
let a = [1; 2; 3; 4; 5;] |> checkIfAllPrime
// Error ["1 must be greater than 1"]

let b = [1; 2; 3; 4; 5; 0;] |> checkIfAllPrime
// Error ["1 must be greater than 1"]

let a = [2; 3; 4; 5;] |> checkIfAllPrime
// Ok false

let a = [2; 3; 5;] |> checkIfAllPrime
// Ok true
```
