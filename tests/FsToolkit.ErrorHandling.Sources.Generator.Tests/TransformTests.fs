module FsToolkit.ErrorHandling.Sources.Generator.Tests.TransformTests

open System.IO
open System.Text.RegularExpressions
open Expecto
open FsCheck
open FsToolkit.Build.SourcesPackage
open FsToolkit.ErrorHandling.Sources.Generator.Tests.LineModel

let private sourceProject =
    Path.GetFullPath(
        Path.Combine(
            __SOURCE_DIRECTORY__,
            "../../src/FsToolkit.ErrorHandling/FsToolkit.ErrorHandling.fsproj"
        )
    )

let private toLf (text: string) =
    text.TrimStart(char 0xFEFF).Replace("\r\n", "\n").Replace("\r", "\n")

type private RealSource = { Name: string; Text: string }

let private realSources =
    lazy
        (match
            File.ReadAllText sourceProject
            |> packagedCompileItems sourceProject
         with
         | Ok sources ->
             sources
             |> List.map (fun source -> {
                 Name = source.Include
                 Text =
                     File.ReadAllText(
                         Path.Combine(Path.GetDirectoryName sourceProject, source.Include)
                     )
                     |> toLf
             })
             |> List.toArray
         | Error errors -> failwithf "%s cannot be read: %A" sourceProject errors)

let private transformOk name source =
    match transform AllCompilers name source with
    | Ok text -> text
    | Error errors ->
        failtestf
            "transform %s failed: %s"
            name
            (errors
             |> List.map describeSourceError
             |> String.concat "\n")

type Terminator =
    | LF
    | CRLF
    | CR

type LineEndingCase = {
    File: int
    Bom: bool
    Terminators: Terminator array
}

let private renderLineEndings (case: LineEndingCase) =
    let text = realSources.Value[case.File].Text
    let lines = text.Split '\n'

    let chosen index =
        if index < case.Terminators.Length then
            case.Terminators[index]
        else
            LF

    let terminator index =
        match chosen index with
        // A CR followed by an empty LF-terminated line reads as one CRLF, so the line count would change.
        | CR when
            lines[index + 1] = ""
            && index + 1 < lines.Length
                           - 1
            && chosen (index + 1) = LF
            ->
            "\r\n"
        | CR -> "\r"
        | CRLF -> "\r\n"
        | LF -> "\n"

    let body =
        lines
        |> Array.mapi (fun index line ->
            if
                index < lines.Length
                        - 1
            then
                line
                + terminator index
            else
                line
        )
        |> String.concat ""

    if case.Bom then "﻿" + body else body

let private genLineEndingCase =
    gen {
        let! file =
            Gen.choose (
                0,
                realSources.Value.Length
                - 1
            )

        let lineCount = realSources.Value[file].Text.Split('\n').Length

        let! bom =
            Gen.elements [
                false
                true
            ]

        let! terminators =
            Gen.arrayOfLength
                lineCount
                (Gen.elements [
                    LF
                    CRLF
                    CR
                ])

        return {
            File = file
            Bom = bom
            Terminators = terminators
        }
    }

let private shrinkLineEndingCase (case: LineEndingCase) =
    seq {
        if case.Bom then
            yield { case with Bom = false }

        for index, terminator in Array.indexed case.Terminators do
            if
                terminator
                <> LF
            then
                yield {
                    case with
                        Terminators = Array.updateAt index LF case.Terminators
                }
    }

let private lineStart = Regex(@"^\s*(namespace|module|open)\s")

let private undoInsertions (output: string) =
    output.Split '\n'
    |> Array.filter (fun line ->
        line
        <> NoWarnLine
    )
    |> Array.map (fun line ->
        let line =
            line.Replace(
                " "
                + VisibilityToken,
                ""
            )

        if lineStart.IsMatch line then
            line.Replace(NamespaceToken, SourceNamespace)
        else
            line
    )
    |> String.concat "\n"

type private Observed = {
    TokenLines: int list
    NoWarnAfterLines: int list
    NamespaceLines: int list
}

