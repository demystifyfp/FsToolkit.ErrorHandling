module OptionCETests 


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open FsToolkit.ErrorHandling
open System
open System.Collections.Generic

let makeDisposable () =
    { new System.IDisposable
        with member this.Dispose() = () }


// type Option<'a> = | Some of 'a | None

let ceTests = 
    testList "CE Tests" [
        testCase "Return value" <| fun _ ->


            let expected = Some 42
            let actual = option  {
                return 42
            }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom Some" <| fun _ ->
            let expected = Some 42
            let actual = option  {
                return! (Some 42)
            }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom None" <| fun _ ->
            let expected = None
            let actual = option  {
                return! None
            }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "Bind Some" <| fun _ -> 
            let expected = Some 42
            let actual = option {
                let! value = Some 42
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Bind None" <| fun _ -> 
            let expected = None
            let actual = option {
                let! value = None
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Zero/Combine/Delay/Run" <| fun () ->
            let data = 42
            let actual = option {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "Try With" <| fun () ->
            let data = 42
            let actual = option {
                try 
                    return data
                with e -> 
                    return raise e
            }
            Expect.equal actual (Some data) "Try with failed"
        testCase "Try Finally" <| fun () ->
            let data = 42
            let actual = option {
                try 
                    return data
                finally
                    ()
            }
            Expect.equal actual (Some data) "Try with failed"
        testCase "Using null" <| fun () ->
            let data = 42
            let actual = option {
                use d = null
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "Using disposeable" <| fun () ->
            let data = 42
            let actual = option {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "Using bind disposeable" <| fun () ->
            let data = 42
            let actual = option {
                use! d = (makeDisposable () |> Some)
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "While" <| fun () ->
            let data = 42
            let mutable index = 0
            let actual = option {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "For in" <| fun () ->
            let data = 42
            let actual = option {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "For in ResizeArray" <| fun () ->
            let data = 42
            let actual = option {
                for i in ResizeArray [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "For to" <| fun () ->
            let data = 42
            let actual = option {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
#if !FABLE_COMPILER
        testCase "Nullable value" <| fun () ->
            let data = 42
            let actual = option {
                let! value = System.Nullable<int> data
                return value
            }
            Expect.equal actual (Some data) ""
        testCase "Nullable null" <| fun () ->
            let actual = option {
                return! System.Nullable<_> ()
            }
            Expect.equal actual None ""
#endif
        testCase "string value" <| fun () ->
            let data = "hello"
            let actual = option {
                let! v = data 
                return v
            }
            Expect.equal actual (Some data) ""

        testCase "string null" <| fun () ->
            let (data : string) = null
            let actual = option {
                let! v = data
                return v
            }
            Expect.equal actual None ""
            
        testCase "Uri null" <| fun () ->
            let (data : Uri) = null
            let actual = option {
                let! v = data
                return v
            }
            Expect.equal actual None ""

        testCase "MemoryStream null" <| fun () ->
            let (data : IO.MemoryStream) = null
            let actual = option {
                let! v = data
                return v
            }
            Expect.equal actual None ""

        testCase "ResizeArray null" <| fun () ->
            let (data : ResizeArray<string>) = null
            let actual = option {
                let! v = data
                return v
            }
            Expect.equal actual None ""
    ]

[<AllowNullLiteral>]
type CustomClass(x : int) =

    member _.getX = x


let ``OptionCE applicative tests`` = 
    testList "OptionCE applicative tests" [
        testCase "Happy Path Option.Some" <| fun () ->
            let actual = option {
                let! a = Some 3
                and! b = Some 2
                and! c = Some 1
                return a + b - c
            }
            Expect.equal actual (Some 4) "Should be Some 4"

#if !FABLE_COMPILER
        testCase "Happy Path Nullable" <| fun () ->
            let actual = option {
                let! a = Nullable<_> 3
                and! b = Nullable<_> 2
                and! c = Nullable<_> 1
                return a + b - c
            }
            Expect.equal actual (Some 4) "Should be Some 4"
#endif
        testCase "Happy Path null Objects" <| fun () ->
            // let hello = CustomClass
            let actual = option {
                let! a = CustomClass 3
                and! b = CustomClass 2
                and! c = CustomClass 1
                return a.getX + b.getX - c.getX
            }
            Expect.equal actual (Some 4) "Should be Some 4"


        testCase "Happy Path strings" <| fun () ->
            let hello = "Hello "
            let world = "world "
            let fromfsharp = "from F#"
            let actual = option {
                let! a = hello
                and! b = world
                and! c = fromfsharp
                return a + b + c
            }
            Expect.equal actual (Some "Hello world from F#") "Should be Some"

        testCase "Happy Path ResizeArray" <| fun () ->
            let r1 = ResizeArray [3]
            let r2 = ResizeArray [2]
            let r3 = ResizeArray [1]
            let actual = option {
                let! a = r1
                and! b = r2
                and! c = r3
                a.AddRange b
                a.AddRange c
                
                return Seq.sum a
            }
            Expect.equal actual (Some 6) "Should be Some"

#if !FABLE_COMPILER
        testCase "Happy Path Option.Some/Nullable" <| fun () ->
            let actual = option {
                let! a = Some 3
                and! b = Nullable 2
                and! c = Nullable 1
                return a + b - c
            }
            Expect.equal actual (Some 4) "Should be Some 4"

        testCase "Happy Path Option.Some/Nullable/Objects" <| fun () ->
            let actual = option {
                let! a = Some 3
                and! b = Nullable 2
                and! c = CustomClass 1
                return a + b - c.getX
            }
            Expect.equal actual (Some 4) "Should be Some 4"

                        
        testCase "Happy Combo all" <| fun () ->
            let actual = option {
                let! a = Nullable<_> 3
                and! b = Some 2
                and! c = "hello"
                and! d = ResizeArray [1]
                and! e = CustomClass 5
                and! f = Uri "https://github.com/"
                return sprintf "%d %d %s %d %d %s" a b c (Seq.head d) e.getX (string f)
            }
            
            Expect.equal actual (Some "3 2 hello 1 5 https://github.com/") "Should be Some"
#endif 
        testCase "Fail Path Option.None" <| fun () ->
            let actual = option {
                let! a = Some 3
                and! b = Some 2
                and! c = None
                return a + b - c
            }
            Expect.equal actual None "Should be None"

#if !FABLE_COMPILER     
        testCase "Fail Path Nullable" <| fun () ->
            let actual = option {
                let! a = Nullable 3
                and! b = Nullable 2
                and! c = Nullable<_>()
                return a + b - c
            }
            Expect.equal actual (None) "Should be None"    
#endif
        testCase "Fail Path Objects" <| fun () ->
            let c1 = CustomClass 3
            let c2 = CustomClass 2
            let c3 : CustomClass = null
            let actual = option {
                let! a = c1
                and! b = c2
                and! c = c3
                return a.getX + b.getX - c.getX
            }
            Expect.equal actual (None) "Should be None"


        testCase "Fail Path strings" <| fun () ->
            let c1 = CustomClass 3
            let c2 = CustomClass 2
            let c3 : CustomClass = null
            let actual = option {
                let! a = c1
                and! b = c2
                and! c = c3
                return a.getX + b.getX - c.getX
            }
            Expect.equal actual (None) "Should be None"

#if !FABLE_COMPILER
        testCase "Fail Path Option.Some/Nullable" <| fun () ->
            let actual = option {
                let! a = Nullable<_> 3
                and! b = Some 2
                and! c = Nullable<_>()
                return a + b - c
            }
            Expect.equal actual None "Should be None"
#endif

        testCase "ValueOption.Some" <| fun () ->
            let actual = option {
                let! a = ValueSome 3
                return a
            }
            Expect.equal actual (Some 3) "Should be None"

        testCase "ValueOption.None" <| fun () ->
            let actual = option {
                let! a = ValueNone
                return a
            }
            Expect.equal actual (None) "Should be None"

    ]

let allTests = testList "Option CE tests" [
    ceTests
    ``OptionCE applicative tests``
]