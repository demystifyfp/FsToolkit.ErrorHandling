# FsToolkit.ErrorHandling

FsToolkit.ErrorHandling is an F# utility library to work with the Result type, enabling clear, simple and powerful error handling. The library provides computation expressions, utility functions, and supports multiple target frameworks including .NET Standard 2.0/2.1, .NET 9.0, .NET 10.0, and .NET 11.0, and compiles to JavaScript and Python via Fable.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build the Repository
- **Install .NET SDK**: Download and install .NET SDK 11.0.100 or a later SDK 11 feature band (`global.json`)
  - Also install the .NET 10.0.x runtime (the test projects target `net10.0` and `net11.0`)
  - Use `dotnet --version` to verify installation
  - **CRITICAL**: Ensure the SDK is in PATH before running any build commands
- **Install Node.js**: Install Node.js v18.0.0 or higher (required for Fable JavaScript builds)
  - Current environment has v20.19.5 which works perfectly
  - Use `node --version` to verify installation
- **Install Python**: Install Python 3.12.0 or higher (required for Fable Python builds)
  - Current environment has Python 3.12.3 which works perfectly
  - Use `python3 --version` to verify installation

### Build Commands and Timing
**NEVER CANCEL ANY BUILD COMMAND** - Use appropriate timeouts and wait for completion.

