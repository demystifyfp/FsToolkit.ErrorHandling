## AsyncOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> int option
tryFindPersonById : int -> Async<Person option>
updatePerson : Person -> Async<unit>
```

```fsharp
// Async<unit option>
let addResult = asyncOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId "US-OH"
  let person = { person with Age = age }
  do! updatePerson person
}
```