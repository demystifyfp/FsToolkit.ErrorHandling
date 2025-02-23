## Task Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

Given a personId and an age, find a person and update their age.

```fsharp
parseInt : string -> int
findPersonById : int -> Task<Person>
updatePerson : Person -> Task<unit>
```

```fsharp
// Task<unit>
let addResult = task {
  let personId = parseInt "3001"
  let age = parseInt "35"
  let! person = findPersonById personId
  let person = { person with Age = age }
  do! updatePerson person
}
```