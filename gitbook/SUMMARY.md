# Summary

* FsToolkit.ErrorHandling
  * Result
    * [apply](result/apply.md)
    * [bind](result/bind.md)
    * [Computation Expression](result/ce.md)
    * [either Functions](result/eitherFunctions.md)
    * [fold](result/fold.md)
    * [ignore](result/ignore.md)
    * [map](result/map.md)
    * [map2](result/map2.md)
    * [map3](result/map3.md)
    * [mapError](result/mapError.md)
    * [ofChoice](result/ofChoice.md)
    * [Operators](result/operators.md)
    * [orElse Functions](result/orElseFunctions.md)
    * [Other Functions](result/others.md)
    * [require Functions](result/requireFunctions.md)
    * [tee Functions](result/teeFunctions.md)
    * [tryCreate](result/tryCreate.md)
    * [zip](result/zip.md)
    * [zipError](result/zipError.md)
    * Lists
      * [traverseResultM](list/traverseResultM.md)
      * [sequenceResultM](list/sequenceResultM.md)
      * [traverseResultA](list/traverseResultA.md)
      * [sequenceResultA](list/sequenceResultA.md)

  * Option
    * [bind](pr.md)
    * [bindNull](pr.md)
    * [Computation Expression](option/ce.md)
    * [either](pr.md)
    * [map](pr.md)
    * [ofPair](pr.md)
    * [ofValueOption](pr.md)
    * [toValueOption](pr.md)
    * [zip](pr.md)
    * Lists
      * [traverseResult](option/traverseResult.md)
      * [sequenceResult](option/sequenceResult.md)

  * ResultOption
    * [apply](resultOption/apply.md)
    * [bind](resultOption/bind.md)
    * [Computation Expression](resultOption/ce.md)
    * [ignore](resultOption/ignore.md)
    * [map](resultOption/map.md)
    * [map2](resultOption/map2.md)
    * [map3](resultOption/map3.md)
    * [mapError](pr.md)
    * [ofResult](pr.md)
    * [ofOption](pr.md)
    * [ofChoice](pr.md)
    * [Operators](resultOption/operators.md)
    * [zip](resultOption/zip.md)
    * [zipError](resultOption/zipError.md)

  * AsyncResult
    * [apply](asyncResult/apply.md)
    * [bind](asyncResult/bind.md)
    * [Computation Expression](asyncResult/ce.md)
    * [error](pr.md)
    * [foldResult](asyncResult/foldResult.md)
    * [ignore](asyncResult/ignore.md)
    * [map](asyncResult/map.md)
    * [map2](asyncResult/map2.md)
    * [map3](asyncResult/map3.md)
    * [mapError](asyncResult/mapError.md)
    * [ofAsync](pr.md)
    * [ofResult](pr.md)
    * [ofTask](asyncResult/ofTask.md)
    * [ofTaskAction](asyncResult/ofTaskAction.md)
    * [ok](pr.md)
    * [Operators](asyncResult/operators.md)
    * [Other Functions](asyncResult/others.md)
    * [retn](pr.md)
    * [zip](pr.md)
    * [zipError](pr.md)
    * List
      * [traverseAsyncResultM](list/traverseAsyncResultM.md)
      * [sequenceAsyncResultM](list/sequenceAsyncResultM.md)
      * [traverseAsyncResultA](list/traverseAsyncResultA.md)
      * [sequenceAsyncResultA](list/sequenceAsyncResultA.md)

  * AsyncResultOption
    * [apply](asyncResultOption/apply.md)
    * [bind](asyncResultOption/bind.md)
    * [Computation Expression](asyncResultOption/ce.md)
    * [ignore](asyncResultOption/ignore.md)
    * [map](asyncResultOption/map.md)
    * [map2](asyncResultOption/map2.md)
    * [map3](asyncResultOption/map3.md)
    * [ofAsyncOption](pr.md)
    * [ofAsyncResult](pr.md)
    * [ofOption](pr.md)
    * [ofResult](pr.md)
    * [Operators](asyncResultOption/operators.md)
    * [retn](pr.md)

  * [Validation](validation/index.md)
    * [apply](validation/apply.md)
    * [Computation Expression](validation/ce.md)
    * [error](pr.md)
    * [map](pr.md)
    * [map2](validation/map2.md)
    * [map3](validation/map3.md)
    * [mapError](pr.md)
    * [mapErrors](pr.md)
    * [ofChoice](pr.md)
    * [ofResult](validation/ofResult.md)
    * [ok](pr.md)
    * [Operators](validation/operators.md)
    * [retn](pr.md)
    * [returnError](pr.md)
    * [zip](pr.md)

  * [AsyncValidation](asyncValidation/index.md)
    * [apply](asyncValidation/apply.md)
    * [Computation Expression](asyncValidation/ce.md)
    * [error](pr.md)
    * [map](pr.md)
    * [map2](asyncValidation/map2.md)
    * [map3](asyncValidation/map3.md)
    * [mapError](pr.md)
    * [mapErrors](pr.md)
    * [ofChoice](pr.md)
    * [ofResult](asyncValidation/ofResult.md)
    * [ok](pr.md)
    * [Operators](asyncValidation/operators.md)
    * [retn](pr.md)
    * [returnError](pr.md)
    * [zip](pr.md)

