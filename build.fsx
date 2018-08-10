#r "paket: groupref Build //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.DotNet.Testing
open Fake.IO.Globbing.Operators

let configuration = "Release"
let solutionFile = "FsToolkit.ErrorHandling.sln"

Target.create "Clean" (fun _ ->
  Shell.cleanDirs ["bin"]
)



Target.create "Build" (fun _ ->
  let setParams (defaults:MSBuildParams) =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", configuration
                ]
         }
  MSBuild.build setParams solutionFile
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

    execWithExitCode testAssembly (setParams Expecto.Params.DefaultParams) System.TimeSpan.MaxValue

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

// *** Define Dependencies ***
"Clean"
  ==> "Build"
  ==> "RunTests"

// *** Start Build ***
Target.runOrDefault "Build"