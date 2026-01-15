# Filebuloso - Functional Requirements Specification (FRS)
*Organize. Deduplicate. Simplify.*

### Document Information
- **Product Name:** Filebuloso
- **Document Version:** 1.0
- **Date:** January 14, 2026
- **Author:** Moses Ling
- **Purpose:** Specification for Windows GUI application to organize and deduplicate files

---

## 1. Executive Summary

### 1.1 Purpose
This document defines the functional requirements for **Filebuloso**, a Windows desktop application that provides a user-friendly graphical interface for organizing files by type and removing duplicates. The application is designed for users who are not computer-savvy and need a simple, intuitive way to clean up their download folders.

### 1.2 Scope
Filebuloso will:
- Provide a GUI for selecting directories to organize
- Allow users to configure file type categories
- Remove duplicate files using hash-first detection (keeping originals)
- Organize files into categorized subdirectories with lazy collision detection
- Save and load user configurations
- Preserve multiple versions of files with different content

---

## 2. System Overview

### 2.1 Target Users
- Non-technical computer users
- Users who need to organize large numbers of downloaded files
- Users who accumulate duplicate files with naming patterns like "file (1).pdf", "file (2).pdf"

### 2.2 Operating Environment
- **Platform:** Windows 10 (version 1607+) / Windows 11
- **Framework:** .NET 6.0 or .NET 8.0 (self-contained, no installation required)
- **Architecture:** x64 (64-bit)
- **Distribution:** Single-file executable (.exe) with embedded runtime

---

## 3. Functional Requirements

### 3.1 Main Window

#### 3.1.1 Directory Selection
- **FR-001:** The application SHALL display a text field showing the currently selected directory path
- **FR-002:** The application SHALL provide a "Browse" button to open a directory picker dialog
- **FR-003:** The default directory SHALL be the user's Windows Downloads folder (%USERPROFILE%\Downloads) on first launch
- **FR-004:** The application SHALL remember the last selected directory between sessions (persist on change and on exit)
- **FR-005:** The application SHALL validate that the selected directory exists before proceeding

#### 3.1.2 Action Buttons
- **FR-006:** The application SHALL provide a "Start Organization" button to execute the file organization process
- **FR-007:** The "Start Organization" button SHALL be disabled if no valid directory is selected
- **FR-008:** The application SHALL provide a "Configure Categories" button to open the category editor
- **FR-009:** The application SHALL provide a "Settings" or "Preferences" button to open the settings dialog
- **FR-010:** The application SHALL provide an "Exit" button to close the application

#### 3.1.3 Progress Display
- **FR-011:** The application SHALL display a progress bar during the organization process
- **FR-012:** The application SHALL display status messages during execution (e.g., "Scanning files...", "Removing duplicates...", "Organizing files...")
- **FR-013:** The application SHALL display a summary of results after completion:
  - Number of duplicate files removed
  - Number of files organized
  - Number of categories created
  - Collision handling statistics
- **FR-014:** The application SHALL provide a scrollable log area showing detailed operations

---

### 3.3 Settings/Preferences Window

#### 3.7.1 General Settings
- **FR-015:** The application SHALL provide a Settings/Preferences window accessible from the main window
- **FR-016:** The Settings window SHALL include a section for "General" settings
- **FR-017:** General settings SHALL include:
  - Default directory path
  - Enable/disable confirmation dialogs
  - Enable/disable dry run mode by default
- **FR-018:** The Settings window SHALL have "Save", "Cancel", and "Reset to Defaults" buttons

**Note:** Collision handling uses the built-in SmartAuto strategy (hash-based detection with automatic version preservation). No user configuration needed - the system intelligently handles all scenarios.

---

### 3.4 Category Configuration Window

#### 3.8.1 Category Display
- **FR-022:** The application SHALL display all file type categories in a list or tree view
- **FR-023:** Each category SHALL show:
  - Category name (e.g., "documents_word", "cad_autocad")
  - Associated file extensions (e.g., ".doc, .docx")
- **FR-016:** The category list SHALL be sortable alphabetically

#### 3.8.2 Category Management
- **FR-017:** The application SHALL provide an "Add Category" button
- **FR-018:** The application SHALL provide an "Edit Category" button (enabled when a category is selected)
- **FR-019:** The application SHALL provide a "Delete Category" button (enabled when a category is selected)
- **FR-020:** The application SHALL provide a "Reset to Defaults" button to restore original categories

#### 3.8.3 Add/Edit Category Dialog
- **FR-021:** The dialog SHALL provide a text field for category name
- **FR-022:** The category name SHALL only allow alphanumeric characters and underscores
- **FR-023:** The dialog SHALL provide a multi-line text area for file extensions
- **FR-024:** File extensions SHALL be entered one per line or comma-separated
- **FR-025:** File extensions SHALL automatically remove leading dots if present
- **FR-026:** The dialog SHALL provide "Save" and "Cancel" buttons
- **FR-027:** The application SHALL validate that category names are unique
- **FR-028:** The application SHALL validate that file extensions are in valid format

