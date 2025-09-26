module AsyncResultTests


#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif


open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open System


let mapTests =
    testList "AsyncResult.map tests" [
        testCaseAsync "map with Async(Ok x)"
        <| async {
            do!
                createPostSuccess validCreatePostRequest
                |> AsyncResult.map (fun (PostId id) -> { Id = id })
                |> Expect.hasAsyncOkValue { Id = newPostId }
        }

        testCaseAsync "map with Async(Error x)"
        <| async {
            do!
                createPostFailure validCreatePostRequest
                |> AsyncResult.mapError (fun ex -> { Message = ex.Message })
                |> Expect.hasAsyncErrorValue { Message = "something went wrong!" }
        }
    ]

let map2Tests =
    testList "AyncResult.map2 tests" [
        testCaseAsync "map2 with Async(Ok x) Async(Ok y)"
        <| async {
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersSuccess (UserId userId)
            let createPostResult = createPostSuccess validCreatePostRequest

            do!
                AsyncResult.map2 newPostRequest getFollowersResult createPostResult
                |> Expect.hasAsyncOkValue {
                    NewPostId = PostId newPostId
                    UserIds = followerIds
                }
        }

        testCaseAsync "map2 with Async(Error x) Async(Ok y)"
        <| async {
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersFailure (UserId userId)
            let createPostResult = createPostSuccess validCreatePostRequest

            do!
                AsyncResult.map2 newPostRequest getFollowersResult createPostResult
                |> Expect.hasAsyncErrorValue getFollowersEx
        }
        testCaseAsync "map2 with Async(Ok x) Async(Error y)"
        <| async {
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersSuccess (UserId userId)
            let createPostResult = createPostFailure validCreatePostRequest

            do!
                AsyncResult.map2 newPostRequest getFollowersResult createPostResult
                |> Expect.hasAsyncErrorValue commonEx
        }
        testCaseAsync "map2 with Async(Error x) Async(Error y)"
        <| async {
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersFailure (UserId userId)
            let createPostResult = createPostFailure validCreatePostRequest

            do!
                AsyncResult.map2 newPostRequest getFollowersResult createPostResult
                |> Expect.hasAsyncErrorValue getFollowersEx
        }
    ]

let foldResultTests =

    testList "AsyncResult.foldResult tests" [
        testCaseAsync "foldResult with Async(Ok x)"
        <| async {
            let! actual =
                createPostSuccess validCreatePostRequest
                |> AsyncResult.foldResult (fun (PostId id) -> id.ToString()) string

            Expect.same (newPostId.ToString()) actual
        }

        testCaseAsync "foldResult with Async(Error x)"
        <| async {
            let! actual =
                createPostFailure validCreatePostRequest
                |> AsyncResult.foldResult string (fun ex -> ex.Message)

            Expect.same (commonEx.Message) actual
        }
    ]


let mapErrorTests =
    testList "AsyncResult.mapError tests" [
        testCaseAsync "mapError with Async(Ok x)"
        <| async {
            let! actual =
                createPostSuccess validCreatePostRequest
                |> AsyncResult.mapError id

            flip Expect.isOk "mapError should not map Ok" actual
        }

        testCaseAsync "mapError with Async(Error x)"
        <| async {
            do!
                createPostFailure validCreatePostRequest
                |> AsyncResult.mapError (fun ex -> ex.Message)
                |> Expect.hasAsyncErrorValue (commonEx.Message)
        }
    ]


let bindTests =
    testList "AsyncResult.bind tests" [
        testCaseAsync
            "bind with Async(Ok x)"
            (allowedToPost sampleUserId
             |> AsyncResult.bind (fun isAllowed ->
                 async {
                     if isAllowed then
                         return! createPostSuccess validCreatePostRequest
                     else
                         return (Error(Exception "not allowed to post"))
                 }
             )
             |> Expect.hasAsyncOkValue (PostId newPostId))

        testCaseAsync "bind with Async(Error x)"
        <| (allowedToPost (UserId(Guid.NewGuid()))
            |> AsyncResult.bind (fun isAllowed -> async { return Ok isAllowed })
            |> Expect.hasAsyncErrorValue commonEx)

        testCaseAsync
            "bind with Async(Ok x) that returns Async (Error x)"
            (let ex = Exception "not allowed to post"

             allowedToPost sampleUserId
             |> AsyncResult.bind (fun _ -> async { return (Error ex) })
             |> Expect.hasAsyncErrorValue ex)
    ]


