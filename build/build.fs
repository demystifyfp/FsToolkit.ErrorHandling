open Fake.Api
open Fake.Core
open Fake.IO
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.DotNet.Testing
open Fake.IO.Globbing.Operators
open Fake.Tools
open Fake.JavaScript
open System
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

let summary =
    "FsToolkit.ErrorHandling is a utility library to work with the Result type in F#, and allows you to do clear, simple and powerful error handling."

let isCI = lazy (environVarAsBoolOrDefault "CI" false)

let isRelease (targets: Target list) =
    targets
    |> Seq.map (fun t -> t.Name)
    |> Seq.exists ((=) "Release")

let configuration (targets: Target list) =
    let defaultVal = if isRelease targets then "Release" else "Debug"

    match Environment.environVarOrDefault "CONFIGURATION" defaultVal with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let solutionFile = "FsToolkit.ErrorHandling.sln"

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
    ++ "js-dist"
    ++ "**/.python-tests"
    |> Shell.cleanDirs

    [ "paket-files/paket.restore.cached" ]
    |> Seq.iter Shell.rm


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
    Fake.DotNet.Paket.restore (fun p -> {
        p with
            ToolType = ToolType.CreateLocalTool()
    })

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

let runPythonTests _ =
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
    ReleaseNotes.load (
        rootDir
        </> "RELEASE_NOTES.md"
    )

let generateAssemblyInfo ctx =
    let getAssemblyInfoAttributes projectName = [
        AssemblyInfo.Title(projectName)
        AssemblyInfo.Product project
        AssemblyInfo.Description summary
        AssemblyInfo.Version release.AssemblyVersion
        AssemblyInfo.FileVersion release.AssemblyVersion
        AssemblyInfo.Configuration(string (configuration (ctx.Context.AllExecutingTargets)))
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

let dotnetPack ctx =
    [ solutionFile ]
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
    Paket.push (fun p -> {
        p with
            ToolType = ToolType.CreateLocalTool()
            PublishUrl = "https://www.nuget.org"
            WorkingDir = distDir
            ApiKey =
                match nugetToken.Value with
                | Some s -> s
                | _ -> p.ApiKey // assume paket-config was set properly
    })


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
    Target.create "DotnetPack" dotnetPack
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

    "Clean"
    ==> "CheckFormatCode"
    ==> "DotnetPack"
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
    |> ignore

    Target.runOrDefaultWithArguments "DotnetPack"

    0 // return an integer exit code