let private observe (output: string) =
    let lines =
        output.Split '\n'
        |> Array.toList

    let rec walk inputIndex lines observed =
        match lines with
        | [] -> observed
        | line :: rest when line = NoWarnLine ->
            walk inputIndex rest {
                observed with
                    NoWarnAfterLines =
                        (inputIndex
                         - 1)
                        :: observed.NoWarnAfterLines
            }
        | (line: string) :: rest ->
            walk
                (inputIndex
                 + 1)
                rest
                {
                    observed with
                        TokenLines =
                            if line.Contains VisibilityToken then
                                inputIndex
                                :: observed.TokenLines
                            else
                                observed.TokenLines
                        NamespaceLines =
                            if line.Contains NamespaceToken then
                                inputIndex
                                :: observed.NamespaceLines
                            else
                                observed.NamespaceLines
                }

    let observed =
        walk 0 lines {
            TokenLines = []
            NoWarnAfterLines = []
            NamespaceLines = []
        }

    {
        TokenLines = List.rev observed.TokenLines
        NoWarnAfterLines = List.rev observed.NoWarnAfterLines
        NamespaceLines = List.rev observed.NamespaceLines
    }

let private predicted (lines: ModelLine list) (select: Expected -> bool) =
    lines
    |> List.indexed
    |> List.filter (fun (_, line) -> select line.Expected)
    |> List.map fst

let private problems (result: Result<string, SourceError list>) =
    match result with
    | Ok text -> failtestf "Expected a failure, but the transform gave:\n%s" text
    | Error errors ->
        errors
        |> List.map (fun e -> e.Line, e.Problem)

let private failsWith (source: string) (expected: (int option * SourceProblem) list) =
    Expect.equal (problems (transform AllCompilers "Probe.fs" source)) expected $"Source:\n{source}"

[<Tests>]
let properties =
    testList "transform properties" [
        testProperty "line endings and a BOM do not change the output"
        <| Prop.forAll
            (Arb.fromGenShrink (genLineEndingCase, shrinkLineEndingCase))
            (fun case ->
                let { Name = name; Text = lfText } = realSources.Value[case.File]
                let expected = transformOk name lfText
                let actual = transformOk name (renderLineEndings case)
                Expect.equal actual expected $"{name}: output differs from the LF output"
                Expect.isFalse (actual.Contains '\r') $"{name}: output contains CR"
                Expect.isFalse (actual.Contains '﻿') $"{name}: output contains a BOM"
            )

        testProperty "transform only inserts in a model file"
        <| Prop.forAll
            arbModelFile
            (fun file ->
                let input = toText (render file)

                Expect.equal
                    (undoInsertions (transformOk "Model.fs" input))
                    input
                    "Removing the insertions must give the input"
            )

        testProperty "visibility tokens, #nowarn lines, and namespace tokens match the model oracle"
        <| Prop.forAll
            arbModelFile
            (fun file ->
                let lines = render file
                let observed = observe (transformOk "Model.fs" (toText lines))

                Expect.equal
                    observed.TokenLines
                    (predicted lines _.Token)
                    "Lines with a visibility token"

                Expect.equal
                    observed.NoWarnAfterLines
                    (predicted lines _.NoWarnAfter)
                    "Lines followed by #nowarn"

                Expect.equal
                    observed.NamespaceLines
                    (predicted lines _.NamespaceReplaced)
                    "Lines with a namespace token"
            )

        test "transform only inserts in every real source file" {
            for { Name = name; Text = text } in realSources.Value do
                Expect.equal
                    (undoInsertions (transformOk name text))
                    text
                    $"{name}: removing the insertions must give the input"
        }

        test "the model generator reaches every line kind" {
            let files = [
                for seed in 1..300 -> Gen.eval 20 (Random.mkStdGen (int64 seed)) genModelFile
            ]

            let reached =
                files
                |> List.collect (fun file ->
                    caseName file.Header
                    :: (file.Leading
                        |> List.map caseName)
                    @ List.collect itemCases file.Items
                )
                |> Set.ofList

            let required =
                Set.ofList [
                    "NamespaceHeader"
                    "TopLevelModuleHeader"
                    "IfDefNamespaceHeader"
                    "LeadingBlank"
                    "LeadingLineComment"
                    "LeadingBlockComment"
                    "Blank"
                    "Declaration"
                    "ColumnZeroLet"
                    "Open"
                    "Attribute"
                    "IfDef"
                    "LineComment"
                    "SingleLineBlockComment"
                    "MultiLineBlockComment"
                    "NestedBlockComment"
                    "Multiply"
                    "StringWithCommentStart"
                    "MultiLineString"
                    "CharLiteralQuote"
                    "EscapedQuotes"
                    "IndentedOpenComment"
                    "LaterNamespace"
                ]

            Expect.isEmpty
                (Set.difference required reached)
                "Line kinds that the generator did not reach"
        }
    ]

