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
  ==> "Restore"
  ==> "Build"
  ==> "RunTests"

// *** Start Build ***
Target.runOrDefault "Build"