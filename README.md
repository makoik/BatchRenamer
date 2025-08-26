# BatchRenamer

Tiny Windows utility for cleaning and batch-renaming messy filenames — perfect for anime episodes, TV series, image dumps, and other cluttered downloads.
Predicts a clean base title, finds related files, previews the result, and applies the rename in one click.

## Features

**Two modes**

Single Files Mode* (default): pick one representative file, match related ones, preview, apply.

Folder Mode: scan a folder, group by predicted names, and preview likely batch groups.

**Smart title prediction**

Strips groups, tags, and episode indicators to guess the core title automatically when a file is selected (WinForms UI pre-fills “New Filename”). Uses nested-bracket removal and pattern cleanup under the hood 

**Pattern-based matching**

Finds companion files in the same folder that “share the core pattern” with the selected file (with lenient fallbacks for Japanese titles or bracket structures)

**Episode/sequence number extraction**

Detects numbers via multiple strategies (Japanese 第01話, S01E01, - 01, etc.) and formats to - 01, - 02, … by default. Avoids common traps like resolutions/years where possible

**Preview before rename**

Clear “before → after” list with counts; only applies when you confirm

**“Add All” one-shot sequencing**

If no matches are found for the smart pattern, you can Add All to rename every file with the same (or associated) extension(s) as New Name - 01, - 02, …great for random image dumps (6545435463435.jpg, etc.)

  Add All appears when smart pattern matching finds nothing, offering a quick sequential run over same/associated extensions

**Aggressive Mode (toggle)**

When on, pattern-matching can include all file types; when off, matches only the same extension as the selected file

**Safety nets**

  Sanitizes invalid Windows filename characters automatically 
  De-duplicates collisions by appending _<nn> so no rename fails due to duplicates
  Skips files that are in use; shows a concise error summary at the end


## Getting Started

### Requirements

- Windows 10/11
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or newer

Want to build your own local version?
run:

```
dotnet publish -c Release -r win-x64 --self-contained false
```

Want to build your own portable version without .NET dependency?
run:

```
dotnet publish -c Release -r win-x64 --self-contained true
```


## Usage

1. **Select a file** to use as the template for renaming.
2. The app will **predict a clean name** and show a preview of all matching files.
3. **Review the preview** and adjust the template if needed.
4. Click **Rename** to batch rename all matching files.

**License**
MIT
