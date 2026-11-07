namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

[<RequireQualifiedAccess>]
module TaskResultOption =

    let inline some value : Task<Result<'ok option, 'error>> = TaskResult.ok (Some value)
    let inline none<'ok, 'error> : Task<Result<'ok option, 'error>> = TaskResult.ok None

    [<System.Obsolete "Please use some instead of ok">]
    let inline ok x = some x

    [<System.Obsolete "Please use TaskResult.error instead of error">]
    let inline error x = TaskResult.error x

    [<System.Obsolete "Please use some instead of singleton">]
    let inline singleton value = some value

    let inline map ([<InlineIfLambda>] f) tro = TaskResult.map (Option.map f) tro

    let inline bind ([<InlineIfLambda>] f) tro =
        let binder opt =
            match opt with
            | Some x -> f x
            | None -> none

        TaskResult.bind binder tro

    let inline map2 ([<InlineIfLambda>] f) xTRO yTRO =
        TaskResult.map2 (Option.map2 f) xTRO yTRO

    let inline map3 ([<InlineIfLambda>] f) xTRO yTRO zTRO =
        TaskResult.map3 (Option.map3 f) xTRO yTRO zTRO

    let inline apply fTRO xTRO = map2 (fun f x -> f x) fTRO xTRO

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error> (tro: Task<Result<'ok option, 'error>>) =
        tro
        |> map ignore<'ok>

    /// <summary>
    /// Transforms a <c>Result&lt;'ok, 'error&gt;</c> into a <c>Task&lt;Result&lt;'ok option, 'error&gt;&gt;</c>.
    /// </summary>
    let inline ofResult (result: Result<'ok, 'error>) : Task<Result<'ok option, 'error>> =
        result
        |> Result.map Some
        |> Task.result

    /// <summary>
    /// Transforms a <c>Task&lt;Result&lt;'ok, 'error&gt;&gt;</c> into a <c>Task&lt;Result&lt;'ok option, 'error&gt;&gt;</c>.
    /// </summary>
    let inline ofTaskResult
        (taskResult: Task<Result<'ok, 'error>>)
        : Task<Result<'ok option, 'error>> =
        taskResult
        |> TaskResult.map Some

    /// <summary>
    /// Transforms a <c>'ok option</c> into a <c>Task&lt;Result&lt;'ok option, 'error&gt;&gt;</c>.
    /// </summary>
    let inline ofOption (option: 'ok option) : Task<Result<'ok option, 'error>> =
        TaskResult.ok option

    /// <summary>
    /// Transforms a <c>Task&lt;'ok option&gt;</c> into a <c>Task&lt;Result&lt;'ok option, 'error&gt;&gt;</c>.
    /// </summary>
    let inline ofTaskOption (taskOption: Task<'ok option>) : Task<Result<'ok option, 'error>> =
        taskOption
        |> Task.map Ok

    /// <summary>
    /// Transforms a <c>Task&lt;'ok&gt;</c> into a <c>Task&lt;Result&lt;'ok option, 'error&gt;&gt;</c>.
    /// </summary>
    let inline ofTask (task: Task<'ok>) : Task<Result<'ok option, 'error>> =
        task
        |> Task.bind some

    /// Bind the Task&lt;'input option&gt; with a synchronous Result-returning function.
    /// A None input will be mapped to Ok None.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Task<'input option>)
        : Task<Result<'output option, 'error>> =
        input
        |> Task.bindResult (
            function
            | Some x ->
                binder x
                |> Result.map Some
            | None -> Ok None
        )

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value