let orElseTests =
    testList "AsyncResult.orElseWith Tests" [
        testCaseAsync "Ok Ok takes first Ok"
        <| async {
            return!
                AsyncResult.ok "First"
                |> AsyncResult.orElse (AsyncResult.ok "Second")
                |> Expect.hasAsyncOkValue "First"
        }
        testCaseAsync "Ok Error takes first Ok"
        <| async {
            return!
                AsyncResult.ok "First"
                |> AsyncResult.orElse (AsyncResult.error "Second")
                |> Expect.hasAsyncOkValue "First"
        }
        testCaseAsync "Error Ok takes second Ok"
        <| async {
            return!
                AsyncResult.error "First"
                |> AsyncResult.orElse (AsyncResult.ok "Second")
                |> Expect.hasAsyncOkValue "Second"
        }
        testCaseAsync "Error Error takes second error"
        <| async {
            return!
                AsyncResult.error "First"
                |> AsyncResult.orElse (AsyncResult.error "Second")
                |> Expect.hasAsyncErrorValue "Second"
        }
    ]

let orElseWithTests =
    testList "AsyncResult.orElse Tests" [
        testCaseAsync "Ok Ok takes first Ok"
        <| async {
            return!
                AsyncResult.ok "First"
                |> AsyncResult.orElseWith (fun _ -> AsyncResult.ok "Second")
                |> Expect.hasAsyncOkValue "First"
        }
        testCaseAsync "Ok Error takes first Ok"
        <| async {
            return!
                AsyncResult.ok "First"
                |> AsyncResult.orElseWith (fun _ -> AsyncResult.error "Second")
                |> Expect.hasAsyncOkValue "First"
        }
        testCaseAsync "Error Ok takes second Ok"
        <| async {
            return!
                AsyncResult.error "First"
                |> AsyncResult.orElseWith (fun _ -> AsyncResult.ok "Second")
                |> Expect.hasAsyncOkValue "Second"
        }
        testCaseAsync "Error Error takes second error"
        <| async {
            return!
                AsyncResult.error "First"
                |> AsyncResult.orElseWith (fun _ -> AsyncResult.error "Second")
                |> Expect.hasAsyncErrorValue "Second"
        }
    ]


let ignoreTests =
    testList "AsyncResult.ignore tests" [
        testCaseAsync "ignore with Async(Ok x)"
        <| async {
            do!
                createPostSuccess validCreatePostRequest
                |> AsyncResult.ignore
                |> Expect.hasAsyncOkValue ()
        }

        testCaseAsync "ignore with Async(Error x)"
        <| async {
            do!
                createPostFailure validCreatePostRequest
                |> AsyncResult.ignore
                |> Expect.hasAsyncErrorValue commonEx
        }

        testCase "can call ignore without type parameters"
        <| fun () -> ignore AsyncResult.ignore

        testCase "can call ignore with type parameters"
        <| fun () ->
            ignore<Async<Result<int, string>> -> Async<Result<unit, string>>>
                AsyncResult.ignore<int, string>
    ]


let err = "foobar"
let toAsync x = async { return x }

