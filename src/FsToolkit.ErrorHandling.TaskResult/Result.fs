namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
#if NETSTANDARD2_0
open FSharp.Control.Tasks.Affine
#endif

module Result =

    let sequenceTask (resTask: Result<Task<'a>, 'b>) : Task<Result<'a, 'b>> = task {
        match resTask with
        | Ok task ->
            let! x = task
            return Ok x
        | Error err -> return Error err
    }
