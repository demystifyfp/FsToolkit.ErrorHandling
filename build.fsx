#r "paket: groupref Build //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.DotNet.Testing
open Fake.IO.Globbing.Operators
open Fake.Tools
open System
open System.IO

let project = "FsToolkit.ErrorHandling"
let summary = "An opinionated error handling library for F#"
let configuration = "Release"
let solutionFile = "FsToolkit.ErrorHandling.sln"

Target.create "Clean" (fun _ ->
  Shell.cleanDirs ["bin"]
)

if not (Environment.isWindows) then
  let inferFrameworkPathOverride () =
    let mscorlib = "mscorlib.dll"
    let possibleFrameworkPaths =
      [ 
        "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/"
        "/usr/local/Cellar/mono/4.6.12/lib/mono/4.5/"
        "/usr/lib/mono/4.5/"
      ]
    possibleFrameworkPaths
    |> Seq.find (fun p -> System.IO.File.Exists(p @@ mscorlib))
  Environment.setEnvironVar "FrameworkPathOverride" (inferFrameworkPathOverride())

Target.create "Build" (fun _ ->
  let setParams (defaults:DotNet.BuildOptions) =
        { defaults with
            Configuration = DotNet.BuildConfiguration.fromString configuration}
  DotNet.build setParams solutionFile
)


Target.create "Restore" (fun _ ->
  DotNet.restore id solutionFile
)

Target.create "GenerateDocs" (fun _ ->
  let exitCode =
    Process.execSimple (fun (info : ProcStartInfo) -> 
                        {info with 
                          FileName = "gitbook"
                          Arguments = "build ./gitbook/ ./docs"}) TimeSpan.MaxValue
  if exitCode <> 0 then
    failwith "An error occurred while generating the docs"                        
)

let runTestAssembly setParams testAssembly =
  let exitCode =
    let fakeStartInfo testAssembly (args : Expecto.Params) =
      let workingDir =
        if String.isNotNullOrEmpty args.WorkingDirectory
        then args.WorkingDirectory else Fake.IO.Path.getDirectory testAssembly
      (fun (info: ProcStartInfo) ->
        { info with 
            FileName = "dotnet"
            Arguments = sprintf "%s %s" testAssembly (string args)
            WorkingDirectory = workingDir } )

    let execWithExitCode testAssembly argsString timeout = 
      Process.execSimple (fakeStartInfo testAssembly argsString) timeout

    execWithExitCode testAssembly (setParams Expecto.Params.DefaultParams) TimeSpan.MaxValue

  testAssembly, exitCode

let runTests setParams testAssemblies =
  let details = testAssemblies |> String.separated ", "
  use __ = Trace.traceTask "Expecto" details
  let res =
    testAssemblies
    |> Seq.map (runTestAssembly setParams)
    |> Seq.filter( snd >> (<>) 0)
    |> Seq.toList

  match res with
  | [] -> ()
  | failedAssemblies ->
      failedAssemblies
      |> List.map (fun (testAssembly,exitCode) -> 
          sprintf "Expecto test of assembly '%s' failed. Process finished with exit code %d." testAssembly exitCode )
      |> String.concat System.Environment.NewLine
      |> Fake.Testing.Common.FailedTestsException |> raise
  __.MarkSuccess()

let testAssemblies = "tests/**/bin" </> configuration </> "**" </> "*Tests.dll"

Target.create "RunTests" (fun _ ->
  runTests id (!! testAssemblies)
)
let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ AssemblyInfo.Title (projectName)
          AssemblyInfo.Product project
          AssemblyInfo.Description summary
          AssemblyInfo.Version release.AssemblyVersion
          AssemblyInfo.FileVersion release.AssemblyVersion
          AssemblyInfo.Configuration configuration ]

    let getProjectDetails projectPath =
        let projectName = Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! "src/**/*.??proj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (_, _, folderName, attributes) ->
        AssemblyInfoFile.createFSharp (folderName </> "AssemblyInfo.fs") attributes)
)

Target.create "NuGet" (fun _ ->
    Paket.pack(fun p ->
        { p with
            OutputPath = "bin"
            Version = release.NugetVersion
            ReleaseNotes = String.toLines release.Notes})
)

Target.create "PublishNuget" (fun _ ->
    Paket.push(fun p ->
        { p with
            PublishUrl = "https://www.nuget.org"
            WorkingDir = "bin" })
)

Target.create "Release" (fun _ ->
  Git.Staging.stageAll ""
  Git.Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
  Git.Branches.push ""

  Git.Branches.tag "" release.NugetVersion
  Git.Branches.pushTag "" "origin" release.NugetVersion
)

Target.create "CommitDocs" (fun _ ->
  Git.Staging.stageAll ""
  Git.Commit.exec "" "update docs"
  Git.Branches.push ""
)



// *** Define Dependencies ***
"Clean"
  ==> "AssemblyInfo"
  ==> "Restore"
  ==> "Build"
  ==> "RunTests"
  ==> "NuGet"
  ==> "PublishNuGet"
  ==> "Release"

"GenerateDocs"  
  ==> "CommitDocs"

// *** Start Build ***
Target.runOrDefault "Build"