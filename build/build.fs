open Fake.Api
open Fake.Core
open Fake.IO
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators
open Fake.Tools
open Fake.JavaScript
open System.IO
open Fake.BuildServer
open FsToolkit.Build

let environVarAsBoolOrDefault varName defaultValue =
    let truthyConsts = [
        "1"
        "Y"
        "YES"
        "T"
        "TRUE"
    ]

    try
        let envvar = (Environment.environVar varName).ToUpper()

        truthyConsts
        |> List.exists ((=) envvar)
    with _ ->
        defaultValue

let project = "FsToolkit.ErrorHandling"

let publishUrl = "https://www.nuget.org"

let summary =
    "FsToolkit.ErrorHandling is a utility library to work with the Result type in F# enabling you to do clear, simple and powerful error handling."

let isCI = lazy (environVarAsBoolOrDefault "CI" false)

let isRelease (targets: Target list) =
    targets
    |> Seq.map _.Name
    |> Seq.exists ((=) "Release")

let configuration (targets: Target list) =
    let defaultVal = if isRelease targets then "Release" else "Debug"

    match Environment.environVarOrDefault "CONFIGURATION" defaultVal with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let solutionFile = "FsToolkit.ErrorHandling.slnx"

let rootDir =
    __SOURCE_DIRECTORY__
    </> ".."

let srcGlob =
    rootDir
    </> "src/**/*.??proj"

let testsGlob =
    rootDir
    </> "tests/**/*.??proj"

let srcAndTest =
    !!srcGlob
    ++ testsGlob
    // These projects do not track an AssemblyInfo.fs, and GitRelease stages every file.
    -- (rootDir
        </> "tests/FsToolkit.ErrorHandling.Sources.*/**")
    -- (rootDir
        </> "tests/FsToolkit.ErrorHandling.WebSharper/**")

let srcCodeGlob =
    !!(rootDir
       </> "src/**/*.fs")
    ++ (rootDir
        </> "src/**/*.fsx")
    -- (rootDir
        </> "src/**/obj/**/*.fs")

let testsCodeGlob =
    !!(rootDir
       </> "tests/**/*.fs")
    ++ (rootDir
        </> "tests/**/*.fsx")
    -- (rootDir
        </> "tests/**/obj/**/*.fs")

let gitOwner = "demystifyfp"

let distDir =
    rootDir
    @@ "bin"

let distGlob =
    distDir
    @@ "*.nupkg"

let githubToken = lazy (Environment.environVarOrNone "GITHUB_TOKEN")

let nugetToken =
    lazy
        (Environment.environVarOrNone "NUGET_TOKEN"
         |> Option.orElseWith (fun () -> Environment.environVarOrNone "FSTK_NUGET_TOKEN"))


let failOnBadExitAndPrint (p: ProcessResult) =
    if
        p.ExitCode
        <> 0
    then
        p.Errors
        |> Seq.iter Trace.traceError

        failwithf "failed with exitcode %d" p.ExitCode

// github actions are terrible and will cancel runner operations if using too much CPU
// https://github.com/actions/runner-images/discussions/7188#discussioncomment-6672934
let maxCpuCount = lazy (if isCI.Value then Some(Some 2) else None)

/// MaxCpu not used on unix https://github.com/fsprojects/FAKE/blob/82e38df01e4b31e5daa3623abff57e6462430395/src/app/Fake.DotNet.MSBuild/MSBuild.fs#L858-L861
let maxCpuMsBuild =
    lazy
        (match maxCpuCount.Value with
         | None -> ""
         | Some None -> "/m"
         | Some(Some x) -> $"/m:%d{x}")

module dotnet =
    let watch cmdParam program args =
        DotNet.exec cmdParam (sprintf "watch %s" program) args

    let run cmdParam args = DotNet.exec cmdParam "run" args

    let tool optionConfig command args =
        DotNet.exec optionConfig (sprintf "%s" command) args
        |> failOnBadExitAndPrint

    let fantomas args = DotNet.exec id "fantomas" args

    let analyzers args = DotNet.exec id "fsharp-analyzers" args


let formatCode _ =
    let result = dotnet.fantomas "."

    if not result.OK then
        Trace.traceErrorfn "Errors while formatting all files: %A" result.Messages

