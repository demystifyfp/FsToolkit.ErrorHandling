## JobOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Job<'b option>) -> Job<'a option> -> Job<'b option>
```

## Examples

Note: Many use-cases requiring `bind` can also be solved using [the `jobOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
tryFindPersonById : int -> Job<Person option>
tryFindAddressById : int -> Job<Address option>
```

We can look up a person's address like this:

```fsharp
// Job<Address option>
tryFindPersonById 42
|> JobOption.bind (fun person -> tryFindAddressById person.AddressId)
```

### Example 2

```fsharp
tryParseInt : string -> int option
tryFindPersonById : int -> Job<Person option>

// Job<Person option>
job { return tryParseInt "3001" }
|> JobOption.bind tryFindPersonById
```
