## TaskOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> int option
tryFindPersonById : int -> Task<Person option>
updatePerson : Person -> Task<unit>
```

```fsharp
// Task<unit option>
let addResult = taskOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId
  let person = { person with Age = age }
  do! updatePerson person
}
```

### Example 2 - IAsyncEnumerable

The `taskOption` CE supports `for .. in ..` iteration over `IAsyncEnumerable<'T>` sequences. Iteration stops immediately when the body returns `None`, without consuming further elements.

```fsharp
tryProcessItem : Item -> Task<unit option>
getItemsAsync : unit -> IAsyncEnumerable<Item>
```

```fsharp
// Task<string option>
let processItems () =
  taskOption {
    for item in getItemsAsync () do
      do! tryProcessItem item
    return "done"
  }
// Returns None and stops iteration on the first None result
```

The `backgroundTaskOption` CE inherits this support automatically.
