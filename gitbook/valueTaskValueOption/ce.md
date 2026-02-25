## ValueTaskValueOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
tryParseInt : string -> int voption
tryFindPersonById : int -> ValueTask<Person voption>
updatePerson : Person -> ValueTask<unit>
```

```fsharp
// ValueTask<unit voption>
let addResult = valueTaskValueOption {
  let! personId = tryParseInt "3001"
  let! age = tryParseInt "35"
  let! person = tryFindPersonById personId
  let person = { person with Age = age }
  do! updatePerson person
}
```