let analyze _ =
    let analyzerPaths = !!"packages/analyzers/**/analyzers/dotnet/fs"

    let createArgsForProject (project: string) analyzerPaths =
        let projectName = Path.GetFileNameWithoutExtension project

        [
            yield "--project"
            yield project
            yield "--analyzers-path"
            yield! analyzerPaths
            if isCI.Value then
                yield "--report"
                yield $"analysisreports/{projectName}-analysis.sarif"
        ]
        |> String.concat " "

    !!"src/**/*.fsproj"
    |> Seq.iter (fun fsproj ->
        let result =
            createArgsForProject fsproj analyzerPaths
            |> dotnet.analyzers

        result.Errors
        |> Seq.iter Trace.traceError
    )


let checkFormatCode _ =
    let result = dotnet.fantomas "--check ."

    if result.ExitCode = 0 then
        Trace.log "No files need formatting"
    elif result.ExitCode = 99 then
        failwith "Some files need formatting, check output for more info"
    else
        let msg = sprintf "Errors while formatting: %A" result.Errors
        Trace.log msg
        failwith msg


let clean _ =
    !!"bin"
    ++ "benchmarks/**/bin"
    ++ "src/**/bin"
    ++ "tests/**/bin"
    ++ "tools/**/bin"
    ++ "benchmarks/**/obj"
    ++ "src/**/obj"
    ++ "tests/**/obj"
    ++ "tools/**/obj"
    ++ "dist"
    ++ "**/.js-tests"
    ++ "**/.python-tests"
    |> Shell.cleanDirs


module BuildParameters =
    let common (defaults: MSBuild.CliArguments) = {
        defaults with
            DisableInternalBinLog = true
    }

let build ctx =

    let args = [ maxCpuMsBuild.Value ]

    let setParams (c: DotNet.BuildOptions) = {
        c with
            NoRestore = true
            Configuration = (configuration ctx.Context.AllExecutingTargets)
            MSBuildParams = BuildParameters.common c.MSBuildParams
            Common =
                c.Common
                |> DotNet.Options.withAdditionalArgs args
    }

    DotNet.build setParams solutionFile


let restore _ =
    let setParams (c: DotNet.RestoreOptions) = {
        c with
            MSBuildParams = BuildParameters.common c.MSBuildParams
    }

    DotNet.restore setParams solutionFile

let npmRestore _ = Npm.install id


let dotnetTest ctx =

    let args = [
        "--no-build"
        maxCpuMsBuild.Value
    ]

    DotNet.test
        (fun c ->

            {
                c with
                    Configuration = configuration ctx.Context.AllExecutingTargets
                    Common =
                        c.Common
                        |> DotNet.Options.withAdditionalArgs args
                    MSBuildParams = BuildParameters.common c.MSBuildParams
            })
        solutionFile


let runNpmTest _ = Npm.test id


let fableAwareTests = [
    "tests/FsToolkit.ErrorHandling.Tests"
    "tests/FsToolkit.ErrorHandling.AsyncSeq.Tests"
]


let femtoValidate _ =
    for testProject in fableAwareTests do
        let result =
            CreateProcess.fromRawCommand "dotnet" [
                "femto"
                testProject
                "--validate"
            ]
            |> Proc.run

        if
            result.ExitCode
            <> 0
        then
            Fake.Testing.Common.FailedTestsException
                "Femto failed; perhaps you need to update the package.json?"
            |> raise

// Fable 5 ships its Python runtime library on PyPI, and that version must match the fable tool version.
let installFableLibraryForPython () =
    let manifest =
        rootDir
        </> ".config"
        </> "dotnet-tools.json"

    use doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText manifest)

    let fableVersion =
        doc.RootElement.GetProperty("tools").GetProperty("fable").GetProperty("version").GetString()

    let result =
        CreateProcess.fromRawCommand "python" [
            "-m"
            "pip"
            "install"
            "--disable-pip-version-check"
            $"fable-library=={fableVersion}"
        ]
        |> Proc.run

    if
        result.ExitCode
        <> 0
    then
        Fake.Testing.Common.FailedTestsException $"Failed to install fable-library {fableVersion}"
        |> raise