#### 3.8.4 Default Categories
The application SHALL include the following default categories:

| Category Name | File Extensions |
|--------------|----------------|
| documents_word | doc, docx |
| documents_text | txt |
| spreadsheets | xlsx, xls, csv |
| presentations | pptx, ppt |
| pdf | pdf |
| cad_autocad | dwg, dxf |
| cad_solidworks | sldprt, sldasm, slddrw |
| cad_3d | step, stp, iges, igs, stl, sat, x_t, x_b, shapr |
| tif | tif, tiff |
| images | png, jpg, jpeg, jfif, gif, bmp, heic, heif, webp, svg, ico |
| webpages | html, htm |
| archives | zip, rar, 7z, tar, gz |
| executables | exe, msi, bat, ps1 |
| disk_images | iso |
| data | json, xml, yaml, yml |
| video | mp4, avi, mov, mkv, wmv, flv |
| audio | mp3, wav, flac, aac, m4a, ogg |
| code | py, js, java, cpp, c, h, cs, php, rb, go, rs |
| notebooks | ipynb |

---

### 3.7 File Organization Logic

#### 3.7.0 Processing Sequence
- **FR-028:** The application SHALL process files in the following mandatory order:
  1. **Pre-scan phase**: Quick file count in root directory (for progress bar and confirmation dialog)
  2. **Duplicate detection and removal**: Hash-first approach on root directory files only
  3. **File categorization**: Move remaining files from root directory into category folders
  4. **Collision handling**: Lazy detection - only hash when filename collision occurs during categorization
  5. **Summary report**: Display results to user
  
- **FR-028.1:** Duplicate detection (Step 2) SHALL ONLY scan the root directory:
  - Hash all files in root directory (single pass)
  - Group files by hash value
  - Process only groups with 2+ files (duplicates detected)
  - Pattern detection for duplicates: (n), _n, -n
  - Selection priority: No pattern > Lowest number > Alphabetical
  - Clean filenames after selection (remove pattern from kept file)
  - **DO NOT scan subdirectories** during this phase
  - Subdirectory files are handled later during lazy collision detection
  
- **FR-028.2:** File categorization (Step 3) SHALL only process:
  - Files remaining in the root directory after duplicate removal
  - Files already in category subdirectories SHALL NOT be re-categorized
  - This prevents moving files that are already properly organized

- **FR-028.3:** Lazy collision detection (Step 4) handles subdirectory files:
  - When moving a file from root to category folder
  - If destination file exists (collision), hash ONLY those 2 files
  - This is when subdirectory files are examined
  - Minimal hashing: only on actual collisions

- **FR-028.4:** The processing order is critical because:
  - Root-only duplicate removal is FAST (no subdirectory scanning overhead)
  - Lazy collision detection is EFFICIENT (hash only on actual collisions)
  - Categorizing AFTER duplicate removal prevents unnecessary collision checks
  - This approach is optimal for both first runs and subsequent runs

#### 3.7.0.1 Processing Order Example
To illustrate the optimized hash-first approach with root-only scanning:

**Initial State (Download folder after 2 months):**
```
Download/
├── report.pdf (hash: abc123)
├── report(1).pdf (hash: abc123) ← Duplicate
├── report(2).pdf (hash: abc123) ← Duplicate
├── photo.jpg (hash: def456)
├── photo(1).jpg (hash: def456) ← Duplicate
└── pdf/
    └── report.pdf (hash: abc123) ← From previous cleanup
```

**✅ CORRECT Approach (Hash-First, Root-Only):**
```
STEP 1: Pre-scan (FAST - just count files)
- Count files in root: 5 files
- Time: <1 millisecond
- No pattern analysis, no hashing - just count!

STEP 2: Duplicate Detection (Root Only - DO NOT scan subdirectories)
Phase 1: Hash all files in root
- report.pdf → abc123
- report(1).pdf → abc123
- report(2).pdf → abc123
- photo.jpg → def456
- photo(1).jpg → def456

Phase 2: Group by hash
- Group abc123: [report.pdf, report(1).pdf, report(2).pdf]
- Group def456: [photo.jpg, photo(1).jpg]

Phase 3: Process groups with 2+ files
Group abc123 (3 files):
  - Pattern check: report.pdf (no pattern), report(1).pdf, report(2).pdf
  - Keep: report.pdf (priority: no pattern)
  - Delete: report(1).pdf, report(2).pdf
  
Group def456 (2 files):
  - Pattern check: photo.jpg (no pattern), photo(1).jpg
  - Keep: photo.jpg (priority: no pattern)
  - Delete: photo(1).jpg

Result after duplicate removal:
Download/
├── report.pdf (ready to categorize)
├── photo.jpg (ready to categorize)
└── pdf/
    └── report.pdf (NOT touched during duplicate detection)

STEP 3: Categorization with Lazy Collision Detection

Categorizing photo.jpg:
- Destination: images/photo.jpg
- Check collision: NO (images/ folder empty or no photo.jpg)
- Action: Move directly (no hashing)
- Result: images/photo.jpg ✅

Categorizing report.pdf:
- Destination: pdf/report.pdf
- Check collision: YES (pdf/report.pdf exists from previous run)
- Lazy collision detection triggered!
- Hash source (root): abc123
- Hash destination (pdf/): abc123
- Compare: abc123 == abc123 (IDENTICAL - it's a duplicate!)
- Action: Delete source file (report.pdf from root)
- Result: pdf/report.pdf (original kept) ✅

Final State:
Download/
├── images/
│   └── photo.jpg
└── pdf/
    └── report.pdf (only one copy remains)

Summary:
- Pre-scan: Just counted 5 files (instant)
- Hashed in duplicate detection: 5 files (root only)
- Deleted as duplicates: 2 files (report(1), photo(1))
- Hashed during collision: 2 files (report.pdf source + destination)
- Total hashes: 7 files
- vs. scanning subdirectories upfront: would hash 1000+ files
- Performance: 140x faster! ✅
```

