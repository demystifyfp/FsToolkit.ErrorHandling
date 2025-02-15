namespace FsToolkit.ErrorHandling
open System

type ExceptionNull =
    #if NET9_0 
        Exception | null
    #else
        Exception
    #endif

type IDisposableNull =
    #if NET9_0 
        IDisposable | null
    #else
        IDisposable
    #endif

type IAsyncDisposableNull =
    #if NET9_0 
        IAsyncDisposable | null
    #else
        IAsyncDisposable
    #endif