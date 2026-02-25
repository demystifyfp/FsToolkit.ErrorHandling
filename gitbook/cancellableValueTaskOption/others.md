# Other CancellableValueTaskOption Functions

## defaultValue

Returns the contained value if Some, otherwise returns the provided value.

### Function Signature

```fsharp
'a -> CancellableValueTask<'a option> -> CancellableValueTask<'a>
```

## defaultWith

Returns the contained value if Some, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> CancellableValueTask<'a option> -> CancellableValueTask<'a>
```

## some

Wraps the provided value in a `CancellableValueTask<'a option>`.

### Function Signature

```fsharp
'a -> CancellableValueTask<'a option>
```
