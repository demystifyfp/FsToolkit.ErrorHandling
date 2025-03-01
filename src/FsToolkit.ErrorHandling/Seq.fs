// See here for previous design discussions:
// 1. https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/277
// 2. https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/310

[<RequireQualifiedAccess>]
module FsToolkit.ErrorHandling.Seq

/// <summary>
/// Applies a function to each element of a sequence and returns a single result
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>If state and all elements in 'sequence' are Ok, a result with the elements in an array. Alternately, the first error encountered</returns>
let inline traverseResultM'
    (state: Result<'okOutput seq, 'error>)
    ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
    (xs: 'okInput seq)
    : Result<'okOutput[], 'error> =
    if isNull xs then
        nullArg (nameof xs)

    match state with
    | Error e -> Error e
    | Ok initialSuccesses ->

        let oks = ResizeArray(initialSuccesses)
        let mutable ok = true
        let mutable err = Unchecked.defaultof<'error>
        use e = xs.GetEnumerator()

        while ok
              && e.MoveNext() do
            match f e.Current with
            | Ok r -> oks.Add r
            | Error e ->
                ok <- false
                err <- e

        if ok then Ok(oks.ToArray()) else Error err

/// <summary>
/// Applies a function to each element of a sequence and returns a single result
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>A result with the ok elements in an array, or the first error occurring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseResultM'"/> but applying an initial state of 'Seq.empty'</remarks>
let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs

/// <summary>
/// Converts a sequence of results into a single result
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>A result bearing all the results as an array, or the first error occurring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseResultM"/> but auto-applying the 'id' function</remarks>
let sequenceResultM xs = traverseResultM id xs

/// <summary>
/// Applies a function to each element of a sequence and returns a single result
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>If no Errors encountered, an Ok result bearing an array of the ok elements from the 'state' followed by those gathered from the sequence, or an Error bearing an array of all errors from the 'state' and/or those in the sequence</returns>
let inline traverseResultA'
    (state: Result<'okOutput seq, 'error seq>)
    ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
    xs
    =

    if isNull xs then
        nullArg (nameof xs)

    match state with
    | Error failuresToDate ->
        let errs = ResizeArray(failuresToDate)

        for x in xs do
            match f x with
            | Ok _ -> () // as the initial state was failure, oks are irrelevant
            | Error e -> errs.Add e

        Error(errs.ToArray())
    | Ok initialSuccesses ->

        let oks = ResizeArray(initialSuccesses)
        let errs = ResizeArray()

        for x in xs do
            match f x with
            | Error e -> errs.Add e
            | Ok r when errs.Count = 0 -> oks.Add r
            | Ok _ -> () // no point saving results we won't use given the end result will be Error

        match errs.ToArray() with
        | [||] -> Ok(oks.ToArray())
        | errs -> Error errs

/// <summary>
/// Applies a function to each element of a sequence and returns a single result
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>A result with the ok elements in an array or an array of all errors from the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseResultA'"/> but applying an initial state of Seq.empty'</remarks>
let traverseResultA f xs = traverseResultA' (Ok Seq.empty) f xs

/// <summary>
/// Converts a sequence of results into a single result
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>A result with the elements in an array or an array of all errors from the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseResultA"/> but auto-applying the 'id' function</remarks>
let sequenceResultA xs = traverseResultA id xs

/// <summary>
/// Applies a function to each element of a sequence and returns a single async result
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An async result with the ok elements in an array or the first error encountered in the state or the sequence</returns>
let inline traverseAsyncResultM'
    (state: Async<Result<'okOutput seq, 'error>>)
    ([<InlineIfLambda>] f: 'okInput -> Async<Result<'okOutput, 'error>>)
    (xs: 'okInput seq)
    : Async<Result<'okOutput[], 'error>> =
    if isNull xs then
        nullArg (nameof xs)

    async {
        match! state with
        | Error e -> return Error e
        | Ok initialSuccesses ->
            let oks = ResizeArray(initialSuccesses)
            let mutable ok = true
            let mutable err = Unchecked.defaultof<'error>
            use e = xs.GetEnumerator()

            while ok
                  && e.MoveNext() do
                match! f e.Current with
                | Ok r -> oks.Add r
                | Error e ->
                    ok <- false
                    err <- e

            return if ok then Ok(oks.ToArray()) else Error err
    }

/// <summary>
/// Applies a function to each element of a sequence and returns a single async result
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An async result with the ok elements in an array or the first error occurring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseAsyncResultM'"/> but applying an initial state of 'Seq.empty'</remarks>
let traverseAsyncResultM f xs =
    traverseAsyncResultM' (async { return Ok Seq.empty }) f xs

/// <summary>
/// Converts a sequence of async results into a single async result
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>An async result with the ok elements in an array or the first error occurring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseAsyncResultM"/> but auto-applying the 'id' function</remarks>
let sequenceAsyncResultM xs = traverseAsyncResultM id xs

/// <summary>
/// Applies a function to each element of a sequence and returns a single Task result
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>A task result with the ok elements in an array or the first error encountered in the state or the sequence</returns>
let inline traverseTaskResultM'
    (state: TaskResult<'okOutput seq, 'error>)
    ([<InlineIfLambda>] f: 'okInput -> TaskResult<'okOutput, 'error>)
    (xs: 'okInput seq)
    : TaskResult<'okOutput[], 'error> =
    if isNull xs then
        nullArg (nameof xs)

    task {
        match! state with
        | Error e -> return Error e
        | Ok initialSuccesses ->
            let oks = ResizeArray(initialSuccesses)
            let mutable ok = true
            let mutable err = Unchecked.defaultof<'error>
            use e = xs.GetEnumerator()

            while ok
                  && e.MoveNext() do
                match! f e.Current with
                | Ok r -> oks.Add r
                | Error e ->
                    ok <- false
                    err <- e

            return if ok then Ok(oks.ToArray()) else Error err
    }

/// <summary>
/// Applies a function to each element of a sequence and returns a single Task result
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>A task result with the ok elements in an array or the first error occurring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseTaskResultM'"/> but applying an initial state of 'Seq.empty'</remarks>
let traverseTaskResultM f xs =
    traverseTaskResultM' (TaskResult.ok Seq.empty) f xs

/// <summary>
/// Converts a sequence of Task results into a single Task result
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>A task result with the ok elements in an array or the first error occurring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseTaskResultM"/> but auto-applying the 'id' function</remarks>
let sequenceTaskResultM xs = traverseTaskResultM id xs

/// <summary>
/// Applies a function to each element of a sequence and returns a single async result
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An async result with the ok elements in an array or an array of all errors from the state and the original sequence</returns>
let inline traverseAsyncResultA'
    (state: Async<Result<'okOutput seq, 'error seq>>)
    ([<InlineIfLambda>] f: 'okInput -> Async<Result<'okOutput, 'error>>)
    (xs: 'okInput seq)
    : Async<Result<'okOutput[], 'error[]>> =
    if isNull xs then
        nullArg (nameof xs)

    async {
        match! state with
        | Error failuresToDate ->
            let errs = ResizeArray(failuresToDate)

            for x in xs do
                match! f x with
                | Ok _ -> () // as the initial state was failure, oks are irrelevant
                | Error e -> errs.Add e

            return Error(errs.ToArray())
        | Ok initialSuccesses ->

            let oks = ResizeArray(initialSuccesses)
            let errs = ResizeArray()

            for x in xs do
                match! f x with
                | Error e -> errs.Add e
                | Ok r when errs.Count = 0 -> oks.Add r
                | Ok _ -> () // no point saving results we won't use given the end result will be Error

            match errs.ToArray() with
            | [||] -> return Ok(oks.ToArray())
            | errs -> return Error errs
    }

/// <summary>
/// Applies a function to each element of a sequence and returns a single async result
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An async result with the ok elements in an array or an array of all errors occuring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseAsyncResultA'"/> but applying an initial state of 'Seq.empty'</remarks>
let traverseAsyncResultA f xs =
    traverseAsyncResultA' (async { return Ok Seq.empty }) f xs

/// <summary>
/// Converts a sequence of async results into a single async result
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>An async result with all ok elements in an array or an error result with an array of all errors occuring in the sequence</returns>
/// <remarks>This function is equivalent to <see cref="traverseAsyncResultA"/> but auto-applying the 'id' function</remarks>
let sequenceAsyncResultA xs = traverseAsyncResultA id xs

/// <summary>
/// Applies a function to each element of a sequence and returns a single option
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An option containing Some array of elements or None if any of the function applications return None</returns>
let inline traverseOptionM'
    (state: seq<'okOutput> option)
    ([<InlineIfLambda>] f: 'okInput -> 'okOutput option)
    (xs: 'okInput seq)
    : 'okOutput[] option =
    if isNull xs then
        nullArg (nameof xs)

    match state with
    | None -> None
    | Some values ->
        let values = ResizeArray(values)
        let mutable ok = true
        use enumerator = xs.GetEnumerator()

        while ok
              && enumerator.MoveNext() do
            match f enumerator.Current with
            | Some value -> values.Add value
            | None -> ok <- false

        if ok then Some(values.ToArray()) else None

/// <summary>
/// Applies a function to each element of a sequence and returns a single option
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An option containing Some array of elements or None if any of the function applications return None</returns>
/// <remarks>This function is equivalent to <see cref="traverseOptionM'"/> but applying an initial state of 'Seq.empty'</remarks>
let traverseOptionM f xs = traverseOptionM' (Some Seq.empty) f xs

/// <summary>
/// Converts a sequence of options into a single option
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>An option containing Some array of elements or None if any of the function applications return None</returns>
/// <remarks>This function is equivalent to <see cref="traverseOptionM"/> but auto-applying the 'id' function</remarks>
let sequenceOptionM xs = traverseOptionM id xs

/// <summary>
/// Applies a function to each element of a sequence and returns a single async option
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An async option containing Some array of elements or None if any of the function applications return None</returns>
let inline traverseAsyncOptionM'
    (state: Async<seq<'okOutput> option>)
    ([<InlineIfLambda>] f: 'okInput -> Async<'okOutput option>)
    (xs: 'okInput seq)
    : Async<'okOutput[] option> =
    if isNull xs then
        nullArg (nameof xs)

    async {
        match! state with
        | None -> return None
        | Some values ->
            let values = ResizeArray(values)
            let mutable ok = true
            use enumerator = xs.GetEnumerator()

            while ok
                  && enumerator.MoveNext() do
                match! f enumerator.Current with
                | Some value -> values.Add value
                | None -> ok <- false

            return if ok then Some(values.ToArray()) else None
    }

/// <summary>
/// Applies a function to each element of a sequence and returns a single async option
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>An async option containing Some array of elements or None if any of the function applications return None</returns>
/// <remarks>This function is equivalent to <see cref="traverseAsyncOptionM'"/> but applying an initial state of 'Async { return Some Seq.empty }'</remarks>
let traverseAsyncOptionM f xs =
    traverseAsyncOptionM' (async { return Some Seq.empty }) f xs

/// <summary>
/// Converts a sequence of async options into a single async option
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>An async option containing Some array of elements or None if any of the function applications return None</returns>
/// <remarks>This function is equivalent to <see cref="traverseAsyncOptionM"/> but auto-applying the 'id' function</remarks>
let sequenceAsyncOptionM xs = traverseAsyncOptionM id xs

#if !FABLE_COMPILER

/// <summary>
/// Applies a function to each element of a sequence and returns a single voption
/// </summary>
/// <param name="state">The initial state</param>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>A voption containing an Array of elements or None if any of the function applications return None</returns>
let inline traverseVOptionM'
    (state: ValueOption<'okOutput seq>)
    ([<InlineIfLambda>] f: 'okInput -> 'okOutput voption)
    (xs: 'okInput seq)
    : ValueOption<'okOutput[]> =
    if isNull xs then
        nullArg (nameof xs)

    match state with
    | ValueNone -> ValueNone
    | ValueSome values ->
        let values = ResizeArray(values)
        let mutable ok = true
        use enumerator = xs.GetEnumerator()

        while ok
              && enumerator.MoveNext() do
            match f enumerator.Current with
            | ValueSome value -> values.Add value
            | ValueNone -> ok <- false

        if ok then ValueSome(values.ToArray()) else ValueNone

/// <summary>
/// Applies a function to each element of a sequence and returns a single voption
/// </summary>
/// <param name="f">The function to apply to each element</param>
/// <param name="xs">The input sequence</param>
/// <returns>A voption containing Some array of elements or None if any of the function applications return None</returns>
/// <remarks>This function is equivalent to <see cref="traverseVOptionM'"/> but applying an initial state of 'ValueSome Seq.empty'</remarks>
let traverseVOptionM f xs =
    traverseVOptionM' (ValueSome Seq.empty) f xs

/// <summary>
/// Converts a sequence of voptions into a single voption
/// </summary>
/// <param name="xs">The input sequence</param>
/// <returns>A voption containing Some array of elements or None if any of the function applications return None</returns>
/// <remarks>This function is equivalent to <see cref="traverseVOptionM"/> but auto-applying the 'id' function</remarks>
let sequenceVOptionM xs = traverseVOptionM id xs

#endif
