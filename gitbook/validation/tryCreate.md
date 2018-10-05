## Result.tryCreate 

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
string -> 'a -> Result<^b, (string * 'c)>
```

`^b` is a [statically resolved parameter](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/generics/statically-resolved-type-parameters) with the below constraint

```fsharp
^b : (static member TryCreate : 'a -> Result< ^b, 'c>)
```
