## ResultOption.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd) -> Result<'a option, 'e> 
  -> Result<'b option, 'e> -> Result<'c option, 'e>
  -> Result<'d option, 'e>
```

### Example 1

Assume that we have the following function,

```fsharp
// int -> int -> int -> int
let add a b c = a + b + c
```

Then using `ResultOption.map3` function, we can achieve the following

```fsharp
// returns - Ok (Some 42)
ResultOption.map3 add (Ok (Some 30)) (Ok (Some 10)) (Ok (Some 2)) 
```