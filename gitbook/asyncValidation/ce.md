## AsyncValidation Computation Expression

Namespace: `FsToolkit.ErrorHandling`

The `AsyncValidation` type is defined as:

```fsharp
type AsyncValidation<'a,'err> = Async<Result<'a, 'err list>>
```

This CE can take advantage of the [and! operator](https://github.com/fsharp/fslang-suggestions/issues/579) to join multiple error results into a list.

## Examples

See [here](../validation/ce.md) for other validation-like examples

```fsharp
// Result<string, string> -> Async<Result<string, string>>
let downloadAsync stuff = async {
    return stuff
}

// AsyncValidation<string, string>
let addResult = asyncValidation {
  let! x = downloadAsync (Ok "I")
  and! y = downloadAsync (Ok "am")
  and! z = downloadAsync (Ok "async!")
  return sprintf "%s %s %s" x y z
}
// async { return Ok "I am async!" }

// AsyncValidation<string, string>
let addResult = asyncValidation {
  let! x = downloadAsync (Error "Am")
  and! y = downloadAsync (Error "I")
  and! z = downloadAsync (Error "async?")
  return sprintf "%s %s %s" x y z
}

// async { return Error [ "Am"; "I"; "async?" ] }
```
