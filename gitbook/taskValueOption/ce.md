## TaskValueOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> int voption
tryFindPersonById : int -> Task<Person voption>
updatePerson : Person -> Task<unit>
```

```fsharp
// Task<unit voption>
let addResult = taskValueOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId
  let person = { person with Age = age }
  do! updatePerson person
}
```

### Example 2 - IAsyncEnumerable

The `taskValueOption` CE supports `for .. in ..` iteration over `IAsyncEnumerable<'T>` sequences. Iteration stops immediately when the body returns `ValueNone`, without consuming further elements.

```fsharp
tryProcessItem : Item -> Task<unit voption>
getItemsAsync : unit -> IAsyncEnumerable<Item>
```

```fsharp
// Task<string voption>
let processItems () =
  taskValueOption {
    for item in getItemsAsync () do
      do! tryProcessItem item
    return "done"
  }
// Returns ValueNone and stops iteration on the first ValueNone result
```

The `backgroundTaskValueOption` CE inherits this support automatically.
