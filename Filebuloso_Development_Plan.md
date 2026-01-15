# Filebuloso - Development Plan
*Organize. Deduplicate. Simplify.*

## Document Information
- **Project:** Filebuloso GUI Application
- **Version:** 1.0
- **Date:** January 14, 2026
- **Development Approach:** Phased implementation with testing gates

---

## Overview

This development plan breaks down the Filebuloso application into 6 phases, each building on the previous phase. Each phase has specific deliverables and must pass testing before proceeding to the next phase.

**Key Principle:** Test and verify each phase before moving forward. This ensures a solid foundation and catches issues early.

---

## Phase 1: Core Infrastructure & Configuration
**Duration:** 3-5 days  
**Dependencies:** None

### Objectives
- Set up project structure
- Implement configuration management
- Create first-run setup
- Basic logging infrastructure

### Deliverables

#### 1.1 Project Setup
- Create C# WPF project (.NET 6.0 or .NET 8.0)
- Set up solution structure as defined in FRS Section 10.2
- Configure single-file deployment settings
- Add NuGet dependencies (if any)

#### 1.2 Configuration Management (FR-058 to FR-063)
```
Implementation:
- Default configuration as embedded resource (JSON)
- Configuration file structure (Section 3.8.4)
- ConfigurationService class
  - LoadConfiguration()
  - SaveConfiguration()
  - CreateDefaultConfiguration()
  - ValidateConfiguration()
```

#### 1.3 First-Run Setup (FR-065 to FR-067, INST-003 to INST-005)
```
Implementation:
- Check if %AppData%\Filebuloso\config.json exists
- Create directory structure:
  - %AppData%\Filebuloso\
  - %UserProfile%\Documents\Filebuloso\Logs\
  - %UserProfile%\Documents\Filebuloso\Logs\Errors\
- Extract and write default configuration
- Handle permission errors gracefully
```

#### 1.4 Basic Logging (FR-077 to FR-079)
```
Implementation:
- Logger class with log levels: INFO, WARNING, ERROR
- CreateLogFile() - format: Filebuloso_YYYYMMDD_HHMMSS.log
- WriteLog(level, message)
- Log directory creation and verification
```

### Testing Phase 1

#### Test 1.1: Fresh Installation
```
Test Case: First run on clean system
Steps:
1. Delete all Filebuloso directories (AppData, Documents)
2. Run application
Expected Results:
✓ Application starts without errors
✓ %AppData%\Filebuloso\ created
✓ %UserProfile%\Documents\Filebuloso\Logs\ created
✓ config.json created with default values
✓ Application log file created
```

#### Test 1.2: Configuration Persistence
```
Test Case: Configuration saves and loads correctly
Steps:
1. Run application
2. Modify configuration (change default directory)
3. Close application
4. Reopen application
Expected Results:
✓ Configuration changes persisted
✓ Modified values loaded on startup
```

#### Test 1.3: Permission Handling
```
Test Case: Graceful handling of restricted permissions
Steps:
1. Run application from read-only location
2. Verify error handling
Expected Results:
✓ User-friendly error message displayed
✓ Suggests alternative action
✓ Application doesn't crash
```

#### Test 1.4: Configuration Backup
```
Test Case: Configuration backup rotation (FR-066)
Steps:
1. Modify and save configuration 4 times
2. Check AppData directory
Expected Results:
✓ config.json exists (current)
✓ config.json.backup1 exists
✓ config.json.backup2 exists
✓ config.json.backup3 exists
✓ Oldest backup rotated out
```

**Gate Criteria:** All Phase 1 tests must pass before proceeding to Phase 2.

---

## Phase 2: Basic UI & Category Management
**Duration:** 4-6 days  
**Dependencies:** Phase 1 complete

### Objectives
- Create main window UI
- Implement category configuration window
- Directory selection
- Settings window

### Deliverables

#### 2.1 Main Window UI (Section 3.1)
```
Implementation:
- Main window layout (800x600 default)
- Directory selection controls (FR-001 to FR-005)
  - Text box showing current directory
  - Browse button with FolderBrowserDialog
  - Default: %USERPROFILE%\Downloads (FR-003)
- Action buttons (FR-006 to FR-010)
  - Start Organization (disabled until directory selected)
  - Configure Categories
  - Settings
  - Exit
- Progress display area (FR-011 to FR-014)
  - Progress bar control
  - Status text label
  - Log text box (scrollable)
- Window size/position persistence (FR-063)
```

#### 2.2 Category Configuration Window (Section 3.4)
```
Implementation:
- Category list/grid view (FR-019 to FR-021)
  - Display category name and extensions
  - Sortable columns
- Management buttons (FR-022 to FR-025)
  - Add Category
  - Edit Category (enabled when selected)
  - Delete Category (enabled when selected)
  - Reset to Defaults
- Add/Edit Dialog (FR-026 to FR-028)
  - Category name text field (alphanumeric + underscore validation)
  - Extensions multi-line text area
  - Auto-remove leading dots from extensions
  - Save/Cancel buttons
- Validation (FR-029, FR-030)
  - Unique category names
  - Valid extension format
```

