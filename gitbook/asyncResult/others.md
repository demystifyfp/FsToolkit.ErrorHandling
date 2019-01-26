## Other Useful Functions

Ported from [Cvdm.ErrorHandling](https://github.com/cmeeren/Cvdm.ErrorHandling). Credits [Christer van der Meeren](https://github.com/cmeeren)


### requireTrue

Returns the specified error if the async-wrapped value is false.
```
'a -> Async<bool> -> Async<Result<unit, 'a>>`
```
### requireFalse

Returns the specified error if the async-wrapped value is true.
```
'a -> Async<bool> -> Async<Result<unit, 'a>>`
```