module FsToolkit.ErrorHandling.Sources.Generator.Tests.LineModel

open FsCheck

type Modifier =
    | NoModifier
    | Internal
    | Private

type DeclarationKeyword =
    | NestedModule
    | Type
    | And

type Trailing =
    | NoTrailing
    | ClosedBlockComment
    | LineCommentAfter

type Hidden =
    | HiddenNamespace
    | HiddenModule
    | HiddenType
    | HiddenOpen
    | HiddenWords
    | HiddenMarkers

type Item =
    | Blank
    | Declaration of DeclarationKeyword * Modifier * Trailing
    | ColumnZeroLet
    | Open of indented: bool
    | Attribute
    | IfDef of Item list * Item list
    | LineComment of indented: bool * Hidden
    | SingleLineBlockComment of indented: bool * Hidden
    | MultiLineBlockComment of Hidden list
    | NestedBlockComment of Hidden list
    | Multiply of indented: bool
    | StringWithCommentStart of indented: bool
    | MultiLineString of Hidden list
    | CharLiteralQuote
    | EscapedQuotes
    | IndentedOpenComment of Hidden list
    | LaterNamespace

type LeadingItem =
    | LeadingBlank
    | LeadingLineComment of Hidden
    | LeadingBlockComment of Hidden list

type Header =
    | NamespaceHeader
    | TopLevelModuleHeader of Modifier
    | IfDefNamespaceHeader

type ModelFile = {
    Leading: LeadingItem list
    Header: Header
    Items: Item list
}

type Expected = {
    Token: bool
    NoWarnAfter: bool
    NamespaceReplaced: bool
}

type ModelLine = { Text: string; Expected: Expected }

let private nothing = {
    Token = false
    NoWarnAfter = false
    NamespaceReplaced = false
}

let private plain text = { Text = text; Expected = nothing }

let private indent indented = if indented then "    " else ""

let private modifierText modifier =
    match modifier with
    | NoModifier -> ""
    | Internal -> "internal "
    | Private -> "private "

let private hiddenText hidden =
    match hidden with
    | HiddenNamespace -> "namespace Hidden"
    | HiddenModule -> "module Hidden"
    | HiddenType -> "type Hidden = int"
    | HiddenOpen -> "open FsToolkit.ErrorHandling"
    | HiddenWords -> "words about FsToolkit.ErrorHandling"
    | HiddenMarkers -> "a \"*)\" string, a // marker, and (* a nested *) comment"

let private hiddenLines hidden =
    hidden
    |> List.map (
        hiddenText
        >> plain
    )

let rec private renderItem item =
    match item with
    | Blank -> [ plain "" ]
    | Declaration(keyword, modifier, trailing) ->
        let head, body =
            match keyword with
            | NestedModule -> "module", "Alpha ="
            | Type -> "type", "Beta = int"
            | And -> "and", "Gamma = string"

        let trail =
            match trailing with
            | NoTrailing -> ""
            | ClosedBlockComment -> " (* type Hidden *)"
            | LineCommentAfter -> " // module Hidden"

        [
            {
                Text = $"{head} {modifierText modifier}{body}{trail}"
                Expected = {
                    nothing with
                        Token = (modifier = NoModifier)
                }
            }
            if keyword = NestedModule then
                plain "    let value = 1"
        ]
    | ColumnZeroLet -> [ plain "let value = 1" ]
    | Open indented -> [
        {
            Text =
                indent indented
                + "open FsToolkit.ErrorHandling.Operator.Result"
            Expected = {
                nothing with
                    NamespaceReplaced = true
            }
        }
      ]
    | Attribute -> [ plain "[<AutoOpen>]" ]
    | IfDef(active, inactive) -> [
        plain "#if MODEL_FLAG"
        yield! renderItems active
        plain "#else"
        yield! renderItems inactive
        plain "#endif"
      ]
    | LineComment(indented, hidden) -> [
        plain (
            indent indented
            + "// "
            + hiddenText hidden
        )
      ]
    | SingleLineBlockComment(indented, hidden) -> [
        plain (
            indent indented
            + "(* "
            + hiddenText hidden
            + " *)"
        )
      ]
    | MultiLineBlockComment hidden ->
        plain "(*"
        :: hiddenLines hidden
        @ [ plain "*)" ]
    | NestedBlockComment hidden ->
        plain "(* outer"
        :: plain "(* inner *)"
        :: hiddenLines hidden
        @ [ plain "*)" ]
    | Multiply indented -> [
        plain (
            indent indented
            + "let product = (*) 2 3"
        )
      ]
    | StringWithCommentStart indented -> [
        plain (
            indent indented
            + "let text = \"(* not a comment\""
        )
      ]
    | MultiLineString hidden ->
        plain "    let text = \"\"\""
        :: hiddenLines hidden
        @ [ plain "\"\"\"" ]
    | CharLiteralQuote -> [ plain "    let quote = '\"'" ]
    | EscapedQuotes -> [
        plain "    let escaped = \"a \\\"(* b\""
        plain "    let verbatim = @\"a \"\"(* b\""
      ]
    | IndentedOpenComment hidden ->
        plain "    let value = 1 (* opens"
        :: hiddenLines hidden
        @ [ plain "*)" ]
    | LaterNamespace -> [
        {
            Text = "namespace FsToolkit.ErrorHandling.Later"
            Expected = {
                nothing with
                    NoWarnAfter = true
                    NamespaceReplaced = true
            }
        }
      ]

