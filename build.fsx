#r "paket: groupref Build //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.DotNet

let configuration = "Release"
let solutionFile = "FsToolkit.ErrorHandling.sln"
let buildConfiguration = 
  Environment.environVarOrDefault "configuration" configuration
  |> DotNet.Custom 

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

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"Clean"
  ==> "Build"

// *** Start Build ***
Target.runOrDefault "Build"