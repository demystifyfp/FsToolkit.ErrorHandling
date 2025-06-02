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
