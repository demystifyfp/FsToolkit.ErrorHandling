# TaskOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'T -> Task<'output>) 
	-> (onNone : Task<'output>) 
	-> (input : Task<'T option>) 
	-> Task<'output>
```

## Examples

### Example 1

```fsharp
TaskOption.either (fun x -> task { x * 2 }) (task { 0 }) (TaskOption.some 5)

// task { 10 }
```

### Example 2

```fsharp
TaskOption.either (fun x -> x * 2) (task { 0 }) None

// task { 0 }
```