let runPythonTests _ =
    installFableLibraryForPython ()

    for testProject in fableAwareTests do
        let pythonBuildDir =
            testProject
            </> ".python-tests"

        let mainFilePath =
            pythonBuildDir
            </> "main.py"

        let result =
            CreateProcess.fromRawCommand "dotnet" [
                "fable"
                testProject
                "--noCache"
                "--lang"
                "py"
                "-o"
                pythonBuildDir
            ]
            |> Proc.run

        if
            result.ExitCode
            <> 0
        then
            Fake.Testing.Common.FailedTestsException "Failed to build python tests"
            |> raise
        else
            let testsResult =
                CreateProcess.fromRawCommand "python" [ mainFilePath ]
                |> Proc.run

            if
                testsResult.ExitCode
                <> 0
            then
                Fake.Testing.Common.FailedTestsException
                    "Python tests failed, see output for more information."
                |> raise


let release =
    let path =
        rootDir
        </> "RELEASE_NOTES.md"

    let release = ReleaseNotes.load path

    let firstHeadingVersion =
        File.ReadLines path
        |> Seq.tryPick (fun line ->
            let m = System.Text.RegularExpressions.Regex.Match(line, @"^#+\s+\[?([^\]\s]+)")

            if m.Success then Some m.Groups[1].Value else None
        )

    // FAKE 6 parses a "### [x.y.z]" heading as the latest release, whatever its position.
    match firstHeadingVersion with
    | Some version when version = release.NugetVersion -> release
    | other ->
        failwithf
            "RELEASE_NOTES.md: FAKE parsed version %s, but the first heading has version %A."
            release.NugetVersion
            other

let generateAssemblyInfo ctx =
    let getAssemblyInfoAttributes projectName = [
        AssemblyInfo.Title(projectName)
        AssemblyInfo.Product project
        AssemblyInfo.Description summary
        AssemblyInfo.Version release.AssemblyVersion
        AssemblyInfo.FileVersion release.AssemblyVersion
        AssemblyInfo.Configuration(string (configuration ctx.Context.AllExecutingTargets))
    ]

    let getProjectDetails (projectPath: string) =
        let projectName = Path.GetFileNameWithoutExtension(projectPath)

        (projectPath,
         projectName,
         Path.GetDirectoryName(projectPath),
         (getAssemblyInfoAttributes projectName))

    srcAndTest
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (_, _, folderName, attributes) ->
        AssemblyInfoFile.createFSharp
            (folderName
             </> "AssemblyInfo.fs")
            attributes
    )


let releaseNotes = String.toLines release.Notes

let sourcesProjectDir =
    rootDir
    </> "src/FsToolkit.ErrorHandling.Sources"

let sourcesProject =
    sourcesProjectDir
    </> "FsToolkit.ErrorHandling.Sources.proj"

let generateSources _ =
    match
        SourcesPackage.generate
            (rootDir
             </> "src/FsToolkit.ErrorHandling/FsToolkit.ErrorHandling.fsproj")
            (sourcesProjectDir
             </> "obj/generated")
    with
    | Ok files -> Trace.tracefn "Generated %d source files for %s" files.Length sourcesProject
    | Error error -> failwith (SourcesPackage.describeGenerateError error)

let sourcesTestsFixture =
    rootDir
    </> "tests/FsToolkit.ErrorHandling.Sources.Tests/FsToolkit.ErrorHandling.Sources.Tests.fsproj"

let sourcesConsumerFixture =
    rootDir
    </> "tests/FsToolkit.ErrorHandling.Sources.Consumer/FsToolkit.ErrorHandling.Sources.Consumer.fsproj"

let failOnTestFileDrift () =
    let testFiles projectPath =
        match
            File.ReadAllText projectPath
            |> SourcesPackage.readCompileItems projectPath
        with
        | Ok items ->
            items
            |> List.map (fun item ->
                Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(projectPath: string), item.Include)
                ),
                item.Condition
            )
        | Error errors ->
            errors
            |> List.map SourcesPackage.describeProjectError
            |> String.concat "\n"
            |> failwith

    let expected =
        testFiles (
            rootDir
            </> "tests/FsToolkit.ErrorHandling.Tests/FsToolkit.ErrorHandling.Tests.fsproj"
        )

    let actual = testFiles sourcesTestsFixture

    if
        actual
        <> expected
    then
        failwithf
            "The Compile items of %s must resolve to the files of FsToolkit.ErrorHandling.Tests.fsproj, in the same order and with the same conditions.\nExpected: %A\nActual: %A"
            sourcesTestsFixture
            expected
            actual

