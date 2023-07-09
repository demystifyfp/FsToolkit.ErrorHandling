# Bind Mappings

This pages shows you which computation expressions can implicitly bind to other CEs

## Example

Here we can bind an 'T Option to 'T AsyncOption without having to lift from one type to another. See [here](https://fsharpforfunandprofit.com/posts/elevated-world/) for a better understanding on 'lifting'. To associate with the table below, the AsyncOption is the CE and the Option is the CE that can bind to it.

```fsharp
asyncOption {
    let! x = Some 1 
    return x
}
```

### Note

Each CE can bind to itself so we don't list that here to reduce clutter

| CE                        | Can Bind                                                                                                                                   |
| :------------------------ | :----------------------------------------------------------------------------------------------------------------------------------------- |
| AsyncOption               | Async, Task, Option                                                                                                                        |
| AsyncResult               | Result, Choice, Async, Task, TaskResult                                                                                                    |
| AsyncResultOption         | TaskResultOption, AsyncResult, TaskResult, AsyncOption, Result, Choice, Async, Task, Option,                                               |
| AsyncValidation           | AsyncResult, Validation, Result, Choice, Async                                                                                             |
| CancellableTaskResult     | ValueTaskResult, AsyncResult, TaskResult, AsyncChoice, CancellableTask, ColdTask, Result, Choice, Async, Task                              |
| CancellableTaskValidation | AsyncValidation, ValueTaskResult, AsyncResult, TaskResult, AsyncChoice, CancellableTask, ColdTask, Validation, Result, Choice, Async, Task |
| JopOption                 | TaskOption, AsyncOption, Job, Async, Task, Option                                                                                          |
| JobResult                 | AsyncResult, TaskResult, Job, Result, Choice, Async, Task                                                                                  |
| JobResultOption           |                                                                                                                                            |
| Option                    | ValueOption, nullable object                                                                                                               |
| Result                    | Choice                                                                                                                                     |
| ResultOption              |                                                                                                                                            |
| Task                      |                                                                                                                                            |
| TaskOption                | ValueTaskOption, AsyncOption, PlyOption, Async, Task, ValueTask, Option                                                                    |
| TaskResult                | AsyncResult, PlyResult, TaskResult, ValueTaskResult, Result, Choice, Ply, Async, Task, ValueTask                                           |
| TaskResultOption          |                                                                                                                                            |
| Validation                | Result, Choice                                                                                                                             |

## Don't see the bindings you want?

Luckily the CEs are extensible so you can add your own Source overloads to the builders. Take for example the AsyncOption builder. Let's pretend it didn't support being able to bind Async values. You can add an overload as follows shown below. The code shown would go in your own codebase, unless you'd want to submit a PR 😉 If you need an example of how to do this with various syntax, check out any of the CE files in the codebase.

```fsharp
module MyCustomCodeModule =

    type AsyncOptionBuilder with
        member _.Source(value: Async<'T>) : Async<'T option> =
           value |> Async.map Some

```
