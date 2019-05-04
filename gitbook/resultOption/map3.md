## ResultOption.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Result<'a option, 'e>
  -> Result<'b option, 'e>
  -> Result<'c option, 'e>
  -> Result<'d option, 'e>
```

### Example 1

Given the following function:

```fsharp
add : int -> int -> int -> int
```

Then using `ResultOption.map3`, we can do the following:

```fsharp
ResultOption.map3 add (Ok (Some 30)) (Ok (Some 10)) (Ok (Some 2)) 
// Ok (Some 42)
```