let failOnDocsDrift () =
    let readNormalized (path: string) =
        (File.ReadAllText path).Replace("\r\n", "\n")

    let readmePath =
        sourcesProjectDir
        </> "README.md"

    let gitbookPath =
        rootDir
        </> "gitbook/sources/sourcesPackage.md"

    if
        not (
            (readNormalized readmePath)
                .StartsWith(
                    readNormalized gitbookPath
                    + "\n## Documentation\n",
                    System.StringComparison.Ordinal
                )
        )
    then
        failwithf
            "%s must be the full text of %s followed by its ## Documentation section. Make the same change in both files."
            readmePath
            gitbookPath

type TargetsRun = {
    Command: string
    ExitCode: int
    Output: string
}

type InvalidTargetsValue = {
    Property: string
    Value: string
    Message: string
}

let failOnTargetsDiagnostics () =
    let validate project properties =
        let result =
            CreateProcess.fromRawCommand
                "dotnet"
                ([
                    "msbuild"
                    project
                    "-nologo"
                    "-tl:off"
                    "-t:FsToolkitErrorHandlingSourcesValidate"
                 ]
                 @ (properties
                    |> List.map (fun (name, value) -> $"-p:{name}={value}")))
            |> CreateProcess.redirectOutput
            |> Proc.run

        let command =
            properties
            |> List.map (fun (name, value) -> $"{name}={value}")
            |> String.concat " "

        Trace.tracefn "%s %s: exit %d" (Path.GetFileName(project: string)) command result.ExitCode

        {
            Command = command
            ExitCode = result.ExitCode
            Output = result.Result.Output
        }

    let targets =
        sourcesProjectDir
        </> "build/FsToolkit.ErrorHandling.Sources.targets"

    let keywordMessage = "A segment is '_', an F# keyword, or a reserved word."

    let invalidValues = [
        {
            Property = "FsToolkitErrorHandlingVisibility"
            Value = "PUBLIC"
            Message = "Set it to 'internal' or 'public' in lowercase."
        }
        {
            Property = "FsToolkitErrorHandlingNamespace"
            Value = "Contoso.type"
            Message = keywordMessage
        }
        {
            Property = "FsToolkitErrorHandlingNamespace"
            Value = "Contoso..Bad"
            Message = "Set it to dotted ASCII F# identifiers"
        }
        {
            Property = "FsToolkitErrorHandlingNamespace"
            Value = "Contoso._"
            Message = keywordMessage
        }
    ]

    for invalid in invalidValues do
        let run = validate targets [ invalid.Property, invalid.Value ]

        let expected =
            $"error : FsToolkit.ErrorHandling.Sources: {invalid.Property} is '{invalid.Value}'. {invalid.Message}"

        if
            run.ExitCode = 0
            || not (run.Output.Contains expected)
        then
            failwithf
                "%s must fail with \"%s\". Exit code %d. Output:\n%s"
                run.Command
                expected
                run.ExitCode
                run.Output

    let control =
        validate targets [
            "FsToolkitErrorHandlingNamespace", "Contoso.ErrorHandling"
            "FsToolkitErrorHandlingVisibility", "public"
        ]

    if
        control.ExitCode
        <> 0
    then
        failwithf
            "%s must pass. Exit code %d. Output:\n%s"
            control.Command
            control.ExitCode
            control.Output

    let diagnosticsProject =
        rootDir
        </> "tests/FsToolkit.ErrorHandling.Sources.Consumer/diagnostics/Diagnostics.proj"

    let warningCases = [
        [], true
        [ "IsPackable", "true" ], true
        [ "IsPackable", "false" ], false
        [
            "IsPackable", "true"
            "DiagnosticsPrivateAssets", "All"
        ],
        false
    ]

    for properties, warns in warningCases do
        let run = validate diagnosticsProject properties

        if
            run.ExitCode
            <> 0
            || run.Output.Contains "warning FSTKSRC001"
               <> warns
        then
            failwithf
                "%s must exit 0 and %s warning FSTKSRC001. Exit code %d. Output:\n%s"
                run.Command
                (if warns then "give" else "not give")
                run.ExitCode
                run.Output