**Key Insight:**
The old duplicate in pdf/report.pdf is discovered DURING categorization via lazy collision detection, not during duplicate removal. This is optimal because:
- We don't waste time hashing 1000+ organized files upfront
- We only hash when there's an actual collision (rare)
- Most files move without any collision checking

#### 3.7.1 Duplicate Detection (Hash-First Approach)

**Overview:**
The application uses a hash-first approach for optimal performance. All files in the root directory are hashed once, grouped by hash value, and duplicates are removed based on filename patterns.

- **FR-029:** The application SHALL use hash-first duplicate detection for optimal performance:
  - Calculate MD5 hash for all files in root directory (single pass)
  - Group files by hash value
  - Process only groups with multiple files (duplicates)
  - This minimizes unnecessary filename parsing

- **FR-030:** Hash calculation scope:
  - ONLY files in root directory are hashed during duplicate detection phase
  - Hash calculation includes ONLY file content (not filename, path, or metadata)
  - Files in category subdirectories are NOT hashed during duplicate detection
  - Subdirectory files are only hashed later during lazy collision detection (if collision occurs)
  
- **FR-031:** Grouping and duplicate identification:
  - Group all files by their MD5 hash value
  - Hash groups with 1 file: No duplicates, skip processing
  - Hash groups with 2+ files: Duplicates detected, proceed to pattern analysis

- **FR-032:** Pattern detection (only for hash groups with duplicates):
  - Detect duplicate naming patterns in filenames:
    - `filename(n).ext` - e.g., report(1).pdf, report(2).pdf
    - `filename_n.ext` - e.g., report_1.pdf, report_2.pdf
    - `filename-n.ext` - e.g., report-1.pdf, report-2.pdf
  - Where n is a positive integer (1, 2, 3, ...)
  - Patterns are case-insensitive
  - Do NOT detect: `filenamen.ext` (no separator before number)

- **FR-033:** Duplicate selection priority (when multiple files have same hash):
  - **Priority 1:** File WITHOUT any pattern (n), _n, or -n
  - **Priority 2:** If all files have patterns: keep file with HIGHEST number
  - **Priority 3:** If no patterns detected: keep first file alphabetically
  - Delete all other files in the hash group

- **FR-034:** Pattern removal and collision-safe renaming:
  - If kept file has a pattern, remove the pattern: `report(3).pdf` → `report.pdf`
  - If `report.pdf` already exists with a DIFFERENT hash, rename kept file as `report_YYYYMMDD_HHMMSS.pdf`
  - This prevents collisions and preserves distinct versions

**Examples:**

**Example 1: Simple duplicates**
```
Files with same hash (aaa):
- report.pdf (no pattern)
- report(1).pdf (pattern: (1))
- report(2).pdf (pattern: (2))

Action:
→ Keep: report.pdf (Priority 1: no pattern)
→ Delete: report(1).pdf, report(2).pdf
```

**Example 2: All files have patterns**
```
Files with same hash (bbb):
- report(2).pdf (pattern: (2))
- report(3).pdf (pattern: (3))
- report_1.pdf (pattern: _1)

Action:
→ Keep: report_1.pdf (Priority 2: lowest number = 1)
→ Remove pattern: report_1.pdf → report.pdf
→ Delete: report(2).pdf, report(3).pdf
```

**Example 3: No patterns detected**
```
Files with same hash (ccc):
- report-final.pdf
- report-copy.pdf
- report-backup.pdf

Action:
→ Keep: report-backup.pdf (Priority 3: first alphabetically)
→ Delete: report-copy.pdf, report-final.pdf
```

**Example 4: No duplicates**
```
Files with unique hashes:
- report.pdf (hash: aaa) - only file with this hash
- budget.xlsx (hash: bbb) - only file with this hash

Action:
→ Keep both (no duplicates)
→ No pattern processing needed
```

#### 3.7.2 File Categorization

