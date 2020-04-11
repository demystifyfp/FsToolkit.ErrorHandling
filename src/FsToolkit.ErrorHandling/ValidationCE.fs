namespace FsToolkit.ErrorHandling

open System 

[<AutoOpen>]
module ValidationCE =
    type ValidationBuilder() =
        member __.Return (value: 'T) =
            Validation.ok value

        member _.BindReturn(x: Validation<'T,'U>, f) : Validation<_,_> = Result.map f x


        member __.Bind
            (result: Validation<'T, 'TError>, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            Validation.bind binder result
            

        member _.MergeSources(t1: Validation<'T,'U>, t2: Validation<'T1,'U>) : Validation<_,_> = Validation.zip t1 t2

    let validation = ValidationBuilder()

[<AutoOpen>]
module ValidationCEExtensions =

  // Having Choice<_> members as extensions gives them lower priority in
  // overload resolution and allows skipping more type annotations.
    type ValidationBuilder with
        member __.Bind
            (result: Result<'T, 'TError>, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            result
            |> Validation.ofResult
            |> Validation.bind binder 
            
        member _.BindReturn(x: Result<'T,'U>, f) : Validation<_,_> = x |> Validation.ofResult |> Result.map f

        member _.MergeSources(t1: Validation<'T,'U>, t2: Result<'T1,'U>) : Validation<_,_> = Validation.zip t1 (Result.mapError List.singleton t2)
        member _.MergeSources(t1: Result<'T,'U>, t2: Validation<'T1,'U>) : Validation<_,_> = Validation.zip (Result.mapError List.singleton t1) t2
        member _.MergeSources(t1: Result<'T,'U>, t2: Result<'T1,'U>) : Validation<_,_> = Validation.zip (Result.mapError List.singleton t1) (Result.mapError List.singleton t2)
        
