namespace FsToolkit.ErrorHandling

(*
This Polyfills the F# 9.0 and later types for nullability, which are not available in earlier versions of F# or in Fable.
This is useful for keeping compatibility with older versions of F# or when using Fable, which does not support the new nullability features.
*)


open System

/// Represents errors that occur during application execution. In F# 9.0 and later, this type is marked as nullable.
type ExceptionNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    Exception | null
#else
    Exception
#endif

/// Provides a mechanism for releasing unmanaged resources. In F# 9.0 and later, this type is marked as nullable.
type IDisposableNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    IDisposable | null
#else
    IDisposable
#endif

/// Provides a mechanism for releasing unmanaged resources asynchronously. In F# 9.0 and later, this type is marked as nullable.
type IAsyncDisposableNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    IAsyncDisposable | null
#else
    IAsyncDisposable
#endif

/// An abbreviation for the CLI type System.Object. In F# 9.0 and later, this type is marked as nullable.
type ObjNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    objnull
#else
    obj
#endif


/// An abbreviation for the CLI type System.Collections.Generic.IEnumerable. In F# 9.0 and later, this type is marked as nullable.
type SeqNull<'T> =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    seq<'T> | null
#else
    seq<'T>
#endif

/// An abbreviation for the CLI type System.String. In F# 9.0 and later, this type is marked as nullable.
type StringNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    string | null
#else
    string
#endif

/// An abbreviation for the CLI type System.Collections.Generic.List`1 . In F# 9.0 and later, this type is marked as nullable.
type ResizeArrayNull<'T> =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    ResizeArray<'T> | null
#else
    ResizeArray<'T>
#endif


module internal Nullness =

#if NET9_0_OR_GREATER && !FABLE_COMPILER
    /// Throw a System.ArgumentNullException if the given value is null exception
    let inline nullArgCheck name value = nullArgCheck name value
#else
    /// Throw a System.ArgumentNullException if the given value is null exception
    let inline nullArgCheck (name: string) (value: 'T) : 'T =
        if isNull value then
            raise (ArgumentNullException(name))
        else
            value
#endif