#### 2.3 Settings Window (Section 3.3)
```
Implementation:
- General Settings (FR-015 to FR-017)
  - Default directory path
  - Enable/disable confirmation dialogs
  - Enable/disable dry run mode
- Save/Cancel/Reset buttons (FR-018)
```

#### 2.4 Category Manager Service
```
Implementation:
- LoadCategories() from configuration
- SaveCategories() to configuration
- AddCategory(name, extensions)
- EditCategory(oldName, newName, extensions)
- DeleteCategory(name)
- ResetToDefaults()
- ValidateCategoryName(name)
- ValidateExtensions(extensions)
```

### Testing Phase 2

#### Test 2.1: UI Navigation
```
Test Case: All windows open and navigate correctly
Steps:
1. Open main window
2. Click "Configure Categories" → Category window opens
3. Close category window
4. Click "Settings" → Settings window opens
5. Close settings window
Expected Results:
✓ All windows open without errors
✓ Modal dialogs work correctly
✓ Windows remember size/position
```

#### Test 2.2: Directory Selection
```
Test Case: Browse and select directory
Steps:
1. Click Browse button
2. Select Downloads folder
3. Close and reopen application
Expected Results:
✓ FolderBrowserDialog opens
✓ Selected path displays in text box
✓ Start button becomes enabled
✓ Path persists after restart
```

#### Test 2.3: Category Management - Add
```
Test Case: Add new category
Steps:
1. Open Category Configuration
2. Click "Add Category"
3. Enter name: "test_files"
4. Enter extensions: "test, tst"
5. Click Save
Expected Results:
✓ Dialog validates input
✓ Category appears in list
✓ Configuration file updated
✓ Reload shows new category
```

#### Test 2.4: Category Management - Edit
```
Test Case: Edit existing category
Steps:
1. Select "documents_word" category
2. Click "Edit Category"
3. Add extension: "docm"
4. Click Save
Expected Results:
✓ Changes appear in category list
✓ Configuration file updated
✓ Original category replaced (not duplicated)
```

#### Test 2.5: Category Management - Delete
```
Test Case: Delete category
Steps:
1. Add test category
2. Select test category
3. Click "Delete Category"
4. Confirm deletion
Expected Results:
✓ Category removed from list
✓ Configuration file updated
✓ Category not present after reload
```

#### Test 2.6: Category Management - Reset
```
Test Case: Reset to defaults
Steps:
1. Modify several categories
2. Click "Reset to Defaults"
3. Confirm action
Expected Results:
✓ All categories restored to default set
✓ Custom categories removed
✓ Configuration file updated
```

#### Test 2.7: Validation Tests
```
Test Case: Category name validation (FR-027)
Input: "test category" (contains space)
Expected: ✓ Validation error message

Test Case: Category name validation
Input: "test-category" (contains hyphen)
Expected: ✓ Validation error message

Test Case: Category name validation
Input: "test_category_123" (valid)
Expected: ✓ Accepted

Test Case: Duplicate category name (FR-029)
Input: "pdf" (already exists)
Expected: ✓ Validation error message

Test Case: Extension format (FR-028)
Input: ".txt, .doc" (with dots)
Expected: ✓ Dots auto-removed, saved as "txt, doc"
```

**Gate Criteria:** All Phase 2 tests must pass before proceeding to Phase 3.

---

## Phase 3: File Scanning & Hash-First Duplicate Detection
**Duration:** 5-7 days  
**Dependencies:** Phase 2 complete

### Objectives
- Implement file scanning (root only)
- Hash-first duplicate detection
- Pattern detection and selection
- No UI interaction yet - just core logic

### Deliverables

#### 3.1 File Scanner Service
```
Implementation:
- ScanDirectory(path) → List<FileInfo>
  - Enumerate files in root directory only
  - Skip subdirectories
  - Skip locked/inaccessible files
  - Return file list
- GetFileCount(path) → int
  - Quick count for pre-scan (FR-052, FR-053)
```

#### 3.2 Hash Calculator Service
```
Implementation:
- CalculateMD5Hash(filePath) → string
  - Read file content
  - Calculate MD5 hash
  - Return hash as hex string
  - Handle file access errors
- BatchCalculateHashes(files) → Dictionary<string, string>
  - Hash multiple files
  - Return filepath → hash mapping
```

#### 3.3 Duplicate Detector Service (FR-028.1, FR-029 to FR-034)
```
Implementation:
- DetectDuplicates(directory) → DuplicateDetectionResult
  
  Phase 1: Hash all files in root
  - Call FileScanner.ScanDirectory(directory)
  - Call HashCalculator.BatchCalculateHashes(files)
  - Store: filepath → hash mapping
  
  Phase 2: Group by hash
  - Dictionary<hash, List<filepath>>
  - Identify groups with 2+ files (duplicates)
  
  Phase 3: Process duplicate groups
  - For each group with duplicates:
    - Call PatternDetector.AnalyzeGroup(files)
    - Select file to keep (priority logic)
    - Mark others for deletion
  
  Return:
  - FilesToKeep: List<string>
  - FilesToDelete: List<string>
  - Statistics: total files, duplicates found, etc.
```

