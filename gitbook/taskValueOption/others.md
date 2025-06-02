# Other TaskValueOption Functions

## defaultValue

Returns the contained value if ValueSome, otherwise returns the provided value

### Function Signature

```fsharp
'a -> Task<'a voption> -> Task<'a>
```

## defaultWith

Returns the contained value if ValueSome, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> Task<'a voption> -> Task<'a>
```

## valueSome

Wraps the provided value in an Task<'a voption>

### Function Signature

```fsharp
'a -> Task<'a voption>
```