- `./build.sh DotnetRestore` -- restores .NET dependencies. Takes ~2 seconds. NEVER CANCEL.
- `./build.sh Build` -- compiles all target frameworks. Takes ~90 seconds. NEVER CANCEL. Set timeout to 180+ seconds.
- `./build.sh DotnetTest` -- runs F# unit tests. Takes ~12 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
- `./build.sh NpmRestore` -- installs JavaScript dependencies. Takes ~11 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
- `./build.sh NpmTest` -- compiles F# to JavaScript via Fable and runs tests. Takes ~50 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
- `./build.sh PythonTest` -- compiles F# to Python via Fable and runs tests. Takes ~2 minutes. NEVER CANCEL. Set timeout to 300+ seconds.
- `./build.sh WebSharperTest` -- packs `FsToolkit.ErrorHandling.Sources`, compiles a WebSharper proxy from it, and runs the `FsToolkit.ErrorHandling.Tests` tests under WebSharper in Node.js. Takes ~1 minute. NEVER CANCEL. Set timeout to 300+ seconds.
- `./build.sh RunTests` -- runs ALL tests (F#, JavaScript, Python, WebSharper). Takes ~4.5 minutes. NEVER CANCEL. Set timeout to 600+ seconds.
- `./build.sh FormatCode` -- formats F# code using Fantomas. Takes ~5 seconds. NEVER CANCEL.
- `./build.sh CheckFormatCode` -- validates code formatting. Takes ~5 seconds. NEVER CANCEL.
- `./build.sh Clean` -- cleans bin/obj folders. Takes <1 second.

### Default Build Target
- `./build.sh` (no arguments) -- runs `SourcesTest` target which builds, tests, and packages all projects, and then tests the `FsToolkit.ErrorHandling.Sources` package. Takes ~5-7 minutes. NEVER CANCEL. Set timeout to 600+ seconds.

## Validation Requirements

### Pre-commit Validation
Always run these commands before committing changes:
- `./build.sh CheckFormatCode` -- ensures code follows formatting standards
- `./build.sh RunTests` -- runs complete test suite across all platforms (F#, JavaScript, Python, WebSharper)
- `./build.sh SourcesTest` -- tests the `FsToolkit.ErrorHandling.Sources` package, as CI does. Run it when you change `src/FsToolkit.ErrorHandling`, `tests/FsToolkit.ErrorHandling.Tests`, `build/`, or the Sources README

### Complete Development Workflow Validation
Test the complete workflow with: `./build.sh CheckFormatCode && ./build.sh RunTests`
- Total time: ~4.5-5 minutes
- Must pass all 3422+ tests across F#, JavaScript, and Python platforms
- **NEVER CANCEL** - Set timeout to 600+ seconds

### Manual Testing Scenarios
After making changes, validate functionality by:
1. **Build Validation**: Run `./build.sh Build` to ensure compilation succeeds across all target frameworks
2. **Test Validation**: Run `./build.sh RunTests` to execute the complete test suite (835 JS tests + 2 Python tests + 3406+ F# tests)
3. **Format Validation**: Run `./build.sh CheckFormatCode` to ensure code formatting compliance

### CI Build Requirements
The GitHub Actions CI will fail if:
- Code formatting is incorrect (run `./build.sh FormatCode` to fix)
- Any tests fail in F#, JavaScript, Python, or WebSharper targets
- Build fails on any supported platform (Ubuntu, Windows, macOS)
- Code analysis issues are detected

## Project Structure

### Key Directories
```
src/                           # Source code for all packages
├── FsToolkit.ErrorHandling/          # Core library (Result, AsyncResult, etc.)
├── FsToolkit.ErrorHandling.AsyncSeq/ # AsyncSeq extensions  
├── FsToolkit.ErrorHandling.JobResult/ # JobResult extensions
└── FsToolkit.ErrorHandling.IcedTasks/ # IcedTasks extensions

tests/                         # Test projects
├── FsToolkit.ErrorHandling.Tests/          # Core tests (majority of test coverage)
├── FsToolkit.ErrorHandling.AsyncSeq.Tests/ # AsyncSeq tests (limited Fable support)
├── FsToolkit.ErrorHandling.JobResult.Tests/ # JobResult tests
├── FsToolkit.ErrorHandling.IcedTasks.Tests/ # IcedTasks tests
└── FsToolkit.ErrorHandling.WebSharper/      # Runs the core tests under WebSharper (proxy, test adapter, runner)

build/                         # Build scripts and configuration
gitbook/                       # Documentation source
.github/workflows/build.yml    # CI/CD pipeline
```

### Target Frameworks
- **.NET Standard 2.0/2.1**: For broad compatibility
- **.NET 9.0, 10.0, and 11.0**: `net11.0` forwards the `Async` and `Task` shims to FSharp.Core 11
- **Fable JavaScript**: Compiles to JavaScript for web usage
- **Fable Python**: Compiles to Python for cross-platform usage (limited test coverage)
- **WebSharper**: A proxy project compiles `FsToolkit.ErrorHandling.Sources` to JavaScript. `WebSharperPending.fs` lists the tests that fail only under WebSharper

## Development Workflow

### Making Changes
1. **Create feature branch**: Work on focused changes
2. **Build frequently**: Run `./build.sh Build` after significant changes
3. **Test early**: Run relevant test commands during development
4. **Format code**: Run `./build.sh FormatCode` before committing
5. **Full validation**: Run `./build.sh CheckFormatCode && ./build.sh RunTests` before PR

### Common Development Tasks

#### Adding New Functionality
1. Add implementation to appropriate project in `src/`
2. Add tests to corresponding test project in `tests/`
3. Run `./build.sh DotnetTest` for quick F# validation
4. Run `./build.sh RunTests` for full cross-platform validation

#### Fixing Issues
1. Reproduce issue with failing test
2. Implement fix in source
3. Ensure tests pass: `./build.sh RunTests`
4. Validate formatting: `./build.sh CheckFormatCode`

#### Documentation Updates
- Update inline documentation in source files
- Update `gitbook/` content for user-facing documentation
- No separate build required for documentation changes

## Environment Dependencies

### Required SDKs and Tools
- **.NET SDK 11.0.100**: Primary development SDK (`global.json`)
- **.NET Runtime 10.0.x**: Required for the `net10.0` tests
- **Node.js 18.0.0+**: Required for Fable JavaScript compilation and the WebSharper tests
- **Python 3.12.0+**: Required for Fable Python compilation

### Package Management
- **NuGet**: .NET package dependencies (automatic via `dotnet restore`)
- **npm**: JavaScript dependencies for Fable builds (via `npm install`)
- **Femto**: Manages Fable package compatibility (via `dotnet femto`)

## Troubleshooting

### Build Issues
- **"SDK not found"**: Ensure .NET SDK 11.0.100 or later is installed and in PATH
- **"Runtime not found"**: Install the .NET 10.0.x runtime for the `net10.0` tests
- **npm errors**: Run `npm install` manually in repository root
- **Fable compilation errors**: Run `./build.sh FemtoValidate` to check package compatibility
- **Security vulnerability errors (NU1902)**: Known issue with System.Security.Cryptography.Xml package in build dependencies
  - **TEMPORARY WORKAROUND**: Add `<NuGetAudit>false</NuGetAudit>` to build/build.fsproj PropertyGroup to bypass vulnerability check
  - This is a build-time dependency issue and does not affect the runtime library security
  - Remove the workaround once the underlying dependency is updated

### Test Failures
- **F# test failures**: Run `./build.sh DotnetTest` to isolate .NET issues
- **JavaScript test failures**: Check Node.js version (requires 18.0.0+), run `./build.sh NpmTest`
- **Python test failures**: Check Python version (requires 3.12.0+), run `./build.sh PythonTest`
- **WebSharper test failures**: Run `./build.sh WebSharperTest`. A test that fails only under WebSharper because of a WebSharper limitation goes in `tests/FsToolkit.ErrorHandling.WebSharper/TestAdapter/WebSharperPending.fs`, with the reason
- **Fable plugin warnings**: Ignore "Could not scan" warnings for Microsoft.TestPlatform assemblies - these are normal

### Format Issues
- **Format check failures**: Run `./build.sh FormatCode` to auto-fix formatting
- **Persistent format issues**: Check `.fantomasignore` for excluded files

### Performance Notes
- **Fable compilation warnings**: Expect warnings about scanning TestPlatform assemblies - these are normal and don't affect functionality
- **Parallel builds**: Build system uses parallel compilation (`/m:2`) to optimize performance
- **Incremental builds**: Subsequent builds are faster due to incremental compilation

## Performance Notes

### Build Timing Expectations
- **Quick validation** (~15s): `./build.sh DotnetTest`
- **Medium validation** (~90s): `./build.sh Build`  
- **Full validation** (~270s): `./build.sh RunTests`
- **Cross-platform CI** (~10-15 minutes): Full matrix builds across OS/versions

### Test Coverage Distribution
- **F# Tests**: ~3406 tests (comprehensive coverage)
- **JavaScript Tests**: ~835 tests (via Fable compilation)
- **Python Tests**: ~2 tests (limited Fable Python support)
- **Total**: 4243+ tests across all platforms

### Optimization Tips
- Use specific targets for faster iteration during development
- Run `./build.sh Build` before `RunTests` to catch compilation issues early
- Use `./build.sh DotnetTest` for rapid F# validation during development
- Fable compilation takes ~10 seconds for project parsing - this is normal
- Python tests have minimal coverage due to Fable Python being in beta status