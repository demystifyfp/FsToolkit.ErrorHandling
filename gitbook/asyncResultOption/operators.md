## AsyncResultOption Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.AsyncResultOption`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Result<Option<_>,_>` type.

## Examples

### Example 1

The [AsyncResultOption map2 example](../asyncResultOption/map2.md#example-1) can be written using operators like this:

```fsharp
open FsToolkit.ErrorHandling.Operator.AsyncResult

// Async<Result<UserTweet option, Exception>>
userTweet 
<!> (getPostById samplePostId) 
<*> (getUserById sampleUserId)
```
