# Filebuloso

Filebuloso is a Windows WPF desktop app that organizes folders by removing duplicates and sorting files into categories. It is designed for non‑technical users who want a simple way to clean up download folders.

## Features
- Hash‑first duplicate detection (root directory only).
- Pattern handling for duplicates like `file(1).ext`, `file_2.ext`, `file-3.ext`.
- Categorization by file extension with lazy collision detection.
- Optional subfolder duplicate cleanup (keeps oldest file by date).
- Configurable categories and settings with auto‑save.
- Summary report and logging.

## Quick Start
1. Launch the app.
2. Choose a folder to organize.
3. Click **Start Organization** and confirm.
4. Review the summary when complete.

## Configuration
User settings are stored at:
- `%AppData%\Filebuloso\config.json`

Logs are stored at:
- `%UserProfile%\Documents\Filebuloso\Logs\`

## Build
```bash
dotnet build
```

## Test
```bash
dotnet test
```

## Release Build
```bash
dotnet publish -c Release
```

## License
MIT. See `LICENSE.txt`.