[<Tests>]
let examples =
    testList "transform examples" [
        test "column-0 type extension" {
            failsWith "namespace Demo\ntype Foo with\n" [ Some 2, ColumnZeroTypeExtension ]
        }
        test "column-0 module rec" {
            failsWith "namespace Demo\nmodule rec Foo =\n" [ Some 2, ColumnZeroModuleRec ]
        }
        test "column-0 exception" {
            failsWith "namespace Demo\nexception Boom\n" [ Some 2, ColumnZeroException ]
        }
        test "column-0 do" { failsWith "namespace Demo\ndo ()\n" [ Some 2, UnrecognizedLine ] }

        test "attribute and declaration on one line" {
            failsWith "namespace Demo\n[<Struct>] type Foo = { X: int }\n" [
                Some 2, UnrecognizedLine
            ]
        }

        test "attribute after the type keyword" {
            failsWith "namespace Demo\ntype [<Struct>] Foo = { X: int }\n" [
                Some 2, UnrecognizedDeclaration "type"
            ]
        }

        test "NuGet token" {
            failsWith "namespace Demo\n// $version$\n" [ Some 2, PreprocessorToken "$version$" ]
        }

        test "explicit public" {
            failsWith
                "namespace Demo\ntype public Foo = int\nmodule public Bar =\n    let x = 1\nand public Baz = int\n"
                [
                    Some 2, ExplicitPublic
                    Some 3, ExplicitPublic
                    Some 5, ExplicitPublic
                ]
        }

        test "explicit public on a top-level module" {
            failsWith "module public Demo.Foo\nlet x = 1\n" [ Some 1, ExplicitPublic ]
        }

        test "no declaration" { failsWith "// only a comment\n" [ None, NoDeclaration ] }

        test "a declaration only inside a comment" {
            failsWith "(*\nnamespace Hidden\n*)\nlet x = 1\n" [ None, NoDeclaration ]
        }

        test "code after a block comment on a column-0 line" {
            failsWith "namespace Demo\n(* note *) type Foo = int\n" [ Some 2, CodeAfterComment ]
        }

        test "code after a multi-line block comment closes" {
            failsWith "namespace Demo\n(*\nnote\n*) type Foo = int\n" [ Some 4, CodeAfterComment ]
        }

        test "block comment opened after code on a column-0 line" {
            failsWith "namespace Demo\ntype Foo = int (* note\n*)\n" [
                Some 2, CommentOpenAfterCode
            ]
        }

        test "file ends inside a block comment" {
            failsWith "namespace Demo\n(* note\n" [ Some 3, UnterminatedCommentOrString ]
        }

        test "every failing line is reported" {
            failsWith "namespace Demo\ntype Foo with\nexception Boom\n" [
                Some 2, ColumnZeroTypeExtension
                Some 3, ColumnZeroException
            ]
        }

        test "the message names the file, line, and text" {
            match transform AllCompilers "Probe.fs" "namespace Demo\ntype Foo with\n" with
            | Error [ error ] ->
                Expect.equal
                    (describeSourceError error)
                    "Probe.fs(2): column-0 type extension: type Foo with"
                    "Message"
            | other -> failtestf "Unexpected result: %A" other
        }

        test "a column-0 single-line block comment is not a declaration" {
            Expect.equal
                (transformOk "Probe.fs" "namespace Demo\n(* note *)\nmodule Good =\n    let x = 1\n")
                $"namespace Demo\n{NoWarnLine}\n(* note *)\nmodule {VisibilityToken} Good =\n    let x = 1\n"
                "Output"
        }

        test "nested block comments keep the outer comment open" {
            Expect.equal
                (transformOk
                    "Probe.fs"
                    "namespace Demo\n(* outer\n(* inner *)\ntype Hidden = int\n*)\ntype Shown = int\n")
                $"namespace Demo\n{NoWarnLine}\n(* outer\n(* inner *)\ntype Hidden = int\n*)\ntype {VisibilityToken} Shown = int\n"
                "Output"
        }

        test "#nowarn does not go into a header comment" {
            Expect.equal
                (transformOk "Probe.fs" "(*\nnamespace CommentOnly\n*)\nmodule Demo\nlet x = 1\n")
                $"(*\nnamespace CommentOnly\n*)\nmodule {VisibilityToken} Demo\n{NoWarnLine}\nlet x = 1\n"
                "Output"
        }

        test "(*) is the multiply operator" {
            Expect.equal
                (transformOk "Probe.fs" "namespace Demo\nlet product = (*) 2 3\ntype Foo = int\n")
                $"namespace Demo\n{NoWarnLine}\nlet product = (*) 2 3\ntype {VisibilityToken} Foo = int\n"
                "Output"
        }

        test "#nowarn follows each namespace in #if branches" {
            Expect.equal
                (transformOk "Probe.fs" "#if A\nnamespace One\n#else\nnamespace Two\n#endif\n")
                $"#if A\nnamespace One\n{NoWarnLine}\n#else\nnamespace Two\n{NoWarnLine}\n#endif\n"
                "Output"
        }

        test "the namespace changes only in namespace, module, and open lines" {
            let source =
                "namespace FsToolkit.ErrorHandling\n// https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/277\nopen FsToolkit.ErrorHandling.Operator.Result\nmodule M =\n    open FsToolkit.ErrorHandling\n    let key = \"FsToolkit.ErrorHandling\"\n"

            Expect.equal
                (transformOk "Probe.fs" source)
                $"namespace {NamespaceToken}\n{NoWarnLine}\n// https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/277\nopen {NamespaceToken}.Operator.Result\nmodule {VisibilityToken} M =\n    open {NamespaceToken}\n    let key = \"FsToolkit.ErrorHandling\"\n"
                "Output"
        }
    ]

