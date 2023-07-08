# Bind Mappings

This pages shows you which computation expressions can implicitly bind to other CEs

Example

Here we can bind an 'T Option to 'T AsyncOption without having to lift from one type to another. See [here](https://fsharpforfunandprofit.com/posts/elevated-world/) for a better understanding on 'lifting'. To associate with the table below, the AsyncOption is the CE and the Option is the CE that can bind to it.

```fsharp
asyncOption {
    let! x = Some 1 
    return x
}
```

*Each CE can bind to itself so we don't list that here to reduce redundancy*

| CE                        | Can Bind (FsToolkit.ErrorHandling)                                                                                                                                      | Can Bind (FsToolkit.ErrorHandling.IcedTasks) | Can Bind (FsToolkit.ErrorHandling.JobResult)  | Can Bind (FsToolkit.ErrorHandling.TaskResult) |
|:--------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------|--|-----------------------------------------------|
| AsyncOption               | Async, Task, Option                                                                                                                                                     |                                              ||                                               |
| AsyncResult               | Result, Choice, Async, Task                                                                                                                                             |                                              || TaskResult                                    |
| AsyncResultOption         | AsyncResult, AsyncOption, Result, Choice, Async, Task, Option                                                                                                           |                                              || TaskResultOption, TaskResult, TaskOption                |
| AsyncValidation           | AsyncResult, Validation, Result, Choice, Async                                                                                                                          |                                              ||                                               |
| CancellableTaskResult     | AsyncResult, AsyncChoice, Result, Choice, Async, Task,              | CancellableTask<sup>1</sup>, ColdTask<sup>4</sup>                                             ||  ValueTaskResult, TaskResult        |
| CancellableTaskValidation | AsyncResult, AsyncChoice, ValueTaskResult<sup>3</sup>, TaskResult<sup>3</sup>, Validation, Result, Choice, Async, Task, CancellableTask<sup>1</sup>, ColdTask<sup>4</sup> |                                              ||                                               |
| JopOption                 | Job <sup>2</sup>, TaskOption <sup>3</sup>, AsyncOption                                                                                                                  |                                              ||                                               |
| JobResult                 |                                                                                                                                                                         |                                              ||                                               |
| JobResultOption           |                                                                                                                                                                         |                                              ||                                               |
| Option                    |                                                                                                                                                                         |                                              ||                                               |
| Result                    |                                                                                                                                                                         |                                              ||                                               |
| ResultOption              |                                                                                                                                                                         |                                              ||                                               |
| Task                      |                                                                                                                                                                         |                                              ||                                               |
| TaskOption                |                                                                                                                                                                         |                                              ||                                               |
| TaskResult                |                                                                                                                                                                         |                                              ||                                               |
| TaskResultOption          |                                                                                                                                                                         |                                              ||                                               |
| Validation                |                                                                                                                                                                         |                                              ||                                               |