#### 3.4 Pattern Detector Service (FR-032)
```
Implementation:
- DetectPattern(filename) → PatternInfo
  - Check for (n) pattern: filename(1).ext
  - Check for _n pattern: filename_1.ext
  - Check for -n pattern: filename-1.ext
  - Return: hasPattern, number, baseFilename
  
- SelectFileToKeep(files) → string
  - Priority 1: File without pattern
  - Priority 2: Lowest number if all have patterns
  - Priority 3: Alphabetical if no patterns detected
  - Return: filepath to keep
  
- RemovePattern(filename) → string (FR-034)
  - Strip (n), _n, -n pattern from filename
  - Return clean filename
```

### Testing Phase 3

#### Test 3.1: File Scanning
```
Test Case: Scan directory with files and subdirectories
Setup:
TestDir/
├── file1.txt
├── file2.pdf
├── subfolder/
│   └── file3.txt (should be ignored)

Steps:
1. Call ScanDirectory(TestDir)
Expected Results:
✓ Returns 2 files (file1.txt, file2.pdf)
✓ Ignores subfolder contents
✓ No errors or exceptions
```

#### Test 3.2: Hash Calculation
```
Test Case: Calculate hash of known file
Setup:
- Create file with content: "Hello World"
- Known MD5: b10a8db164e0754105b7a99be72e3fe5

Steps:
1. Call CalculateMD5Hash(file)
Expected Results:
✓ Returns correct hash: b10a8db164e0754105b7a99be72e3fe5
✓ Consistent results on multiple calls
```

#### Test 3.3: Pattern Detection - (n) pattern
```
Test Case: Detect (n) pattern
Input: "report(1).pdf"
Expected: 
✓ hasPattern = true
✓ number = 1
✓ baseFilename = "report"

Input: "report(10).pdf"
Expected:
✓ hasPattern = true
✓ number = 10
✓ baseFilename = "report"

Input: "report.pdf"
Expected:
✓ hasPattern = false
```

#### Test 3.4: Pattern Detection - _n pattern
```
Test Case: Detect _n pattern
Input: "report_1.pdf"
Expected:
✓ hasPattern = true
✓ number = 1
✓ baseFilename = "report"

Input: "report_25.pdf"
Expected:
✓ hasPattern = true
✓ number = 25
```

#### Test 3.5: Pattern Detection - -n pattern
```
Test Case: Detect -n pattern
Input: "report-1.pdf"
Expected:
✓ hasPattern = true
✓ number = 1

Input: "report-copy.pdf" (not a pattern)
Expected:
✓ hasPattern = false
```

#### Test 3.6: Selection Priority - No Pattern Wins
```
Test Case: Priority 1 - No pattern
Files:
- report.pdf (no pattern)
- report(1).pdf (has pattern)
- report(2).pdf (has pattern)

Expected:
✓ Keeps: report.pdf
✓ Deletes: report(1).pdf, report(2).pdf
```

#### Test 3.7: Selection Priority - Lowest Number
```
Test Case: Priority 2 - All have patterns, keep lowest
Files:
- report(2).pdf
- report_1.pdf
- report(5).pdf

Expected:
✓ Keeps: report_1.pdf (number = 1, lowest)
✓ Deletes: report(2).pdf, report(5).pdf
✓ Renames kept file: report_1.pdf → report.pdf
```

#### Test 3.8: Selection Priority - Alphabetical
```
Test Case: Priority 3 - No patterns detected
Files:
- report-final.pdf
- report-copy.pdf
- report-backup.pdf

Expected:
✓ Keeps: report-backup.pdf (alphabetically first)
✓ Deletes: report-copy.pdf, report-final.pdf
```

#### Test 3.9: Hash-First Full Integration Test
```
Test Case: Complete duplicate detection flow
Setup:
TestDir/
├── document.pdf (content: "A", hash: aaa)
├── document(1).pdf (content: "A", hash: aaa) - duplicate
├── document(2).pdf (content: "A", hash: aaa) - duplicate
├── photo.jpg (content: "B", hash: bbb)
├── photo(1).jpg (content: "B", hash: bbb) - duplicate
├── report.xlsx (content: "C", hash: ccc)
└── unique.txt (content: "D", hash: ddd)

Steps:
1. Call DetectDuplicates(TestDir)

Expected Results:
✓ Total files scanned: 7
✓ Duplicate groups found: 2 (document group, photo group)
✓ FilesToKeep: 
  - document.pdf (no pattern)
  - photo.jpg (no pattern)
  - report.xlsx (only one)
  - unique.txt (only one)
✓ FilesToDelete:
  - document(1).pdf
  - document(2).pdf
  - photo(1).jpg
✓ Statistics accurate
```

#### Test 3.10: Performance Test
```
Test Case: Handle large number of files
Setup:
- Create 1000 files in root directory
- 500 unique files
- 500 duplicates with (n) patterns

Steps:
1. Call DetectDuplicates(TestDir)
2. Measure execution time

Expected Results:
✓ Completes in under 10 seconds
✓ Correctly identifies 500 duplicates
✓ No memory leaks or crashes
```

**Gate Criteria:** All Phase 3 tests must pass before proceeding to Phase 4.

---

## Phase 4: File Categorization & Lazy Collision Detection
**Duration:** 5-7 days  
**Dependencies:** Phase 3 complete

### Objectives
- Implement file categorization by extension
- Lazy collision detection during moves
- Version preservation with timestamps
- File operations (move, delete)

