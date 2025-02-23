# Task.map3

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('a -> 'b -> 'c -> 'd) 
    -> Task<'a> 
    -> Task<'b>
    -> Task<'c> 
    -> Task<'d>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `task` computation expression](../task/ce.md).

### Example 1

Let's assume that we have an `add` function that adds three numbers:

```fsharp
// int -> int -> int -> int
let add a b c = a + b + c
```

And an another function that asynchronously retrieves an integer, say a person's age:

```fsharp
type AccountId = int

// AccountId -> Task<int>
let getAge accountId =
    task {
        let ages = [
            (1, 19);
            (2, 21);
            (3, 34);
            (4, 47);
            (5, 55);
        ]
        return ages |> Map.ofList |> Map.find accountId
    }
```

With the help of `Result.map3` function, we can now do the following:

```fsharp
let summedAges =
  Task.map3 add (getAge 1) (getAge 3) (getAge 5)
  // task { 108 }
```
