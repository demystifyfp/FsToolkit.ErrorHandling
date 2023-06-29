## CancellableTaskValidation Computation Expression

Namespace: `FsToolkit.ErrorHandling`

The `CancellableTaskValidation` type is defined as:

```fsharp
type CancellableTaskValidation<'a,'err> = CancellableTask<Result<'a, 'err list>>
```

This CE can take advantage of the [and! operator](https://github.com/fsharp/fslang-suggestions/issues/579) to join multiple error results into a list.

## Examples

See [here](../validation/ce.md) for other validation-like examples

```fsharp
// Result<string, string> -> CancellableTask<Result<string, string>>
let downloadCancellableTask stuff = cancellableTask {
    return stuff
}

// CancellableTaskValidation<string, string>
let result = cancellableTaskValidation {
  let! x = downloadCancellableTask (Ok "I")
  and! y = downloadCancellableTask (Ok "am")
  and! z = downloadCancellableTask (Ok "cancellableTask!")
  return sprintf "%s %s %s" x y z
}
// cancellableTask { return Ok "I am cancellableTask!" }

// CancellableTaskValidation<string, string>
let result = cancellableTaskValidation {
  let! x = downloadCancellableTask (Error "Am")
  and! y = downloadCancellableTask (Error "I")
  and! z = downloadCancellableTask (Error "cancellableTask?")
  return sprintf "%s %s %s" x y z
}

// cancellableTask { return Error [ "Am"; "I"; "cancellableTask?" ] }
```