### Deliverables

#### 4.1 File Operations Service
```
Implementation:
- MoveFile(source, destination) → OperationResult
  - Move file from source to destination
  - Handle errors (locked files, permissions)
  - Return success/failure with details
  
- DeleteFile(path) → OperationResult
  - Delete file safely
  - Handle errors
  - Return success/failure
  
- FileExists(path) → bool
  - Check if file exists
  
- CreateDirectory(path)
  - Create directory if doesn't exist
  - Create parent directories as needed
```

#### 4.2 Categorizer Service (FR-036 to FR-042)
```
Implementation:
- GetCategoryForFile(filename, categories) → string
  - Extract file extension
  - Match against category definitions
  - Return category name or null if uncategorized
  
- CategorizeFiles(files, targetDir, categories) → CategorizationResult
  - For each file in root:
    - Determine category
    - Generate destination path
    - Check for collision
    - If collision: call CollisionHandler
    - If no collision: move file
  - Return statistics and results
```

#### 4.3 Collision Handler Service (FR-043 to FR-048)
```
Implementation:
- HandleCollision(sourceFile, destFile) → CollisionResult
  
  Phase 1: Check if destination exists
  - If not: return "NoCollision"
  
  Phase 2: Hash both files (lazy approach)
  - Hash source file
  - Hash destination file
  
  Phase 3: Compare hashes
  - If same hash:
    - Action: Delete source (it's a duplicate)
    - Return: "DuplicateRemoved"
  
  - If different hash:
    - Action: Add timestamp to source filename
    - Format: filename_YYYYMMDD_HHMMSS.ext
    - Use source file's LastModifiedDate
    - Return: "VersionPreserved", new filename
```

#### 4.4 Timestamp Service (FR-046, FR-047)
```
Implementation:
- AddTimestampToFilename(filename, date) → string
  - Extract base name and extension
  - Format: basename_YYYYMMDD_HHMMSS.extension
  - Example: report.pdf → report_20260114_143022.pdf
  
- HandleTimestampCollision(filepath) → string
  - If timestamped filename exists
  - Append counter: basename_YYYYMMDD_HHMMSS_1.ext
  - Increment until unique
```

### Testing Phase 4

#### Test 4.1: File Operations - Move
```
Test Case: Successfully move file
Setup:
- Source: TestDir/file.txt
- Dest: TestDir/subfolder/file.txt

Steps:
1. Call MoveFile(source, dest)
Expected Results:
✓ File moved successfully
✓ Source file no longer exists
✓ Destination file exists
✓ Content preserved
```

#### Test 4.2: File Operations - Delete
```
Test Case: Successfully delete file
Setup:
- File: TestDir/file.txt

Steps:
1. Call DeleteFile(file)
Expected Results:
✓ File deleted
✓ File no longer exists
✓ Returns success
```

#### Test 4.3: Category Detection
```
Test Case: Match file to category
Categories:
- pdf: [pdf]
- documents_word: [doc, docx]

Input: "report.pdf"
Expected: ✓ Returns "pdf"

Input: "letter.docx"
Expected: ✓ Returns "documents_word"

Input: "unknown.xyz"
Expected: ✓ Returns null (uncategorized)
```

#### Test 4.4: Categorization - No Collision
```
Test Case: Move file to category folder (no existing file)
Setup:
TestDir/
└── report.pdf

Steps:
1. Categorize report.pdf to "pdf" category
Expected Results:
✓ Creates: TestDir/pdf/ directory
✓ Moves: report.pdf → TestDir/pdf/report.pdf
✓ No hashing performed (no collision)
✓ Root directory empty
```

#### Test 4.5: Lazy Collision - Same Hash (Duplicate)
```
Test Case: Collision with same content
Setup:
TestDir/
├── report.pdf (root, hash: aaa, modified: Jan 14)
└── pdf/
    └── report.pdf (existing, hash: aaa, modified: Jan 10)

Steps:
1. Categorize report.pdf from root
Expected Results:
✓ Collision detected
✓ Hash both files (2 hashes total)
✓ Hashes match (aaa == aaa)
✓ Delete source (root/report.pdf)
✓ Keep destination (pdf/report.pdf)
✓ Log: "Duplicate removed: report.pdf"
```

#### Test 4.6: Lazy Collision - Different Hash (Version)
```
Test Case: Collision with different content
Setup:
TestDir/
├── budget.xlsx (root, hash: bbb, modified: 2026-01-14 14:30:22)
└── spreadsheets/
    └── budget.xlsx (existing, hash: ccc, modified: 2025-12-01)

Steps:
1. Categorize budget.xlsx from root
Expected Results:
✓ Collision detected
✓ Hash both files (2 hashes total)
✓ Hashes differ (bbb != ccc)
✓ Rename source: budget_20260114_143022.xlsx
✓ Move renamed file to spreadsheets/
✓ Both files now in spreadsheets/:
  - budget.xlsx (original)
  - budget_20260114_143022.xlsx (new version)
✓ Log: "Version preserved: budget.xlsx → budget_20260114_143022.xlsx"
```