- **FR-036:** The application SHALL categorize files based on their file extension
- **FR-037:** The application SHALL create subdirectories for each category in the target directory
- **FR-038:** The application SHALL move files into their respective category subdirectories
- **FR-039:** File categorization SHALL only process files remaining in the root directory:
  - After duplicate removal phase completes
  - Files already in category subdirectories are not re-categorized
  - This prevents re-organizing files from previous runs
- **FR-040:** Files with extensions NOT in any defined category SHALL remain in the root directory (uncategorized)
- **FR-041:** Files with NO extension SHALL be moved to an "unclassified" directory
- **FR-042:** The application SHALL report the count of uncategorized files (those left in root) so users can:
  - Add these extensions to existing categories
  - Create new categories for these file types
  - Run the organization again after configuration changes

#### 3.7.2.1 Lazy Collision Detection During Categorization

**Overview:**
Instead of hashing all files in subdirectories upfront, the application only hashes files when a name collision is detected during the move operation. This is significantly faster for subsequent runs.

- **FR-043:** The application SHALL use lazy collision detection:
  - Hash files ONLY when a filename collision is detected
  - Do NOT hash files in subdirectories proactively
  - This minimizes unnecessary hash calculations

- **FR-044:** Collision detection process:
  1. Attempt to move file from root to category folder
  2. Check if destination file already exists
  3. **If NO collision:** Move file immediately (no hashing needed)
  4. **If collision detected:** Proceed to hash comparison

