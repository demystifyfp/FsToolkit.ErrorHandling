## TaskOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> Option<int>
tryFindPersonById : int -> Task<Option<Person>>
updatePerson : Person -> Task<unit>
```

```fsharp
// Task<Option<unit>>
let addResult = taskOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId "US-OH"
  let person = { person with Age = age }
  do! updatePerson person
}
```