namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module JobResultOptionCE =
    open Hopac

    type JobResultOptionBuilder() =
        member inline _.Return value = JobResultOption.retn value
        member inline _.ReturnFrom value = value
        member inline _.Bind(result, [<InlineIfLambda>] binder) = JobResultOption.bind binder result

        member inline _.Combine(aro1, aro2) =
            aro1 |> JobResultOption.bind (fun _ -> aro2)

        member inline _.Delay([<InlineIfLambda>] f) = Job.delay f

    let jobResultOption = new JobResultOptionBuilder()