let requireTrueTests =
    testList "AsyncResult.requireTrue Tests" [
        testCaseAsync "requireTrue happy path"
        <| (toAsync true
            |> AsyncResult.requireTrue err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireTrue error path"
        <| (toAsync false
            |> AsyncResult.requireTrue err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireFalseTests =
    testList "AsyncResult.requireFalse Tests" [
        testCaseAsync "requireFalse happy path"
        <| (toAsync false
            |> AsyncResult.requireFalse err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireFalse error path"
        <| (toAsync true
            |> AsyncResult.requireFalse err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireSomeTests =
    testList "AsyncResult.requireSome Tests" [
        testCaseAsync "requireSome happy path"
        <| (toAsync (Some 42)
            |> AsyncResult.requireSome err
            |> Expect.hasAsyncOkValue 42)

        testCaseAsync "requireSome error path"
        <| (toAsync None
            |> AsyncResult.requireSome err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireNoneTests =
    testList "AsyncResult.requireNone Tests" [
        testCaseAsync "requireNone happy path"
        <| (toAsync None
            |> AsyncResult.requireNone err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireNone error path"
        <| (toAsync (Some 42)
            |> AsyncResult.requireNone err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireValueSomeTests =
    testList "AsyncResult.requireValueSome Tests" [
        testCaseAsync "requireValueSome happy path"
        <| (toAsync (ValueSome 42)
            |> AsyncResult.requireValueSome err
            |> Expect.hasAsyncOkValue 42)

        testCaseAsync "requireValueSome error path"
        <| (toAsync ValueNone
            |> AsyncResult.requireValueSome err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireValueNoneTests =
    testList "AsyncResult.requireValueNone Tests" [
        testCaseAsync "requireValueNone happy path"
        <| (toAsync ValueNone
            |> AsyncResult.requireValueNone err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireValueNone error path"
        <| (toAsync (ValueSome 42)
            |> AsyncResult.requireValueNone err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireEqualToTests =
    testList "AsyncResult.requireEqualTo Tests" [
        testCaseAsync "requireEqualTo happy path"
        <| (toAsync 42
            |> AsyncResult.requireEqualTo 42 err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireEqualTo error path"
        <| (toAsync 43
            |> AsyncResult.requireEqualTo 42 err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireEqualTests =
    testList "AsyncResult.requireEqual Tests" [
        testCaseAsync "requireEqual happy path"
        <| (AsyncResult.requireEqual 42 (toAsync 42) err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireEqual error path"
        <| (AsyncResult.requireEqual 42 (toAsync 43) err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireEmptyTests =
    testList "AsyncResult.requireEmpty Tests" [
        testCaseAsync "requireEmpty happy path"
        <| (toAsync []
            |> AsyncResult.requireEmpty err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireEmpty error path"
        <| (toAsync [ 42 ]
            |> AsyncResult.requireEmpty err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireNotEmptyTests =
    testList "AsyncResult.requireNotEmpty Tests" [
        testCaseAsync "requireNotEmpty happy path"
        <| (toAsync [ 42 ]
            |> AsyncResult.requireNotEmpty err
            |> Expect.hasAsyncOkValue ())

        testCaseAsync "requireNotEmpty error path"
        <| (toAsync []
            |> AsyncResult.requireNotEmpty err
            |> Expect.hasAsyncErrorValue err)
    ]


let requireHeadTests =
    testList "AsyncResult.requireHead Tests" [
        testCaseAsync "requireHead happy path"
        <| (toAsync [ 42 ]
            |> AsyncResult.requireHead err
            |> Expect.hasAsyncOkValue 42)

        testCaseAsync "requireHead error path"
        <| (toAsync []
            |> AsyncResult.requireHead err
            |> Expect.hasAsyncErrorValue err)
    ]

let requireTests =
    testList "AsyncResult.require Tests" [
        testCaseAsync "True, Ok"
        <| async {
            do!
                AsyncResult.require (fun _ -> true) ("Error!") (AsyncResult.ok (1))
                |> Expect.hasAsyncOkValue (1)
        }

        testCaseAsync "True, Error"
        <| async {
            do!
                AsyncResult.require (fun _ -> true) ("Error!") (AsyncResult.error ("AHH"))
                |> Expect.hasAsyncErrorValue ("AHH")
        }

        testCaseAsync "False, Ok"
        <| async {
            do!
                AsyncResult.require (fun _ -> false) ("Error!") (AsyncResult.ok (1))
                |> Expect.hasAsyncErrorValue ("Error!")
        }

        testCaseAsync "False, Error"
        <| async {
            do!
                AsyncResult.require (fun _ -> false) ("Error!") (AsyncResult.error ("Ahh"))
                |> Expect.hasAsyncErrorValue ("Ahh")
        }
    ]

let setErrorTests =
    testList "AsyncResult.setError Tests" [
        testCaseAsync "setError replaces a any error value with a custom error value"
        <| (toAsync (Error "foo")
            |> AsyncResult.setError err
            |> Expect.hasAsyncErrorValue err)

        testCaseAsync "setError does not change an ok value"
        <| (toAsync (Ok 42)
            |> AsyncResult.setError err
            |> Expect.hasAsyncOkValue 42)
    ]


let withErrorTests =
    testList "AsyncResult.withError Tests" [
        testCaseAsync "withError replaces a any error value with a custom error value"
        <| (toAsync (Error())
            |> AsyncResult.withError err
            |> Expect.hasAsyncErrorValue err)

        testCaseAsync "withError does not change an ok value"
        <| (toAsync (Ok 42)
            |> AsyncResult.withError err
            |> Expect.hasAsyncOkValue 42)
    ]


let defaultValueTests =
    testList "AsyncResult.defaultValue Tests" [
        testCaseAsync "defaultValue returns the ok value"
        <| (let v = AsyncResult.defaultValue 43 (toAsync (Ok 42))

            Expect.hasAsyncValue 42 v)

        testCaseAsync "defaultValue returns the given value for Error"
        <| (let v = AsyncResult.defaultValue 43 (toAsync (Error err))

            Expect.hasAsyncValue 43 v)
    ]


let defaultErrorTests =
    testList "AsyncResult.defaultError Tests" [
        testCaseAsync "defaultError returns the error value"
        <| (let v = AsyncResult.defaultError 43 (toAsync (Error 42))

            Expect.hasAsyncValue 42 v)

        testCaseAsync "defaultError returns the given value for Ok"
        <| (let v = AsyncResult.defaultError 43 (toAsync (Ok 42))

            Expect.hasAsyncValue 43 v)
    ]


let defaultWithTests =
    testList "AsyncResult.defaultWith Tests" [
        testCaseAsync "defaultWith returns the ok value"
        <| (let v = AsyncResult.defaultWith (fun _ -> 43) (toAsync (Ok 42))

            Expect.hasAsyncValue 42 v)

        testCaseAsync "defaultValue invoks the given thunk for Error"
        <| (let v = AsyncResult.defaultWith (fun _ -> 42) (toAsync (Error err))

            Expect.hasAsyncValue 42 v)
    ]


let ignoreErrorTests =
    testList "AsyncResult.ignoreError Tests" [
        testCaseAsync "ignoreError returns the unit for ok"
        <| Expect.hasAsyncValue () (AsyncResult.ignoreError (toAsync (Ok())))

        testCaseAsync "ignoreError returns the unit for Error"
        <| Expect.hasAsyncValue () (AsyncResult.ignoreError (toAsync (Error err)))

        testCase "can call ignoreError without type parameter"
        <| fun () -> ignore AsyncResult.ignoreError

        testCase "can call ignoreError with type parameter"
        <| fun () ->
            ignore<Async<Result<unit, string>> -> Async<unit>> AsyncResult.ignoreError<string>
    ]


let teeTests =
    testList "AsyncResult.tee Tests" [
        testCaseAsync "tee executes the function for ok"
        <| async {
            let mutable foo = "foo"
            let mutable input = 0

            let bar x =
                input <- x

                foo <- "bar"

            let result = AsyncResult.tee bar (toAsync (Ok 42))
            do! Expect.hasAsyncOkValue 42 result
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""
        }

        testCaseAsync "tee ignores the function for Error"
        <| async {
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = AsyncResult.tee bar (toAsync (Error err))

            do! Expect.hasAsyncErrorValue err result
            Expect.equal foo "foo" ""
        }
    ]

let returnTrue _ = true
let returnFalse _ = false


let teeIfTests =
    testList "AsyncResult.teeIf Tests" [
        testCaseAsync "teeIf executes the function for ok and true predicate "
        <| async {
            let mutable foo = "foo"
            let mutable input = 0
            let mutable pInput = 0

            let returnTrue x =
                pInput <- x

                true

            let bar x =
                input <- x

                foo <- "bar"

            let result = AsyncResult.teeIf returnTrue bar (toAsync (Ok 42))

            do! Expect.hasAsyncOkValue 42 result
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""
            Expect.equal pInput 42 ""
        }

        testCaseAsync "teeIf ignores the function for Ok and false predicate"
        <| async {
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = AsyncResult.teeIf returnFalse bar (toAsync (Ok 42))

            do! Expect.hasAsyncOkValue 42 result
            Expect.equal foo "foo" ""
        }

        testCaseAsync "teeIf ignores the function for Error"
        <| async {
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = AsyncResult.teeIf returnTrue bar (toAsync (Error err))

            do! Expect.hasAsyncErrorValue err result
            Expect.equal foo "foo" ""
        }
    ]


let teeErrorTests =
    testList "AsyncResult.teeError Tests" [
        testCaseAsync "teeError executes the function for Error"
        <| async {
            let mutable foo = "foo"
            let mutable input = ""

            let bar x =
                input <- x

                foo <- "bar"

            let result = AsyncResult.teeError bar (toAsync (Error err))

            do! Expect.hasAsyncErrorValue err result
            Expect.equal foo "bar" ""
            Expect.equal input err ""
        }

        testCaseAsync "teeError ignores the function for Ok"
        <| async {
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = AsyncResult.teeError bar (toAsync (Ok 42))

            do! Expect.hasAsyncOkValue 42 result
            Expect.equal foo "foo" ""
        }
    ]


let teeErrorIfTests =
    testList "AsyncResult.teeErrorIf Tests" [
        testCaseAsync "teeErrorIf executes the function for Error and true predicate "
        <| async {
            let mutable foo = "foo"
            let mutable input = ""
            let mutable pInput = ""

            let returnTrue x =
                pInput <- x

                true

            let bar x =
                input <- x

                foo <- "bar"

            let result = AsyncResult.teeErrorIf returnTrue bar (toAsync (Error err))

            do! Expect.hasAsyncErrorValue err result
            Expect.equal foo "bar" ""
            Expect.equal input err ""
            Expect.equal pInput err ""
        }

        testCaseAsync "teeErrorIf ignores the function for Error and false predicate"
        <| async {
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = AsyncResult.teeErrorIf returnFalse bar (toAsync (Error err))

            do! Expect.hasAsyncErrorValue err result
            Expect.equal foo "foo" ""
        }

        testCaseAsync "teeErrorIf ignores the function for Ok"
        <| async {
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = AsyncResult.teeErrorIf returnTrue bar (toAsync (Ok 42))

            do! Expect.hasAsyncOkValue 42 result
            Expect.equal foo "foo" ""
        }
    ]

let catchTests =
    let f (e: exn) = e.Message

    let asyncThrow () =
        async {
            failwith err
            return Error ""
        }

    testList "AsyncResult.catch tests" [
        testCaseAsync "catch returns success for Ok"
        <| Expect.hasAsyncOkValue 42 (AsyncResult.catch f (toAsync (Ok 42)))

        testCaseAsync "catch returns mapped Error for exception"
        <| Expect.hasAsyncErrorValue err (AsyncResult.catch f (asyncThrow ()))

        testCaseAsync "catch returns unmapped error without exception"
        <| Expect.hasAsyncErrorValue "unmapped" (AsyncResult.catch f (toAsync (Error "unmapped")))
    ]

let getOrReraiseTests =
    let makeException () : exn =
        try
            failwith "Kaboom"
        with exn ->
            exn

    testList "AsyncResult.getOrReraise tests" [
        testCaseAsync "getOrReraise preserves message and stack-trace"
        <| async {
            let exn = makeException ()

            let! captured =
                async {
                    try
                        let! () =
                            AsyncResult.error exn
                            |> AsyncResult.getOrReraise

                        return failwith "Unexpected"
                    with exn ->
                        return exn
                }

            Expect.equal captured.Message exn.Message ""
#if !FABLE_COMPILER
            Expect.equal captured.StackTrace exn.StackTrace ""
#endif
        }
    ]

type CreatePostResult =
    | PostSuccess of NotifyNewPostRequest
    | NotAllowedToPost


let asyncResultCETests =
    let createPost userId =
        asyncResult {
            let! isAllowed = allowedToPost userId

            if isAllowed then
                let! postId = createPostSuccess validCreatePostRequest
                let! followerIds = getFollowersSuccess sampleUserId

                return
                    PostSuccess {
                        NewPostId = postId
                        UserIds = followerIds
                    }
            else
                return NotAllowedToPost
        }

    testList "asyncResult Computation Expression tests" [
        testCaseAsync "bind with all Ok"
        <| (createPost sampleUserId
            |> Expect.hasAsyncOkValue (
                PostSuccess {
                    NewPostId = PostId newPostId
                    UserIds = followerIds
                }
            ))

        testCaseAsync "bind with an Error"
        <| (createPost (UserId(System.Guid.NewGuid()))
            |> Expect.hasAsyncErrorValue commonEx)
    ]


let asyncResultOperatorTests =
    testList "AsyncResult Operators Tests" [
        testCaseAsync "map & apply operators"
        <| async {
            let getFollowersResult = getFollowersSuccess sampleUserId
            let createPostResult = createPostSuccess validCreatePostRequest

            do!
                newPostRequest
                <!> getFollowersResult
                <*> createPostResult
                |> Expect.hasAsyncOkValue {
                    NewPostId = PostId newPostId
                    UserIds = followerIds
                }
        }

        testCaseAsync "bind operator"
        <| async {
            do!
                allowedToPost sampleUserId
                >>= (fun isAllowed ->
                    if isAllowed then
                        createPostSuccess validCreatePostRequest
                    else
                        AsyncResult.error (Exception "")
                )
                |> Expect.hasAsyncOkValue (PostId newPostId)
        }
    ]


let zipTests =
    testList "AsyncResult.zip Tests" [
        testCaseAsync "Ok, Ok"
        <| async {
            let! v = AsyncResult.zip (toAsync (Ok 1)) (toAsync (Ok 2))
            Expect.equal v (Ok(1, 2)) "Should be ok"
        }
        testCaseAsync "Ok, Error"
        <| async {
            let! v = AsyncResult.zip (toAsync (Ok 1)) (toAsync (Error "Bad"))
            Expect.equal v (Error("Bad")) "Should be Error"
        }
        testCaseAsync "Error, Ok"
        <| async {
            let! v = AsyncResult.zip (toAsync (Error "Bad")) (toAsync (Ok 1))
            Expect.equal v (Error("Bad")) "Should be Error"
        }
        testCaseAsync "Error, Error"
        <| async {
            let! v = AsyncResult.zip (toAsync (Error "Bad1")) (toAsync (Error "Bad2"))
            Expect.equal v (Error("Bad1")) "Should be Error"
        }
    ]


let zipErrorTests =
    testList "AsyncResult.zipError Tests" [
        testCaseAsync "Ok, Ok"
        <| async {
            let! v = AsyncResult.zipError (toAsync (Ok 1)) (toAsync (Ok 2))
            Expect.equal v (Ok(1)) "Should be ok"
        }
        testCaseAsync "Ok, Error"
        <| async {
            let! v = AsyncResult.zipError (toAsync (Ok 1)) (toAsync (Error "Bad"))
            Expect.equal v (Ok 1) "Should be ok"
        }
        testCaseAsync "Error, Ok"
        <| async {
            let! v = AsyncResult.zipError (toAsync (Error "Bad")) (toAsync (Ok 1))
            Expect.equal v (Ok 1) "Should be ok"
        }
        testCaseAsync "Error, Error"
        <| async {
            let! v = AsyncResult.zipError (toAsync (Error "Bad1")) (toAsync (Error "Bad2"))
            Expect.equal v (Error("Bad1", "Bad2")) "Should be Error"
        }
    ]


let asyncResultBindRequireTests =
    testList "AsyncResult Bind + Require Tests" [
        testCaseAsync "bindRequireNone"
        <| async {
            do!
                Some "john_doe"
                |> AsyncResult.ok
                |> AsyncResult.bindRequireNone "User exists"
                |> Expect.hasAsyncErrorValue "User exists"
        }

        testCaseAsync "bindRequireSome"
        <| async {
            do!
                Some "john_doe"
                |> AsyncResult.ok
                |> AsyncResult.bindRequireSome "User doesn't exist"
                |> Expect.hasAsyncOkValue "john_doe"
        }
    ]


let asyncResultBindRequireValueOptionTests =
    testList "AsyncResult Bind + RequireValueOption Tests" [
        testCaseAsync "bindRequireValueNone"
        <| async {
            do!
                ValueSome "john_doe"
                |> AsyncResult.ok
                |> AsyncResult.bindRequireValueNone "User exists"
                |> Expect.hasAsyncErrorValue "User exists"
        }

        testCaseAsync "bindRequireValueSome"
        <| async {
            do!
                ValueSome "john_doe"
                |> AsyncResult.ok
                |> AsyncResult.bindRequireValueSome "User doesn't exist"
                |> Expect.hasAsyncOkValue "john_doe"
        }
    ]

let asyncResultBindRequireTrueTests =
    testList "AsyncResult Bind + RequireTrue Tests" [
        testCaseAsync "bindRequireTrue"
        <| async {
            do!
                true
                |> AsyncResult.ok
                |> AsyncResult.bindRequireTrue "Should be true"
                |> Expect.hasAsyncOkValue ()
        }

        testCaseAsync "bindRequireFalse"
        <| async {
            do!
                false
                |> AsyncResult.ok
                |> AsyncResult.bindRequireFalse "Should be false"
                |> Expect.hasAsyncOkValue ()
        }
    ]

let asyncResultBindRequireNotNullTests =
    testList "AsyncResult Bind + RequireNotNull Tests" [
        testCaseAsync "bindRequireNotNull"
        <| async {
            do!
                ("Test": StringNull)
                |> AsyncResult.ok
                |> AsyncResult.bindRequireNotNull "Should not be null"
                |> Expect.hasAsyncOkValue "Test"
        }
    ]

let asyncResultBindRequireEqualTests =
    testList "AsyncResult Bind + RequireEqual Tests" [
        testCaseAsync "bindRequireEqual"
        <| async {
            do!
                2
                |> AsyncResult.ok
                |> AsyncResult.bindRequireEqual 2 "Should be equal"
                |> Expect.hasAsyncOkValue ()
        }
    ]

let asyncResultBindRequireEmptyTests =
    testList "AsyncResult Bind + RequireEmpty Tests" [
        testCaseAsync "bindRequireEmpty"
        <| async {
            do!
                []
                |> AsyncResult.ok
                |> AsyncResult.bindRequireEmpty "Should be empty"
                |> Expect.hasAsyncOkValue ()
        }
    ]

let asyncResultBindRequireNotEmptyTests =
    testList "AsyncResult Bind + RequireNotEmpty Tests" [
        testCaseAsync "bindRequireNotEmpty"
        <| async {
            do!
                [ 1 ]
                |> AsyncResult.ok
                |> AsyncResult.bindRequireNotEmpty "Should not be empty"
                |> Expect.hasAsyncOkValue ()
        }
    ]

let asyncResultBindRequireHeadTests =
    testList "AsyncResult Bind + RequireHead Tests" [
        testCaseAsync "bindRequireHead"
        <| async {
            do!
                [ 1 ]
                |> AsyncResult.ok
                |> AsyncResult.bindRequireHead "Should not be empty"
                |> Expect.hasAsyncOkValue 1
        }
    ]

let checkTests =
    testList "AsyncResult.check Tests" [
        testCaseAsync "Ok, Ok"
        <| async {
            do!
                AsyncResult.check (fun number -> AsyncResult.ok ()) (AsyncResult.ok (1))
                |> Expect.hasAsyncOkValue (1)
        }

        testCaseAsync "Ok, Error"
        <| async {
            do!
                AsyncResult.check (fun number -> AsyncResult.ok ()) (AsyncResult.error (2))
                |> Expect.hasAsyncErrorValue (2)
        }

        testCaseAsync "Error, OK"
        <| async {
            do!
                AsyncResult.check (fun number -> AsyncResult.error ()) (AsyncResult.ok (2))
                |> Expect.hasAsyncErrorValue (())
        }

        testCaseAsync "Error, Error"
        <| async {
            do!
                AsyncResult.check (fun number -> AsyncResult.error (1)) (AsyncResult.error (2))
                |> Expect.hasAsyncErrorValue (2)
        }
    ]

let allTests =
    testList "Async Result tests" [
        mapTests
        map2Tests
        foldResultTests
        mapErrorTests
        bindTests
        orElseTests
        orElseWithTests
        ignoreTests
        requireTrueTests
        requireFalseTests
        requireSomeTests
        requireNoneTests
        requireValueSomeTests
        requireValueNoneTests
        requireEqualToTests
        requireEqualTests
        requireEmptyTests
        requireNotEmptyTests
        requireHeadTests
        requireTests
        setErrorTests
        withErrorTests
        defaultValueTests
        defaultErrorTests
        defaultWithTests
        ignoreErrorTests
        teeTests
        teeIfTests
        teeErrorTests
        teeErrorIfTests
        catchTests
        getOrReraiseTests
        asyncResultCETests
        asyncResultOperatorTests
        zipTests
        zipErrorTests
        asyncResultBindRequireTests
        asyncResultBindRequireValueOptionTests
        asyncResultBindRequireTrueTests
        asyncResultBindRequireNotNullTests
        asyncResultBindRequireEqualTests
        asyncResultBindRequireEmptyTests
        asyncResultBindRequireNotEmptyTests
        asyncResultBindRequireHeadTests
        checkTests
    ]
