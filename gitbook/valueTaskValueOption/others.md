# Other ValueTaskValueOption Functions

## defaultValue

Returns the contained value if ValueSome, otherwise returns the provided value

### Function Signature

```fsharp
'a -> ValueTask<'a voption> -> ValueTask<'a>
```

## defaultWith

Returns the contained value if ValueSome, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> ValueTask<'a voption> -> ValueTask<'a>
```

## valueSome

Wraps the provided value in a `ValueTask<'a voption>`

### Function Signature

```fsharp
'a -> ValueTask<'a voption>
```
