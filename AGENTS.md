# Repository Guidelines

## Project Structure & Module Organization

This repository contains the initial WPF solution scaffold plus the product specs. The implementation should follow the layout in `Filebuloso_FRS.md`:

- `Filebuloso.sln` (solution root)
- `Filebuloso/` (WPF app project)
- `Filebuloso.Tests/` (xUnit test project)
- `Filebuloso/Views/`, `ViewModels/`, `Models/`, `Services/`, `Helpers/`
- `Filebuloso/Resources/` and `Filebuloso/Config/DefaultCategories.json`

Keep source under the app project folder and avoid mixing docs with code. If you add assets, keep them under `Resources/` and name them descriptively (e.g., `Icons/app.ico`).

## Build, Test, and Development Commands

Core commands for local work:

- `dotnet build` (compile solution)
- `dotnet run --project Filebuloso/Filebuloso.csproj` (run the WPF app)
- `dotnet test` (run xUnit tests)
- `./build.ps1` and `./test.ps1` (preferred wrappers that clear `MSBUILD_EXE_PATH` before running build/test)

Release publish (single-file exe):

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=true
```

## Coding Style & Naming Conventions

- Language: C# 10+ on .NET 8, WPF.
- Indentation: 4 spaces; keep XAML consistent with C#.
- Naming: PascalCase for classes and public members, camelCase for locals, and `IInterface` for interfaces.
- Category names and config keys should match spec examples (e.g., `documents_word`, `defaultDirectory`).

## Testing Guidelines

Testing uses xUnit in `Filebuloso.Tests/` and targets `net8.0-windows` to match the WPF app. Use clear names like `DuplicateFinderServiceTests` with methods such as `DetectDuplicates_UsesRootOnly()`.

## Commit & Pull Request Guidelines

No Git history exists in this repo. Use conventional commits if you introduce Git (e.g., `feat:`, `fix:`, `docs:`) and keep PRs small with:

- a clear description of behavior changes
- linked requirements (FR-###) when applicable
- screenshots for UI changes

## Security & Configuration Tips

Configuration and logs are user-scoped:

- Config: `%AppData%\Filebuloso\config.json`
- Logs: `%UserProfile%\Documents\Filebuloso\Logs\`
- Error logs: `%UserProfile%\Documents\Filebuloso\Logs\Errors\`

Do not write to the executable directory; keep all user data in the locations above.

## Troubleshooting

- If `dotnet test` fails with MSBuild/hostpolicy errors, clear the override and retry: `powershell -Command "$env:MSBUILD_EXE_PATH=$null; dotnet test"`.
