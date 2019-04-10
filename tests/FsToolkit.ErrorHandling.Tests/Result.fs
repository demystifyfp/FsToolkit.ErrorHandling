module ResultTests 

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result


[<Tests>]
let map2Tests =
  testList "Result.map2 Tests" [
    testCase "map2 with two ok parts" <| fun _ ->
      Result.map2 location validLatR validLngR
      |> Expect.hasOkValue validLocation 

    testCase "map2 with one Error and one Ok parts" <| fun _ -> 
      Result.map2 location invalidLatR validLngR
      |> Expect.hasErrorValue invalidLatMsg
    
    testCase "map2 with one Ok and one Error parts" <| fun _ ->
      Result.map2 location validLatR invalidLngR
      |> Expect.hasErrorValue invalidLngMsg

    testCase "map2 with two Error parts" <| fun _ -> 
      Result.map2 location invalidLatR invalidLngR
      |> Expect.hasErrorValue invalidLatMsg
  ]


[<Tests>]
let map3Tests =
  testList "Result.map3 Tests" [
    testCase "map3 with three ok parts" <| fun _ ->
      
      Result.map3 createPostRequest validLatR validLngR validTweetR
      |> Expect.hasOkValue validCreatePostRequest

    testCase "map3 with (Error, Ok, Ok)" <| fun _ ->
      Result.map3 createPostRequest invalidLatR validLngR validTweetR
      |> Expect.hasErrorValue  invalidLatMsg
    
    testCase "map3 with (Ok, Error, Ok)" <| fun _ ->
      Result.map3 createPostRequest validLatR invalidLngR  validTweetR
      |> Expect.hasErrorValue invalidLngMsg


    testCase "map3 with (Ok, Ok, Error)" <| fun _ ->
      Result.map3 createPostRequest validLatR validLngR emptyInvalidTweetR
      |> Expect.hasErrorValue emptyTweetErrMsg
    
    testCase "map3 with (Error, Error, Error)" <| fun _ ->
      Result.map3 createPostRequest invalidLatR invalidLngR  emptyInvalidTweetR
      |> Expect.hasErrorValue invalidLatMsg
  ]

[<Tests>]
let applyTests =

  testList "Result.apply tests" [
    testCase "apply with Ok" <| fun _ ->
      Tweet.TryCreate "foobar"
      |> Result.apply (Ok remainingCharacters) 
      |> Expect.hasOkValue 274
    
    testCase "apply with Error" <| fun _ ->
      Result.apply (Ok remainingCharacters) emptyInvalidTweetR
      |> Expect.hasErrorValue emptyTweetErrMsg
  ]

[<Tests>]
let foldTests =

  testList "Result.fold tests" [
    testCase "fold with Ok" <| fun _ ->
      let actual =
        Tweet.TryCreate "foobar"
        |> Result.fold (fun t -> t.Value) id
      Expect.equal actual "foobar" "fold to string should succeed"
    
    testCase "fold with Error" <| fun _ ->
      let actual =
        Tweet.TryCreate ""
        |> Result.fold (fun t -> t.Value) id
      Expect.equal actual emptyTweetErrMsg "fold to string should fail"
  ]


[<Tests>]
let ofChoiceTests =

  testList "Result.ofChoice tests" [
    testCase "ofChoice with Choice1Of2" <| fun _ ->
      Result.ofChoice (Choice1Of2 1)
      |> Expect.hasOkValue 1
    
    testCase "ofChoice with Choice2Of2" <| fun _ ->
      Result.ofChoice (Choice2Of2 1)
      |> Expect.hasErrorValue 1
  ]


[<Tests>]
let resultCETests =
  testList "result Computation Expression tests" [
    testCase "bind with all Ok" <| fun _ ->
      let createPostRequest = result {
        let! lat = validLatR 
        let! lng = validLngR 
        let! tweet = validTweetR
        return createPostRequest lat lng tweet
      }
      Expect.hasOkValue validCreatePostRequest createPostRequest 

    testCase "bind with Error" <| fun _ -> 
      let post = result {
        let! lat = invalidLatR
        Tests.failtestf "this should not get executed!"
        let! lng = validLngR 
        let! tweet = validTweetR
        return createPostRequest lat lng tweet
      }
      post
      |> Expect.hasErrorValue invalidLatMsg
  ]