#### Test 4.7: Timestamp Collision Handling
```
Test Case: Timestamped filename already exists
Setup:
TestDir/spreadsheets/
├── report.xlsx (hash: aaa)
└── report_20260114_143022.xlsx (already exists)

New file:
TestDir/report.xlsx (hash: bbb, modified: 2026-01-14 14:30:22)

Steps:
1. Categorize report.xlsx
Expected Results:
✓ Collision detected with report.xlsx
✓ Different hashes (bbb != aaa)
✓ Attempt timestamp: report_20260114_143022.xlsx
✓ Collision with timestamped name!
✓ Append counter: report_20260114_143022_1.xlsx
✓ File moved successfully with counter
```

#### Test 4.8: Uncategorized Files Remain in Root
```
Test Case: Files with unknown extensions stay in root (FR-040)
Setup:
TestDir/
├── document.pdf (known extension)
└── custom.xyz (unknown extension)

Steps:
1. Run categorization
Expected Results:
✓ document.pdf moved to pdf/
✓ custom.xyz remains in root
✓ Report shows 1 uncategorized file
```

#### Test 4.9: Files with No Extension
```
Test Case: Files without extension go to "unclassified" (FR-041)
Setup:
TestDir/
├── README (no extension)
└── document.pdf

Steps:
1. Run categorization
Expected Results:
✓ document.pdf → pdf/
✓ README → unclassified/README
✓ Report shows 1 file in unclassified
```

#### Test 4.10: Full Integration Test
```
Test Case: Complete workflow - duplicate detection + categorization
Setup:
TestDir/
├── report.pdf (hash: aaa)
├── report(1).pdf (hash: aaa) - duplicate
├── report(2).pdf (hash: aaa) - duplicate
├── budget.xlsx (hash: bbb)
├── photo.jpg (hash: ccc)
├── photo(1).jpg (hash: ccc) - duplicate
├── unknown.xyz (uncategorized)
└── pdf/
    └── report.pdf (hash: aaa) - from previous run

Steps:
1. Run duplicate detection
2. Run categorization

Expected Results After Duplicate Detection:
✓ Keeps: report.pdf, budget.xlsx, photo.jpg, unknown.xyz
✓ Deletes: report(1).pdf, report(2).pdf, photo(1).jpg

Expected Results After Categorization:
✓ report.pdf → collision with pdf/report.pdf
  - Same hash → delete root copy
  - Result: only pdf/report.pdf remains
✓ budget.xlsx → spreadsheets/budget.xlsx
✓ photo.jpg → images/photo.jpg
✓ unknown.xyz → stays in root

Final State:
TestDir/
├── unknown.xyz (uncategorized)
├── pdf/
│   └── report.pdf
├── spreadsheets/
│   └── budget.xlsx
└── images/
    └── photo.jpg
```

**Gate Criteria:** All Phase 4 tests must pass before proceeding to Phase 5.

---

## Phase 5: Orchestration & User Interface Integration
**Duration:** 4-6 days  
**Dependencies:** Phase 4 complete

### Objectives
- Integrate all services into main workflow
- Connect UI to backend services
- Implement progress tracking and cancellation
- Add confirmation dialogs and dry run

### Deliverables

#### 5.1 Orchestrator Service
```
Implementation:
- OrganizeDirectory(directory, dryRun) → OrganizationResult
  
  Phase 1: Pre-scan (FR-052, FR-053)
  - Count files in root directory
  - Update progress: 0%
  
  Phase 2: Duplicate Detection
  - Call DuplicateDetector.DetectDuplicates(directory)
  - Update progress: 25%
  - If dryRun: log actions, don't execute
  - If not dryRun: delete duplicate files
  - Update progress: 50%
  
  Phase 3: Categorization
  - Call Categorizer.CategorizeFiles(...)
  - For each file:
    - Update progress incrementally
    - Handle collisions (lazy detection)
    - Check for cancellation
  - Update progress: 90%
  
  Phase 4: Generate report
  - Compile statistics
  - Update progress: 100%
  
  Return: OrganizationResult with full statistics
```

#### 5.2 Progress Tracking
```
Implementation:
- IProgress<OrganizationProgress> interface
- OrganizationProgress class:
  - Percentage (0-100)
  - CurrentOperation (string)
  - FilesProcessed (int)
  - TotalFiles (int)
  
- Progress callback system:
  - "Scanning directory..."
  - "Detecting duplicates (file 5/50)..."
  - "Removing duplicates..."
  - "Categorizing files (file 10/48)..."
  - "Complete!"
```

#### 5.3 Cancellation Support
```
Implementation:
- CancellationTokenSource integration
- Check for cancellation in loops
- Clean shutdown on cancel:
  - Stop processing
  - Log cancellation
  - Return partial results
```

#### 5.4 UI Integration
```
Implementation:
Main Window:
- "Start Organization" button click handler:
  - Validate directory selected
  - Show confirmation dialog (FR-055, FR-056)
  - Disable UI during processing
  - Create CancellationTokenSource
  - Call orchestrator asynchronously
  - Update progress bar and status text
  - Show summary dialog on completion
  - Re-enable UI
  
- "Cancel" button (appears during processing):
  - Request cancellation
  - Wait for graceful shutdown
  
Confirmation Dialog (FR-055, FR-056):
- Show directory path
- Show file count
- Show warning about permanent deletion
- Show processing strategy description
- Checkbox for "Dry Run" mode
- OK / Cancel buttons
```

