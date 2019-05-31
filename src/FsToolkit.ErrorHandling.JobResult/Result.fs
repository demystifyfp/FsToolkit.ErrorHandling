namespace FsToolkit.ErrorHandling

open Hopac

module Result =

  let sequenceJob (resJob: Result<Job<'a>, 'b>) : Job<Result<'a, 'b>> =
    job {
      match resJob with
      | Ok job ->
          let! x = job
          return Ok x
      | Error err -> return Error err
    }
