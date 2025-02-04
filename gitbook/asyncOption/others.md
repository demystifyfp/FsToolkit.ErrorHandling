# Other AsyncOption Functions

## defaultValue

Returns the contained value if Some, otherwise returns the provided value

### Function Signature

```fsharp
'a -> Async<'a option> -> Async<'a>
```

## defaultWith

Returns the contained value if Some, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> Async<'a option> -> Async<'a>
```

## some

Wraps the provided value in an Async<value option>

### Function Signature

```fsharp
'a -> Async<'a option>
```


