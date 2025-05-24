# Other CancellableTaskOption Functions

## defaultValue

Returns the contained value if Some, otherwise returns the provided value

### Function Signature

```fsharp
'a -> CancellableTask<'a option> -> CancellableTask<'a>
```

## defaultWith

Returns the contained value if Some, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> CancellableTask<'a option> -> CancellableTask<'a>
```

## some

Wraps the provided value in an CancellableTask<'a option>

### Function Signature

```fsharp
'a -> CancellableTask<'a option>
```