#### 5.5 Summary Report Dialog (FR-095 to FR-098)
```
Implementation:
- Display results in formatted text
- Statistics:
  - Total files scanned
  - Duplicates removed (count + space saved)
  - Versions preserved (count)
  - Files organized by category
  - Uncategorized files
  - Errors (if any)
  - Execution time
- Buttons:
  - Copy to Clipboard
  - Save Report
  - View Log
  - Open Directory
  - Close
```

### Testing Phase 5

#### Test 5.1: Pre-scan Integration
```
Test Case: Pre-scan displays file count
Setup:
TestDir/ with 50 files

Steps:
1. Click "Start Organization"
Expected Results:
✓ Confirmation dialog shows: "50 files to process"
✓ Displays in less than 1 second
```

#### Test 5.2: Progress Updates
```
Test Case: Progress bar updates during processing
Setup:
TestDir/ with 100 files (including duplicates)

Steps:
1. Start organization
2. Observe progress
Expected Results:
✓ Progress bar starts at 0%
✓ Updates smoothly during duplicate detection (0-50%)
✓ Updates during categorization (50-90%)
✓ Reaches 100% on completion
✓ Status text updates with current operation
```

#### Test 5.3: Cancellation
```
Test Case: Cancel operation mid-processing
Setup:
TestDir/ with 1000 files

Steps:
1. Start organization
2. Wait for 25% progress
3. Click Cancel button
Expected Results:
✓ Processing stops within 2 seconds
✓ No files left in inconsistent state
✓ Log shows cancellation
✓ Summary shows partial results
✓ UI re-enabled
```

#### Test 5.4: Dry Run Mode
```
Test Case: Dry run doesn't modify files
Setup:
TestDir/ with duplicates and files to organize

Steps:
1. Enable "Dry Run" checkbox
2. Start organization
Expected Results:
✓ Duplicate detection runs
✓ Categorization analysis runs
✓ Report shows what WOULD happen
✓ No files actually moved or deleted
✓ Directory unchanged after dry run
```

#### Test 5.5: Confirmation Dialog
```
Test Case: Confirmation dialog shows correct info
Setup:
TestDir/ with 123 files

Steps:
1. Select directory
2. Click "Start Organization"
Expected Results:
✓ Dialog appears
✓ Shows directory path
✓ Shows "123 files to process"
✓ Shows warning about deletion
✓ Shows processing strategy
✓ Has Dry Run checkbox
✓ Has OK/Cancel buttons
```

#### Test 5.6: Summary Report - Success Case
```
Test Case: Summary shows all statistics
Setup:
TestDir/ with:
- 10 duplicate files (5 pairs)
- 40 unique files
- 5 files with collisions
- 2 uncategorized files

Steps:
1. Run organization
Expected Results:
✓ Summary dialog appears
✓ Shows: "50 files scanned"
✓ Shows: "5 duplicates removed (saved 2.5 MB)"
✓ Shows: "2 versions preserved"
✓ Shows: "43 files organized"
✓ Shows: "2 uncategorized files"
✓ Shows execution time
✓ All counts accurate
```

#### Test 5.7: Summary Report - Buttons
```
Test Case: Summary report buttons work
Steps:
1. Complete organization
2. Click each button in summary

"Copy to Clipboard":
✓ Report text copied to clipboard

"Save Report":
✓ File save dialog opens
✓ Report saved to selected location
✓ File contains full report text

"View Log":
✓ Opens most recent log file in default text editor

"Open Directory":
✓ Opens target directory in Windows Explorer

"Close":
✓ Dialog closes
```

#### Test 5.8: Error Handling - Locked File
```
Test Case: Handle locked file during processing
Setup:
- TestDir/ with files
- Lock one file (open in another app)

Steps:
1. Run organization
Expected Results:
✓ Processing continues
✓ Locked file skipped
✓ Error logged
✓ Summary shows: "1 error (locked file)"
✓ Other files processed successfully
```

#### Test 5.9: Full User Workflow Test
```
Test Case: Complete user experience from start to finish
Setup:
Clean Downloads folder with 200 mixed files

Steps:
1. Open application (first run)
2. Default directory shows Downloads
3. Click "Start Organization"
4. Review confirmation (200 files)
5. Click OK
6. Observe progress
7. Review summary
8. Click "Open Directory"

Expected Results:
✓ Smooth workflow, no errors
✓ Progress updates visible
✓ Summary accurate
✓ Files organized correctly
✓ Professional user experience
```

**Gate Criteria:** All Phase 5 tests must pass before proceeding to Phase 6.

---

## Phase 6: Logging, Error Handling & Deployment
**Duration:** 3-5 days  
**Dependencies:** Phase 5 complete

### Objectives
- Complete logging implementation
- Comprehensive error handling
- Log rotation and cleanup
- Single-file .exe deployment
- Final testing and polish

### Deliverables

