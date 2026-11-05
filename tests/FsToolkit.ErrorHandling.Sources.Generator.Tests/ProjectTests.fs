module FsToolkit.ErrorHandling.Sources.Generator.Tests.ProjectTests

open System
open System.IO
open Expecto
open FsToolkit.Build.SourcesPackage

let private project (body: string) = $"<Project>{body}</Project>"

let private itemGroup (items: string) = $"<ItemGroup>{items}</ItemGroup>"

let private projectProblems (result: Result<'T, ProjectError list>) =
    match result with
    | Ok value -> failtestf "Expected a failure, but got %A" value
    | Error errors ->
        errors
        |> List.map _.Problem

let private rejectedByBoth (xml: string) (expected: ProjectProblem) =
    Expect.equal (projectProblems (readCompileItems "P.fsproj" xml)) [ expected ] "readCompileItems"

    Expect.equal
        (projectProblems (packagedCompileItems "P.fsproj" xml))
        [ expected ]
        "packagedCompileItems"

let private withTempDirectory (action: string -> unit) =
    let directory =
        Path.Combine(
            Path.GetTempPath(),
            "fstk-sources-generator-tests",
            Guid.NewGuid().ToString "N"
        )

    Directory.CreateDirectory directory
    |> ignore

    try
        action directory
    finally
        Directory.Delete(directory, true)

let private writeProject (directory: string) (body: string) (files: (string * string) list) =
    let path = Path.Combine(directory, "Source.fsproj")
    File.WriteAllText(path, project body)

    for name, text in files do
        File.WriteAllText(Path.Combine(directory, name), text)

    path

let private validSource =
    "namespace FsToolkit.ErrorHandling\n\nmodule M =\n    let x = 1\n"

let private expectNoOutput (outputDir: string) =
    Expect.isFalse (Directory.Exists outputDir) "The output directory must not exist"

    Expect.isFalse
        (Directory.Exists(
            outputDir
            + ".staging"
        ))
        "The staging directory must not exist"

let private generateFails
    (body: string)
    (files: (string * string) list)
    (check: GenerateError -> unit)
    =
    withTempDirectory (fun directory ->
        let projectPath = writeProject directory body files
        let outputDir = Path.Combine(directory, "out")

        Directory.CreateDirectory outputDir
        |> ignore

        File.WriteAllText(Path.Combine(outputDir, "001_Stale.fs.pp"), "stale")

        match generate projectPath outputDir with
        | Ok files -> failtestf "Expected a failure, but generate wrote %A" files
        | Error error -> check error

        expectNoOutput outputDir
    )

[<Tests>]
let projectShape =
    testList "project shape" [
        test "an accepted project keeps the order and the Fable condition" {
            let xml =
                project (
                    itemGroup
                        "<Compile Include=\"A.fs\" /><Compile Include=\"B.fs\" Condition=\"'$(FABLE_COMPILER)' != 'true'\" />"
                )

            Expect.equal
                (packagedCompileItems "P.fsproj" xml)
                (Ok [
                    {
                        Include = "A.fs"
                        Condition = AllCompilers
                    }
                    {
                        Include = "B.fs"
                        Condition = ExcludedFromFable
                    }
                ])
                "Packaged items"
        }

        test "Compile without Include" {
            rejectedByBoth (project (itemGroup "<Compile />")) MissingInclude
        }

        test "Compile with Update" {
            rejectedByBoth (project (itemGroup "<Compile Update=\"A.fs\" />")) UpdateOrRemove
        }

        test "Compile with Remove" {
            rejectedByBoth (project (itemGroup "<Compile Remove=\"A.fs\" />")) UpdateOrRemove
        }

        test "Compile in an ItemGroup with a Condition" {
            rejectedByBoth
                (project "<ItemGroup Condition=\"false\"><Compile Include=\"A.fs\" /></ItemGroup>")
                ConditionalItemGroup
        }

        test "Compile in Choose" {
            rejectedByBoth
                (project
                    "<Choose><When Condition=\"true\"><ItemGroup><Compile Include=\"A.fs\" /></ItemGroup></When></Choose>")
                NotInTopLevelItemGroup
        }

        test "Compile in a Target" {
            rejectedByBoth
                (project
                    "<Target Name=\"T\"><ItemGroup><Compile Include=\"A.fs\" /></ItemGroup></Target>")
                NotInTopLevelItemGroup
        }

        test "Compile with a wildcard" {
            rejectedByBoth (project (itemGroup "<Compile Include=\"*.fs\" />")) UnsupportedInclude
        }

        test "Compile with CompileOrder" {
            rejectedByBoth
                (project (itemGroup "<Compile Include=\"A.fs\" CompileOrder=\"CompileFirst\" />"))
                (UnsupportedAttribute "CompileOrder")
        }

        test "Compile with a metadata element" {
            rejectedByBoth
                (project (itemGroup "<Compile Include=\"A.fs\"><Link>B.fs</Link></Compile>"))
                (UnsupportedMetadataElement "Link")
        }

        test "malformed XML" {
            match projectProblems (packagedCompileItems "P.fsproj" "<Project>") with
            | [ MalformedXml _ ] -> ()
            | other -> failtestf "Unexpected problems: %A" other
        }

        test "an unsupported item condition fails only for packaging" {
            let xml =
                project (itemGroup "<Compile Include=\"A.fs\" Condition=\"'$(X)' == 'y'\" />")

            Expect.equal
                (readCompileItems "P.fsproj" xml)
                (Ok [
                    {
                        Include = "A.fs"
                        Condition = Some "'$(X)' == 'y'"
                    }
                ])
                "readCompileItems"

            Expect.equal
                (projectProblems (packagedCompileItems "P.fsproj" xml))
                [ UnsupportedCondition "'$(X)' == 'y'" ]
                "packagedCompileItems"
        }

        test "every rejected element is reported" {
            Expect.equal
                (projectProblems (
                    readCompileItems
                        "P.fsproj"
                        (project (itemGroup "<Compile /><Compile Update=\"A.fs\" />"))
                ))
                [
                    MissingInclude
                    UpdateOrRemove
                ]
                "Problems"
        }

        test "the message names the project and the element" {
            match
                readCompileItems "P.fsproj" (project (itemGroup "<Compile Update=\"A.fs\" />"))
            with
            | Error [ error ] ->
                Expect.equal
                    (describeProjectError error)
                    "P.fsproj: Compile item uses Update or Remove: <Compile Update=\"A.fs\" />"
                    "Message"
            | other -> failtestf "Unexpected result: %A" other
        }
    ]

[<Tests>]
let generation =
    testList "generate" [
        test "a malformed project leaves no output directory" {
            generateFails
                "<ItemGroup>"
                []
                (fun error ->
                    match error with
                    | InvalidProject [ { Problem = MalformedXml _ } ] -> ()
                    | other -> failtestf "Unexpected error: %A" other
                )
        }

        test "an unsupported project shape leaves no output directory" {
            generateFails
                (itemGroup "<Compile Update=\"A.fs\" />")
                [ "A.fs", validSource ]
                (fun error ->
                    match error with
                    | InvalidProject [ {
                                           Element = "<Compile Update=\"A.fs\" />"
                                           Problem = UpdateOrRemove
                                       } ] -> ()
                    | other -> failtestf "Unexpected error: %A" other
                )
        }

        test "too many files leaves no output directory" {
            let items =
                [ 1..1000 ]
                |> List.map (sprintf "<Compile Include=\"F%d.fs\" />")
                |> String.concat ""

            generateFails
                (itemGroup items)
                []
                (fun error ->
                    match error with
                    | TooManyFiles(_, 1000) -> ()
                    | other -> failtestf "Unexpected error: %A" other
                )
        }

        test "a missing source file leaves no output directory" {
            generateFails
                (itemGroup "<Compile Include=\"Missing.fs\" />")
                []
                (fun error ->
                    match error with
                    | InvalidSources [ { Problem = SourceFileNotFound } ] -> ()
                    | other -> failtestf "Unexpected error: %A" other
                )
        }

        test "a source failure in a later file leaves no output directory" {
            generateFails
                (itemGroup "<Compile Include=\"A.fs\" /><Compile Include=\"B.fs\" />")
                [
                    "A.fs", validSource
                    "B.fs", "namespace FsToolkit.ErrorHandling\ntype Foo with\n"
                ]
                (fun error ->
                    match error with
                    | InvalidSources [ {
                                           Line = Some 2
                                           Problem = ColumnZeroTypeExtension
                                       } ] -> ()
                    | other -> failtestf "Unexpected error: %A" other
                )
        }

        test "a success replaces stale output and staging files" {
            withTempDirectory (fun directory ->
                let projectPath =
                    writeProject
                        directory
                        (itemGroup
                            "<Compile Include=\"A.fs\" /><Compile Include=\"Task.fs\" Condition=\"'$(FABLE_COMPILER)' != 'true'\" />")
                        [
                            "A.fs", validSource
                            "Task.fs", validSource
                        ]

                let outputDir = Path.Combine(directory, "out")

                for stale in
                    [
                        outputDir
                        outputDir
                        + ".staging"
                    ] do
                    Directory.CreateDirectory stale
                    |> ignore

                    File.WriteAllText(Path.Combine(stale, "009_Stale.fs.pp"), "stale")

                match generate projectPath outputDir with
                | Error error -> failtestf "Unexpected error: %s" (describeGenerateError error)
                | Ok files ->
                    Expect.equal
                        (files
                         |> List.map Path.GetFileName)
                        [
                            "001_A.fs.pp"
                            "002_Task.fs.pp"
                        ]
                        "Returned files"

                    Expect.equal
                        (Directory.GetFiles outputDir
                         |> Array.map Path.GetFileName
                         |> Array.sort)
                        [|
                            "001_A.fs.pp"
                            "002_Task.fs.pp"
                        |]
                        "Files on disk"

                    Expect.isFalse
                        (Directory.Exists(
                            outputDir
                            + ".staging"
                        ))
                        "The staging directory must not exist"

                    let read name =
                        File.ReadAllText(Path.Combine(outputDir, name))

                    Expect.isFalse
                        ((read "001_A.fs.pp").Contains FableGuardStart)
                        "A.fs is compiled by Fable"

                    Expect.stringContains
                        (read "002_Task.fs.pp")
                        FableGuardStart
                        "Task.fs is excluded from Fable"

                    Expect.isTrue
                        ((read "002_Task.fs.pp")
                            .EndsWith(
                                FableGuardEnd
                                + "\n"
                            ))
                        "Task.fs ends the Fable guard"
            )
        }
    ]
