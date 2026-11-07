[<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
[<System.Obsolete "These compatibility redirects are not needed when using FSharp.Core directly.">]
module FsToolkit.ErrorHandling.FSharpCore11OptionShims

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
[<System.Obsolete "These compatibility redirects are not needed when using FSharp.Core directly.">]
module Option =

    /// <summary>
    /// Binds a function to an option, applying the function to the value if the option is <c>Some</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/bind</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the value.</param>
    /// <param name="input">The input option.</param>
    /// <typeparam name="'TInput">The input type of the option.</typeparam>
    /// <typeparam name="'TOutput">The output type of the option.</typeparam>
    /// <returns>The result of applying the function to the value, or <c>None</c> if the option is <c>None</c>.</returns>
    let inline bind
        ([<InlineIfLambda>] mapper: 'TInput -> 'TOutput option)
        (input: 'TInput option)
        : 'TOutput option =
        Microsoft.FSharp.Core.Option.bind mapper input

    /// <summary>
    /// Applies a mapper function to the value inside an option, returning a new option with the mapped value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the value inside the option.</param>
    /// <param name="input">The input option.</param>
    /// <returns>An option with the mapped value if the input option is <c>Some</c>, otherwise <c>None</c>.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'TInput -> 'TOutput)
        (input: 'TInput option)
        : 'TOutput option =
        Microsoft.FSharp.Core.Option.map mapper input

    /// <summary>
    /// Applies a mapper function to the values inside two options, returning a new option with the mapped value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map2</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the values inside the options.</param>
    /// <param name="input1">The first input option.</param>
    /// <param name="input2">The second input option.</param>
    /// <returns>An option with the mapped value if both input options are <c>Some</c>, otherwise <c>None</c>.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'TInput1 -> 'TInput2 -> 'TOutput)
        (input1: 'TInput1 option)
        (input2: 'TInput2 option)
        : 'TOutput option =
        Microsoft.FSharp.Core.Option.map2 mapper input1 input2

    /// <summary>
    /// Applies a mapper function to the values inside three options, returning a new option with the mapped value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map3</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the values inside the options.</param>
    /// <param name="input1">The first input option.</param>
    /// <param name="input2">The second input option.</param>
    /// <param name="input3">The third input option.</param>
    /// <returns>An option with the mapped value if all input options are <c>Some</c>, otherwise <c>None</c>.</returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'TInput1 -> 'TInput2 -> 'TInput3 -> 'TOutput)
        (input1: 'TInput1 option)
        (input2: 'TInput2 option)
        (input3: 'TInput3 option)
        : 'TOutput option =
        Microsoft.FSharp.Core.Option.map3 mapper input1 input2 input3
