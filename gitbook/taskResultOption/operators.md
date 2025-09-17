## TaskResultOption Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.TaskResultOption`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Result<'T option, 'E>` type.

## Examples

### Example 1

The [TaskResultOption map2 example](../taskResultOption/map2.md#example-1) can be written using operators like this:

```fsharp
open FsToolkit.ErrorHandling.Operator.TaskResult

// Task<Result<UserTweet option, Exception>>
userTweet 
<!> (getPostById samplePostId) 
<*> (getUserById sampleUserId)
```