#### 6.1 Enhanced Logging (FR-080 to FR-094)
```
Implementation:
- Structured log format:
  [YYYY-MM-DD HH:MM:SS] [LEVEL] Message
  
- Log all operations:
  [SCAN] Started scanning directory: C:\Downloads
  [DUPLICATE] Found duplicate: file(1).pdf (hash: abc123)
  [DELETE] Removed: file(1).pdf
  [MOVE] Moved: report.pdf → pdf/report.pdf
  [COLLISION] Detected collision: budget.xlsx
  [HASH] Comparing hashes: bbb vs ccc
  [VERSION] Preserved version: budget_20260114.xlsx
  [ERROR] Failed to move: locked.txt (file in use)
  
- Error log (separate file):
  - Only ERROR and CRITICAL entries
  - Stack traces included
  - System info on critical errors
  
- Log rotation (FR-089 to FR-094):
  - Keep 30 days or 50 files
  - Background cleanup on startup
  - "Clear Old Logs" button in Settings
```

#### 6.2 Comprehensive Error Handling (FR-099 to FR-103)
```
Implementation:
Error Types:
1. User Errors (FR-099, FR-100):
   - Directory not found
   - Permission denied
   - Disk full
   - File in use
   → User-friendly messages with suggestions
   
2. System Errors (FR-101 to FR-103):
   - Unhandled exceptions
   - Out of memory
   - Configuration corruption
   → Graceful degradation, emergency logging
   
3. Partial Failure Handling:
   - Continue on non-critical errors
   - Track failed operations
   - Report in summary
   - Option to continue or abort
```

#### 6.3 Help System (HELP-001 to HELP-003)
```
Implementation:
- Help menu:
  - User Guide (opens PDF or online docs)
  - About dialog (version, credits)
  
- Tooltips on all major controls
  
- Context-sensitive help where appropriate
```

#### 6.4 Single-File Deployment (DIST-001 to DIST-006)
```
Implementation:
- .csproj configuration:
  <PropertyGroup>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PublishTrimmed>true</PublishTrimmed>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>
  
- Build script:
  dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
  
- Verify:
  - Single .exe file produced
  - Size: 15-40 MB
  - Runs on Windows 10/11 without .NET installed
```

### Testing Phase 6

#### Test 6.1: Logging - Operation Log
```
Test Case: All operations logged correctly
Setup:
TestDir/ with various files

Steps:
1. Run complete organization
2. Open operation log file
Expected Results:
✓ Log file created with timestamp name
✓ All operations logged with timestamps
✓ Log format consistent
✓ No sensitive data in logs
```

#### Test 6.2: Logging - Error Log
```
Test Case: Errors logged separately
Setup:
- TestDir/ with locked file

Steps:
1. Run organization
2. Check error log file
Expected Results:
✓ Error log file created (if errors occurred)
✓ Contains only ERROR entries
✓ Includes details and suggestions
✓ Stack trace present (if exception)
```

#### Test 6.3: Log Rotation
```
Test Case: Old logs cleaned up automatically
Setup:
- Create 60 old log files (>30 days old)

Steps:
1. Start application
2. Wait for background cleanup
3. Check logs directory
Expected Results:
✓ Only 50 most recent logs remain
✓ Logs older than 30 days removed
✓ Cleanup runs in background (non-blocking)
```

#### Test 6.4: Log Cleanup Button
```
Test Case: Manual log cleanup
Setup:
- 100 old log files

Steps:
1. Open Settings
2. Click "Clear Old Logs"
3. Confirm action
Expected Results:
✓ Shows count and size to be freed
✓ Confirmation dialog appears
✓ Old logs deleted on confirm
✓ Summary shows results
```

#### Test 6.5: Error Handling - Directory Not Found
```
Test Case: Handle non-existent directory
Steps:
1. Enter directory: "C:\DoesNotExist"
2. Click "Start Organization"
Expected Results:
✓ User-friendly error message
✓ Message: "Directory does not exist. Please select a valid directory."
✓ Suggestion to use Browse button
✓ No crash or exception visible to user
```

#### Test 6.6: Error Handling - Permission Denied
```
Test Case: Handle permission errors
Setup:
- Directory: C:\Windows\System32

Steps:
1. Select System32 directory
2. Attempt organization
Expected Results:
✓ Error message: "Cannot access directory. Please check folder permissions..."
✓ Suggests running from accessible location
✓ Suggests administrator if needed
✓ No crash
```

#### Test 6.7: Error Handling - Disk Full
```
Test Case: Handle disk full scenario
Setup:
- Target directory on nearly full drive

Steps:
1. Attempt organization
Expected Results:
✓ Pre-check detects insufficient space
✓ Warning message before processing
✓ Option to abort or continue
✓ If continue: graceful handling when space runs out
```

#### Test 6.8: Error Handling - Configuration Corruption
```
Test Case: Handle corrupted configuration
Setup:
- Manually corrupt config.json file

Steps:
1. Start application
Expected Results:
✓ Detects corruption
✓ Creates backup: config.json.corrupted
✓ Regenerates default configuration
✓ Application starts successfully
✓ User notified of reset
```

#### Test 6.9: Single-File Deployment
```
Test Case: Deploy as single executable
Steps:
1. Run publish command
2. Copy .exe to clean test machine (no .NET)
3. Run application

Expected Results:
✓ Single .exe file produced
✓ File size: 15-40 MB
✓ Runs on Windows 10 without .NET installed
✓ All features work
✓ No external dependencies needed
```

