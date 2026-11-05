namespace FsToolkit.Build

module SourcesPackage =
    open System
    open System.IO
    open System.Reflection
    open System.Reflection.Metadata
    open System.Reflection.PortableExecutable
    open System.Text
    open System.Text.RegularExpressions
    open System.Xml
    open System.Xml.Linq

    [<Literal>]
    let SourceNamespace = "FsToolkit.ErrorHandling"

    [<Literal>]
    let NamespaceToken = "$fstoolkiterrorhandlingnamespace$"

    [<Literal>]
    let VisibilityToken = "$fstoolkiterrorhandlingvisibility$"

    [<Literal>]
    let NoWarnLine =
        "#nowarn \"57\" \"1182\" \"3180\" \"3261\" \"3390\" \"3513\" \"3517\" \"3559\""

    [<Literal>]
    let FableGuardStart = "#if !FABLE_COMPILER"

    [<Literal>]
    let FableGuardEnd = "#endif"

    [<Literal>]
    let MaxFiles = 999

    [<Literal>]
    let NotFableCondition = "'$(FABLE_COMPILER)' != 'true'"

    type CompileItem = {
        Include: string
        Condition: string option
    }

    type PackagedCondition =
        | AllCompilers
        | ExcludedFromFable

    type PackagedSource = {
        Include: string
        Condition: PackagedCondition
    }

    type ProjectProblem =
        | MalformedXml of message: string
        | NotInTopLevelItemGroup
        | ConditionalItemGroup
        | UpdateOrRemove
        | UnsupportedAttribute of name: string
        | UnsupportedMetadataElement of name: string
        | MissingInclude
        | UnsupportedInclude
        | UnsupportedCondition of condition: string

    type ProjectError = {
        Project: string
        Element: string
        Problem: ProjectProblem
    }

    type SourceProblem =
        | SourceFileNotFound
        | PreprocessorToken of token: string
        | ExplicitPublic
        | ColumnZeroTypeExtension
        | ColumnZeroModuleRec
        | ColumnZeroException
        | UnrecognizedDeclaration of keyword: string
        | UnrecognizedLine
        | CodeAfterComment
        | CommentOpenAfterCode
        | UnterminatedCommentOrString
        | NoDeclaration
        | CodeBeforeFableGuard

    type SourceError = {
        File: string
        Line: int option
        Text: string
        Problem: SourceProblem
    }

    type GenerateError =
        | InvalidProject of ProjectError list
        | TooManyFiles of project: string * count: int
        | InvalidSources of SourceError list

    let describeProjectError (error: ProjectError) =
        let reason =
            match error.Problem with
            | MalformedXml message -> $"is not valid XML: {message}"
            | NotInTopLevelItemGroup -> "Compile item is not in a top-level ItemGroup"
            | ConditionalItemGroup -> "Compile item is in an ItemGroup with a Condition"
            | UpdateOrRemove -> "Compile item uses Update or Remove"
            | UnsupportedAttribute name -> $"Compile item has the unsupported attribute '{name}'"
            | UnsupportedMetadataElement name ->
                $"Compile item has the unsupported metadata element '{name}'"
            | MissingInclude -> "Compile item has no Include"
            | UnsupportedInclude ->
                "Compile Include must name one file without wildcards, lists, or MSBuild expressions"
            | UnsupportedCondition condition ->
                $"Compile item has the condition \"{condition}\". The Sources package supports no condition or \"{NotFableCondition}\""

        match error.Problem with
        | MalformedXml _ -> $"{error.Project}: {reason}"
        | _ -> $"{error.Project}: {reason}: {error.Element}"

    let describeSourceError (error: SourceError) =
        let reason =
            match error.Problem with
            | SourceFileNotFound -> "file not found"
            | PreprocessorToken token ->
                $"contains '{token}', which NuGet treats as a preprocessor token"
            | ExplicitPublic ->
                "column-0 declaration with an explicit 'public' modifier ignores FsToolkitErrorHandlingVisibility"
            | ColumnZeroTypeExtension -> "column-0 type extension"
            | ColumnZeroModuleRec -> "column-0 'module rec'"
            | ColumnZeroException -> "column-0 exception declaration"
            | UnrecognizedDeclaration keyword -> $"unrecognized '{keyword}' declaration"
            | UnrecognizedLine -> "unrecognized column-0 line"
            | CodeAfterComment -> "column-0 line has code after a block comment"
            | CommentOpenAfterCode ->
                "column-0 line opens a block comment after code and does not close it"
            | UnterminatedCommentOrString -> "the file ends inside a block comment or string"
            | NoDeclaration -> "has no column-0 namespace or module declaration"
            | CodeBeforeFableGuard ->
                "a file excluded from Fable must have only comments before its first namespace or module declaration"

        match error.Line with
        | Some line -> $"{error.File}({line}): {reason}: {error.Text}"
        | None -> $"{error.File}: {reason}"

    let describeGenerateError (error: GenerateError) =
        match error with
        | InvalidProject errors ->
            errors
            |> List.map describeProjectError
            |> String.concat "\n"
        | TooManyFiles(project, count) ->
            $"{project} has {count} Compile items. The file name prefix supports at most {MaxFiles}."
        | InvalidSources errors ->
            errors
            |> List.map describeSourceError
            |> String.concat "\n"

    let private allowedCompileAttributes =
        set [
            "Include"
            "Condition"
        ]

    let private unsupportedIncludeCharacters = [
        "*"
        "?"
        ";"
        "$("
        "@("
        "%("
    ]

    let private parseCompileElement (project: string) (element: XElement) =
        let fail problem =
            Error {
                Project = project
                Element = element.ToString(SaveOptions.DisableFormatting)
                Problem = problem
            }

        let attribute name =
            element.Attribute(XName.Get name)
            |> Option.ofObj
            |> Option.map _.Value

        let group = element.Parent

        let unsupportedAttribute =
            element.Attributes()
            |> Seq.map _.Name.LocalName
            |> Seq.tryFind (fun name -> not (allowedCompileAttributes.Contains name))

        if
            isNull group
            || group.Name.LocalName
               <> "ItemGroup"
            || not (obj.ReferenceEquals(group.Parent, element.Document.Root))
        then
            fail NotInTopLevelItemGroup
        elif not (isNull (group.Attribute(XName.Get "Condition"))) then
            fail ConditionalItemGroup
        elif
            (attribute "Update").IsSome
            || (attribute "Remove").IsSome
        then
            fail UpdateOrRemove
        else
            match unsupportedAttribute, Seq.tryHead (element.Elements()), attribute "Include" with
            | Some name, _, _ -> fail (UnsupportedAttribute name)
            | None, Some child, _ -> fail (UnsupportedMetadataElement child.Name.LocalName)
            | None, None, None -> fail MissingInclude
            | None, None, Some path when
                String.IsNullOrWhiteSpace path
                || unsupportedIncludeCharacters
                   |> List.exists (fun text -> path.Contains(text, StringComparison.Ordinal))
                ->
                fail UnsupportedInclude
            | None, None, Some path ->
                Ok(
                    element.ToString(SaveOptions.DisableFormatting),
                    {
                        CompileItem.Include = path
                        Condition = attribute "Condition"
                    }
                )

    let private collect (results: Result<'T, 'E list> list) =
        match
            results
            |> List.collect (
                function
                | Error errors -> errors
                | Ok _ -> []
            )
        with
        | [] ->
            results
            |> List.choose (
                function
                | Ok value -> Some value
                | Error _ -> None
            )
            |> Ok
        | errors -> Error errors

    let private parseCompileElements (project: string) (projectXml: string) =
        let document =
            try
                Ok(XDocument.Parse projectXml)
            with :? XmlException as ex ->
                Error [
                    {
                        Project = project
                        Element = ""
                        Problem = MalformedXml ex.Message
                    }
                ]

        document
        |> Result.bind (fun document ->
            document.Descendants()
            |> Seq.filter (fun e -> e.Name.LocalName = "Compile")
            |> Seq.map (
                parseCompileElement project
                >> Result.mapError List.singleton
            )
            |> Seq.toList
            |> collect
        )

    let readCompileItems (project: string) (projectXml: string) =
        parseCompileElements project projectXml
        |> Result.map (List.map snd)

    let packagedCompileItems (project: string) (projectXml: string) =
        parseCompileElements project projectXml
        |> Result.bind (fun items ->
            items
            |> List.map (fun (element, item) ->
                match item.Condition with
                | None ->
                    Ok {
                        Include = item.Include
                        Condition = AllCompilers
                    }
                | Some NotFableCondition ->
                    Ok {
                        Include = item.Include
                        Condition = ExcludedFromFable
                    }
                | Some condition ->
                    Error [
                        {
                            Project = project
                            Element = element
                            Problem = UnsupportedCondition condition
                        }
                    ]
            )
            |> collect
        )

    type private StringKind =
        | Regular
        | Verbatim
        | Triple

    type private StringStep =
        | StringClosed of next: int
        | StringOpen of next: int

    type private LexState =
        | Code
        | InString of StringKind
        | InComment of depth: int
        | InCommentString of depth: int * StringKind

    let private charLiteral =
        Regex(@"\G'(?:[^'\\\n]|\\(?:[ntbrafv\\""'0]|u[0-9a-fA-F]{4}|U[0-9a-fA-F]{8}|[0-9]{3}))'")

    let private scanLine (start: LexState) (line: string) =
        let codeOnly = line.ToCharArray()

        let at (index: int) (text: string) =
            index
            + text.Length
            <= line.Length
            && String.CompareOrdinal(line, index, text, 0, text.Length) = 0

        let blank first last =
            for index in first .. last - 1 do
                codeOnly[index] <- ' '

        let stringEnd kind index =
            match kind with
            | Triple when at index "\"\"\"" -> StringClosed(index + 3)
            | Verbatim when at index "\"\"" -> StringOpen(index + 2)
            | Regular when line[index] = '\\' -> StringOpen(min line.Length (index + 2))
            | Verbatim
            | Regular when line[index] = '"' -> StringClosed(index + 1)
            | Triple
            | Verbatim
            | Regular -> StringOpen(index + 1)

        let stringStart index =
            if at index "\"\"\"" then Some(Triple, index + 3)
            elif at index "@$\"" then Some(Verbatim, index + 3)
            elif at index "@\"" then Some(Verbatim, index + 2)
            elif line[index] = '"' then Some(Regular, index + 1)
            else None

        let charLiteralEnd index =
            let m = charLiteral.Match(line, index)

            if m.Success then
                index
                + m.Length
            else
                index + 1

        let rec scan state index =
            if
                index
                >= line.Length
            then
                state
            else
                match state with
                | Code ->
                    if at index "//" then
                        blank index line.Length
                        Code
                    elif at index "(*)" then
                        scan Code (index + 3)
                    elif at index "(*" then
                        blank index (index + 2)
                        scan (InComment 1) (index + 2)
                    else
                        match stringStart index with
                        | Some(kind, next) -> scan (InString kind) next
                        | None when line[index] = '\'' -> scan Code (charLiteralEnd index)
                        | None -> scan Code (index + 1)
                | InString kind ->
                    match stringEnd kind index with
                    | StringClosed next -> scan Code next
                    | StringOpen next -> scan (InString kind) next
                | InComment depth ->
                    let next, after =
                        if at index "(*)" then
                            InComment depth, index + 3
                        elif at index "(*" then
                            InComment(depth + 1), index + 2
                        elif at index "*)" then
                            (if depth = 1 then Code else InComment(depth - 1)), index + 2
                        else
                            match stringStart index with
                            | Some(kind, next) -> InCommentString(depth, kind), next
                            | None when line[index] = '\'' -> InComment depth, charLiteralEnd index
                            | None -> InComment depth, index + 1

                    blank index after
                    scan next after
                | InCommentString(depth, kind) ->
                    let next, after =
                        match stringEnd kind index with
                        | StringClosed after -> InComment depth, after
                        | StringOpen after -> InCommentString(depth, kind), after

                    blank index after
                    scan next after

        let finish = scan start 0
        finish, String(codeOnly)

    type private Declaration =
        | NotDeclaration
        | Header of visibilityAt: int option
        | Member of visibilityAt: int option

    type private LineKind =
        | NoCode
        | StringContent
        | Indented of codeStart: int
        | TopLevel of Declaration

    let private declaration = Regex(@"^(module|type|and|exception)(\s|$)")
    let private accessModifier = Regex(@"^(internal|private|public)(\s|$)")
    let private typeExtension = Regex(@"\swith$")
    let private identifierStart = Regex(@"^[A-Za-z_]")
    let private attributeWithCode = Regex(@"^\[<.*>\]\s*\S")

    let private notDeclarationStarts = [
        "#"
        "open "
        "let "
    ]

    let private startsWith (text: string) (prefix: string) =
        text.StartsWith(prefix, StringComparison.Ordinal)

    let private classifyColumnZero (code: string) =
        let m = declaration.Match code

        if startsWith code "namespace " then
            Ok(Header None)
        elif startsWith code "[<" then
            if attributeWithCode.IsMatch code then
                Error UnrecognizedLine
            else
                Ok NotDeclaration
        elif not m.Success then
            if
                notDeclarationStarts
                |> List.exists (startsWith code)
            then
                Ok NotDeclaration
            else
                Error UnrecognizedLine
        else
            let keyword = m.Groups[1].Value
            let rest = code.Substring(keyword.Length).TrimStart()
            let modifier = accessModifier.Match rest

            let place visibilityAt =
                if
                    keyword = "module"
                    && not (code.Contains '=')
                then
                    Header visibilityAt
                else
                    Member visibilityAt

            match keyword with
            | "exception" -> Error ColumnZeroException
            | _ when
                modifier.Success
                && modifier.Groups[1].Value = "public"
                ->
                Error ExplicitPublic
            | _ when modifier.Success -> Ok(place None)
            | "module" when
                rest = "rec"
                || startsWith rest "rec "
                ->
                Error ColumnZeroModuleRec
            | "type"
            | "and" when typeExtension.IsMatch rest -> Error ColumnZeroTypeExtension
            | _ when identifierStart.IsMatch rest -> Ok(place (Some keyword.Length))
            | _ -> Error(UnrecognizedDeclaration keyword)

    let private isCommentState state =
        match state with
        | InComment _
        | InCommentString _ -> true
        | Code
        | InString _ -> false

    let private classifyLine (start: LexState) (finish: LexState) (line: string) (code: string) =
        let codeStart =
            code
            |> Seq.tryFindIndex (
                Char.IsWhiteSpace
                >> not
            )

        match start, codeStart with
        | InString _, _ -> Ok StringContent
        | _, None -> Ok NoCode
        | _, Some 0 when isCommentState finish -> Error CommentOpenAfterCode
        | _, Some 0 ->
            classifyColumnZero (code.TrimEnd())
            |> Result.map TopLevel
        | _, Some _ when not (Char.IsWhiteSpace line[0]) -> Error CodeAfterComment
        | _, Some index -> Ok(Indented index)

    let private namespaceReference =
        Regex(
            @"\G(?<head>(?:namespace|module|open)\s+(?:(?:internal|private|public|rec|type|"
            + Regex.Escape VisibilityToken
            + @")\s+)*(?:global\.)?)"
            + Regex.Escape SourceNamespace
            + @"(?![\w'])"
        )

    let private replaceNamespace (codeStart: int) (line: string) =
        namespaceReference.Replace(
            line,
            "${head}"
            + NamespaceToken,
            1,
            codeStart
        )

    let private insertVisibility (visibilityAt: int option) (line: string) =
        match visibilityAt with
        | Some index ->
            line.Substring(0, index)
            + " "
            + VisibilityToken
            + line.Substring index
        | None -> line

    let private preprocessorToken = Regex(@"\$\w+\$")

    let transform (condition: PackagedCondition) (fileName: string) (source: string) =
        let lines =
            source.TrimStart(char 0xFEFF).Replace("\r\n", "\n").Replace("\r", "\n").Split '\n'

        let error (index: int) problem = {
            File = fileName
            Line = Some(index + 1)
            Text = lines[index]
            Problem = problem
        }

        let tokenErrors =
            lines
            |> Array.toList
            |> List.indexed
            |> List.choose (fun (index, line) ->
                let m = preprocessorToken.Match line

                if m.Success then
                    Some(error index (PreprocessorToken m.Value))
                else
                    None
            )

        let finalState, classified =
            ((Code, []), Array.indexed lines)
            ||> Array.fold (fun (state, acc) (index, line) ->
                let finish, code = scanLine state line

                let kind =
                    classifyLine state finish line code
                    |> Result.mapError (error index)

                finish,
                (index, kind)
                :: acc
            )

        let kinds = List.rev classified

        let lineErrors =
            kinds
            |> List.choose (
                function
                | _, Error e -> Some e
                | _, Ok _ -> None
            )

        let endErrors =
            match finalState with
            | Code -> []
            | InString _
            | InComment _
            | InCommentString _ -> [
                error
                    (lines.Length
                     - 1)
                    UnterminatedCommentOrString
              ]

        let rewritten =
            kinds
            |> List.choose (
                function
                | index, Ok kind -> Some(index, kind)
                | _, Error _ -> None
            )

        let isHeader kind =
            match kind with
            | TopLevel(Header _) -> true
            | _ -> false

        let firstHeader =
            rewritten
            |> List.tryFind (
                snd
                >> isHeader
            )
            |> Option.map fst

        let structureErrors =
            match firstHeader, condition with
            | None, _ when not lineErrors.IsEmpty -> []
            | None, _ -> [
                {
                    File = fileName
                    Line = None
                    Text = ""
                    Problem = NoDeclaration
                }
              ]
            | Some header, ExcludedFromFable ->
                rewritten
                |> List.tryFind (fun (index, kind) ->
                    index < header
                    && kind
                       <> NoCode
                )
                |> Option.map (fun (index, _) -> error index CodeBeforeFableGuard)
                |> Option.toList
            | Some _, AllCompilers -> []

        match
            tokenErrors
            @ lineErrors
            @ endErrors
            @ structureErrors,
            firstHeader
        with
        | [], Some header ->
            let output =
                rewritten
                |> List.collect (fun (index, kind) ->
                    let line = lines[index]

                    let text =
                        match kind with
                        | TopLevel(Header visibilityAt)
                        | TopLevel(Member visibilityAt) ->
                            insertVisibility visibilityAt line
                            |> replaceNamespace 0
                        | TopLevel NotDeclaration -> replaceNamespace 0 line
                        | Indented codeStart -> replaceNamespace codeStart line
                        | NoCode
                        | StringContent -> line

                    [
                        text
                        if isHeader kind then
                            // F# 9 rejects a #nowarn line before a top-level 'module A.B' declaration (FS0010, FS0222).
                            NoWarnLine
                        if
                            index = header
                            && condition = ExcludedFromFable
                        then
                            FableGuardStart
                    ]
                )

            let guarded =
                match condition, List.rev output with
                | AllCompilers, _ -> output
                | ExcludedFromFable, "" :: body ->
                    List.rev (
                        ""
                        :: FableGuardEnd
                        :: body
                    )
                | ExcludedFromFable, body ->
                    List.rev (
                        FableGuardEnd
                        :: body
                    )

            Ok(String.concat "\n" guarded)
        | errors, _ -> Error errors

    let packageFileName (position: int) (sourceFileName: string) =
        sprintf "%03d_%s.pp" position (Path.GetFileName sourceFileName)

    type private GeneratedFile = { FileName: string; Text: string }

    let private deleteDirectory (path: string) =
        if Directory.Exists path then
            Directory.Delete(path, true)

    let generate (projectPath: string) (outputDir: string) =
        let outputDir = Path.TrimEndingDirectorySeparator(Path.GetFullPath outputDir)

        let stagingDir =
            outputDir
            + ".staging"

        deleteDirectory outputDir
        deleteDirectory stagingDir

        let projectDir = Path.GetDirectoryName(Path.GetFullPath projectPath)
        let encoding = UTF8Encoding(false)

        let readAndTransform index (source: PackagedSource) =
            let sourcePath = Path.Combine(projectDir, source.Include)

            if File.Exists sourcePath then
                File.ReadAllText(sourcePath, encoding)
                |> transform source.Condition sourcePath
                |> Result.map (fun text -> {
                    FileName = packageFileName (index + 1) source.Include
                    Text = text
                })
            else
                Error [
                    {
                        File = sourcePath
                        Line = None
                        Text = ""
                        Problem = SourceFileNotFound
                    }
                ]

        match
            File.ReadAllText projectPath
            |> packagedCompileItems projectPath
        with
        | Error errors -> Error(InvalidProject errors)
        | Ok sources when sources.Length > MaxFiles ->
            Error(TooManyFiles(projectPath, sources.Length))
        | Ok sources ->
            match
                sources
                |> List.mapi readAndTransform
                |> collect
            with
            | Error errors -> Error(InvalidSources errors)
            | Ok files ->
                try
                    Directory.CreateDirectory stagingDir
                    |> ignore

                    for file in files do
                        File.WriteAllText(
                            Path.Combine(stagingDir, file.FileName),
                            file.Text,
                            encoding
                        )

                    Directory.Move(stagingDir, outputDir)
                finally
                    deleteDirectory stagingDir

                files
                |> List.map (fun file -> Path.Combine(outputDir, file.FileName))
                |> Ok

    let publicTypesInNamespace (assemblyPath: string) (rootNamespace: string) =
        use stream = File.OpenRead assemblyPath
        use peReader = new PEReader(stream)
        let reader = peReader.GetMetadataReader()

        reader.TypeDefinitions
        |> Seq.map reader.GetTypeDefinition
        |> Seq.filter (fun t ->
            t.Attributes
            &&& TypeAttributes.VisibilityMask = TypeAttributes.Public
        )
        |> Seq.map (fun t -> reader.GetString t.Namespace, reader.GetString t.Name)
        |> Seq.filter (fun (ns, _) ->
            ns = rootNamespace
            || startsWith
                ns
                (rootNamespace
                 + ".")
        )
        |> Seq.map (fun (ns, name) ->
            ns
            + "."
            + name
        )
        |> Seq.sort
        |> Seq.toList