let fixtureAssemblies (fixture: string) =
    let assemblies =
        !!(Path.GetDirectoryName fixture
           </> "bin/**"
           </> Path.GetFileNameWithoutExtension fixture
               + ".dll")
        |> Seq.toList

    if List.isEmpty assemblies then
        failwithf "No build output found for %s" fixture

    assemblies

let failOnSourceVisibility () =
    for assembly in fixtureAssemblies sourcesConsumerFixture do
        match SourcesPackage.publicTypesInNamespace assembly "Contoso.Internal.ErrorHandling" with
        | [] -> ()
        | types ->
            failwithf
                "%s must not export source package types with the default 'internal' visibility. Public types: %A"
                assembly
                types

    for assembly in fixtureAssemblies sourcesTestsFixture do
        let types = SourcesPackage.publicTypesInNamespace assembly "FsToolkit.ErrorHandling"

        if not (List.contains "FsToolkit.ErrorHandling.TaskResultBuilder" types) then
            failwithf
                "%s must export FsToolkit.ErrorHandling.TaskResultBuilder with 'public' visibility."
                assembly

let failOnDependencyDrift () =
    let dependencyGroups packageId =
        let nupkg =
            distDir
            </> $"{packageId}.{release.NugetVersion}.nupkg"

        use archive = System.IO.Compression.ZipFile.OpenRead nupkg

        match
            archive.Entries
            |> Seq.tryFind (fun entry -> entry.FullName = $"{packageId}.nuspec")
        with
        | Some entry ->
            use reader = new StreamReader(entry.Open())
            SourcesPackage.nuspecDependencyGroups (reader.ReadToEnd())
        | None -> failwithf "%s has no %s.nuspec" nupkg packageId

    let expected = dependencyGroups "FsToolkit.ErrorHandling"
    let actual = dependencyGroups "FsToolkit.ErrorHandling.Sources"

    if
        actual
        <> expected
    then
        failwithf
            "FsToolkit.ErrorHandling.Sources must have the dependency groups of FsToolkit.ErrorHandling, so that each consumer gets the same FSharp.Core minimum version. Set the same TargetFrameworks in both projects.\nFsToolkit.ErrorHandling: %A\nFsToolkit.ErrorHandling.Sources: %A"
            expected
            actual

/// Packs FsToolkit.ErrorHandling.Sources into feedDir with a version that nuget.org does not have,
/// so that a restore from nuget.org and feedDir can only take this package.
let packSourcesForTest ctx (feedDir: string) =
    Shell.cleanDirs [ feedDir ]
    let version = $"0.0.0-t.{System.DateTime.UtcNow:yyMMddHHmmss}"

    DotNet.pack
        (fun p -> {
            p with
                OutputPath = Some feedDir
                Configuration = configuration ctx.Context.AllExecutingTargets
                MSBuildParams =
                    {
                        p.MSBuildParams with
                            Properties = [ ("Version", version) ]
                    }
                    |> BuildParameters.common
        })
        sourcesProject

    version

let failOnPackageContentDrift (testPackage: string) =
    let releasePackage =
        distDir
        </> $"FsToolkit.ErrorHandling.Sources.{release.NugetVersion}.nupkg"

    let content (nupkg: string) =
        use archive = System.IO.Compression.ZipFile.OpenRead nupkg
        use sha = System.Security.Cryptography.SHA256.Create()

        archive.Entries
        |> Seq.filter (fun entry ->
            entry.FullName.StartsWith("contentFiles/", System.StringComparison.Ordinal)
            || entry.FullName.StartsWith("build/", System.StringComparison.Ordinal)
        )
        |> Seq.map (fun entry ->
            use stream = entry.Open()
            entry.FullName, System.Convert.ToHexString(sha.ComputeHash stream)
        )
        |> Seq.sort
        |> Seq.toList

    let expected = content releasePackage
    let actual = content testPackage

    if expected.IsEmpty then
        failwithf "%s has no contentFiles or build entries." releasePackage

    if
        actual
        <> expected
    then
        failwithf
            "The contentFiles and build entries of %s must be the same as in %s, so that SourcesTest tests the files that the release ships.\nRelease: %A\nTest: %A"
            testPackage
            releasePackage
            expected
            actual