[<Tests>]
let resultOperatorsTests =

  testList "Result Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      createPostRequest <!> validLatR <*> validLngR <*> validTweetR
      |> Expect.hasOkValue validCreatePostRequest
    
    testCase "bind operator" <| fun _ ->
      validLatR
      >>= (fun lat -> 
            validLngR
            >>= (fun lng ->
                  validTweetR
                  >>= (fun tweet ->
                        Ok (createPostRequest lat lng tweet))))
      |> Expect.hasOkValue validCreatePostRequest
  ]


[<Tests>]
let tryCreateTests =
  testList "tryCreate Tests" [
    testCase "tryCreate happy path" <| fun _ ->
      Result.tryCreate "lat" lat
      |> Expect.hasOkValue validLat
    
    testCase "tryCreate error path" <| fun _ ->
      let r : Result<Latitude, (string * string)> = Result.tryCreate "lat" 200.
      Expect.hasErrorValue ("lat", invalidLatMsg) r
  ]

let err = "foobar"

[<Tests>]
let requireTrueTests =
  testList "requireTrue Tests" [
    testCase "requireTrue happy path" <| fun _ ->
      Result.requireTrue err true 
      |> Expect.hasOkValue ()

    testCase "requireTrue error path" <| fun _ ->
      Result.requireTrue err false 
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let requireFalseTests =
  testList "requireFalse Tests" [
    testCase "requireFalse happy path" <| fun _ ->
      Result.requireFalse err false 
      |> Expect.hasOkValue ()

    testCase "requireFalse error path" <| fun _ ->
      Result.requireFalse err true 
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let requireSomeTests =
  testList "requireSome Tests" [
    testCase "requireSome happy path" <| fun _ ->
      Result.requireSome err (Some 42) 
      |> Expect.hasOkValue 42

    testCase "requireSome error path" <| fun _ ->
      Result.requireSome err None 
      |> Expect.hasErrorValue err
  ]


[<Tests>]
let requireNoneTests =
  testList "requireNone Tests" [
    testCase "requireNone happy path" <| fun _ ->
      Result.requireNone err None 
      |> Expect.hasOkValue ()

    testCase "requireNone error path" <| fun _ ->
      Result.requireNone err (Some 42) 
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let requireEqualsTests =
  testList "requireEquals Tests" [
    testCase "requireEquals happy path" <| fun _ ->
      Result.requireEquals 42 err 42 
      |> Expect.hasOkValue ()

    testCase "requireEquals error path" <| fun _ ->
      Result.requireEquals 42 err 43
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let requireEmptyTests =
  testList "requireEmpty Tests" [
    testCase "requireEmpty happy path" <| fun _ ->
      Result.requireEmpty err []
      |> Expect.hasOkValue ()

    testCase "requireEmpty error path" <| fun _ ->
      Result.requireEmpty err [42]
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let requireNotEmptyTests =
  testList "requireNotEmpty Tests" [
    testCase "requireNotEmpty happy path" <| fun _ ->
      Result.requireNotEmpty err [42]
      |> Expect.hasOkValue ()

    testCase "requireNotEmpty error path" <| fun _ ->
      Result.requireNotEmpty err []
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let requireHeadTests =
  testList "requireHead Tests" [
    testCase "requireHead happy path" <| fun _ ->
      Result.requireHead err [42]
      |> Expect.hasOkValue 42

    testCase "requireHead error path" <| fun _ ->
      Result.requireHead err []
      |> Expect.hasErrorValue err
  ]

[<Tests>]
let setErrorTests =
  testList "setError Tests" [
    testCase "setError replaces a any error value with a custom error value" <| fun _ ->
      Result.setError err (Error "foo")
      |> Expect.hasErrorValue err

    testCase "setError does not change an ok value" <| fun _ ->
      Result.setError err (Ok 42)
      |> Expect.hasOkValue 42
  ]

[<Tests>]
let withErrorTests =
  testList "withError Tests" [
    testCase "withError replaces the unit error value with a custom error value" <| fun _ ->
      Result.withError err (Error ())
      |> Expect.hasErrorValue err

    testCase "withError does not change an ok value" <| fun _ ->
      Result.withError err (Ok 42)
      |> Expect.hasOkValue 42
  ]

[<Tests>]
let defaultValueTests = 
  testList "defaultValue Tests" [
    testCase "defaultValue returns the ok value" <| fun _ ->
      let v = Result.defaultValue 43 (Ok 42)
      Expect.equal v 42 ""

    testCase "defaultValue returns the given value for Error" <| fun _ ->
      let v = Result.defaultValue 43 (Error err)
      Expect.equal v 43 ""
  ]

[<Tests>]
let defaultWithTests =
  testList "defaultWith Tests" [
    testCase "defaultWith returns the ok value" <| fun _ ->
      let v = Result.defaultWith (fun () -> 43) (Ok 42)
      Expect.equal v 42 ""

    testCase "defaultValue invoks the given thunk for Error" <| fun _ ->
      let v = Result.defaultWith (fun () -> 42) (Error err)
      Expect.equal v 42 ""
  ]

[<Tests>]
let ignoreErrorTests =
  testList "ignoreError Tests" [
    testCase "ignoreError returns the unit for ok" <| fun _ ->
      Expect.equal (Result.ignoreError (Ok ())) () ""

    testCase "ignoreError returns the unit for Error" <| fun _ ->
      Expect.equal (Result.ignoreError (Error 42)) () ""
  ]


[<Tests>]
let teeTests =

  testList "tee Tests" [
    testCase "tee executes the function for ok" <| fun _ ->
      let foo = ref "foo"
      let input = ref 0
      let bar x = 
        input := x
        foo := "bar"
      let result = Result.tee bar (Ok 42)
      Expect.hasOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""

    testCase "tee ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = Result.tee bar (Error err)
      Expect.hasErrorValue err result
      Expect.equal !foo "foo" ""
  ]

let returnTrue _ = true
let returnFalse _ = false

[<Tests>]
let teeIfTests =
  testList "teeIf Tests" [
    testCase "teeIf executes the function for ok and true predicate " <| fun _ ->
      let foo = ref "foo"
      let input = ref 0
      let pInput = ref 0
      let returnTrue x = 
        pInput := x
        true
      let bar x = 
        input := x
        foo := "bar"
      let result = Result.teeIf returnTrue bar (Ok 42)
      Expect.hasOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""
      Expect.equal !pInput 42 ""

    testCase "teeIf ignores the function for Ok and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = Result.teeIf returnFalse bar (Ok 42)
      Expect.hasOkValue 42 result
      Expect.equal !foo "foo" ""

    testCase "teeIf ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = Result.teeIf returnTrue bar (Error err)
      Expect.hasErrorValue err result
      Expect.equal !foo "foo" ""
  ]

[<Tests>]
let teeErrorTests =

  testList "teeError Tests" [
    testCase "teeError executes the function for Error" <| fun _ ->
      let foo = ref "foo"
      let input = ref ""
      let bar x = 
        input := x
        foo := "bar"
      let result = Result.teeError bar (Error err)
      Expect.hasErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""

    testCase "teeError ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = Result.teeError bar (Ok 42)
      Expect.hasOkValue 42 result
      Expect.equal !foo "foo" ""
  ]

[<Tests>]
let teeErrorIfTests =
  testList "teeErrorIf Tests" [
    testCase "teeErrorIf executes the function for Error and true predicate " <| fun _ ->
      let foo = ref "foo"
      let input = ref ""
      let pInput = ref ""
      let returnTrue x = 
        pInput := x
        true
      let bar x = 
        input := x
        foo := "bar"
      let result = Result.teeErrorIf returnTrue bar (Error err)
      Expect.hasErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""
      Expect.equal !pInput err ""

    testCase "teeErrorIf ignores the function for Error and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = Result.teeErrorIf returnFalse bar (Error err)
      Expect.hasErrorValue err result
      Expect.equal !foo "foo" ""

    testCase "teeErrorIf ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = Result.teeErrorIf returnTrue bar (Ok 42)
      Expect.hasOkValue 42 result
      Expect.equal !foo "foo" ""
  ]