module FsToolkit.ErrorHandling.WebSharper.TestAdapter.WebSharperPending

let private unitEquality =
    "WebSharper compiles () to null in some places and to undefined in others, and = treats them as different"

let private undeclaredVariable =
    "WebSharper 10.1.10 inlines this code into JavaScript that assigns an undeclared MISSINGVAR variable"

let private memberConstraint =
    "WebSharper calls the TryGetValue member constraint as a JavaScript method that the dict object does not have"

/// Tests that fail only under WebSharper, as full test names with the reason.
/// The run fails when a name does not match a test.
let tests: (string * string) list = [
    "All Tests / Result Tests / ignoreError Tests / ignoreError returns the unit for ok",
    unitEquality
    "All Tests / Result Tests / ignoreError Tests / ignoreError returns the unit for Error",
    unitEquality
    "All Tests / Result CE Tests / ResultCE bind Tests / do! Ok Choice", unitEquality
    "All Tests / Result CE Tests / ResultCE using Tests / use normal disposable", undeclaredVariable
    "All Tests / Result CE Tests / ResultCE using Tests / use null disposable", undeclaredVariable
    "All Tests / Result Option Tests / ResultOption.ignore Tests / ignore with Ok(Some x)",
    unitEquality
    "All Tests / Result CE Tests / ResultOptionCE return Tests / return / unit", unitEquality
    "All Tests / Result CE Tests / ResultOptionCE bind Tests / do! / Ok result", unitEquality
    "All Tests / Result CE Tests / ResultOptionCE bind Tests / do! / Option some", unitEquality
    "All Tests / Result CE Tests / ResultOptionCE bind Tests / do! / Ok Choice", unitEquality
    "All Tests / Result CE Tests / ResultOptionCE using Tests / use / normal disposable",
    undeclaredVariable
    "All Tests / Result CE Tests / ResultOptionCE using Tests / use / null disposable",
    undeclaredVariable
    "All Tests / Async Result tests / AsyncResult.ignore tests / ignore with Async(Ok x)",
    unitEquality
    "All Tests / AsyncResultOptionTests / AsyncResultOption.ignore tests / ignore with Async(Ok Some(x))",
    unitEquality
    "All Tests / Validation CE Tests / AsyncValidationCE bind Tests / do! Error result",
    unitEquality
    "All Tests / Validation CE Tests / AsyncValidationCE bind Tests / do! Error Choice",
    unitEquality
    "All Tests / ValidationTests / Validation.apply tests / apply with Ok", undeclaredVariable
    "All Tests / Validation CE Tests / ValidationCE bind Tests / do! Error result", unitEquality
    "All Tests / Validation CE Tests / ValidationCE bind Tests / do! Error Choice", unitEquality
    "All Tests / Validation CE Tests / ValidationCE using Tests / use normal disposable",
    undeclaredVariable
    "All Tests / Validation CE Tests / ValidationCE using Tests / use null disposable",
    undeclaredVariable
    "All Tests / ValueOption Tests / ValueOption.tryGetValue / Can Parse int", memberConstraint
    "All Tests / ValueOption CE tests / CE Tests / Try Finally", undeclaredVariable
    "All Tests / ValueOption CE tests / CE Tests / Using null", undeclaredVariable
    "All Tests / ValueOption CE tests / CE Tests / Using disposeable", undeclaredVariable
]