let sourcesTest ctx =
    failOnTestFileDrift ()
    failOnDocsDrift ()
    failOnTargetsDiagnostics ()
    failOnDependencyDrift ()

    let packagesDir =
        rootDir
        </> "obj/sources-test-packages"

    Shell.deleteDir packagesDir

    let feedDir =
        rootDir
        </> "obj/sources-test-feed"

    let sourcesVersion = packSourcesForTest ctx feedDir

    failOnPackageContentDrift (
        feedDir
        </> $"FsToolkit.ErrorHandling.Sources.{sourcesVersion}.nupkg"
    )

    let withFixtureParams (msbuildParams: MSBuild.CliArguments) =
        {
            msbuildParams with
                Properties = [
                    ("RestorePackagesPath", packagesDir)
                    ("FsToolkitErrorHandlingSourcesVersion", sourcesVersion)
                ]
        }
        |> BuildParameters.common

    let fixtures = [
        sourcesConsumerFixture
        sourcesTestsFixture
    ]

    for fixture in fixtures do
        let fixtureDir = Path.GetDirectoryName fixture

        Shell.cleanDirs [
            fixtureDir
            </> "obj"
            fixtureDir
            </> "bin"
        ]

        DotNet.restore
            (fun c -> {
                c with
                    // SDK 10.0.403 reads a URL source that follows a local folder source as a relative path (NU1301).
                    Sources = [
                        "https://api.nuget.org/v3/index.json"
                        feedDir
                    ]
                    MSBuildParams = withFixtureParams c.MSBuildParams
            })
            fixture

    let buildConsumer extraArgs =
        DotNet.build
            (fun c -> {
                c with
                    NoRestore = true
                    Configuration = configuration ctx.Context.AllExecutingTargets
                    MSBuildParams = withFixtureParams c.MSBuildParams
                    Common =
                        c.Common
                        |> DotNet.Options.withAdditionalArgs (
                            maxCpuMsBuild.Value
                            :: extraArgs
                        )
            })
            sourcesConsumerFixture

    buildConsumer []
    // A multi-target rebuild skips NuGet content preprocessing in the inner builds unless the package targets repair it.
    buildConsumer [ "--no-incremental" ]

    buildConsumer [
        "--framework"
        "net11.0"
        "-p:TestRuntimeAsync=multiline-off"
    ]

    buildConsumer [
        "--framework"
        "net11.0"
        "-p:Features=runtime-async=on"
    ]

    buildConsumer [
        "--framework"
        "net11.0"
        "--no-incremental"
        "-p:TestRuntimeAsync=true"
    ]

    DotNet.test
        (fun c -> {
            c with
                NoRestore = true
                Configuration = configuration ctx.Context.AllExecutingTargets
                MSBuildParams = withFixtureParams c.MSBuildParams
                Common =
                    c.Common
                    |> DotNet.Options.withAdditionalArgs [ maxCpuMsBuild.Value ]
        })
        sourcesTestsFixture

    failOnSourceVisibility ()

let webSharperDir =
    rootDir
    </> "tests/FsToolkit.ErrorHandling.WebSharper"

let webSharperRunnerDir =
    webSharperDir
    </> "Runner"

let failOnWebSharperDocsDrift () =
    let readNormalized (path: string) =
        (File.ReadAllText path).Replace("\r\n", "\n")

    let targetsPath =
        webSharperDir
        </> "Directory.Build.targets"

    let gitbookPath =
        rootDir
        </> "gitbook/sources/sourcesPackage.md"

    let expected =
        "```xml\n"
        + (readNormalized targetsPath).TrimEnd('\n')
        + "\n```\n"

    if not ((readNormalized gitbookPath).Contains(expected, System.StringComparison.Ordinal)) then
        failwithf
            "%s must contain the full text of %s in an xml code block. Make the same change in both files."
            gitbookPath
            targetsPath

