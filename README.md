# BatchRenamer

BatchRenamer is a Windows Forms application for batch renaming files, designed to make renaming large sets of files (such as media, documents, or music) fast and easy. The app automatically detects similar files in a folder, predicts a clean base name, and previews the new names before renaming.

If you have .NET 9.0 installed. Feel free to grab the pre-built version from https://github.com/makoik/BatchRenamer/releases

## Features

- **Automatic file name prediction** using smart pattern matching
- **Batch renaming** of similar files with sequential numbering
- **Preview changes** before renaming
- **Theme support:** Light, Dark, or System theme
- **File type filtering** (only rename files of the same type as selected)
- **User-friendly interface**

## Getting Started

### Requirements

- Windows 10/11
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or newer


### Build & Run

1. Clone or download this repository.
2. Open a terminal in the project folder.
3. Build the project:
    ```
    dotnet build
    ```
4. Run the app:
    ```
    dotnet run
    ```

## Usage

1. **Select a file** to use as the template for renaming.
2. The app will **predict a clean name** and show a preview of all matching files.
3. **Review the preview** and adjust the template if needed.
4. Click **Rename** to batch rename all matching files.



