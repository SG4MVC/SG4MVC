# SG4MVC Copilot Instructions

## What This Project Is

SG4MVC is a **Roslyn incremental source generator** for ASP.NET Core MVC/Razor Pages. It analyzes a project at compile-time and generates strongly-typed static helper classes that replace magic strings in route and view references:

```csharp
// Without SG4MVC
Html.ActionLink("Details", "Dinners", new { id = 123 })

// With SG4MVC
Html.ActionLink(MVC.Dinners.Details(123))
```

There are two NuGet packages:
- **`Sg4Mvc`** (`src/Sg4Mvc/`) — runtime library (tag helpers, action result wrappers, model unbinders)
- **`Sg4Mvc.Generator`** (`src/Sg4Mvc.Generator/`) — the source generator itself (targets `netstandard2.0`, shipped as an analyzer)

Activation in a consuming project requires `[assembly: GenerateSg4Mvc]`.

## Build and Test

```sh
dotnet build
dotnet test

# Single test by fully qualified name
dotnet test --filter "Sg4Mvc.Test.ControllerDefinitionTests.FullyQualifiedName_Default"

# Single test class
dotnet test --filter "ClassName=ClassBuilderTests"

# Pack NuGet packages
dotnet pack --configuration Release
```

## Architecture

### Source Generation Pipeline (`Sg4Mvc.Generator`)

```
Compilation
  → ControllerTargetFilter / PageTargetFilter / AdditionalTextsTargetFilter
  → *DefinitionTransform   (build ControllerDefinition / PageDefinition models)
  → DataGroupingService    (correlate controllers ↔ views ↔ static files)
  → GeneratorServiceFactory
  → Sg4MvcGeneratorService (generate partial controllers + MVC/MVCPages/Links helpers)
  → CodeGen layer          (ClassBuilder → MethodBuilder → BodyBuilder → SyntaxFactory)
```

The entry point is `Generator` (implements `IIncrementalGenerator`, attributed `[Generator(LanguageNames.CSharp)]`).

### Key Abstractions

| Interface | Role |
|-----------|------|
| `IControllerGeneratorService` | Generates partial controller class + `MVC.*` helpers |
| `IPageGeneratorService` | Generates `MVCPages.*` helpers |
| `IStaticFileGeneratorService` | Generates `Links.*` helpers |
| `IViewLocator` / `IPageViewLocator` | Discovers `.cshtml` files |
| `IControllerRewriterService` | Rewrites action method bodies |
| `IModelUnbinder` / `IModelUnbinderProvider` | Convert models → route values (runtime) |

### Generated Output Shape

For each controller, the generator emits a `<FullyQualifiedName>.generated.cs` file containing:
1. A `partial` class matching the original controller
2. A nested static class under `MVC.<ControllerName>` (or `MVCPages.<PageName>`) with strongly-typed action calls, `Views`, and `Actions` sub-classes

### Settings

`Settings` is a POCO loaded via `AnalyzerConfigOptionsProvider`. Key properties:

| Property | Default | Effect |
|----------|---------|--------|
| `HelpersPrefix` | `"MVC"` | Top-level controller helper class name |
| `PageHelpersPrefix` | `"MVCPages"` | Top-level page helper class name |
| `Sg4MvcNamespace` | `"Sg4Mvc"` | Root namespace for generated files |
| `LinksNamespace` | `"Links"` | Static file helper class name |
| `StaticFilesPath` | `"wwwroot"` | Where to scan for static files |
| `FeatureFolders` | `false` | Enable feature-folder view discovery |

## Key Conventions

### Code Style (enforced via `.editorconfig`)
- CRLF line endings
- Private fields: `_camelCase`
- Constants and statics: `PascalCase`

### CodeGen Layer (`src/Sg4Mvc.Generator/CodeGen/`)
Use the fluent builder classes—never write raw `SyntaxFactory` calls directly in generator services:
```csharp
new ClassBuilder("HomeController")
    .WithModifiers("public", "partial")
    .WithMethod(new MethodBuilder("Index").WithReturnType("IActionResult") ...)
    .Build();
```

### Test Structure
- Unit tests live in `test/Sg4Mvc.Test/` and mirror the source namespace structure
- Integration tests live in `test/AspNetSimple.Test/` and exercise the full generation pipeline against sample apps
- Use **xUnit** (`[Fact]` / `[Theory]` / `[InlineData]`) and **Moq** for mocking

### View Discovery Patterns
- **Default**: `Views/<Controller>/`, `Areas/<Area>/Views/<Controller>/`
- **Feature folders**: `Features/<Feature>/Views/` (opt-in via `Settings.FeatureFolders`)
- Extend by implementing `IViewLocator` / `IPageViewLocator`

### Excluding from Generation
- `[Sg4MvcExclude]` on a controller or action suppresses generation for that target
- `[NonAction]` on a method causes it to be skipped
- Non-public and abstract controllers are ignored automatically

