namespace FsToolkit.Build

module DotEnv =
    open Fake.Core
    open System
    open System.IO

    let private parseLine (line: string) =
        match line.Split('=', StringSplitOptions.RemoveEmptyEntries) with
        | args when args.Length = 2 -> Environment.SetEnvironmentVariable(args.[0], args.[1])
        | _ -> ()

    let load (rootDir) =
        let filePath = Path.Combine(rootDir, ".env")

        if File.Exists filePath then
            filePath
            |> File.ReadAllLines
            |> Seq.iter parseLine
        else
            Trace.traceImportantfn "No .env file found. %s" rootDir