- **FR-045:** Hash comparison on collision (SmartAuto strategy):
  - Calculate hash of source file (the one being moved)
  - Calculate hash of destination file (existing file in category folder)
  - Compare the two hashes:
    
    **CASE A: Hashes are IDENTICAL (true duplicate)**
    - Delete source file (don't move it)
    - Keep existing destination file
    - Log: "Duplicate removed: [filename] already exists in [category]"
    - Update counter: duplicates_removed++
    
    **CASE B: Hashes are DIFFERENT (different versions)**
    - Source file is a different version, must be preserved
    - Add timestamp to source filename: `filename_YYYYMMDD_HHMMSS.ext`
    - Use source file's LastModifiedDate for timestamp
    - Move source with new timestamped name to category folder
    - Log: "Version preserved: [original] → [timestamped]"
    - Update counter: versions_preserved++

- **FR-046:** Timestamp format for version preservation:
  - Format: `basename_YYYYMMDD_HHMMSS.extension`
  - Example: `report.pdf` → `report_20260114_143022.pdf`
  - Use file's LastModifiedDate (not current time)
  - If LastModifiedDate is unavailable, use CreationDate

- **FR-047:** Timestamp collision handling (rare edge case):
  - If timestamped filename already exists (same LastModifiedDate)
  - Append counter: `basename_YYYYMMDD_HHMMSS_1.ext`
  - Increment counter until unique filename found

- **FR-048:** Performance optimization:
  - Only 2 files are hashed per collision (source + destination)
  - Typical scenario: 50 new files, 2 collisions = 4 hashes total
  - vs. old approach: hashing 1000+ files in subdirectories
  - Speed improvement: 250x faster on subsequent runs

**Example Scenarios:**

**Scenario 1: No collision (most common)**
```
Moving: report.pdf → pdf/report.pdf
Check: pdf/report.pdf exists? NO
Action: Move immediately (0 hashes calculated) ✅
```

**Scenario 2: Collision - same content (duplicate)**
```
Moving: report.pdf (hash: aaa) → pdf/report.pdf
Check: pdf/report.pdf exists? YES
Hash source: aaa
Hash destination: aaa
Comparison: aaa == aaa (IDENTICAL)
Action: Delete source, keep destination
Log: "Duplicate removed: report.pdf already in pdf/"
Hashes calculated: 2
```

**Scenario 3: Collision - different content (version)**
```
Moving: budget.xlsx (hash: bbb, modified: 2026-01-14) → spreadsheets/budget.xlsx
Check: spreadsheets/budget.xlsx exists? YES
Hash source: bbb
Hash destination: ccc
Comparison: bbb != ccc (DIFFERENT)
Action: 
  - Rename source: budget_20260114_143022.xlsx
  - Move to: spreadsheets/budget_20260114_143022.xlsx
Log: "Version preserved: budget.xlsx → budget_20260114_143022.xlsx"
Hashes calculated: 2
Result: Both versions preserved in spreadsheets/
```

**Scenario 4: Multiple new files, few collisions**
```
50 new files in root:
- 48 files: no collision → move immediately (0 hashes)
- 2 files: collision detected → hash comparison (4 hashes total)

Total hashes: 4 (vs 1000+ if scanning all subdirectories)
Performance: Extremely fast ✅
```

#### 3.7.3 File Operations Safety
- **FR-049:** The application SHALL provide error handling for locked or in-use files:
  - Display user-friendly error message
  - Skip the locked file and continue with others
  - Log the error with filename and reason
  - Report locked files in summary
- **FR-050:** The application SHALL skip directories and only process files
- **FR-051:** The application SHALL verify sufficient disk space before operations:
  - Estimate space needed for moves (file sizes + buffer)
  - Warn user if insufficient space available
  - Optionally abort operation to prevent disk full errors

#### 3.7.4 Confirmation and Safety
- **FR-053:** The application SHALL perform a quick pre-scan of the root directory before starting organization
- **FR-054:** The pre-scan SHALL count:
  - Total number of files in root directory (for progress bar percentage)
  - **Note:** No pattern analysis, no hash calculation - just a simple file count
  - All duplicate detection and analysis happens later in FR-028.1
- **FR-055:** The application SHALL display a confirmation dialog before starting the organization process
- **FR-056:** The confirmation dialog SHALL show:
  - Target directory path
  - Number of files to be processed in root directory (from pre-scan)
  - Processing strategy: "SmartAuto (hash-first root scanning + lazy collision detection)"
  - Warning that duplicates will be permanently deleted
  - Estimated time to completion (optional, based on file count)
- **FR-057:** The application SHALL provide a "Dry Run" option that shows what would happen without making changes
- **FR-058:** The dry run SHALL perform actual duplicate detection and categorization analysis without making changes:
  - Run full duplicate detection (FR-028.1)
  - Simulate file moves
  - Detect potential collisions
  - Generate detailed report showing what would happen
- **FR-059:** The application SHALL allow users to cancel the operation at any time during processing

---

### 3.8 Configuration Management

#### 3.8.1 Configuration Architecture
- **FR-58:** The application SHALL use an embedded default configuration as a resource within the single executable
- **FR-58:** User-specific configuration SHALL be stored separately from the application executable
- **FR-58:** The configuration file format SHALL be JSON for human readability and easy editing

#### 3.8.2 Configuration File Locations
- **FR-58:** User configuration SHALL be stored at: `%AppData%\Filebuloso\config.json`
- **FR-58:** Application logs SHALL be stored at: `%UserProfile%\Documents\Filebuloso\Logs\`
- **FR-58:** Error logs SHALL be stored at: `%UserProfile%\Documents\Filebuloso\Logs\Errors\`
- **FR-58:** These directories SHALL be created automatically on first run if they do not exist

#### 3.8.3 First Run Behavior
- **FR-58:** On application startup, the application SHALL check if `%AppData%\Filebuloso\config.json` exists
- **FR-58:** If configuration file does NOT exist (first run):
  - Create the `%AppData%\Filebuloso\` directory
  - Extract the embedded default configuration
  - Write default configuration to `%AppData%\Filebuloso\config.json`
  - Create log directories: `%UserProfile%\Documents\Filebuloso\Logs\`
  - Create error log directory: `%UserProfile%\Documents\Filebuloso\Logs\Errors\`
  - Initialize default settings with:
    - Default directory: `%USERPROFILE%\Downloads`
    - Collision handling: "SmartMerge"
    - All default categories as defined in section 3.4.4
    - Default window size: 800x600
- **FR-58:** If configuration file exists:
  - Load user's configuration from `%AppData%\Filebuloso\config.json`
  - Validate configuration structure
  - If validation fails: create backup of corrupted config and regenerate from defaults
  - Apply user's custom categories and settings

#### 3.8.4 Configuration File Structure
- **FR-58:** The configuration file SHALL contain the following structure:
```json
{
  "version": "1.0",
  "defaultDirectory": "C:\\Users\\Username\\Downloads",
  "confirmBeforeProcessing": true,
  "categories": [
    {
      "name": "documents_word",
      "extensions": ["doc", "docx"]
    },
    {
      "name": "pdf",
      "extensions": ["pdf"]
    }
    // ... additional categories
  ],
  "window": {
    "width": 800,
    "height": 600,
    "x": 100,
    "y": 100
  },
  "logging": {
    "keepDays": 30,
    "maxLogFiles": 50
  }
}
```

**Note:** No collision handling configuration needed. The application uses the built-in SmartAuto strategy which intelligently handles all duplicate and version scenarios automatically.

#### 3.8.5 Settings Persistence
- **FR-58:** The application SHALL save user settings to the configuration file whenever:
  - User adds, edits, or deletes a category
  - User closes the application (save window size and position)
  - User changes the default directory
- **FR-58:** Settings changes SHALL be written immediately to `%AppData%\Filebuloso\config.json` to prevent data loss
- **FR-58:** The application SHALL create a backup of the previous configuration before overwriting
  - Keep last 3 configuration versions as: `config.json.backup1`, `config.json.backup2`, `config.json.backup3`
  - Rotate backups (backup1 becomes backup2, backup2 becomes backup3, etc.)

#### 3.8.6 Import/Export
- **FR-58:** The application SHALL provide an "Export Configuration" option to save settings to a user-chosen file location
- **FR-58:** The application SHALL provide an "Import Configuration" option to load settings from a file
- **FR-58:** Exported configuration files SHALL be in JSON format for easy sharing with colleagues
- **FR-58:** When importing, the application SHALL validate the configuration file structure before applying
- **FR-58:** If import validation fails, the application SHALL display an error message and keep current settings

---

### 3.9 Logging and Reporting

#### 3.9.1 Log File Management
- **FR-58:** The application SHALL create log directories on first run:
  - Operation logs: `%UserProfile%\Documents\Filebuloso\Logs\`
  - Error logs: `%UserProfile%\Documents\Filebuloso\Logs\Errors\`
- **FR-58:** The application SHALL verify log directory existence before each operation
- **FR-58:** If log directories do not exist, create them automatically (no error to user)

#### 3.9.2 Operation Log
- **FR-58:** The application SHALL create a new operation log file for each run
- **FR-58:** Operation log filename format: `Filebuloso_YYYYMMDD_HHMMSS.log`
  - Example: `Filebuloso_20260114_143022.log`
- **FR-58:** The log SHALL include:
  - Timestamp for each operation (ISO 8601 format: YYYY-MM-DD HH:MM:SS)
  - Operation type: [SCAN], [DUPLICATE], [MOVE], [SKIP], [RENAME], [DELETE], [ERROR]
  - Source file path (full path)
  - Destination path (for moves)
  - Collision resolution method used (if applicable)
  - File hash (for duplicate detection operations)
  - Success/failure status
  - Error message (if operation failed)
- **FR-58:** Log entries SHALL be written in real-time (buffered and flushed regularly)
- **FR-58:** Example log format:
```
2026-01-14 14:30:22 [SCAN] Started scanning directory: C:\Users\Moses\Downloads
2026-01-14 14:30:23 [SCAN] Found 1,247 files to process
2026-01-14 14:30:24 [DUPLICATE] Checking: document.pdf (hash: a3f5b8...)
2026-01-14 14:30:24 [DUPLICATE] Found duplicate: document (1).pdf (hash: a3f5b8...)
2026-01-14 14:30:24 [DELETE] Removed: document (1).pdf
2026-01-14 14:30:25 [MOVE] Moved: report.xlsx -> spreadsheets\report.xlsx
2026-01-14 14:30:26 [RENAME] Collision detected: budget.xlsx exists in spreadsheets\
2026-01-14 14:30:26 [RENAME] Using SmartMerge: Different content detected
2026-01-14 14:30:26 [MOVE] Moved: budget.xlsx -> spreadsheets\budget_20260114_143026.xlsx
2026-01-14 14:30:27 [SKIP] Skipped: photo.jpg (already exists in images\)
2026-01-14 14:30:28 [ERROR] Failed to move: locked_file.docx (File is in use)
```

#### 3.9.3 Error Log
- **FR-58:** The application SHALL maintain a separate error log for troubleshooting
- **FR-58:** Error log filename format: `Filebuloso_Errors_YYYYMMDD.log`
  - One error log per day (append if multiple runs same day)
- **FR-58:** Error log SHALL include:
  - Timestamp
  - Error severity: [WARNING], [ERROR], [CRITICAL]
  - Error message
  - Stack trace (for exceptions)
  - System information (on critical errors: OS version, .NET version, available disk space)
- **FR-58:** Errors SHALL be logged to both operation log and error log simultaneously

#### 3.9.4 Log Rotation and Cleanup
- **FR-58:** The application SHALL implement automatic log rotation
- **FR-58:** Default retention policy:
  - Keep operation logs for last 30 days OR last 50 log files (whichever is larger)
  - Keep error logs for last 90 days
- **FR-58:** Log retention settings SHALL be configurable in config.json:
```json
"logging": {
  "keepDays": 30,
  "maxLogFiles": 50,
  "errorLogDays": 90
}
```
- **FR-58:** The application SHALL check and cleanup old logs on startup (async, non-blocking)
- **FR-58:** The application SHALL provide a "Clear Old Logs" button in Settings window
- **FR-58:** When clearing logs, the application SHALL:
  - Show confirmation dialog with count of files to be deleted
  - Display total disk space to be freed
  - Delete files older than retention period
  - Show summary of deleted logs

#### 3.9.5 Log Viewing
- **FR-58:** The application SHALL provide a "View Log" button on the main window
- **FR-58:** Clicking "View Log" SHALL open the most recent operation log in the default text editor
- **FR-58:** The application SHALL provide a "View All Logs" option that opens the logs folder in Windows Explorer
- **FR-58:** The application SHALL handle missing log files gracefully (show message: "No logs found")

#### 3.9.6 Summary Report
- **FR-58:** After completion, the application SHALL display a summary report showing:
  - Total files scanned (including subdirectories)
  - Total files processed (moved/organized)
  - Duplicates removed (with count and total space freed):
    - True duplicates (identical content): X files removed
    - Space saved: Y MB
  - Multiple versions detected and preserved:
    - Number of base files with multiple versions
    - Total versions kept with timestamps
    - Example: "report.pdf: 3 versions preserved (Nov 15, Dec 1, Jan 10)"
  - Files organized by category (with counts per category)
  - Collision handling statistics:
    - Files with true duplicate content removed (SmartMerge)
    - Files renamed due to name conflicts
    - Files skipped (left in root directory)
    - Files replaced (if using AlwaysReplace strategy)
  - Uncategorized files remaining in root (with list of unknown extensions)
  - Files with no extension moved to "unclassified" folder
  - Errors encountered (with count and link to error log)
  - Total execution time
  - Disk space freed (from duplicate removal)
- **FR-58:** The summary report SHALL be copyable to clipboard
- **FR-58:** The summary report SHALL have a "Save Report" button to export as text file
- **FR-58:** The summary report SHALL provide hyperlinks to:
  - Operation log file
  - Error log file (if errors occurred)
  - Target directory (open in Explorer)

---

### 3.10 Error Handling

#### 3.10.1 User Error Handling
- **FR-58:** The application SHALL display user-friendly error messages for common issues:
  - Directory not found: "The selected directory does not exist. Please choose a valid directory."
  - Permission denied: "Cannot access directory. Please check folder permissions or run as administrator."
  - Disk full: "Not enough disk space to complete operation. Please free up space and try again."
  - File in use: "Cannot process [filename] because it is currently open in another program."
- **FR-58:** Error messages SHALL suggest corrective actions when possible
- **FR-58:** Error messages SHALL include a "Show Details" button to view technical information
- **FR-58:** Critical errors SHALL be logged to the error log automatically

#### 3.10.2 System Error Handling
- **FR-58:** The application SHALL gracefully handle unexpected errors without crashing
- **FR-58:** When an unhandled exception occurs:
  - Display a friendly error dialog
  - Log full exception details to error log
  - Offer options: "Continue" (skip problematic file), "Abort" (stop operation), "View Log"
- **FR-58:** The application SHALL implement error recovery for non-critical operations:
  - If one file fails to move, continue with remaining files
  - Track failed operations and report in summary
- **FR-58:** For critical errors (cannot create directories, cannot write config):
  - Display detailed error message
  - Save emergency log to desktop if normal log location inaccessible
  - Provide option to reset configuration to defaults

---

## 4. User Interface Requirements

### 4.1 Design Principles
- **UI-001:** The interface SHALL follow Windows design guidelines
- **UI-002:** The interface SHALL be intuitive for non-technical users
- **UI-003:** All buttons SHALL have clear, descriptive labels
- **UI-004:** The interface SHALL use consistent fonts and colors
- **UI-005:** The interface SHALL be accessible (support high contrast mode, readable fonts)

### 4.2 Layout
- **UI-006:** The main window SHALL be resizable with a minimum size of 800x600 pixels
- **UI-007:** The main window SHALL remember its size and position between sessions
- **UI-008:** All text SHALL be readable at standard Windows DPI settings
- **UI-009:** The interface SHALL use icons in addition to text labels for common actions

### 4.3 Responsive Feedback
- **UI-010:** The application SHALL change the cursor to a "busy" indicator during long operations
- **UI-011:** The application SHALL disable action buttons during processing to prevent duplicate operations
- **UI-012:** The application SHALL provide visual feedback when hovering over interactive elements

---

## 5. Performance Requirements

- **PERF-001:** The application SHALL start within 3 seconds on a standard Windows PC
- **PERF-002:** The application SHALL process at least 100 files per second during organization
- **PERF-003:** The duplicate detection SHALL complete within reasonable time for up to 10,000 files
- **PERF-004:** The UI SHALL remain responsive during file operations
- **PERF-005:** Progress updates SHALL occur at least once per second

---

## 6. Installation and Distribution

### 6.1 Distribution Package
- **DIST-001:** The application SHALL be distributed as a single standalone Windows executable (.exe)
- **DIST-002:** The executable SHALL be self-contained with .NET runtime embedded (no installation required)
- **DIST-003:** The application SHALL be published using .NET's single-file deployment
- **DIST-004:** The package SHALL include all necessary dependencies embedded in the single .exe file
- **DIST-005:** The executable size SHALL be optimized through trimming (target: 15-40 MB)
- **DIST-006:** The application SHALL support Windows 10 (version 1607+) and Windows 11

### 6.2 Installation
- **INST-001:** The application SHALL NOT require administrator privileges to run (runs in user context)
- **INST-002:** The application SHALL be fully portable:
  - Can run from any directory (USB drive, network share, local disk)
  - All user data stored in standard Windows locations (%AppData%, %UserProfile%)
  - No registry modifications required
- **INST-003:** First-run setup SHALL be automatic and transparent:
  - On first execution, check if configuration exists
  - If not found, create directory structure automatically:
    - `%AppData%\Filebuloso\` for configuration
    - `%UserProfile%\Documents\Filebuloso\Logs\` for operation logs
    - `%UserProfile%\Documents\Filebuloso\Logs\Errors\` for error logs
  - Extract embedded default configuration and write to AppData
  - No user interaction required for setup
  - Application ready to use immediately
- **INST-004:** The application SHALL create all necessary directories with appropriate permissions
- **INST-005:** If directory creation fails (e.g., restricted permissions):
  - Display error message explaining the issue
  - Suggest running from a location with write access
  - Optionally fall back to using Temp directory for configuration (with warning)
- **INST-006:** The application SHALL be uninstallable by simply:
  - Deleting the .exe file
  - Optionally deleting `%AppData%\Filebuloso\` to remove configuration
  - Optionally deleting `%UserProfile%\Documents\Filebuloso\` to remove logs
- **INST-007:** The application SHALL provide an "Uninstall" option in the Help menu that:
  - Shows locations of all application data
  - Offers to open these folders in Explorer
  - Provides instructions for manual cleanup

---

## 7. Help and Documentation

- **HELP-001:** The application SHALL include a "Help" menu with:
  - User Guide (opens documentation)
  - About dialog (shows version and credits)
- **HELP-002:** The application SHALL include tooltips for all major UI elements
- **HELP-003:** The application SHALL provide context-sensitive help where appropriate

---

## 8. Future Enhancements (Out of Scope for v1.0)

The following features are identified for potential future versions:
- Undo functionality for organization operations
- Schedule automatic organization
- Integration with cloud storage (OneDrive, Google Drive)
- Preview files before organization
- Custom rules engine for advanced categorization
- Multi-language support

---

## 9. Acceptance Criteria

The application will be considered complete when:
1. All functional requirements (FR-001 through FR-111) are implemented and tested
2. The application successfully organizes a test directory of 1000+ files with correct processing order:
   - Hash-first duplicate removal in root directory
   - Lazy collision detection during categorization
   - Multiple versions preserved with timestamps
   - SmartAuto strategy handles all scenarios automatically
3. Non-technical users can complete the organization process without assistance
4. The application handles common error scenarios gracefully
5. The application runs as a single .exe file without requiring .NET installation
6. First-run setup creates all necessary directories and configuration automatically
7. Configuration persists correctly between sessions
8. Logs are created and maintained properly
9. The executable size is within the 15-40 MB target range
10. The processing is performant: hash-first duplicate removal + lazy collision detection
11. Your 2-month scenario works perfectly: duplicates removed before categorization

---

## 10. Technical Specifications

### 10.1 Recommended Technology Stack
- **Language:** C# 10+
- **Framework:** .NET 6.0 or .NET 8.0
- **GUI Framework:** WPF (Windows Presentation Foundation)
- **Deployment:** Single-file executable with self-contained runtime
- **Configuration Format:** JSON
- **Logging:** Built-in .NET logging (Microsoft.Extensions.Logging)

### 10.2 File Structure
```
Filebuloso/
├── Filebuloso.sln           # Solution file
├── Filebuloso/
│   ├── App.xaml                # WPF application definition
│   ├── App.xaml.cs
│   ├── MainWindow.xaml         # Main window XAML
│   ├── MainWindow.xaml.cs      # Main window code-behind
│   ├── Views/
│   │   ├── CategoryEditorWindow.xaml
│   │   ├── CategoryEditorWindow.xaml.cs
│   │   ├── AddEditCategoryDialog.xaml
│   │   └── AddEditCategoryDialog.xaml.cs
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── CategoryEditorViewModel.cs
│   │   └── CategoryViewModel.cs
│   ├── Models/
│   │   ├── FileCategory.cs
│   │   └── OrganizationResult.cs
│   ├── Services/
│   │   ├── FilebulosoService.cs
│   │   ├── DuplicateFinderService.cs
│   │   ├── CategoryManagerService.cs
│   │   └── ConfigurationService.cs
│   ├── Helpers/
│   │   ├── FileOperations.cs
│   │   └── HashCalculator.cs
│   ├── Resources/
│   │   └── Icons/
│   ├── Config/
│   │   └── DefaultCategories.json
│   └── Filebuloso.csproj    # Project file
```

### 10.3 Single-File Deployment Configuration
The application SHALL be published as a single executable file using the following .NET publish settings:
- **PublishSingleFile:** true
- **SelfContained:** true
- **RuntimeIdentifier:** win-x64
- **PublishTrimmed:** true (to reduce file size)
- **IncludeNativeLibrariesForSelfExtract:** true

Expected executable size: 15-40 MB (depending on trimming optimizations)

### 10.4 Build Command
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:PublishTrimmed=true
```

---

## 11. Glossary

- **Category:** A logical grouping of file types (e.g., "documents_word" contains .doc and .docx files)
- **Duplicate:** A file that has identical content (same MD5 hash) as another file, or has a naming pattern indicating it's a copy (e.g., "file (1).pdf")
- **Organization:** The process of moving files from a flat directory structure into categorized subdirectories
- **Dry Run:** A simulation mode that shows what would happen without actually making changes

---

## 12. Approval

This FRS should be reviewed and approved by:
- [ ] Project Owner: Moses Ling
- [ ] Developer: [To be assigned]
- [ ] Test User: Co-worker (representative of target users)

---

**END OF DOCUMENT**

