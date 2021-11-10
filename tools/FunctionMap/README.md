# FunctionMap

## Table

|Methods                 |Option     |ValueOption|Validation|Async    |Job      |Result     |AsyncResult|TaskResult|JobResult|
|------------------------|-----------|-----------|----------|---------|---------|-----------|-----------|----------|---------|
|abort                   |           |           |          |         |Hopac    |           |           |          |         |
|apply                   |           |           |FsToolkit |FsToolkit|Hopac    |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|apply'                  |           |           |          |         |FsToolkit|           |           |          |         |
|asBeginEnd              |           |           |          |FSharp.Core|         |           |           |          |         |
|awaitEvent              |           |           |          |FSharp.Core|         |           |           |          |         |
|awaitIAsyncResult       |           |           |          |FSharp.Core|         |           |           |          |         |
|awaitTask               |           |           |          |FSharp.Core|Hopac    |           |           |          |         |
|awaitUnitTask           |           |           |          |         |Hopac    |           |           |          |         |
|awaitWaitHandle         |           |           |          |FSharp.Core|         |           |           |          |         |
|bind                    |FSharp.Core|FSharp.Core|FsToolkit |FsToolkit|Hopac    |FSharp.Core|FsToolkit  |FsToolkit |FsToolkit|
|bindAsync               |           |           |          |         |Hopac    |           |           |          |         |
|bindTask                |           |           |          |         |Hopac    |           |           |          |         |
|bindUnitTask            |           |           |          |         |Hopac    |           |           |          |         |
|cancelDefaultToken      |           |           |          |FSharp.Core|         |           |           |          |         |
|catch                   |           |           |          |FSharp.Core|Hopac    |           |FsToolkit  |FsToolkit |FsToolkit|
|choice                  |           |           |          |FSharp.Core|         |           |           |          |         |
|conCollect              |           |           |          |         |Hopac    |           |           |          |         |
|conIgnore               |           |           |          |         |Hopac    |           |           |          |         |
|contains                |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|count                   |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|defaultError            |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|defaultValue            |FSharp.Core|FSharp.Core|          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|defaultWith             |FSharp.Core|FSharp.Core|          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|delay                   |           |           |          |         |Hopac    |           |           |          |         |
|delayWith               |           |           |          |         |Hopac    |           |           |          |         |
|either                  |           |           |          |         |         |FsToolkit  |           |          |         |
|eitherMap               |           |           |          |         |         |FsToolkit  |           |          |         |
|error                   |           |           |FsToolkit |         |         |           |           |          |         |
|exists                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|filter                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|flatten                 |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|fold                    |FSharp.Core|FSharp.Core|          |         |         |FsToolkit  |           |          |         |
|foldBack                |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|foldResult              |           |           |          |         |         |           |FsToolkit  |FsToolkit |FsToolkit|
|forAll                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|forDownTo               |           |           |          |         |Hopac    |           |           |          |         |
|forDownToIgnore         |           |           |          |         |Hopac    |           |           |          |         |
|forN                    |           |           |          |         |Hopac    |           |           |          |         |
|forNIgnore              |           |           |          |         |Hopac    |           |           |          |         |
|forUpTo                 |           |           |          |         |Hopac    |           |           |          |         |
|forUpToIgnore           |           |           |          |         |Hopac    |           |           |          |         |
|forever                 |           |           |          |         |Hopac    |           |           |          |         |
|foreverIgnore           |           |           |          |         |Hopac    |           |           |          |         |
|foreverServer           |           |           |          |         |Hopac    |           |           |          |         |
|fromAsync               |           |           |          |         |Hopac    |           |           |          |         |
|fromBeginEnd            |           |           |          |FSharp.Core|Hopac    |           |           |          |         |
|fromContinuations       |           |           |          |FSharp.Core|Hopac    |           |           |          |         |
|fromEndBegin            |           |           |          |         |Hopac    |           |           |          |         |
|fromTask                |           |           |          |         |Hopac    |           |           |          |FsToolkit|
|fromUnitTask            |           |           |          |         |Hopac    |           |           |          |FsToolkit|
|getMain                 |           |           |          |Hopac    |         |           |           |          |         |
|getValue                |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|get_CancellationToken   |           |           |          |FSharp.Core|         |           |           |          |         |
|get_DefaultCancellationToken|           |           |          |FSharp.Core|         |           |           |          |         |
|ignore                  |           |           |          |FSharp.Core|Hopac    |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|ignoreError             |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|isError                 |           |           |          |         |         |FsToolkit  |           |          |         |
|isNone                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|isOk                    |           |           |          |         |         |FsToolkit  |           |          |         |
|isSome                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|iterate                 |FSharp.Core|FSharp.Core|          |         |Hopac    |           |           |          |         |
|iterateServer           |           |           |          |         |Hopac    |           |           |          |         |
|join                    |           |           |          |         |Hopac    |           |           |          |         |
|lift                    |           |           |          |         |Hopac    |           |           |          |         |
|liftTask                |           |           |          |         |Hopac    |           |           |          |         |
|liftUnitTask            |           |           |          |         |Hopac    |           |           |          |         |
|map                     |FSharp.Core|FSharp.Core|FsToolkit |FsToolkit|Hopac    |FSharp.Core|FsToolkit  |FsToolkit |FsToolkit|
|map2                    |FSharp.Core|FSharp.Core|FsToolkit |FsToolkit|FsToolkit|FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|map3                    |FSharp.Core|FSharp.Core|FsToolkit |FsToolkit|FsToolkit|FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|mapError                |           |           |FsToolkit |         |         |FSharp.Core|FsToolkit  |FsToolkit |FsToolkit|
|mapErrors               |           |           |FsToolkit |         |         |           |           |          |         |
|ofAsync                 |           |           |          |         |         |           |FsToolkit  |FsToolkit |FsToolkit|
|ofChoice                |           |           |FsToolkit |         |         |FsToolkit  |           |          |         |
|ofJob                   |           |           |          |         |         |           |           |          |FsToolkit|
|ofJobOn                 |           |           |          |Hopac    |         |           |           |          |         |
|ofNullable              |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|ofObj                   |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|ofResult                |           |           |FsToolkit |         |         |           |FsToolkit  |FsToolkit |FsToolkit|
|ofTask                  |           |           |          |         |         |           |FsToolkit  |FsToolkit |         |
|ofTaskAction            |           |           |          |         |         |           |FsToolkit  |          |         |
|ofValueOption           |FsToolkit  |           |          |         |         |           |           |          |         |
|ok                      |           |           |FsToolkit |         |         |           |           |          |         |
|onCancel                |           |           |          |FSharp.Core|         |           |           |          |         |
|onThreadPool            |           |           |          |         |Hopac    |           |           |          |         |
|orElse                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|orElseWith              |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|parallel                |           |           |          |FSharp.Core|         |           |           |          |         |
|paranoid                |           |           |          |         |Hopac    |           |           |          |         |
|queue                   |           |           |          |         |Hopac    |           |           |          |         |
|queueIgnore             |           |           |          |         |Hopac    |           |           |          |         |
|raises                  |           |           |          |         |Hopac    |           |           |          |         |
|requireEmpty            |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireEqual            |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireEqualTo          |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireFalse            |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireHead             |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireNone             |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireNotEmpty         |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireNotNull          |           |           |          |         |         |FsToolkit  |           |          |         |
|requireSome             |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|requireTrue             |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|result                  |           |           |          |         |Hopac    |           |           |          |         |
|retn                    |           |           |FsToolkit |         |         |           |FsToolkit  |FsToolkit |FsToolkit|
|returnError             |           |           |          |         |         |           |FsToolkit  |FsToolkit |FsToolkit|
|runSynchronously        |           |           |          |FSharp.Core|         |           |           |          |         |
|seqCollect              |           |           |          |         |Hopac    |           |           |          |         |
|seqIgnore               |           |           |          |         |Hopac    |           |           |          |         |
|sequenceAsync           |           |           |          |         |         |FsToolkit  |           |          |         |
|sequenceJob             |           |           |          |         |         |FsToolkit  |           |          |         |
|sequenceResult          |FsToolkit  |           |          |         |         |           |           |          |         |
|sequenceTask            |           |           |          |         |         |FsToolkit  |           |          |         |
|sequential              |           |           |          |FSharp.Core|         |           |           |          |         |
|server                  |           |           |          |         |Hopac    |           |           |          |         |
|setError                |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|setMain                 |           |           |          |Hopac    |         |           |           |          |         |
|singleton               |           |           |          |FsToolkit|FsToolkit|           |           |          |         |
|sleep                   |           |           |          |FSharp.Core|         |           |           |          |         |
|start                   |           |           |          |FSharp.Core|Hopac    |           |           |          |         |
|startAsTask             |           |           |          |FSharp.Core|         |           |           |          |         |
|startChild              |           |           |          |FSharp.Core|         |           |           |          |         |
|startChildAsTask        |           |           |          |FSharp.Core|         |           |           |          |         |
|startIgnore             |           |           |          |         |Hopac    |           |           |          |         |
|startImmediate          |           |           |          |FSharp.Core|         |           |           |          |         |
|startImmediateAsTask    |           |           |          |FSharp.Core|         |           |           |          |         |
|startWithContinuations  |           |           |          |FSharp.Core|         |           |           |          |         |
|startWithFinalizer      |           |           |          |         |Hopac    |           |           |          |         |
|startWithFinalizerIgnore|           |           |          |         |Hopac    |           |           |          |         |
|switchToContext         |           |           |          |FSharp.Core|         |           |           |          |         |
|switchToNewThread       |           |           |          |FSharp.Core|         |           |           |          |         |
|switchToThreadPool      |           |           |          |FSharp.Core|         |           |           |          |         |
|tee                     |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|teeError                |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|teeErrorIf              |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|teeIf                   |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|thunk                   |           |           |          |         |Hopac    |           |           |          |         |
|toAlt                   |           |           |          |Hopac    |         |           |           |          |         |
|toAltOn                 |           |           |          |Hopac    |         |           |           |          |         |
|toArray                 |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|toAsync                 |           |           |          |         |Hopac    |           |           |          |         |
|toJob                   |           |           |          |Hopac    |         |           |           |          |         |
|toJobOn                 |           |           |          |Hopac    |         |           |           |          |         |
|toList                  |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|toNullable              |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|toObj                   |FSharp.Core|FSharp.Core|          |         |         |           |           |          |         |
|toValueOption           |FsToolkit  |           |          |         |         |           |           |          |         |
|traverseResult          |FsToolkit  |           |          |         |         |           |           |          |         |
|tryCancelled            |           |           |          |FSharp.Core|         |           |           |          |         |
|tryCreate               |           |           |          |         |         |FsToolkit  |           |          |         |
|tryFinallyFun           |           |           |          |         |Hopac    |           |           |          |         |
|tryFinallyFunDelay      |           |           |          |         |Hopac    |           |           |          |         |
|tryFinallyJob           |           |           |          |         |Hopac    |           |           |          |         |
|tryFinallyJobDelay      |           |           |          |         |Hopac    |           |           |          |         |
|tryIn                   |           |           |          |         |Hopac    |           |           |          |         |
|tryInDelay              |           |           |          |         |Hopac    |           |           |          |         |
|tryParse                |FsToolkit  |           |          |         |         |           |           |          |         |
|tryWith                 |           |           |          |         |Hopac    |           |           |          |         |
|tryWithDelay            |           |           |          |         |Hopac    |           |           |          |         |
|unit                    |           |           |          |         |Hopac    |           |           |          |         |
|useIn                   |           |           |          |         |Hopac    |           |           |          |         |
|using                   |           |           |          |         |Hopac    |           |           |          |         |
|usingAsync              |           |           |          |         |Hopac    |           |           |          |         |
|valueOr                 |           |           |          |         |         |FsToolkit  |           |          |         |
|whenDo                  |           |           |          |         |Hopac    |           |           |          |         |
|whileDo                 |           |           |          |         |Hopac    |           |           |          |         |
|whileDoDelay            |           |           |          |         |Hopac    |           |           |          |         |
|whileDoIgnore           |           |           |          |         |Hopac    |           |           |          |         |
|withError               |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|zip                     |FsToolkit  |           |FsToolkit |FsToolkit|FsToolkit|FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|
|zipError                |           |           |          |         |         |FsToolkit  |FsToolkit  |FsToolkit |FsToolkit|


