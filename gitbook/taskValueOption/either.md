# TaskValueOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the voption. If the voption is `ValueSome`, the first function will be executed. If the voption is `ValueNone`, the second function will be executed.

```fsharp
(onValueSome : 'T -> Task<'output>) 
	-> (onValueNone : Task<'output>) 
	-> (input : Task<'T voption>) 
	-> Task<'output>
```

## Examples

### Example 1

```fsharp
TaskValueOption.either (fun x -> task { x * 2 }) (task { 0 }) (TaskValueOption.valueSome 5)

// task { 10 }
```

### Example 2

```fsharp
TaskValueOption.either (fun x -> task { x * 2 }) (task { 0 }) ValueNone

// task { 0 }
```