and private renderItems items = List.collect renderItem items

let private namespaceLine text = {
    Text = text
    Expected = {
        nothing with
            NoWarnAfter = true
            NamespaceReplaced = true
    }
}

let private renderHeader header =
    match header with
    | NamespaceHeader -> [ namespaceLine "namespace FsToolkit.ErrorHandling" ]
    | TopLevelModuleHeader modifier -> [
        {
            Text = $"module {modifierText modifier}FsToolkit.ErrorHandling.Model"
            Expected = {
                Token = (modifier = NoModifier)
                NoWarnAfter = true
                NamespaceReplaced = true
            }
        }
      ]
    | IfDefNamespaceHeader -> [
        plain "#if MODEL_FLAG"
        namespaceLine "namespace FsToolkit.ErrorHandling"
        plain "#else"
        namespaceLine "namespace FsToolkit.ErrorHandling.Other"
        plain "#endif"
      ]

let private renderLeading leading =
    match leading with
    | LeadingBlank -> [ plain "" ]
    | LeadingLineComment hidden -> [
        plain (
            "// "
            + hiddenText hidden
        )
      ]
    | LeadingBlockComment hidden ->
        plain "(*"
        :: hiddenLines hidden
        @ [ plain "*)" ]

let render (file: ModelFile) =
    List.collect renderLeading file.Leading
    @ renderHeader file.Header
    @ renderItems file.Items

let toText (lines: ModelLine list) =
    (lines
     |> List.map _.Text
     |> String.concat "\n")
    + "\n"

let private genModifier =
    Gen.elements [
        NoModifier
        Internal
        Private
    ]

let private genHidden =
    Gen.elements [
        HiddenNamespace
        HiddenModule
        HiddenType
        HiddenOpen
        HiddenWords
        HiddenMarkers
    ]

