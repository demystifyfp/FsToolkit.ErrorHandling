namespace FsToolkit.ErrorHandling.WebSharper.TestAdapter

open WebSharper
open WebSharper.JavaScript

/// WebSharper has no translation of System.Text.RegularExpressions.Regex. SampleDomain.fs calls Regex.IsMatch.
[<Proxy(typeof<System.Text.RegularExpressions.Regex>)>]
type internal RegexProxy =
    [<Inline "new RegExp($pattern).test($input)">]
    static member IsMatch(input: string, pattern: string) : bool = X<bool>
