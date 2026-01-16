# Filebuloso v0.2 Release Notes

Release date: 2026-01-15

## Highlights
- WPF desktop app for organizing files by category and removing duplicates.
- Hash-first duplicate detection with pattern handling and collision-safe renames.
- Optional subfolder duplicate cleanup (keep oldest by date).

## Core Features
- Directory picker, progress feedback, and summary report.
- Category configuration with validation and default set.
- Duplicate detection (root-only), pattern selection, and rename rules.
- Subfolder duplicate cleanup by hash (keep oldest).
- Categorization with lazy collision handling and timestamped version preservation.
- Logging to user documents folder with rotation/cleanup.

## UI Updates
- Main window quick-start guidance.
- Settings page About section with logo, version, credits, and license viewer.

## Distribution
- Single-file, self-contained Windows x64 executable.

## Known Limitations
- Large EXE size due to self-contained runtime and WPF (no trimming).
- Folder picker uses COM-based dialog; no WinForms dependency.

## Files
- EXE: `Filebuloso\bin\Release\net8.0-windows\win-x64\publish\Filebuloso.exe`
- Zip: `Filebuloso_MVP_v0.2.zip`
