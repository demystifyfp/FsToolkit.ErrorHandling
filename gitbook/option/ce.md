## Option Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

```fsharp
// Option<int>
let addResult = option {
  let! x = tryParseInt "35"
  let! y = tryParseInt "5"
  let! z = tryParseInt "2"
  return add x y z
}
```

### Example 2

Example taken from [here](https://github.com/lukaszkrzywizna/learn-fsharp/blob/feature/9-finito/LearnFsharp/Lesson.fs#L436-L456)

```fsharp
// int -> int -> int option
let tryDivide x y =
    match y with
    | 0 -> None
    | _ -> Some (x / y)

// int -> int -> int option
let multiplyIfEven x y =
    match y % 2 with
    | 0 -> Some <| x * y
    | _ -> None
 
// int option  
let resultNone = option {        
    let! result = tryDivide 5 3
    let mapped = result * 5
    return! multiplyIfEven 5 mapped
}
// result: None

// int option  
let resultSome = option {        
    let! result = tryDivide 6 3
    let mapped = result * 5
    return! multiplyIfEven 5 mapped
}
// result: Some 50

```
