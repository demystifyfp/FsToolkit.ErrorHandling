# OrElse Functions

## AsyncOption.orElse

Namespace: `FsToolkit.ErrorHandling`

Returns the option if the option is Some, otherwise returns the given option

### Function Signature

```fsharp
(ifNone : Option<'value>) -> (input : Option<'value>) 
    -> Option<'value>
```

### Examples

#### Example 1

```fsharp
let asyncOption : AsyncOption<int> =
    AsyncOption.some 1
    |> AsyncOption.orElse (AsyncOption.some 2)
    
// async { Some 1 }
```

#### Example 2

```fsharp
let asyncOption : AsyncOption<int> =
    AsyncOption.some 1
    |> AsyncOption.orElse (Async.singleton None)
    
// async { Some 1 }
```

#### Example 3

```fsharp
let asyncOption : AsyncOption<int> =
    Async.singleton None
    |> AsyncOption.orElse (Some 2)
    
// async { Some 2 }
```

#### Example 4

```fsharp
let asyncOption : AsyncOption<int> =
    Async.singleton None
    |> AsyncOption.orElse (Async.singleton None)

// async { None }
```

## AsyncOption.orElseWith

Namespace: `FsToolkit.ErrorHandling`

Returns the option if the option is Some, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(ifNoneFunc : unit -> AsyncOption<'value>) -> (input : AsyncOption<'value>)
    -> AsyncOption<'value>
```

### Examples

#### Example 1

```fsharp
let asyncOption : AsyncOption<int> =
    AsyncOption.some 1
    |> AsyncOption.orElseWith (fun () -> AsyncOption.some 2)

// async { Some 1 }
```

#### Example 2

```fsharp
let asyncOption : AsyncOption<int> =
    AsyncOption.some 1
    |> AsyncOption.orElseWith (fun () -> None)

// async { Some 1 }
```

#### Example 3

```fsharp
let asyncOption : AsyncOption<int> =
    Async.singleton None
    |> AsyncOption.orElseWith (fun () -> AsyncOption.some 2)

// async { Some 2 }
```

#### Example 4

```fsharp
let asyncOption : AsyncOption<int> =
    Async.singleton None
    |> AsyncOption.orElseWith (fun () -> Async.singleton None)

// async { None }
```