[<Tests>]
let fableExamples =
    testList "Fable guard" [
        test "a file excluded from Fable is in an #if block after its first declaration" {
            Expect.equal
                (transform
                    ExcludedFromFable
                    "Task.fs"
                    "namespace FsToolkit.ErrorHandling\n\nmodule M =\n    let x = 1\n")
                (Ok
                    $"namespace {NamespaceToken}\n{NoWarnLine}\n{FableGuardStart}\n\nmodule {VisibilityToken} M =\n    let x = 1\n{FableGuardEnd}\n")
                "Output"
        }

        test "a file excluded from Fable without a final newline ends with the guard" {
            Expect.equal
                (transform
                    ExcludedFromFable
                    "Task.fs"
                    "namespace FsToolkit.ErrorHandling\nmodule M =\n    let x = 1")
                (Ok
                    $"namespace {NamespaceToken}\n{NoWarnLine}\n{FableGuardStart}\nmodule {VisibilityToken} M =\n    let x = 1\n{FableGuardEnd}")
                "Output"
        }

        test "a file for all compilers has no Fable guard" {
            Expect.equal
                (transform
                    AllCompilers
                    "Result.fs"
                    "namespace FsToolkit.ErrorHandling\n\nmodule M =\n    let x = 1\n")
                (Ok
                    $"namespace {NamespaceToken}\n{NoWarnLine}\n\nmodule {VisibilityToken} M =\n    let x = 1\n")
                "Output"
        }

        test "a file excluded from Fable must start with its declaration" {
            Expect.equal
                (problems (
                    transform ExcludedFromFable "Task.fs" "#if NET9_0\nnamespace A\n#endif\n"
                ))
                [ Some 1, CodeBeforeFableGuard ]
                "Problems"
        }
    ]
