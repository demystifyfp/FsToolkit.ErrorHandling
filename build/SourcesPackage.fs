namespace FsToolkit.Build

module SourcesPackage =
    open System
    open System.IO
    open System.Reflection
    open System.Reflection.Metadata
    open System.Reflection.PortableExecutable
    open System.Text
    open System.Text.RegularExpressions
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
    let MaxFiles = 999

    [<Literal>]
    let NotFableCondition = "'$(FABLE_COMPILER)' != 'true'"

    type CompileItem = {
        Include: string
        Condition: string option
    }

    type TopLevelLine =
        | NotDeclaration
        | HasAccessModifier
        | NeedsVisibility of keywordEnd: int
        | Unsupported of reason: string

    let readCompileItems (projectXml: string) =
        XDocument.Parse(projectXml).Descendants()
        |> Seq.filter (fun e -> e.Name.LocalName = "Compile")
        |> Seq.map (fun e -> {
            Include = e.Attribute(XName.Get "Include").Value
            Condition =
                e.Attribute(XName.Get "Condition")
                |> Option.ofObj
                |> Option.map _.Value
        })
        |> Seq.toList

    let packagedCompileItems (projectXml: string) =
        readCompileItems projectXml
        |> List.map (fun item ->
            match item.Condition with
            | None
            | Some NotFableCondition -> item.Include
            | Some condition ->
                failwithf
                    "Compile item '%s' has the condition \"%s\". The Sources package supports no condition or \"%s\"."
                    item.Include
                    condition
                    NotFableCondition
        )

    let private startsWith (text: string) (prefix: string) =
        text.StartsWith(prefix, StringComparison.Ordinal)

    let private declaration = Regex(@"^(module|type|and|exception)(\s|$)")
    let private accessModifier = Regex(@"^(internal|private|public)\s")
    let private typeExtension = Regex(@"\swith\s*$")
    let private identifierStart = Regex(@"^[A-Za-z_]")

    let private notDeclarationStarts = [
        "namespace "
        "open "
        "let "
        "#"
        "//"
        "[<"
    ]

    let classifyLine (line: string) =
        let m = declaration.Match line

        if
            line.Length = 0
            || Char.IsWhiteSpace line[0]
        then
            NotDeclaration
        elif not m.Success then
            if
                notDeclarationStarts
                |> List.exists (startsWith line)
            then
                NotDeclaration
            else
                Unsupported "unrecognized column-0 line"
        else
            let keyword = m.Groups[1].Value
            let rest = line.Substring(keyword.Length).TrimStart()

            match keyword with
            | "exception" -> Unsupported "column-0 exception declaration"
            | _ when accessModifier.IsMatch rest -> HasAccessModifier
            | "module" when
                rest = "rec"
                || startsWith rest "rec "
                ->
                Unsupported "column-0 'module rec'"
            | "type"
            | "and" when typeExtension.IsMatch rest -> Unsupported "column-0 type extension"
            | _ when identifierStart.IsMatch rest -> NeedsVisibility keyword.Length
            | _ -> Unsupported $"unrecognized '{keyword}' declaration"

    let private addVisibility (fileName: string) (lines: string array) =
        let mutable inBlockComment = false

        lines
        |> Array.mapi (fun index line ->
            if inBlockComment then
                inBlockComment <- not (line.Contains "*)")
                line
            elif
                startsWith line "(*"
                && not (line.Contains "*)")
            then
                inBlockComment <- true
                line
            else
                match classifyLine line with
                | NotDeclaration
                | HasAccessModifier -> line
                | NeedsVisibility keywordEnd ->
                    line.Substring(0, keywordEnd)
                    + " "
                    + VisibilityToken
                    + line.Substring keywordEnd
                | Unsupported reason -> failwithf "%s(%d): %s: %s" fileName (index + 1) reason line
        )

    let transform (fileName: string) (source: string) =
        let text = source.TrimStart(char 0xFEFF).Replace("\r\n", "\n").Replace("\r", "\n")

        let token = Regex.Match(text, @"\$\w+\$")

        if token.Success then
            failwithf
                "%s: contains '%s', which NuGet treats as a preprocessor token."
                fileName
                token.Value

        let lines =
            text.Split '\n'
            |> addVisibility fileName

        let firstDeclaration =
            lines
            |> Array.tryFindIndex (fun line ->
                startsWith line "namespace "
                || startsWith line "module "
            )

        match firstDeclaration with
        | None -> failwithf "%s: has no column-0 namespace or module declaration." fileName
        | Some index ->
            // F# 9 rejects a #nowarn line before a top-level 'module A.B' declaration (FS0010, FS0222).
            (Array.insertAt (index + 1) NoWarnLine lines
             |> String.concat "\n")
                .Replace(SourceNamespace, NamespaceToken)

    let packageFileName (position: int) (sourceFileName: string) =
        sprintf "%03d_%s.pp" position (Path.GetFileName sourceFileName)

    let generate (projectPath: string) (outputDir: string) =
        let projectDir = Path.GetDirectoryName projectPath

        let includes =
            File.ReadAllText projectPath
            |> packagedCompileItems

        if includes.Length > MaxFiles then
            failwithf
                "%s has %d Compile items. The file name prefix supports at most %d."
                projectPath
                includes.Length
                MaxFiles

        if Directory.Exists outputDir then
            Directory.Delete(outputDir, true)

        let encoding = UTF8Encoding(false)

        let files =
            includes
            |> List.mapi (fun index includePath ->
                let sourcePath = Path.Combine(projectDir, includePath)

                Path.Combine(outputDir, packageFileName (index + 1) includePath),
                transform sourcePath (File.ReadAllText(sourcePath, encoding))
            )

        Directory.CreateDirectory outputDir
        |> ignore

        files
        |> List.map (fun (outputPath, contents) ->
            File.WriteAllText(outputPath, contents, encoding)
            outputPath
        )

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
