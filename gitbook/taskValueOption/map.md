# TaskValueOption.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of a task voption if it is `ValueSome`. If the option is `ValueNone`, return `ValueNone`.

## Function Signature

```fsharp
('input -> 'output) -> Task<'input voption> -> Task<'output voption>
```

## Examples

### Example 1

```fsharp
TaskValueOption.map (fun x -> x + 1) (TaskValueOption.valueSome 1)

// task { ValueSome 2 }
```

### Example 2

```fsharp
TaskValueOption.map (fun x -> x + 1) (Task.singleton ValueNone)

// task { ValueNone }
```

