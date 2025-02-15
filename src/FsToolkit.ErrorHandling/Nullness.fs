namespace FsToolkit.ErrorHandling
open System

type ExceptionNull =
    #if NET9_0 && !FABLE_COMPILER
        Exception | null
    #else
        Exception
    #endif

type IDisposableNull =
    #if NET9_0 && !FABLE_COMPILER
        IDisposable | null
    #else
        IDisposable
    #endif

type IAsyncDisposableNull =
    #if NET9_0 && !FABLE_COMPILER
        IAsyncDisposable | null
    #else
        IAsyncDisposable
    #endif