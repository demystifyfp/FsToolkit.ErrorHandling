## JobOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Job<'a option> -> Job<'b option>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `jobOption` computation expression](ce.md).

### Example 1

Given the function

```fsharp
tryFindPersonById : int -> Job<Person option>
```

We can get the name of the person like this:

```fsharp
// Job<string option>
tryFindPersonById 42
|> JobOption.map (fun person -> person.Name)
```

### Example 2

```fsharp
let maybeValue : Job<int option> = Job.singleton (Some 10)

maybeValue
|> JobOption.map (fun x -> x * 2)
// job { return Some 20 }
```
