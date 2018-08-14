module ResultOptionTests

open Expecto
open TestData
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.ResultOptionComputationExpression


[<Tests>]
let map2Tests =
  testList "ResultOption.map2 Tests" [

    testCase "map2 with Ok(Some x) Ok(Some y)" <| fun _ ->
      ResultOption.map2 location (Ok (Some validLat)) (Ok (Some validLng))
      |> Expect.hasOkValue (Some validLocation)

    testCase "map2 with Ok(Some x) Ok(None)" <| fun _ ->
      ResultOption.map2 location (Ok (Some validLat)) (Ok None)
      |> Expect.hasOkValue None

    testCase "map2 with Ok(None) Ok(None)" <| fun _ ->
      ResultOption.map2 location (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map2 with Error(Some x) Error(Some x)" <| fun _ ->
      ResultOption.map2 location (Error (Some validLat)) (Error (Some validLat2))
      |> Expect.hasErrorValue (Some validLat)
    
    testCase "map2 with Ok(Some x) Error(Some x)" <| fun _ ->
      ResultOption.map2 location (Ok (Some validLat)) (Error (Some validLng))
      |> Expect.hasErrorValue (Some validLng)
  ]

[<Tests>]
let map3Tests =
  testList "ResultOption.map3 Tests" [

    testCase "map3 with Ok(Some x) Ok(Some y) Ok (Some z)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok (Some validLat)) (Ok (Some validLng)) (Ok (Some validTweet))
      |> Expect.hasOkValue (Some validCreatePostRequest)

    testCase "map3 with Ok(Some x) Ok(None) Ok(None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok (Some validLat)) (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map3 with Ok(None) Ok(None) (Ok None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok None) (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map3 with Error(Some x) Error(Some x) (Ok None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Error (Some validLat)) (Error (Some validLat2)) (Ok None)
      |> Expect.hasErrorValue (Some validLat)
    
    testCase "map3 with Ok(Some x) Error(Some x) (Ok None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok (Some validLat)) (Error (Some validLng)) (Ok None)
      |> Expect.hasErrorValue (Some validLng)
  ]