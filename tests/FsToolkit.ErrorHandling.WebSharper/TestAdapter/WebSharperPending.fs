module FsToolkit.ErrorHandling.WebSharper.TestAdapter.WebSharperPending

type Cause =
    | UnitEquality
    | UndeclaredVariable
    | MemberConstraint

module Cause =
    let reason cause =
        match cause with
        | UnitEquality ->
            "WebSharper compiles () to null in some places and to undefined in others, and = treats them as different"
        | UndeclaredVariable ->
            "WebSharper 10.1.10 inlines this code into JavaScript that assigns an undeclared MISSINGVAR variable"
        | MemberConstraint ->
            "WebSharper calls the TryGetValue member constraint as a JavaScript method that the dict object does not have"

    /// The failure message of a test with this cause contains this text.
    let messageFragment cause =
        match cause with
        | UnitEquality -> "undefined"
        | UndeclaredVariable -> "MISSINGVAR"
        | MemberConstraint -> "TryGetValue is not a function"

/// Tests that fail only under WebSharper, as full test names with the cause.
/// The run fails when a name does not match a test, or when a test passes or fails for another cause.
let tests: (string * Cause) list = [
    "All Tests / Result Tests / ignoreError Tests / ignoreError returns the unit for ok",
    UnitEquality
    "All Tests / Result Tests / ignoreError Tests / ignoreError returns the unit for Error",
    UnitEquality
    "All Tests / Result CE Tests / ResultCE bind Tests / do! Ok Choice", UnitEquality
    "All Tests / Result CE Tests / ResultCE using Tests / use normal disposable", UndeclaredVariable
    "All Tests / Result CE Tests / ResultCE using Tests / use null disposable", UndeclaredVariable
    "All Tests / Result Option Tests / ResultOption.ignore Tests / ignore with Ok(Some x)",
    UnitEquality
    "All Tests / Result CE Tests / ResultOptionCE return Tests / return / unit", UnitEquality
    "All Tests / Result CE Tests / ResultOptionCE bind Tests / do! / Ok result", UnitEquality
    "All Tests / Result CE Tests / ResultOptionCE bind Tests / do! / Option some", UnitEquality
    "All Tests / Result CE Tests / ResultOptionCE bind Tests / do! / Ok Choice", UnitEquality
    "All Tests / Result CE Tests / ResultOptionCE using Tests / use / normal disposable",
    UndeclaredVariable
    "All Tests / Result CE Tests / ResultOptionCE using Tests / use / null disposable",
    UndeclaredVariable
    "All Tests / Async Result tests / AsyncResult.ignore tests / ignore with Async(Ok x)",
    UnitEquality
    "All Tests / AsyncResultOptionTests / AsyncResultOption.ignore tests / ignore with Async(Ok Some(x))",
    UnitEquality
    "All Tests / Validation CE Tests / AsyncValidationCE bind Tests / do! Error result",
    UnitEquality
    "All Tests / Validation CE Tests / AsyncValidationCE bind Tests / do! Error Choice",
    UnitEquality
    "All Tests / ValidationTests / Validation.apply tests / apply with Ok", UndeclaredVariable
    "All Tests / Validation CE Tests / ValidationCE bind Tests / do! Error result", UnitEquality
    "All Tests / Validation CE Tests / ValidationCE bind Tests / do! Error Choice", UnitEquality
    "All Tests / Validation CE Tests / ValidationCE using Tests / use normal disposable",
    UndeclaredVariable
    "All Tests / Validation CE Tests / ValidationCE using Tests / use null disposable",
    UndeclaredVariable
    "All Tests / ValueOption Tests / ValueOption.tryGetValue / Can Parse int", MemberConstraint
    "All Tests / ValueOption CE tests / CE Tests / Try Finally", UndeclaredVariable
    "All Tests / ValueOption CE tests / CE Tests / Using null", UndeclaredVariable
    "All Tests / ValueOption CE tests / CE Tests / Using disposeable", UndeclaredVariable
]