* FsToolkit.ErrorHandling.AsyncSeq
  * AsyncSeq
    * [Computation Expression](pr.md)

* FsToolkit.ErrorHandling.IcedTasks
  * [CancellableTaskResult](cancellableTaskResult/index.md)
    * [apply](cancellableTaskResult/apply.md)
    * [bind](cancellableTaskResult/bind.md)
    * [Computation Expression](cancellableTaskResult/ce.md)
    * [getCancellationToken](pr.md)
    * [map](cancellableTaskResult/map.md)
    * [Operators](cancellableTaskResult/operators.md)
    * [Other Functions](cancellableTaskResult/others.md)
    * [singleton](pr.md)
    * [zip](pr.md)
    * [parallelZip](pr.md)

  * [CancellableTaskValidation](cancellableTaskValidation/index.md)
    * [apply](cancellableTaskValidation/apply.md)
    * [Computation Expression](cancellableTaskValidation/ce.md)
    * [map2](cancellableTaskValidation/map2.md)
    * [map3](cancellableTaskValidation/map3.md)
    * [Operators](cancellableTaskValidation/operators.md)

* FsToolkit.ErrorHandling.JobResult
  * Job
    * [apply](pr.md)
    * [map2](pr.md)
    * [map3](pr.md)
    * [singleton](pr.md)
    * [zip](pr.md)

  * JobOption
    * [apply](pr.md)
    * [bind](pr.md)
    * [ce](jobOption/ce.md)
    * [either](pr.md)
    * [map](pr.md)

  * JobResult
    * [apply](jobResult/apply.md)
    * [bind](jobResult/bind.md)
    * [Computation Expression](jobResult/ce.md)
    * [ignore](jobResult/ignore.md)
    * [map](jobResult/map.md)
    * [map2](jobResult/map2.md)
    * [map3](jobResult/map3.md)
    * [mapError](jobResult/mapError.md)
    * [ofTask](jobResult/ofTask.md)
    * [Operators](jobResult/operators.md)
    * [Other Functions](jobResult/others.md)

  * JobResultOption

* FsToolkit.ErrorHandling.TaskResult
  * Task

  * TaskOption
    * [Computation Expression](taskOption/ce.md)

  * TaskResult
    * [apply](taskResult/apply.md)
    * [bind](taskResult/bind.md)
    * [Computation Expression](taskResult/ce.md)
    * [ignore](taskResult/ignore.md)
    * [map](taskResult/map.md)
    * [map2](taskResult/map2.md)
    * [map3](taskResult/map3.md)
    * [mapError](taskResult/mapError.md)
    * [Operators](taskResult/operators.md)
    * [Other Functions](taskResult/others.md)
    * Lists
      * [traverseTaskResultM](list/traverseTaskResultM.md)
      * [sequenceTaskResultM](list/sequenceTaskResultM.md)
      * [traverseTaskResultA](list/traverseTaskResultA.md)
      * [sequenceTaskResultA](list/sequenceTaskResultA.md)

  * TaskResultOption
    * [apply](taskResultOption/apply.md)
    * [bind](taskResultOption/bind.md)
    * [Computation Expression](taskResultOption/ce.md)
    * [ignore](taskResultOption/ignore.md)
    * [map](taskResultOption/map.md)
    * [map2](taskResultOption/map2.md)
    * [map3](taskResultOption/map3.md)
    * [Operators](taskResultOption/operators.md)

* General Docs
  * [Bind Mappings](bindMappings.md)