#### Test 6.10: Portability Test
```
Test Case: Run from different locations
Steps:
1. Run .exe from USB drive
2. Run .exe from network share
3. Run .exe from Downloads folder

Expected Results:
✓ Runs from all locations
✓ Creates config in %AppData%
✓ Creates logs in %UserProfile%\Documents
✓ No write operations in .exe location
✓ Fully portable
```

#### Test 6.11: Help System
```
Test Case: Help menu items work
Steps:
1. Click Help → User Guide
2. Click Help → About

Expected Results:
✓ User Guide opens (PDF or web browser)
✓ About dialog shows version and credits
✓ Tooltips appear on hover
```

#### Test 6.12: Final Integration Test - 2 Month Scenario
```
Test Case: Your original 2-month scenario
Setup (Run 1):
TestDir/
├── file.txt
├── file(1).txt
├── file(2).txt
└── file(3).txt

Steps:
1. Run organization (Run 1)
Expected Results:
✓ Deletes: file(1).txt, file(2).txt, file(3).txt
✓ Moves: file.txt → documents_text/file.txt

Setup (Run 2 - 2 months later):
TestDir/
├── file.txt (new download)
├── file(1).txt (new download)
├── file(2).txt (new download)
└── documents_text/
    └── file.txt (from Run 1)

Steps:
2. Run organization (Run 2)
Expected Results:
✓ Phase 1: Hash all files in root (3 files)
✓ Phase 1: Groups by hash → 1 group with 3 duplicates
✓ Phase 1: Keeps file.txt (no pattern)
✓ Phase 1: Deletes file(1).txt, file(2).txt
✓ Phase 2: Categorizes file.txt → documents_text/file.txt
✓ Phase 2: Collision detected!
✓ Phase 2: Hashes source + destination (2 files)
✓ Phase 2: Same hash → Delete source
✓ Final: Only documents_text/file.txt remains
✓ Total hashes: 3 (root) + 2 (collision) = 5 hashes
✓ Directory clean! ✅
```

#### Test 6.13: Performance Test - Large Scale
```
Test Case: Handle realistic large directory
Setup:
- 5,000 files in root
- 1,000 duplicates with patterns
- 50 category subdirectories with 10,000 organized files

Steps:
1. Run organization
2. Measure performance

Expected Results:
✓ Completes in under 30 seconds
✓ Hash count: ~5,000 (root) + ~50 (collisions) = ~5,050
✓ NOT hashing 10,000 subdirectory files
✓ UI remains responsive
✓ Progress updates smoothly
✓ All operations logged
✓ No memory leaks
✓ Summary accurate
```

**Gate Criteria:** All Phase 6 tests must pass. Application is ready for release.

---

## Release Checklist

### Pre-Release
- [ ] All 110 functional requirements implemented
- [ ] All phase tests passing (Phases 1-6)
- [ ] Single .exe file builds successfully
- [ ] Runs on Windows 10 (1607+) and Windows 11
- [ ] File size within 15-40 MB target
- [ ] No external dependencies required
- [ ] Configuration and logs create properly on first run

### Documentation
- [ ] User guide written (PDF)
- [ ] README.md created
- [ ] Installation instructions
- [ ] Troubleshooting guide
- [ ] Release notes

### Quality Assurance
- [ ] Code reviewed
- [ ] No compiler warnings
- [ ] Memory profiling completed (no leaks)
- [ ] Exception handling tested
- [ ] Edge cases covered

### Distribution
- [ ] .exe digitally signed (optional but recommended)
- [ ] Antivirus false positive testing
- [ ] Distribution package prepared
- [ ] Version number finalized

---

## Maintenance Plan

### Post-Release
- Monitor user feedback
- Track bug reports
- Collect feature requests
- Plan updates

### Version Updates
- **Patch updates (1.0.x):** Bug fixes only
- **Minor updates (1.x.0):** New features, backward compatible
- **Major updates (x.0.0):** Breaking changes, major features

---

## Development Tools & Environment

### Required Software
- Visual Studio 2022 or VS Code
- .NET 6.0 SDK or .NET 8.0 SDK
- Git for version control

### Recommended Packages
- NUnit or xUnit for unit testing
- Moq for mocking in tests
- WPF UI Toolkit (if using custom controls)

### Build Environment
- Windows 10 or 11 development machine
- Minimum 8 GB RAM
- SSD recommended for build performance

---

## Risk Mitigation

### Known Risks
1. **Large file hashing slowdown**
   - Mitigation: Progress updates, async operations
   
2. **Antivirus false positives**
   - Mitigation: Code signing, documentation
   
3. **Locked file handling**
   - Mitigation: Robust error handling, continue on error
   
4. **Configuration corruption**
   - Mitigation: Backup rotation, validation, recovery

### Contingency Plans
- If Phase X tests fail: Fix issues before proceeding
- If deployment issues: Test on multiple machines
- If performance problems: Profile and optimize

---

## Success Criteria

The project is successful when:
1. ✅ All 110 functional requirements met
2. ✅ All phase tests passing
3. ✅ Your 2-month scenario works perfectly
4. ✅ Non-technical users can use without help
5. ✅ Single .exe distribution works
6. ✅ Performance targets met
7. ✅ Professional user experience
8. ✅ Stable and reliable operation

---

**END OF DEVELOPMENT PLAN**