let webSharperTest ctx =
    failOnWebSharperDocsDrift ()

    let feedDir =
        rootDir
        </> "obj/websharper-test-feed"

    let packagesDir =
        rootDir
        </> "obj/websharper-test-packages"

    // NuGet and the SDK do not write a package or its preprocessed content files again for a version that they already have.
    Shell.deleteDir (
        packagesDir
        </> "fstoolkit.errorhandling.sources"
    )

    [
        for project in
            [
                "Proxy"
                "TestAdapter"
                "Tests"
                "Runner"
            ] do
            for output in
                [
                    "bin"
                    "obj"
                ] do
                webSharperDir
                </> project
                </> output
    ]
    |> Shell.cleanDirs

    let sourcesVersion = packSourcesForTest ctx feedDir

    let withSourcesParams (msbuildParams: MSBuild.CliArguments) =
        {
            msbuildParams with
                Properties = [
                    ("RestorePackagesPath", packagesDir)
                    ("FsToolkitErrorHandlingVersion", sourcesVersion)
                ]
        }
        |> BuildParameters.common

    let runnerProject =
        webSharperRunnerDir
        </> "FsToolkit.ErrorHandling.WebSharper.Runner.fsproj"

    DotNet.restore
        (fun c -> {
            c with
                Sources = [
                    "https://api.nuget.org/v3/index.json"
                    feedDir
                ]
                MSBuildParams = withSourcesParams c.MSBuildParams
        })
        runnerProject

    DotNet.build
        (fun c -> {
            c with
                NoRestore = true
                Configuration = configuration ctx.Context.AllExecutingTargets
                MSBuildParams = withSourcesParams c.MSBuildParams
                Common =
                    c.Common
                    |> DotNet.Options.withAdditionalArgs [ maxCpuMsBuild.Value ]
        })
        runnerProject

    Npm.exec
        "ci --no-audit --no-fund"
        (fun o -> {
            o with
                WorkingDirectory = webSharperRunnerDir
        })

    let result =
        CreateProcess.fromRawCommand "node" [
            webSharperRunnerDir
            </> "bin/js/FsToolkit.ErrorHandling.WebSharper.Runner.js"
        ]
        |> CreateProcess.withWorkingDirectory webSharperRunnerDir
        |> CreateProcess.withTimeout (System.TimeSpan.FromMinutes 15.0)
        |> Proc.run

    if
        result.ExitCode
        <> 0
    then
        Fake.Testing.Common.FailedTestsException
            "WebSharper tests failed, see output for more information."
        |> raise

let dotnetPack ctx =
    [
        solutionFile
        sourcesProject
    ]
    |> Seq.iter (
        DotNet.pack (fun p -> {
            p with
                // ./bin from the solution root matching the "PublishNuget" target WorkingDir
                OutputPath = Some distDir
                Configuration = configuration ctx.Context.AllExecutingTargets
                MSBuildParams =
                    {
                        p.MSBuildParams with
                            // "/p" (property) arguments to MSBuild.exe
                            Properties = [
                                ("Version", release.NugetVersion)
                                ("PackageReleaseNotes", releaseNotes)
                            ]
                    }
                    |> BuildParameters.common
        })
    )


let publishNuget _ =


    DotNet.nugetPush
        (fun c -> {
            c with
                Common = {
                    c.Common with
                        WorkingDirectory = distDir
                }
                PushParams = {
                    c.PushParams with
                        Source = Some publishUrl
                        ApiKey = nugetToken.Value
                }
        })
        "*.nupkg"


let remote = Environment.environVarOrDefault "FSTK_GIT_REMOTE" "origin"

let gitRelease _ =

    Git.Staging.stageAll ""
    Git.Commit.exec "" (sprintf "Bump version to %s\n\n%s" release.NugetVersion releaseNotes)
    Git.Branches.push ""

    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" remote release.NugetVersion


let githubRelease _ =
    let token =
        match githubToken.Value with
        | Some s -> s
        | _ ->
            failwith
                "please set the github_token environment variable to a github personal access token with repo access."

    let files = !!distGlob

    GitHub.createClientWithToken token
    |> GitHub.draftNewRelease
        gitOwner
        project
        release.NugetVersion
        (release.SemVer.PreRelease
         <> None)
        (releaseNotes
         |> Seq.singleton)
    |> GitHub.uploadFiles files
    |> GitHub.publishDraft
    |> Async.RunSynchronously


