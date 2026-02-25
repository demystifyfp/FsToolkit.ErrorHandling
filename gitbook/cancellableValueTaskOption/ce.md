## CancellableValueTaskOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> int option
tryFindPersonById : int -> CancellableValueTask<Person option>
updatePerson : Person -> CancellableValueTask<unit>
```

```fsharp
// CancellableValueTask<unit option>
let addResult = cancellableValueTaskOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId
  let person = { person with Age = age }
  do! updatePerson person
}
```
