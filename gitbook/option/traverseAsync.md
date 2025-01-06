## Option.traverseAsync

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Async<'b>) -> 'a option -> Async<'b option>
```

Note that `traverse` is the same as `map >> sequence`. See also [Option.sequenceAsync](sequenceAsync.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

Let's assume we have a type `Customer`:

```fsharp
type Customer = {
  Id : int
  Email : string
}
```

And we have a function called `getCustomerByEmail` that retrieves a `Customer` by email address asynchronously from some external source -- a database, a web service, etc:

```fsharp
// string -> Async<Customer>
let getCustomerByEmail email : Async<Customer> = async {
    return { Id = 1; Email = "test@test.com" } // return a constant for simplicity
}
```

If we have a value of type `string option` and want to call the `getCustomerByEmail` function, we can achieve it using the `traverseAsync` function as below:

```fsharp
Some "test@test.com" |> Option.traverseAsync getCustomerByEmail
// async { return Some { Id = 1; Email = "test@test.com" } }

None |> Option.traverseAsync getCustomerByEmail
// async { return None }
```