let initTargets () =

    /// Defines a dependency - y is dependent on x. Finishes the chain.
    let (==>!) x y =
        x ==> y
        |> ignore

    /// Defines a soft dependency. x must run before y, if it is present, but y does not require x to be run. Finishes the chain.
    let (?=>!) x y =
        x ?=> y
        |> ignore

    DotEnv.load rootDir
    BuildServer.install [ GitHubActions.Installer ]

    Option.iter (TraceSecrets.register "<GITHUB_TOKEN>") githubToken.Value
    Option.iter (TraceSecrets.register "<NUGET_TOKEN>") nugetToken.Value
    Option.iter (TraceSecrets.register "<FSTK_NUGET_TOKEN>") nugetToken.Value


    Target.create "Clean" clean
    Target.create "Build" build
    Target.create "DotnetRestore" restore
    Target.create "NpmRestore" npmRestore
    Target.create "NpmTest" runNpmTest
    Target.create "PythonTest" runPythonTests
    Target.create "DotnetTest" dotnetTest

    Target.create "RunTests" ignore
    Target.create "FemtoValidate" femtoValidate
    Target.create "GenerateAssemblyInfo" generateAssemblyInfo
    Target.create "GenerateSources" generateSources
    Target.create "DotnetPack" dotnetPack
    Target.create "SourcesTest" sourcesTest
    Target.create "WebSharperTest" webSharperTest
    Target.create "FormatCode" formatCode
    Target.create "CheckFormatCode" checkFormatCode
    Target.create "Analyzers" analyze
    Target.create "PublishToNuGet" publishNuget
    Target.create "GitRelease" gitRelease
    Target.create "GitHubRelease" githubRelease
    Target.create "Release" ignore

    Target.create
        "DebugEnv"
        (fun _ ->
            printfn "githubToken %A" githubToken.Value
            printfn "nugetToken %A" nugetToken.Value
        )

    Target.create
        "UpdateDocs"
        (fun _ ->
            Git.Staging.stageAll ""
            Git.Commit.exec "" "update docs"
            Git.Branches.push ""
        )

    "DotnetRestore"
    ==>! "CheckFormatCode"

    "DotnetRestore"
    ==>! "Analyzers"

    //*** Dotnet Build ***//
    "DotnetRestore"
    ==>! "Build"

    "Build"
    ==> "DotnetTest"
    ==>! "DotnetPack"
    //*** Dotnet Build ***//


    //*** Fable Javascript *** //
    "DotnetRestore"
    ==>! "NpmRestore"

    "NpmRestore"
    ==>! "FemtoValidate"

    "FemtoValidate"
    ==>! "NpmTest"
    //*** Fable Javascript *** //

    //*** Fable Python *** //
    "DotnetRestore"
    ==>! "NpmRestore"

    "NpmRestore"
    ==>! "FemtoValidate"

    "FemtoValidate"
    ==>! "PythonTest"
    //*** Fable Python *** //

    "DotnetTest"
    ==>! "RunTests"

    "PythonTest"
    ==>! "RunTests"

    "NpmTest"
    ==>! "RunTests"

    //*** WebSharper *** //
    "GenerateSources"
    ==>! "WebSharperTest"

    "WebSharperTest"
    ==>! "RunTests"
    //*** WebSharper *** //


    //*** Publishing ***//

    // Only call Clean if DotnetPack was in the call chain
    // Ensure Clean is called before DotnetRestore
    "Clean"
    ?=>! "DotnetRestore"

    // Only call GenerateAssemblyInfo if Publish was in the call chain
    // Ensure GenerateAssemblyInfo is called after DotnetRestore and before DotnetBuild
    "DotnetRestore"
    ?=>! "GenerateAssemblyInfo"

    "GenerateAssemblyInfo"
    ?=>! "DotnetPack"

    "GenerateAssemblyInfo"
    ==>! "PublishToNuGet"

    "RunTests"
    ?=>! "DotnetPack"

    "RunTests"
    ==>! "PublishToNuGet"

    "Clean"
    ?=>! "GenerateSources"

    "GenerateSources"
    ==>! "DotnetPack"

    "Clean"
    ==> "CheckFormatCode"
    ==> "DotnetPack"
    ==> "SourcesTest"
    ==> "PublishToNuGet"
    ==> "GitRelease"
    ==> "GitHubRelease"
    ==>! "Release"


//*** Publishing ***//


//-----------------------------------------------------------------------------
// Target Start
//-----------------------------------------------------------------------------
[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets ()

    Target.runOrDefaultWithArguments "SourcesTest"

    0 // return an integer exit code
