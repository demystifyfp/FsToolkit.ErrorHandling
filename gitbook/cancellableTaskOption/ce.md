## CancellableTaskOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> Option<int>
tryFindPersonById : int -> CancellableTask<Person option>
updatePerson : Person -> CancellableTask<unit>
```

```fsharp
// CancellableTask<unit option>
let addResult = cancellableTaskOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId
  let person = { person with Age = age }
  do! updatePerson person
}
```
