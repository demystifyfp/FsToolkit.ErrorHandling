## TaskValidation Computation Expression

Namespace: `FsToolkit.ErrorHandling`

The `TaskValidation` type is defined as:

```fsharp
type TaskValidation<'a,'err> = Task<Result<'a, 'err list>>
```

This CE can take advantage of the [and! operator](https://github.com/fsharp/fslang-suggestions/issues/579) to join multiple error results into a list.

## Examples

See [here](../validation/ce.md) for other validation-like examples

```fsharp
// Result<string, string> -> Task<Result<string, string>>
let downloadTask stuff = task {
    return stuff
}

// TaskValidation<string, string>
let addResult = taskValidation {
  let! x = downloadTask (Ok "I")
  and! y = downloadTask (Ok "am")
  and! z = downloadTask (Ok "async!")
  return sprintf "%s %s %s" x y z
}
// task { return Ok "I am async!" }

// TaskValidation<string, string>
let addResult = taskValidation {
  let! x = downloadTask (Error "Am")
  and! y = downloadTask (Error "I")
  and! z = downloadTask (Error "async?")
  return sprintf "%s %s %s" x y z
}

// task { return Error [ "Am"; "I"; "async?" ] }
```

### IAsyncEnumerable

The `taskValidation` CE supports `for .. in ..` iteration over `IAsyncEnumerable<'T>` sequences. Iteration stops immediately when the body returns an `Error`, without consuming further elements.

```fsharp
validate : Item -> Task<Result<ValidItem, string>>
save : ValidItem -> Task<Result<unit, string>>
getItemsAsync : unit -> IAsyncEnumerable<Item>
```

```fsharp
// Task<Result<string, string list>>
let processItems () =
  taskValidation {
    for item in getItemsAsync () do
      let! validated = validate item
      do! save validated
    return "done"
  }
// Stops iteration immediately on the first Error
```

The `backgroundTaskValidation` CE inherits this support automatically.
