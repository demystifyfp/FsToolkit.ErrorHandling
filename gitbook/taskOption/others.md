# Other TaskOption Functions

## defaultValue

Returns the contained value if Some, otherwise returns the provided value

### Function Signature

```fsharp
'a -> Task<'a option> -> Task<'a>
```

## defaultWith

Returns the contained value if Some, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> Task<'a option> -> Task<'a>
```

## some

Wraps the provided value in an Task<'a option>

### Function Signature

```fsharp
'a -> Task<'a option>
```


