# FsToolkit.ErrorHandling

FsToolkit.ErrorHandling is an F# utility library to work with the Result type, enabling clear, simple and powerful error handling. The library provides computation expressions, utility functions, and supports multiple target frameworks including .NET Standard 2.0/2.1, .NET 9.0, and compiles to JavaScript and Python via Fable.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build the Repository
- **Install .NET SDK**: Download and install .NET 9.0.100 SDK (required for project compilation)
  - Also install .NET 8.0.x runtime (required for build tools)
  - Use `dotnet --version` to verify installation
  - **CRITICAL**: Ensure both SDKs are in PATH before running any build commands
- **Install Node.js**: Install Node.js v18.0.0 or higher (required for Fable JavaScript builds)
  - Current environment has v20.19.5 which works perfectly
  - Use `node --version` to verify installation
- **Install Python**: Install Python 3.10.0 or higher (required for Fable Python builds)
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
- `./build.sh RunTests` -- runs ALL tests (F#, JavaScript, Python). Takes ~4.5 minutes. NEVER CANCEL. Set timeout to 600+ seconds.
- `./build.sh FormatCode` -- formats F# code using Fantomas. Takes ~5 seconds. NEVER CANCEL.
- `./build.sh CheckFormatCode` -- validates code formatting. Takes ~5 seconds. NEVER CANCEL.
- `./build.sh Clean` -- cleans bin/obj folders. Takes <1 second.

### Default Build Target
- `./build.sh` (no arguments) -- runs `DotnetPack` target which builds and packages all projects. Takes ~5-7 minutes. NEVER CANCEL. Set timeout to 600+ seconds.

## Validation Requirements

### Pre-commit Validation
Always run these commands before committing changes:
- `./build.sh CheckFormatCode` -- ensures code follows formatting standards
- `./build.sh RunTests` -- runs complete test suite across all platforms (F#, JavaScript, Python)

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
- Any tests fail in F#, JavaScript, or Python targets
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
└── FsToolkit.ErrorHandling.IcedTasks.Tests/ # IcedTasks tests

build/                         # Build scripts and configuration
gitbook/                       # Documentation source
.github/workflows/build.yml    # CI/CD pipeline
```

### Target Frameworks
- **.NET Standard 2.0/2.1**: For broad compatibility
- **.NET 9.0**: Latest .NET support
- **Fable JavaScript**: Compiles to JavaScript for web usage
- **Fable Python**: Compiles to Python for cross-platform usage (limited test coverage)

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
- **.NET SDK 9.0.100**: Primary development SDK
- **.NET Runtime 8.0.x**: Required for build tools
- **Node.js 18.0.0+**: Required for Fable JavaScript compilation
- **Python 3.10.0+**: Required for Fable Python compilation

### Package Management
- **NuGet**: .NET package dependencies (automatic via `dotnet restore`)
- **npm**: JavaScript dependencies for Fable builds (via `npm install`)
- **Femto**: Manages Fable package compatibility (via `dotnet femto`)

## Troubleshooting

### Build Issues
- **"SDK not found"**: Ensure .NET 9.0.100 SDK is installed and in PATH
- **"Runtime not found"**: Install .NET 8.0.x runtime for build tools
- **npm errors**: Run `npm install` manually in repository root
- **Fable compilation errors**: Run `./build.sh FemtoValidate` to check package compatibility
- **Security vulnerability errors (NU1902)**: Known issue with System.Security.Cryptography.Xml package in build dependencies
  - **TEMPORARY WORKAROUND**: Add `<NuGetAudit>false</NuGetAudit>` to build/build.fsproj PropertyGroup to bypass vulnerability check
  - This is a build-time dependency issue and does not affect the runtime library security
  - Remove the workaround once the underlying dependency is updated

### Test Failures
- **F# test failures**: Run `./build.sh DotnetTest` to isolate .NET issues
- **JavaScript test failures**: Check Node.js version (requires 18.0.0+), run `./build.sh NpmTest`
- **Python test failures**: Check Python version (requires 3.10.0+), run `./build.sh PythonTest`
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