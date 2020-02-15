#### 1.2.6 - Feb 15, 2020

- Adds Result.valueOr . Credits [Christer van der Meeren](https://github.com/cmeeren).
- Adds Option Computation Expression. Credits [Jimmy Byrd](https://github.com/TheAngryByrd)

#### 1.2.5 - Oct 18, 2019

- Improve result CE overload resolution. Credits [Christer van der Meeren](https://github.com/cmeeren).

#### 1.2.4 - Oct 10, 2019

- Adds `ignore` function. Credits [Cameron Aavik](https://github.com/CameronAavik)

#### 1.2.3 - July 22, 2019

- Adds `requireNotNull` function. Credits [Daniel Bachler](https://github.com/danyx23)

#### 1.2.2 - May 31, 2019

- Adds sequenceAsync, sequenceTask & sequenceJob. Credits [Christer van der Meeren](https://github.com/cmeeren).
- Adds Fable tests. Credits [Jimmy Byrd](https://github.com/TheAngryByrd)

#### 1.2.1 - May 21, 2019

- Adds JobResult. Credits [Jimmy Byrd](https://github.com/TheAngryByrd)

#### 1.1.1 - May 9, 2019

- Fix SourceLinks

#### 1.1.0 - May 4, 2019

- Adds TaskResult. Credits [Jimmy Byrd](https://github.com/TheAngryByrd)
- Adds Symbols in the Pacakge. Credits [Christer van der Meeren](https://github.com/cmeeren).

#### 1.0.0 - April 10, 2019

- Adds AsyncResult and Result CE methods from Cvdm.ErrorHandling. Credits [Jimmy Byrd](https://github.com/TheAngryByrd), [Christer van der Meeren](https://github.com/cmeeren)
- **BREAKING CHANGEs**
  - Namespaces `FsToolkit.ErrorHandling.CE.Result`, `FsToolkit.ErrorHandling.CE.ResultOption`, `FsToolkit.ErrorHandling.CE.AsyncResult`, `FsToolkit.ErrorHandling.CE.AsyncResultOption` renamed to `FsToolkit.ErrorHandling`.
  - Removed `FsToolkit.ErrorHandling.AsyncResultOperators` module. Use `FsToolkit.ErrorHandling.Operator.AsyncResult` module instead.
  - `Result.requireEquals` renamed to `Result.requireEqualTo`
  - `AsyncResult.requireEquals` renamed to `AsyncResult.requireEqualTo`

#### 0.0.14 - January 27, 2019

- add AsyncResult helper functions from Cvdm.ErrorHandling. Credits [@cmeeren](https://github.com/cmeeren)

#### 0.0.13 - January 21, 2019

- add Result helper functions from Cvdm.ErrorHandling. Credits [@cmeeren](https://github.com/cmeeren)

#### 0.0.12 - January 4, 2019

- Add Fable Support

#### 0.0.11 - January 4, 2019

- NuGet Packaging using Dotnet Pack. Credits [@enerqi](www.github.com/enerqi)

#### 0.0.10 - December 30, 2018

- Add Fable support. Credits [@enerqi](www.github.com/enerqi)

#### 0.0.9 - October 5 2018

- Fix error message ordering in List.traverseResultA function.

#### 0.0.8 - October 5 2018

- Remove Statically resolved type constarint on input in tryCreate function.

#### 0.0.7 - October 5 2018

- Add explict type check for Validation map operator.

#### 0.0.6 - October 4 2018

- Move tryCreate function to Result modoule and drop tryCreate2 function.

#### 0.0.5 - September 16 2018

- Remove result option bind overload for result type.

#### 0.0.4 - September 16 2018

- Package Description Fix

#### 0.0.3 - September 8 2018

- Fix F# Core Package Version

#### 0.0.2 - September 8 2018

- Fix Package Description

#### 0.0.1 - September 8 2018

- Initial release
