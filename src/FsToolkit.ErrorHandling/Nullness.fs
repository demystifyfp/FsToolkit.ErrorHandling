namespace FsToolkit.ErrorHandling

open System

type ExceptionNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    Exception | null
#else
    Exception
#endif

type IDisposableNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    IDisposable | null
#else
    IDisposable
#endif

type IAsyncDisposableNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    IAsyncDisposable | null
#else
    IAsyncDisposable
#endif

type ObjNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    objnull
#else
    obj
#endif

type SeqNull<'T> =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    seq<'T> | null
#else
    seq<'T>
#endif

type StringNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    string | null
#else
    string
#endif

type ResizeArrayNull<'T> =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    ResizeArray<'T> | null
#else
    ResizeArray<'T>
#endif


module internal Nullness =

#if NET9_0_OR_GREATER && !FABLE_COMPILER
    let inline nullArgCheck name value = nullArgCheck name value
#else
    let inline nullArgCheck (name: string) (value: 'T) : 'T =
        if isNull value then
            raise (ArgumentNullException(name))
        else
            value
#endif