### Incremental Generator Performance
Filters (`*TargetFilter`) and transforms (`*DefinitionTransform`) must be **pure functions** — no side effects, no I/O — so Roslyn can cache and replay them safely.

## Performance Benchmarking

The project includes a BenchmarkDotNet suite in `test/Sg4Mvc.Benchmarks/` for measuring generator performance and detecting regressions.

### Benchmark Classes

| Class | What it measures |
|-------|-----------------|
| `FullPipelineBenchmarks` | End-to-end generator execution via `CSharpGeneratorDriver` at 3 scale profiles (Small/Medium/Large) |
| `CodeGenBenchmarks` | Individual phases: `GeneratePartialController`, `GenerateSg4Controller`, `CodeFileBuilder.Build` |
| `HotMethodBenchmarks` | Known hotspot methods: `SanitiseFieldName`, `InheritsFrom`, `NormalizeWhitespace` |

All benchmarks include `[MemoryDiagnoser]` for allocation tracking (Gen0/Gen1/Gen2 + bytes).

### Quick Commands

```sh
# Quick smoke test (seconds — single iteration, no statistics)
cd test/Sg4Mvc.Benchmarks
dotnet run -c Release -- --filter "*FullPipeline*" --job Dry

# Full statistically rigorous run (minutes — multiple warmups + iterations)
dotnet run -c Release -- --filter "*"

# Run a specific benchmark class
dotnet run -c Release -- --filter "*HotMethod*"
dotnet run -c Release -- --filter "*CodeGen*"

# Export results to JSON for programmatic comparison
dotnet run -c Release -- --filter "*" --exporters json
```

### Comparing Before and After Performance

To validate a performance improvement or detect a regression, follow this workflow:

#### Step 1 — Baseline (before your changes)

```sh
# Check out the baseline branch/commit
git stash   # or commit your work-in-progress
git checkout main

# Run the full benchmark suite
cd test/Sg4Mvc.Benchmarks
dotnet run -c Release -- --filter "*" --exporters json

# Save the results (BenchmarkDotNet writes to BenchmarkDotNet.Artifacts/results/)
cp -r BenchmarkDotNet.Artifacts/results ../../../baseline-results
```

#### Step 2 — Candidate (after your changes)

```sh
# Switch to your feature branch
git checkout my-perf-improvement

# Run the same benchmarks
cd test/Sg4Mvc.Benchmarks
dotnet run -c Release -- --filter "*" --exporters json

# Save these results too
cp -r BenchmarkDotNet.Artifacts/results ../../../candidate-results
```

#### Step 3 — Compare

**Option A — Visual comparison of the Markdown tables**

Each run produces `*-report-github.md` files in `BenchmarkDotNet.Artifacts/results/`. Open them side by side and compare the `Mean`, `Allocated`, and `Error` columns.

**Option B — Use `ResultsComparer` (recommended for CI or detailed analysis)**

```sh
# Install the dotnet global tool (one-time)
dotnet tool install -g ResultsComparer

# Compare baseline vs candidate JSON exports
results-comparer --base baseline-results --diff candidate-results --threshold 5%
```

This reports which benchmarks got faster, slower, or stayed the same, with statistical significance filtering.

#### What to look for

| Column | Meaning | Regression signal |
|--------|---------|-------------------|
| **Mean** | Average execution time | Increase > 5% is suspicious |
| **Error** | Half of the 99.9% confidence interval | If error bands overlap, the difference may not be real |
| **Allocated** | Managed memory per operation | Any increase warrants investigation |
| **Gen0/Gen1/Gen2** | GC collections per 1000 operations | Increases indicate allocation pressure |

#### Tips

- Always run in **Release** mode (`-c Release`) — Debug builds include instrumentation overhead
- Close other applications during benchmarking to reduce noise
- Run on the **same machine** for both baseline and candidate
- For quick iteration during development, use `--job Short` (fewer iterations than default, but still statistical)
- Use `--filter` to narrow to only the benchmarks relevant to your change
- The `--join` flag produces a single combined report when running multiple benchmark classes

### Adding New Benchmarks

When adding a new pipeline phase or optimising an existing method:

1. Add a new `[Benchmark]` method to the appropriate class (or create a new class in `Benchmarks/`)
2. Use `[GlobalSetup]` to prepare inputs — avoid measuring setup cost
3. Return a value from the benchmark method to prevent dead-code elimination
4. If the method needs a synthetic compilation, use `CompilationHelper.Create(ScaleProfile)`
5. For new helper methods, add them to `HotMethodBenchmarks`

### Scale Profiles

The `CompilationHelper` creates synthetic compilations at three scales:

| Profile | Controllers | Pages | Actions/Controller | Views | Static Files |
|---------|-------------|-------|--------------------|-------|--------------|
| Small | 5 | 2 | 6 | 30 | 15 |
| Medium | 20 | 8 | 6 | 120 | 15 |
| Large | 50 | 20 | 6 | 300 | 15 |
