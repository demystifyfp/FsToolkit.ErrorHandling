module FsToolkit.ErrorHandling.Req

/// <summary>
/// Applies the predicate to the value.
/// If the predicate returns true, then returns the original value wrapped in Ok.
/// Otherwise, returns an <c>Error</c> result with the provided error.
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#filter</href>
/// </summary>
/// <param name="predicate">Predicate applied to the value.</param>
/// <param name="error">The error to return if the predicate returns false.</param>
/// <param name="value">The input result.</param>
/// <returns> The input <paramref name="value"/> wrapped in <c>Ok</c> if the provided <paramref name="predicate"/> returns true. Otherwise returns a new <c>Error</c> result with the value <paramref name="error"/>.</returns>
let inline filter
    ([<InlineIfLambda>] predicate: 'value -> bool)
    (error: 'error)
    (value: 'value)
    : Result<'value, 'error> =
    if predicate value then Ok value else Error error

/// <summary>
/// Returns <c>Ok</c> if the two values are equal, or the specified error if not.<br/>
/// Alternate syntax/parameter ordering for <c>equal</c><br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#equalto</href>
/// </summary>
/// <param name="other">The value to compare to.</param>
/// <param name="error">The error value to return if the values are not equal.</param>
/// <param name="value">The value to compare.</param>
/// <returns>An <c>Ok</c> result if the values are equal, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline equalTo (other: 'value) (error: 'error) (value: 'value) : Result<unit, 'error> =
    if value = other then Ok() else Error error

/// <summary>
/// Returns <c>Ok</c> if the two values are equal, or the specified error if not.<br/>
/// Alternate syntax/parameter ordering for <c>equalTo</c><br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#equal</href>
/// </summary>
/// <param name="error">The error value to return if the values are not equal.</param>
/// <param name="x1">The value to compare to.</param>
/// <param name="x2">The value to compare.</param>
/// <returns>An <c>Ok</c> result if the values are equal, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline equal (error: 'error) (x1, x2) : Result<unit, 'error> = equalTo x1 error x2

/// <summary>Requires a boolean value to be <c>true</c>, otherwise returns an error result.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#istrue</href></summary>
/// <param name="error">The error value to return if the condition is false.</param>
/// <param name="value">The boolean value to check.</param>
/// <returns>An <c>Ok</c> result if the condition is true, otherwise an Error result with the specified error value.</returns>
let inline isTrue (error: 'error) (value: bool) : Result<unit, 'error> = equalTo true error value

/// <summary>Requires a boolean value to be <c>false</c>, otherwise returns an error result.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#isfalse</href></summary>
/// <param name="error">The error value to return if the condition is true.</param>
/// <param name="value">The boolean value to check.</param>
/// <returns>An <c>Ok</c> result if the condition is false, otherwise an Error result with the specified error value.</returns>
let inline isFalse (error: 'error) (value: bool) : Result<unit, 'error> = equalTo false error value

/// <summary>Requires a value to be <c>Some</c>, otherwise returns an error result.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#some</href></summary>
/// <param name="error">The error value to return if the value is <c>None</c>.</param>
/// <param name="option">The <c>Option</c> value to check.</param>
/// <returns>An <c>Ok</c> result if the value is <c>Some</c>, otherwise an Error result with the specified error value.</returns>
let inline some (error: 'error) (option: 'ok option) : Result<'ok, 'error> =
    match option with
    | Some x -> Ok x
    | None -> Error error

/// <summary>Requires a value to be <c>None</c>, otherwise returns an error result.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#none</href></summary>
/// <param name="error">The error value to return if the value is <c>Some</c>.</param>
/// <param name="option">The <c>Option</c> value to check.</param>
/// <returns>An <c>Ok</c> result if the value is <c>None</c>, otherwise an Error result with the specified error value.</returns>
let inline none (error: 'error) (option: 'value option) : Result<unit, 'error> =
    match option with
    | Some _ -> Error error
    | None -> Ok()

/// <summary>Requires a value to be <c>ValueSome</c>, otherwise returns an error result.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#valuesome</href>
/// </summary>
/// <param name="error">The error value to return if the value is <c>ValueNone</c>.</param>
/// <param name="voption">The <c>ValueOption</c> value to check.</param>
/// <returns>An <c>Ok</c> result if the value is <c>ValueSome</c>, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline valueSome (error: 'error) (voption: 'ok voption) : Result<'ok, 'error> =
    match voption with
    | ValueNone -> Error error
    | ValueSome x -> Ok x

/// <summary>Requires a value to be <c>ValueNone</c>, otherwise returns an error result.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#valuenone</href></summary>
/// <param name="error">The error value to return if the value is <c>ValueSome</c>.</param>
/// <param name="voption">The <c>ValueOption</c> value to check.</param>
/// <returns>An <c>Ok</c> result if the value is <c>ValueNone</c>, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline valueNone (error: 'error) (voption: 'value voption) : Result<unit, 'error> =
    match voption with
    | ValueSome _ -> Error error
    | ValueNone -> Ok()

/// <summary>
/// Converts a nullable value into a <c>Result</c>, using the given error if null
///
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#notnull</href>
/// </summary>
/// <param name="error">The error value to return if the value is null.</param>
/// <param name="value">The nullable value to check.</param>
/// <returns>An <c>Ok</c> result if the value is not null, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline notNull (error: 'error) (value: 'ok) : Result<'ok, 'error> =
    match value with
    | null -> Error error
    | nonnull -> Ok nonnull

/// <summary>
/// Returns <c>Ok</c> if the sequence is empty, or the specified error if not.
///
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#empty</href>
/// </summary>
/// <param name="error">The error value to return if the sequence is not empty.</param>
/// <param name="xs">The sequence to check.</param>
/// <returns>An <c>Ok</c> result if the sequence is empty, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline empty (error: 'error) (xs: #seq<'value>) : Result<unit, 'error> =
    Seq.isEmpty xs
    |> isTrue error

/// <summary>
/// Returns <c>Ok</c> if the sequence is not empty, or the specified error if it is.
///
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#notempty</href>
/// </summary>
/// <param name="error">The error value to return if the sequence is empty.</param>
/// <param name="xs">The sequence to check.</param>
/// <returns>An <c>Ok</c> result if the sequence is not empty, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline notEmpty (error: 'error) (xs: #seq<'value>) : Result<unit, 'error> =
    Seq.isEmpty xs
    |> isFalse error

/// <summary>
/// Returns the first item of the sequence if it exists, or the specified error if the sequence is empty.
///
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#head</href>
/// </summary>
/// <param name="error">The error value to return if the sequence is empty.</param>
/// <param name="xs">The sequence to check.</param>
/// <returns>An <c>Ok</c> result containing the first item of the sequence if it exists, otherwise an <c>Error</c> result with the specified error value.</returns>
let inline head (error: 'error) (xs: #seq<'ok>) : Result<'ok, 'error> =
    Seq.tryHead xs
    |> some error

/// <summary>
/// Applies <paramref name="checker"/> to the <paramref name="input"/> value.<br/>
/// If <paramref name="checker"/> returns an <c>Ok</c> unit value, returns <paramref name="input"/>.<br/>
/// Otherwise, returns the <c>Error</c> value from the <paramref name="checker"/> as a new result.<br/>
/// Note there is no <c>checkWith</c> function, as the error creation is already deferred to the <paramref name="checker"/> function.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#check</href>
/// </summary>
/// <param name="checker">The function that performs a check against the <paramref name="input"/>. Returns <c>Error</c> value from the function if error.</param>
/// <param name="input">The input value that the checker inspects, and <c>check</c> returns if the `checker yields <c>Ok ()</c>.</param>
/// <returns>The input <paramref name="input"/> wrapped in <c>Ok</c> if <paramref name="checker"/> returns <c>Ok ()</c>.<br/>
/// Returns the <c>Error</c> value from <paramref name="checker"/> if it returns an <c>Error</c> result.</returns>
let inline check
    ([<InlineIfLambda>] checker: 'input -> Result<unit, 'error>)
    () // stand in for error in the function signature, as the error is already deferred to the checker function
    (input: 'input)
    : Result<'input, 'error> =
    checker input
    |> Result.map (fun () -> input)

/// Internal helper used to provide the <x>With associated with every <x>
let inline private deferError
    ([<InlineIfLambda>] checker: unit -> 'input -> Result<'output, unit>)
    ([<InlineIfLambda>] errorF: unit -> 'error)
    (x: 'input)
    : Result<'output, 'error> =
    checker () x
    |> Result.mapError errorF

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline filterWith predicate errorF x = deferError (filter predicate) errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline equalToWith other errorF x = deferError (equalTo other) errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline equalWith errorF (x1, x2) : Result<unit, 'error> = deferError equal errorF (x1, x2)

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline isTrueWith errorF x = deferError isTrue errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline isFalseWith errorF x = deferError isFalse errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline someWith errorF x = deferError some errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline noneWith errorF x = deferError none errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline valueSomeWith errorF x = deferError valueSome errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline valueNoneWith errorF x = deferError valueNone errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline notNullWith errorF x = deferError notNull errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline emptyWith errorF x = deferError empty errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline notEmptyWith errorF x = deferError notEmpty errorF x

/// <summary>A <c>*With</c> function has identical behavior to the associated function, except that the error
/// becomes a function that is evaluated only if the underlying function has determined the check has failed.<br/>
/// This allows expensive or side-effecting error construction to be avoided unless it is actually needed.<br/>
/// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/req/functions#with</href>
/// </summary>
let inline headWith errorF x = deferError head errorF x
