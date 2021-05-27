# FunctionMap

## Table

|Methods             |ResultModule|OptionModule|Option     |ValueOption|Result     |Validation |Async      |Job        |AsyncResult|TaskResult |JobResult  |
|--------------------|------------|------------|-----------|-----------|-----------|-----------|-----------|-----------|-----------|-----------|-----------|
|apply               |            |            |           |           |FsToolkit  |FsToolkit  |FsToolkit  |           |FsToolkit  |FsToolkit  |FsToolkit  |
|apply'              |            |            |           |           |           |           |           |FsToolkit  |           |           |           |
|bind                |FSharp.Core |FSharp.Core |           |FSharp.Core|           |FsToolkit  |FsToolkit  |           |FsToolkit  |FsToolkit  |FsToolkit  |
|catch               |            |            |           |           |           |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|contains            |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|count               |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|defaultError        |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|defaultValue        |            |FSharp.Core |           |FSharp.Core|FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|defaultWith         |            |FSharp.Core |           |FSharp.Core|FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|either              |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|eitherMap           |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|error               |            |            |           |           |           |FsToolkit  |           |           |           |           |           |
|exists              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|filter              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|flatten             |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|fold                |            |FSharp.Core |           |FSharp.Core|FsToolkit  |           |           |           |           |           |           |
|foldBack            |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|foldResult          |            |            |           |           |           |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|forAll              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|fromTask            |            |            |           |           |           |           |           |           |           |           |FsToolkit  |
|fromUnitTask        |            |            |           |           |           |           |           |           |           |           |FsToolkit  |
|getValue            |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|ignore              |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|ignoreError         |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|isError             |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|isNone              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|isOk                |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|isSome              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|iterate             |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|map                 |FSharp.Core |FSharp.Core |           |FSharp.Core|           |FsToolkit  |FsToolkit  |           |FsToolkit  |FsToolkit  |FsToolkit  |
|map2                |            |FSharp.Core |           |FSharp.Core|FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |
|map3                |            |FSharp.Core |           |FSharp.Core|FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |
|mapError            |FSharp.Core |            |           |           |           |FsToolkit  |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|mapErrors           |            |            |           |           |           |FsToolkit  |           |           |           |           |           |
|ofAsync             |            |            |           |           |           |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|ofChoice            |            |            |           |           |FsToolkit  |FsToolkit  |           |           |           |           |           |
|ofJob               |            |            |           |           |           |           |           |           |           |           |FsToolkit  |
|ofNullable          |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|ofObj               |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|ofResult            |            |            |           |           |           |FsToolkit  |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|ofTask              |            |            |           |           |           |           |           |           |FsToolkit  |FsToolkit  |           |
|ofTaskAction        |            |            |           |           |           |           |           |           |FsToolkit  |           |           |
|ofValueOption       |            |            |FsToolkit  |           |           |           |           |           |           |           |           |
|ok                  |            |            |           |           |           |FsToolkit  |           |           |           |           |           |
|orElse              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|orElseWith          |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|requireEmpty        |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireEqual        |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireEqualTo      |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireFalse        |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireHead         |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireNone         |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireNotEmpty     |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireNotNull      |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|requireSome         |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|requireTrue         |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|retn                |            |            |           |           |           |FsToolkit  |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|returnError         |            |            |           |           |           |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|sequenceAsync       |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|sequenceJob         |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|sequenceResult      |            |            |FsToolkit  |           |           |           |           |           |           |           |           |
|sequenceTask        |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|setError            |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|singleton           |            |            |           |           |           |           |FsToolkit  |FsToolkit  |           |           |           |
|tee                 |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|teeError            |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|teeErrorIf          |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|teeIf               |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|toArray             |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|toList              |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|toNullable          |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|toObj               |            |FSharp.Core |           |FSharp.Core|           |           |           |           |           |           |           |
|toValueOption       |            |            |FsToolkit  |           |           |           |           |           |           |           |           |
|traverseResult      |            |            |FsToolkit  |           |           |           |           |           |           |           |           |
|tryCreate           |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|tryParse            |            |            |FsToolkit  |           |           |           |           |           |           |           |           |
|valueOr             |            |            |           |           |FsToolkit  |           |           |           |           |           |           |
|withError           |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
|zip                 |            |            |FsToolkit  |           |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |FsToolkit  |
|zipError            |            |            |           |           |FsToolkit  |           |           |           |FsToolkit  |FsToolkit  |FsToolkit  |
