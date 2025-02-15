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
