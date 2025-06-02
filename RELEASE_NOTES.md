### 5.0.0-beta014 - Jun 02, 2025
- BREAKING: [Remove Ply and update to FSharp 6](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/248) Credits @TheAngryByrd
- BREAKING: [Remove MergeSources (and!) from some implementations like Result](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/261)  Credits @TheAngryByrd
- BREAKING: [Merge TaskResult into Core library](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/285) Credits @TheAngryByrd 
    - This means FsToolkit.ErrorHandling.TaskResult is no longer a separate package and will not be updated. It is now part of the core library.
- BREAKING: [Rename retn to singleton](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/287) Credits @1eyewonder
- BREAKING: [Rename returnError to error + documentation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/311) Credits @tw0po1nt
- [Use Microsoft.Bcl.AsyncInterfaces in netstandard2.0 (Allows IAsyncDisposable and IAsyncEnumerable)](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/250) Credits @TheAngryByrd
- [Build against Net8](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/251) Credits @TheAngryByrd
- [Fix Overload Resolution to Align to Computation Expression used](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/252) Credits @TheAngryByrd
- [refactor!: Seq.sequenceResultM returns Array instead of seq](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/255) Credits @bartelink
- [feat(Seq): sequenceResultA](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/255) Credits @bartelink
- [Updated uses of Seq.append](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/290) Credits @1eyewonder
- [Add Option.traverseAsync and Option.sequenceAsync](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/298#event-15827853276) Credits @tw0po1nt 
- [Add Require and Check helper methods](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/295) Credits @PI-Gorbo
- [Add new AsyncOption APIs and document all its other functions; minor fixes to documentation for Option module](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/307) Credits @tw0po1nt
- [F# 9 support and nullness](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/308) Credits @TheAngryByrd
- [Update IcedTasks 0.11.7](https://github.com/demystifyfp/FsToolkit.ErrorHandling/commit/0a4cc7b3c52efcef47dbc653b00d56ab029bbd01) Credits @TheAngryByrd
- [Add TaskValidation module](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/313) Credits @tw0po1nt
- [feat(Seq.traverse/sequence*)!: Yield arrays](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/310) Credits @bartelink
- [Add ParallelAsync CEs](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/318) Credits @njlr
- [Add Option.sequenceAsyncResult and Option.traverseAsyncResult](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/321) Credits @JayWearsSocks
- [Add traversals/sequences for Task and TaskResult in the Option module](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/325) Credits @tw0po1nt
- [Add ok and error helper functions to TaskResultOption and AsyncResultOption modules](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/327) Credits @tw0po1nt
- [Add CancellableTaskOption module and CE + tests and documentation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/328) Credits @tw0po1nt
- [Remove paket, Enforce nullness on net9.0, remove mocha](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/331) Credits @TheAngryByrd
- [Add TaskValueOption module, operators, and CE + tests and documentation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/329) Credits @tw0po1nt


### 4.18.0 - October 23, 2024
- [Add Array errorhandling](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/279) Credits @DashieTM

### 4.17.0 - September 23, 2024
- [Adding Seq.traverse & sequence functions](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/277) Credits @1eyewonder

### [4.16.0] - July 15, 2024
- [Add Task.ignore](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/272) Credits @odytrice

### [4.15.3] - July 13, 2024
- [Added XML Comments](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/268) Credits @1eyewonder
- [Fix Using/TryFinally asyncValidation CE](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/271) Credits @1eyewonder

### 4.15.2 - May 05, 2024
- [Fix AsyncValidationCE binding against asyncResult](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/260) Credits @1eyewonder

### 4.15.1 - January 15, 2024
- [Doc updates](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/247) Credits @1eyewonder

### 4.15.0 - January 10, 2024
- [Added Option functions and Added Missing Documentation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/246) Credits @1eyewonder

### 4.14.0 - January 01, 2024
- [Added AsyncResult and TaskResult Helpers](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/245) Credits @1eyewonder

### 4.13.0 - December 10, 2023
- [Add TaskResult.foldResult](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/242) Credits @Maxumka
- [Add support voption to require function](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/243) Credits Credits @Maxumka

### 4.12.0 - November 27, 2023
- [Add Task/AsyncOption defaultValue and defaultWith](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/238) Credits @sheridanchris

### 4.11.1 - November 23, 2023
- [fix: Add PackageLicenseExpression for /src](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/236) Credits @bartelink
- [Fix netstandard2 package version drift](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/237) Credits @TheAngryByrd

### 4.11.0 - November 11, 2023
- [Added sequence and traverse VOptionM](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/233) Credits @1eyewonder

### 4.10.0 - October 16, 2023
- [Added traverse and sequence functions for Option](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/231) Credits @1eyewonder

### 4.9.0 - July 09, 2023
- [ResultOption Updates](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/221) Credits @1eyewonder

### 4.8.0 - July 06, 2023

- [Added cancellableTaskValidation feature, tests, and documentation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/217) Credits @1eyewonder

### 4.7.0 - June 21, 2023
- [Added AsyncValidation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/215) Credits @1eyewonder

### 4.6.0 - April 20, 2023
- [Added bind operator for Validation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/214) Credits @ratsclub
- [Fable 4.0](https://github.com/demystifyfp/FsToolkit.ErrorHandling/commit/9a8682a95c62ea7761f1346e59f2f0ae63ae4440) Credits @TheAngryByrd

### 4.5.0 - March 26, 2023
- [AsyncResultOption updated to be more useful](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/211) Credits @TheAngryByrd
- [Adds use for IAsyncDisposable to async varieties](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/212) Credits @TheAngryByrd

### 4.4.0 - February 21, 2023
- [Added bind operator for Option](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/210) Credits @ratsclub

### 4.3.0 - January 20, 2023
- [Added Option.ofPair and ValueOption.ofPair](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/208) Credits @AlbertoDePena

### 4.2.1 - December 18, 2022
- [StartAsTask -> StartImmediateAsTask](https://github.com/fsprojects/FSharp.Control.TaskSeq/issues/135) Credits @TheAngryByrd

### 4.2.0 - December 17, 2022
- [Fixes while implementation in resumable codes](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/202) Credits @TheAngryByrd
- [Adds CancellableResultTask functions](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/203) Credits @TheAngryByrd

### 4.1.0 - December 13, 2022
- [Updates IcedTask to 0.5.0 and refactoring of Tasklike CEs](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/200) Credits @TheAngryByrd

### 4.0.0 - November 19, 2022
- BREAKING: [Better alignment with FSharp.Core Result functions](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/199). Credits @TheAngryByrd
- [Adding Option.either and friends](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/198). Credits @TheAngryByrd
- [Fixes for Option.tryParse and tryGetValue](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/197). Credits @TheAngryByrd

### 3.3.1 - November 19, 2022
- [FSharp.Core version warning](https://github.com/demystifyfp/FsToolkit.ErrorHandling/issues/194). Credits @TheAngryByrd
- [List.traverseValidationA and List.sequenceValidationA now preserve the order of errors](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/192). Credits @ursenzler
- [Fixing Fable Build](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/196) Credits @alfonsogarciacaro

### 3.3.0 - November 08, 2022
- [Updates for .NET 7](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/193). Credits @TheAngryByrd

### 3.2.0 - October 31, 2022
- [Add Option.maybe](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/189) Credits [@gdziadkiewicz](https://github.com/gdziadkiewicz)

### 3.1.0 - October 19, 2022
- [Add helper functions for options wrapped in a TaskResult, AsyncResult, and JobResult](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/186) Credits [@sheridanchris](https://github.com/sheridanchris)

### 3.0.1 - October 18, 2022
- [Fixes List.traverseA memory issues](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/185)

### 3.0.0 - October 13, 2022

- [Fixing stackoverflows in large while loops](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/182) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)
- [Moves many functions to inline with InlineIfLambda for performance](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/166) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)
- [Native Tasks for TaskResult, TaskOption, and TaskResultOption](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/169) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)
- [Add explicit type parameters to ignore functions](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/174) Credits [@cmeeren](https://github.com/cmeeren)
- [Adds CancellableTaskResult](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/172) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)
- [Fixes TaskResultCE breaking with a bind in branching such as if](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/177)  Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.13.0 - January 11, 2022

- [Option/ValueOption.ofNull and bindNull](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/164) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.12.0 - January 06, 2022

- [Option/ValueOption.tryGetValue and reduce testing complexity](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/163) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.11.1 - December 01, 2021

- [Seq.sequenceResultM returns seq instead of list](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/159) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.11.0 - November 24, 2021

- [integrates FsToolkit.ErrorHandling.AsyncSeq into main repository](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/155#issuecomment-978469011) Credits [@njlr](https://github.com/njlr)

### 2.10.0 - November 13, 2021

- [Adds Result.traverseAsync](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/153) Credits [@saerosV](https://github.com/saerosV)

### 2.9.0 - November 09, 2021

- [Adds ValueOption CE](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/131) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.8.1 - November 07, 2021

- [Adds README to nuget package](https://github.com/demystifyfp/FsToolkit.ErrorHandling/commit/966b0b60fa6e95dcc196a51531f981cf905555e6) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.8.0 - November 07, 2021

- [Fixes to example documentation](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/148) Credits [@njlr](https://github.com/njlr)
- Updates to Fable 3 [#1](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/129) [#2](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/149) Credits [@TheAngryByrd](https://github.com/TheAngryByrd) [@njlr](https://github.com/njlr)
- [Adds Option.ofResult](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/147) Credits [@njlr](https://github.com/njlr)
- [Adds Seq.sequenceResultM](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/146) Credits [@njlr](https://github.com/njlr)
- [Adds Fantomas for formatting](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/151) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)
- [Adds Github Issue and Pull Request templates](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/152) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

### 2.7.1 - October 19, 2021

- [Canonicalize option type annotations](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/140) Credits [@cmeeren](https://github.com/cmeeren)

### 2.7.0 - August 02, 2021

- [Extra Task, ValueTask, Ply CE sources](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/135) Credits [@kerams](https://github.com/kerams)

### 2.6.0 - July 07, 2021

- [Implements orElse and orElseWith](https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/133) Credits [@TheAngryByrd](https://github.com/TheAngryByrd)

#### 2.5.0 - May 26, 2021

- Added Async, Task and Job overloads for the relevant Option CEs to resolve Credits [@Jmaharman](https://github.com/Jmaharman) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/132>)

#### 2.4.0 - May 23, 2021

- Adds defaultError and zipError helpers Credits [@sep2](https://github.com/sep2) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/130>)

#### 2.3.0 - May 14, 2021

- Adds Applicative Support to OptionCE. Also adds bindings for Nullable and null objects Credits [@TheAngryByrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/126>)

#### 2.2.0 - April 21, 2021

- AsyncResult, TaskResult, JobResult error helpers Credits [@meridaio](https://github.com/meridaio) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/124>)

#### 2.1.2 - February 27, 2021

- Converts ValidationCE to use Source overloads Credits [@TheAngryByrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/115>)

#### 2.1.1 - February 26, 2021

- Added Description and License to nuget package Credits [@TheAngryByrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/114>)

#### 2.1.0 - February 26, 2021

- Performance enhancements for traverseValidationA. Credits [@isaacabraham](https://github.com/isaacabraham) Credits [@TheAngryByrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/110>) (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/111>) (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/113>)

#### 2.0.0 - November 20, 2020

- Switches TaskResult Library from TaskBuilder to Ply. Credits [Nino Floris](https://github.com/NinoFloris) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/97>)
  - This change replaces [TaskBuilder](https://github.com/rspeele/TaskBuilder.fs) with [Ply](https://github.com/crowded/ply).  Ply has better performance characteristics and more in line with how C# handles Task execution.  To convert from TaskBuilder to Ply, replace the namespace of `FSharp.Control.Tasks.V2.ContextInsensitive` with `FSharp.Control.Tasks`. -
  - This also removes the TargetFramework net461 as a build target. Current netstandard2.0 supports net472 fully according to [this chart](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support). It's recommended to upgrade your application to net472 if possible. If not, older versions of this library, such as 1.4.3, aren't going anywhere and can still be consumed from older TargetFrameworks.
- Switch to use Affine for Task related. Credits [@Swoorup](https://github.com/Swoorup). - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/107>)

#### 1.4.3 - July 21, 2020

- Adds IF FABLE_COMPILER to any Async.AwaitTask type functions in AsyncResult. Credits [Jimmy Byrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/93>)

#### 1.4.1 - June 23, 2020 [YANKED]

- Fixes Bindings against outer wrappers like async, task, or job to support complex workflows. Credits [Jimmy Byrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/87>)

#### 1.4.0 - June 05, 2020

- Adds AsyncOption, TaskOption, JobOption helpers. Credits [Michael-Jorge Gómez Campos](https://github.com/Micha-kun) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/82>)

#### 1.3.2 - June 05, 2020

- Uses Source Computation Expression overloads to help with maintainability. Credits [Jimmy Byrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/83>)

#### 1.3.1 - May 29, 2020

- Improved Stacktraces within computation expressions. Credits [Jimmy Byrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/81>)

#### 1.3.0 - May 25, 2020

- Adds Applicative Support for FSharp 5.0. Credits [Jimmy Byrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/75>)
- Reduces required FSharp.Core version to 4.3.4. Credits [Jimmy Byrd](https://github.com/TheAngryByrd) - (<https://github.com/demystifyfp/FsToolkit.ErrorHandling/pull/80>)

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