let private listUpTo maxCount (element: Gen<'T>) =
    gen {
        let! count = Gen.choose (0, maxCount)
        return! Gen.listOfLength count element
    }

let private genHiddenLines = listUpTo 3 genHidden

let private genIndented =
    Gen.elements [
        false
        true
    ]

let private leafItems = [
    Gen.constant Blank
    Gen.map3
        (fun keyword modifier trailing -> Declaration(keyword, modifier, trailing))
        (Gen.elements [
            NestedModule
            Type
            And
        ])
        genModifier
        (Gen.elements [
            NoTrailing
            ClosedBlockComment
            LineCommentAfter
        ])
    Gen.constant ColumnZeroLet
    Gen.map Open genIndented
    Gen.constant Attribute
    Gen.map2 (fun indented hidden -> LineComment(indented, hidden)) genIndented genHidden
    Gen.map2 (fun indented hidden -> SingleLineBlockComment(indented, hidden)) genIndented genHidden
    Gen.map MultiLineBlockComment genHiddenLines
    Gen.map NestedBlockComment genHiddenLines
    Gen.map Multiply genIndented
    Gen.map StringWithCommentStart genIndented
    Gen.map MultiLineString genHiddenLines
    Gen.constant CharLiteralQuote
    Gen.constant EscapedQuotes
    Gen.map IndentedOpenComment genHiddenLines
    Gen.constant LaterNamespace
]

let rec private genItem depth =
    if depth <= 0 then
        Gen.oneof leafItems
    else
        Gen.frequency [
            leafItems.Length, Gen.oneof leafItems
            1,
            Gen.map2
                (fun active inactive -> IfDef(active, inactive))
                (genItems (depth - 1))
                (genItems (depth - 1))
        ]

and private genItems depth =
    Gen.sized (fun size -> listUpTo (min 12 (max 1 size)) (genItem depth))

let genModelFile =
    gen {
        let! leading =
            listUpTo
                3
                (Gen.oneof [
                    Gen.constant LeadingBlank
                    Gen.map LeadingLineComment genHidden
                    Gen.map LeadingBlockComment genHiddenLines
                ])

        let! header =
            Gen.oneof [
                Gen.constant NamespaceHeader
                Gen.map TopLevelModuleHeader genModifier
                Gen.constant IfDefNamespaceHeader
            ]

        let! items = genItems 2

        return {
            Leading = leading
            Header = header
            Items = items
        }
    }

let shrinkList (shrinkElement: 'T -> 'T seq) (items: 'T list) =
    seq {
        for index in
            0 .. items.Length
                 - 1 do
            yield List.removeAt index items

        for index, item in List.indexed items do
            for smaller in shrinkElement item do
                yield List.updateAt index smaller items
    }

let private noShrink _ = Seq.empty

let rec private shrinkItem item =
    seq {
        match item with
        | Declaration(keyword, modifier, trailing) ->
            if
                modifier
                <> NoModifier
            then
                yield Declaration(keyword, NoModifier, trailing)

            if
                trailing
                <> NoTrailing
            then
                yield Declaration(keyword, modifier, NoTrailing)
        | IfDef(active, inactive) ->
            for smaller in shrinkList shrinkItem active do
                yield IfDef(smaller, inactive)

            for smaller in shrinkList shrinkItem inactive do
                yield IfDef(active, smaller)
        | MultiLineBlockComment hidden ->
            yield!
                shrinkList noShrink hidden
                |> Seq.map MultiLineBlockComment
        | NestedBlockComment hidden ->
            yield!
                shrinkList noShrink hidden
                |> Seq.map NestedBlockComment
        | MultiLineString hidden ->
            yield!
                shrinkList noShrink hidden
                |> Seq.map MultiLineString
        | IndentedOpenComment hidden ->
            yield!
                shrinkList noShrink hidden
                |> Seq.map IndentedOpenComment
        | Blank
        | ColumnZeroLet
        | Open _
        | Attribute
        | LineComment _
        | SingleLineBlockComment _
        | Multiply _
        | StringWithCommentStart _
        | CharLiteralQuote
        | EscapedQuotes
        | LaterNamespace -> ()
    }

let private shrinkLeading leading =
    match leading with
    | LeadingBlockComment hidden ->
        shrinkList noShrink hidden
        |> Seq.map LeadingBlockComment
    | LeadingBlank
    | LeadingLineComment _ -> Seq.empty

let shrinkModelFile (file: ModelFile) =
    seq {
        for leading in shrinkList shrinkLeading file.Leading do
            yield { file with Leading = leading }

        if
            file.Header
            <> NamespaceHeader
        then
            yield { file with Header = NamespaceHeader }

        for items in shrinkList shrinkItem file.Items do
            yield { file with Items = items }
    }

let arbModelFile = Arb.fromGenShrink (genModelFile, shrinkModelFile)

let caseName (value: 'T) =
    let case, _ =
        Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(value, typeof<'T>)

    case.Name

let rec itemCases item =
    match item with
    | IfDef(active, inactive) ->
        "IfDef"
        :: List.collect
            itemCases
            (active
             @ inactive)
    | other -> [ caseName other ]
