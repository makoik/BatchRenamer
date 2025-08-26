using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;


namespace BatchRenamer
{
    public enum ThemeMode { System, Light, Dark }

    public partial class Form1 : Form
    {
        private TextBox txtOriginalName;
        private TextBox txtNewName;
        private Button btnSelectFile;
        private Button btnSelectFolder;
        private Button btnPreview;
        private Button btnRename;
        private Button btnAddAll;
        private ListBox lstPreview;
        private Label lblStatus;
        private ToolTip toolTip;
        private CheckBox aggressiveMode;
        private Label aggressiveModeLabel;

        private string selectedFilePath = "";
        private List<FileRenameOperation> renameOperations = new List<FileRenameOperation>();

        private ThemeMode currentTheme = ThemeMode.System;

        private bool isSingleFileMode = true;

        public Form1()
        {
            InitializeComponent();

            // Initialize state
            btnPreview.Enabled = false;
            txtOriginalName.Text = "No file selected...";
            txtOriginalName.ForeColor = System.Drawing.Color.Gray;
            lstPreview.Items.Add("Welcome to Batch File Renamer!");
            lstPreview.Items.Add("");
            lstPreview.Items.Add("Getting started:");
            lstPreview.Items.Add("1. Choose either Single Files Mode (default) or Folder Mode using the 📁 button");
            lstPreview.Items.Add("2. Click 'Select File' or 'Select Folder' depending on the mode");
            lstPreview.Items.Add("3. Edit the filename in the second field");
            lstPreview.Items.Add("4. Click 'Preview' to see what will be renamed");
            lstPreview.Items.Add("5. Click 'Apply Rename' to execute the changes");
            lstPreview.Items.Add("");
            lstPreview.Items.Add("Tips:");
            lstPreview.Items.Add("- Use aggressive mode to match files of all types, not just the same extension");
            lstPreview.Items.Add("- Use the ⚙ button to change themes");
            lstPreview.Items.Add("");
            lstPreview.Items.Add("To rename multiple randomly named files:");
            lstPreview.Items.Add("1. Select a file in Single Files mode, add your desired name for the files");
            lstPreview.Items.Add("2. Press Preview, and then Add All (shows up next to Preview button)");

            ApplySystemTheme();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            toolTip = new ToolTip();

            // Form settings
            this.Text = "Batch File Renamer";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Original filename field
            var lblOriginal = new Label();
            lblOriginal.Text = "Original Filename:";
            lblOriginal.Location = new System.Drawing.Point(12, 13);
            lblOriginal.Size = new System.Drawing.Size(120, 23);
            this.Controls.Add(lblOriginal);

            txtOriginalName = new TextBox();
            txtOriginalName.Location = new System.Drawing.Point(12, 35);
            txtOriginalName.Size = new System.Drawing.Size(500, 23);
            txtOriginalName.ReadOnly = true;
            txtOriginalName.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(txtOriginalName);

            btnSelectFile = new Button();
            btnSelectFile.Text = "Select File";
            btnSelectFile.Location = new System.Drawing.Point(520, 34);
            btnSelectFile.Size = new System.Drawing.Size(80, 25);
            btnSelectFile.Click += BtnSelectFile_Click;
            this.Controls.Add(btnSelectFile);

            // New filename field
            var lblNew = new Label();
            lblNew.Text = "New Filename:";
            lblNew.Location = new System.Drawing.Point(12, 73);
            lblNew.Size = new System.Drawing.Size(120, 23);
            this.Controls.Add(lblNew);

            txtNewName = new TextBox();
            txtNewName.Location = new System.Drawing.Point(12, 95);
            txtNewName.Size = new System.Drawing.Size(500, 23);
            this.Controls.Add(txtNewName);

            btnPreview = new Button();
            btnPreview.Text = "Preview";
            btnPreview.Location = new System.Drawing.Point(520, 94);
            btnPreview.Size = new System.Drawing.Size(80, 25);
            btnPreview.Click += BtnPreview_Click;
            this.Controls.Add(btnPreview);

            btnAddAll = new Button();
            btnAddAll.Text = "Add All";
            btnAddAll.Location = new System.Drawing.Point(610, 94);
            btnAddAll.Size = new System.Drawing.Size(80, 25);
            btnAddAll.Click += btnAddAll_Click;
            btnAddAll.Visible = false; // Initially disabled
            btnAddAll.BackColor = System.Drawing.Color.FromArgb(225, 165, 0);
            toolTip.SetToolTip(btnAddAll, "Add all files with matching extension in the folder to the rename list");
            this.Controls.Add(btnAddAll);

            // Preview list
            var lblPreview = new Label();
            lblPreview.Text = "Preview Changes:";
            lblPreview.Location = new System.Drawing.Point(12, 132);
            lblPreview.Size = new System.Drawing.Size(120, 23);
            this.Controls.Add(lblPreview);

            lstPreview = new ListBox();
            lstPreview.Location = new System.Drawing.Point(12, 155);
            lstPreview.Size = new System.Drawing.Size(670, 250);
            lstPreview.HorizontalScrollbar = true;
            this.Controls.Add(lstPreview);

            // Action buttons
            btnRename = new Button();
            btnRename.Text = "Apply Rename";
            btnRename.Location = new System.Drawing.Point(12, 420);
            btnRename.Size = new System.Drawing.Size(100, 30);
            btnRename.Click += BtnRename_Click;
            btnRename.Enabled = false;
            this.Controls.Add(btnRename);

            // Status label
            lblStatus = new Label();
            lblStatus.Location = new System.Drawing.Point(130, 425);
            lblStatus.Size = new System.Drawing.Size(140, 23);
            lblStatus.Text = "Select a file to begin...";
            this.Controls.Add(lblStatus);

            // Settings button
            var btnSettings = new Button();
            btnSettings.Text = "⚙";
            btnSettings.Location = new System.Drawing.Point(650, 10);
            btnSettings.Size = new System.Drawing.Size(40, 30);
            btnSettings.Click += BtnSettings_Click;
            this.Controls.Add(btnSettings);

            // Folder Mode toggle button
            var btnFolderModeToggle = new Button();
            btnFolderModeToggle.Text = "📁";
            btnFolderModeToggle.Location = new System.Drawing.Point(650, 50);
            btnFolderModeToggle.Size = new System.Drawing.Size(40, 30);
            btnFolderModeToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFolderModeToggle.BackColor = SystemColors.Control;
            btnFolderModeToggle.Click += BtnFolderModeToggle_Click;
            btnFolderModeToggle.Font = new System.Drawing.Font("Segoe UI Emoji", 14);
            this.Controls.Add(btnFolderModeToggle);

            // Aggressive mode
            aggressiveMode = new CheckBox();
            aggressiveMode.Text = "Aggressive Mode";
            aggressiveMode.Location = new System.Drawing.Point(560, 127);
            aggressiveMode.Size = new System.Drawing.Size(120, 24);
            aggressiveMode.Checked = false;
            toolTip.SetToolTip(aggressiveMode,
                "When enabled, matches files of all types, not just the same extension");
            aggressiveMode.CheckedChanged += AggressiveModeToggle;
            this.Controls.Add(aggressiveMode);

            // Aggressive mode label
            aggressiveModeLabel = new Label();
            aggressiveModeLabel.Location = new System.Drawing.Point(520, 425);
            aggressiveModeLabel.Size = new System.Drawing.Size(150, 24);
            this.Controls.Add(aggressiveModeLabel);

            // Tooltip for folder mode
            toolTip.SetToolTip(btnFolderModeToggle, "Toggle Folder Mode");

            // Add folder select button (hidden by default, shown in folder mode)
            var btnSelectFolder = new Button();
            btnSelectFolder.Text = "Select Folder";
            btnSelectFolder.Location = new System.Drawing.Point(520, 64);
            btnSelectFolder.Size = new System.Drawing.Size(85, 25);
            btnSelectFolder.Click += BtnSelectFolder_Click;
            btnSelectFolder.Visible = false; // Only visible in folder mode
            this.Controls.Add(btnSelectFolder);

            // Store reference for toggling visibility
            this.btnSelectFolder = btnSelectFolder;

            this.ResumeLayout(false);
        }

        private void EnsureUniqueFilenames(List<FileRenameOperation> operations)
        {
            var nameCount = new Dictionary<string, int>();
            
            foreach (var operation in operations)
            {
                string baseName = Path.GetFileNameWithoutExtension(operation.NewName);
                string extension = Path.GetExtension(operation.NewName);
                string directory = Path.GetDirectoryName(operation.NewPath);
                
                if (nameCount.ContainsKey(operation.NewName.ToLower()))
                {
                    nameCount[operation.NewName.ToLower()]++;
                    string uniqueName = $"{baseName}_{nameCount[operation.NewName.ToLower()]:D2}{extension}";
                    operation.NewName = uniqueName;
                    operation.NewPath = Path.Combine(directory, uniqueName);
                }
                else
                {
                    nameCount[operation.NewName.ToLower()] = 1;
                }
            }
        }

        private string SanitizeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return filename;
            
            // Remove invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = filename;
            
            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }
            
            // Remove problematic characters that Windows doesn't like
            sanitized = sanitized.Replace(":", "_");
            sanitized = sanitized.Replace("*", "_");
            sanitized = sanitized.Replace("?", "_");
            sanitized = sanitized.Replace("\"", "_");
            sanitized = sanitized.Replace("<", "_");
            sanitized = sanitized.Replace(">", "_");
            sanitized = sanitized.Replace("|", "_");
            
            // Trim whitespace and dots from the end (Windows doesn't like this)
            sanitized = sanitized.TrimEnd(' ', '.');
            
            // Ensure it's not empty
            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "renamed_file";
            
            return sanitized;
        }

        private void btnAddAll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFilePath))
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string directory = Path.GetDirectoryName(selectedFilePath);
            string selectedExtension = Path.GetExtension(selectedFilePath).ToLower();

            var associatedExtensions = GetAssociatedExtensions(selectedExtension);

            bool includeAssociated = false;
            if (associatedExtensions.Count > 1)
            {
                string extensionList = string.Join(", ", associatedExtensions.Select(ext => ext.ToUpper()));
                var result = MessageBox.Show(
                    $"Would you like to include associated file types?\n\nSelected: {selectedExtension.ToUpper()}\nAssociated types: {extensionList}\n\nClick Yes to include all associated types, or No to use only {selectedExtension.ToUpper()}.",
                    "Include Associated File Types?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                includeAssociated = (result == DialogResult.Yes);
            }

            try
            {
                btnAddAll.Enabled = false;
                btnAddAll.Text = "Loading...";
                lblStatus.Text = "🔍 Adding all matching files...";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                Application.DoEvents();

                var allFiles = Directory.GetFiles(directory);
                var targetExtensions = includeAssociated
                    ? associatedExtensions
                    : new List<string> { selectedExtension };

                var matchingFiles = allFiles
                    .Where(f => targetExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                matchingFiles = matchingFiles.OrderBy(f => Path.GetFileName(f)).ToList();

                renameOperations.Clear();
                int counter = 1;

                foreach (var file in matchingFiles)
                {
                    string extension = Path.GetExtension(file);
                    string newName = $"{txtNewName.Text.Trim()} - {counter:D2}";
                    string newFileName = newName + extension;

                    renameOperations.Add(new FileRenameOperation
                    {
                        OriginalPath = file,
                        OriginalName = Path.GetFileName(file),
                        NewName = newFileName,
                        NewPath = Path.Combine(directory, newFileName)
                    });

                    counter++;
                }

                foreach (var operation in renameOperations)
                {
                    string baseName = Path.GetFileNameWithoutExtension(operation.NewName);
                    string extension = Path.GetExtension(operation.NewName);
                    string sanitizedBaseName = SanitizeFilename(baseName);
                    
                    if (sanitizedBaseName != baseName)
                    {
                        operation.NewName = sanitizedBaseName + extension;
                        operation.NewPath = Path.Combine(Path.GetDirectoryName(operation.NewPath), operation.NewName);
                    }
                }

                EnsureUniqueFilenames(renameOperations);

                DisplayPreview();
                btnRename.Enabled = renameOperations.Count > 0;

                if (renameOperations.Count > 0)
                {
                    lblStatus.Text = $"✓ Added {renameOperations.Count} files ({targetExtensions.Count} extension types)";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                }
                else
                {
                    lblStatus.Text = "⚠ No matching files found";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding files:\n{ex.Message}", "Add Files Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Error occurred while adding files";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
            }
            finally
            {
                btnAddAll.Enabled = true;
                btnAddAll.Text = "Add All";
            }
        }

        private List<string> GetAssociatedExtensions(string extension)
        {
            var extensionGroups = new Dictionary<string, List<string>>
            {
                // Image formats
                [".jpg"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },
                [".jpeg"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },
                [".png"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },
                [".bmp"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },
                [".gif"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },
                [".tiff"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },
                [".webp"] = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" },

                // Video formats
                [".mp4"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".mkv"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".avi"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".mov"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".wmv"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".flv"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".webm"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },
                [".m4v"] = new List<string> { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v" },

                // Audio formats
                [".mp3"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },
                [".wav"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },
                [".flac"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },
                [".aac"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },
                [".ogg"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },
                [".wma"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },
                [".m4a"] = new List<string> { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" },

                // Document formats
                [".pdf"] = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt" },
                [".doc"] = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt" },
                [".docx"] = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt" },
                [".txt"] = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt" },
                [".rtf"] = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt" },
                [".odt"] = new List<string> { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt" },

                // Archive formats
                [".zip"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" },
                [".rar"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" },
                [".7z"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" },
                [".tar"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" },
                [".gz"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" },
                [".bz2"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" }
            };

            return extensionGroups.ContainsKey(extension.ToLower()) 
                ? extensionGroups[extension.ToLower()] 
                : new List<string> { extension };
        }

        private string PredictCleanName(string originalFilename)
        {
            if (string.IsNullOrWhiteSpace(originalFilename))
                return originalFilename;

            string cleanName = originalFilename;

            // Step 1: Remove bracketed content (more sophisticated to handle nested brackets)
            // Handle nested brackets like [content [nested] more] properly
            cleanName = RemoveNestedBrackets(cleanName);

            // Step 2: Remove Japanese episode indicators and everything after them
            // 第01話「title」 -> just get the part before 第
            var japaneseEpisodeMatch = Regex.Match(cleanName, @"^(.*?)\s*第\d+話", RegexOptions.IgnoreCase);
            if (japaneseEpisodeMatch.Success)
            {
                cleanName = japaneseEpisodeMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Also handle other Japanese patterns like 話 without 第
                var simpleEpisodeMatch = Regex.Match(cleanName, @"^(.*?)\s*\d+話", RegexOptions.IgnoreCase);
                if (simpleEpisodeMatch.Success)
                {
                    cleanName = simpleEpisodeMatch.Groups[1].Value.Trim();
                }
            }

            // Step 3: Remove episode-related content at the end
            // Remove patterns like "- 01", "Episode 01", etc. from the end
            cleanName = Regex.Replace(cleanName, @"\s*-\s*\d{1,3}(?:v\d+)?$", "", RegexOptions.IgnoreCase);
            cleanName = Regex.Replace(cleanName, @"\s*[Ee]pisode\s*\d{1,3}$", "", RegexOptions.IgnoreCase);
            cleanName = Regex.Replace(cleanName, @"\s*[Ee]p\s*\d{1,3}$", "", RegexOptions.IgnoreCase);

            // Step 4: Clean up spaces and separators
            cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();

            // Step 5: Remove trailing separators
            cleanName = Regex.Replace(cleanName, @"\s*[-_\.]\s*$", "").Trim();
            cleanName = Regex.Replace(cleanName, @"^\s*[-_\.]\s*", "").Trim();

            if (string.IsNullOrWhiteSpace(cleanName))
            {
                // Fallback: try to extract any meaningful content
                cleanName = ExtractMeaningfulTitle(originalFilename);
            }

            return cleanName;
        }

        private string RemoveNestedBrackets(string input)
        {
            // Handle nested brackets more carefully
            string result = input;

            // Remove content in square brackets (can be nested)
            while (true)
            {
                string before = result;
                result = Regex.Replace(result, @"\[[^\[\]]*\]", " ");
                if (result == before) break; // No more changes
            }

            // Remove content in parentheses (can be nested)  
            while (true)
            {
                string before = result;
                result = Regex.Replace(result, @"\([^\(\)]*\)", " ");
                if (result == before) break; // No more changes
            }

            return Regex.Replace(result, @"\s+", " ").Trim();
        }

        private string ExtractMeaningfulTitle(string filename)
        {
            // Last resort: try to find the longest meaningful sequence
            // Remove all brackets first
            string clean = RemoveNestedBrackets(filename);

            // Split by common separators and find the longest part that looks like a title
            var parts = Regex.Split(clean, @"[\s\-_\.]+")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Where(p => p.Length > 1)
                .Where(p => !Regex.IsMatch(p, @"^\d+p?$")) // Not resolution
                .Where(p => !Regex.IsMatch(p, @"^\d{4}$")) // Not year
                .ToList();

            if (parts.Any())
            {
                // Return the longest part, or combine the first few meaningful parts
                return string.Join(" ", parts.Take(3)); // Take first 3 meaningful parts
            }

            return filename;
        }


        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select a file to rename";
                ofd.Filter = "All Files (*.*)|*.*|Video Files (*.mp4;*.mkv;*.avi)|*.mp4;*.mkv;*.avi|Documents (*.pdf;*.doc;*.txt)|*.pdf;*.doc;*.txt";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = ofd.FileName;
                    string filenameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFilePath);

                    txtOriginalName.Text = filenameWithoutExtension;
                    txtOriginalName.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);

                    // Use smart prediction instead of just copying the original name
                    string predictedName = PredictCleanName(filenameWithoutExtension);
                    txtNewName.Text = predictedName;
                    txtNewName.Focus();
                    txtNewName.SelectAll();

                    lblStatus.Text = $"✓ Selected: {Path.GetFileName(selectedFilePath)} (Smart name predicted)";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);

                    lstPreview.Items.Clear();
                    lstPreview.Items.Add("Smart prediction applied! Edit the filename above and click Preview to see matching files...");
                    lstPreview.Items.Add($"Original: {filenameWithoutExtension}");
                    lstPreview.Items.Add($"Predicted: {predictedName}");

                    btnRename.Enabled = false;
                    btnPreview.Enabled = true;
                    btnAddAll.Visible = false;
                }
            }
        }

        // Add this method to handle folder selection and preview in folder mode:
        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a folder for batch renaming";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = fbd.SelectedPath;
                    lblStatus.Text = $"✓ Selected folder: {selectedFolder}";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);

                    DisplayFolderBatchPreview(selectedFolder);
                    btnRename.Enabled = false;

                    if (renameOperations.Count > 0)
                    {
                        btnRename.Enabled = true;
                    }
                }
            }
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            if (isSingleFileMode)
            {
                if (string.IsNullOrWhiteSpace(selectedFilePath) || string.IsNullOrWhiteSpace(txtNewName.Text))
                {
                    MessageBox.Show("Please select a file and enter a new filename.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    btnPreview.Enabled = false;
                    btnPreview.Text = "Loading...";
                    lblStatus.Text = "🔍 Scanning for matching files...";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                    Application.DoEvents();

                    renameOperations = FindAndGenerateRenames();
                    DisplayPreview();
                    btnRename.Enabled = renameOperations.Count > 0;

                    if (renameOperations.Count > 0)
                    {
                        lblStatus.Text = $"✓ Found {renameOperations.Count} files to rename";
                        lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                        btnAddAll.Visible = false;
                    }
                    else
                    {
                        lblStatus.Text = "⚠ No matching files found";
                        lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                        btnAddAll.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating preview:\n{ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "❌ Error occurred during preview";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
                    btnAddAll.Visible = false;
                }
                finally
                {
                    btnPreview.Enabled = true;
                    btnPreview.Text = "Preview";
                }
            }
            else
            {
                // Folder mode: Preview is handled after folder selection
                MessageBox.Show("Preview is shown after selecting a folder in Folder Mode.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private List<FileRenameOperation> FindAndGenerateRenames()
        {
            var operations = new List<FileRenameOperation>();
            string directory = Path.GetDirectoryName(selectedFilePath);
            string originalName = Path.GetFileNameWithoutExtension(selectedFilePath);
            string newNameTemplate = txtNewName.Text.Trim();

            // Get all files in the directory
            var allFiles = Directory.GetFiles(directory);

            // Find matching files using basic pattern matching
            var matchingFiles = FindMatchingFiles(allFiles, originalName);

            // Sort files to maintain order (important for sequential numbering)
            matchingFiles = matchingFiles.OrderBy(f => Path.GetFileName(f)).ToList();

            foreach (var file in matchingFiles)
            {
                string currentName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);

                // Extract any numeric patterns from the original name
                string newName = GenerateNewName(currentName, originalName, newNameTemplate);
                string newFileName = newName + extension;

                operations.Add(new FileRenameOperation
                {
                    OriginalPath = file,
                    OriginalName = Path.GetFileName(file),
                    NewName = newFileName,
                    NewPath = Path.Combine(directory, newFileName)
                });
            }

            foreach (var operation in operations)
            {
                string baseName = Path.GetFileNameWithoutExtension(operation.NewName);
                string extension = Path.GetExtension(operation.NewName);
                string sanitizedBaseName = SanitizeFilename(baseName);
                
                if (sanitizedBaseName != baseName)
                {
                    operation.NewName = sanitizedBaseName + extension;
                    operation.NewPath = Path.Combine(Path.GetDirectoryName(operation.NewPath), operation.NewName);
                }
            }

            EnsureUniqueFilenames(operations);

            return operations;
        }

        

        private List<string> FindMatchingFiles(string[] allFiles, string originalName)
        {
            var matching = new List<string>();

            if (string.IsNullOrEmpty(selectedFilePath))
                return matching;

            string selectedExtension = Path.GetExtension(selectedFilePath).ToLower();
            string corePattern = ExtractCorePatternAdvanced(originalName);

            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file).ToLower();

                // Update checkbox reference name
                if (!aggressiveMode.Checked && ext != selectedExtension)
                    continue;

                if (SharesCorePatternAdvanced(fileName, corePattern, originalName))
                {
                    matching.Add(file);
                }
            }

            // If we didn't find enough matches, try a more lenient approach
            if (matching.Count <= 1)
            {
                // Update checkbox reference name
                var filesToSearch = aggressiveMode.Checked ?
                    allFiles :
                    allFiles.Where(f => Path.GetExtension(f).ToLower() == selectedExtension).ToArray();

                matching = FindMatchingFilesLenient(filesToSearch, originalName);
            }

            return matching;
        }

        private string ExtractCorePatternAdvanced(string filename)
        {
            string pattern = filename;

            // Remove all bracketed content (handles nested brackets better)
            pattern = RemoveNestedBrackets(pattern);

            // Remove Japanese episode patterns
            pattern = Regex.Replace(pattern, @"第\d+話.*$", "").Trim(); // 第01話...
            pattern = Regex.Replace(pattern, @"\d+話.*$", "").Trim();   // 01話...

            // Remove common episode patterns
            pattern = Regex.Replace(pattern, @"\s*-\s*\d+\s*$", "").Trim();     // - 01
            pattern = Regex.Replace(pattern, @"\s*[Ee]pisode\s*\d+.*$", "").Trim(); // Episode 01
            pattern = Regex.Replace(pattern, @"\s*[Ee]p\s*\d+.*$", "").Trim();      // Ep 01
            pattern = Regex.Replace(pattern, @"\s*#\d+.*$", "").Trim();             // #01

            // Remove version numbers
            pattern = Regex.Replace(pattern, @"\s*v\d+\s*$", "", RegexOptions.IgnoreCase).Trim();

            // Remove common suffixes
            string[] commonSuffixes = { "draft", "final", "copy", "backup", "temp", "old", "new" };
            foreach (var suffix in commonSuffixes)
            {
                pattern = Regex.Replace(pattern, $@"\s*{suffix}\s*$", "", RegexOptions.IgnoreCase).Trim();
            }

            // Clean up any remaining separators at the end
            pattern = Regex.Replace(pattern, @"[-_\.\s]+$", "").Trim();

            return pattern;
        }

        private bool SharesCorePatternAdvanced(string filename, string corePattern, string originalName)
        {
            // Extract core pattern from the filename being tested
            string fileCorePattern = ExtractCorePatternAdvanced(filename);

            // Check for exact match first
            if (fileCorePattern.Equals(corePattern, StringComparison.OrdinalIgnoreCase))
                return true;

            // For Japanese/Unicode content, also check if they contain the same key characters
            if (ContainsUnicodeCharacters(corePattern) || ContainsUnicodeCharacters(fileCorePattern))
            {
                // More lenient matching for Unicode content
                if (CalculateSimilarity(fileCorePattern, corePattern) > 0.8)
                    return true;

                // Check if one contains the other (useful for Japanese titles)
                if (fileCorePattern.Contains(corePattern) || corePattern.Contains(fileCorePattern))
                    return true;
            }
            else
            {
                // For ASCII content, use stricter similarity
                if (CalculateSimilarity(fileCorePattern, corePattern) > 0.9)
                    return true;
            }

            return false;
        }

        private bool ContainsUnicodeCharacters(string text)
        {
            return text.Any(c => c > 127); // Non-ASCII characters
        }

        private List<string> FindMatchingFilesLenient(string[] allFiles, string originalName)
        {
            var matching = new List<string>();

            // Get the directory of the original file
            string originalDir = Path.GetDirectoryName(allFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals(Path.GetFileNameWithoutExtension(originalName),
                StringComparison.OrdinalIgnoreCase)));

            // Strategy: Look for files that have similar bracket patterns or Japanese episode indicators
            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                // Check for Japanese episode patterns
                if (Regex.IsMatch(fileName, @"第\d+話") || Regex.IsMatch(fileName, @"\d+話"))
                {
                    // Extract the title part and compare
                    string titlePart = ExtractCorePatternAdvanced(fileName);
                    string originalTitlePart = ExtractCorePatternAdvanced(originalName);

                    if (!string.IsNullOrEmpty(titlePart) && !string.IsNullOrEmpty(originalTitlePart))
                    {
                        if (CalculateSimilarity(titlePart, originalTitlePart) > 0.5)
                        {
                            matching.Add(file);
                        }
                    }
                }
                // Check for files with similar bracket structure
                else if (HasSimilarBracketStructure(fileName, originalName))
                {
                    matching.Add(file);
                }
            }

            return matching;
        }

        private bool HasSimilarBracketStructure(string filename1, string filename2)
        {
            // Count brackets in both files
            int brackets1 = filename1.Count(c => c == '[' || c == ']');
            int brackets2 = filename2.Count(c => c == '[' || c == ']');

            // If both have brackets, they might be related
            if (brackets1 > 0 && brackets2 > 0)
            {
                // Remove all bracketed content and compare the remaining parts
                string clean1 = RemoveNestedBrackets(filename1);
                string clean2 = RemoveNestedBrackets(filename2);

                return CalculateSimilarity(clean1, clean2) > 0.4;
            }

            return false;
        }

        private double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            int maxLength = Math.Max(s1.Length, s2.Length);
            int distance = LevenshteinDistance(s1.ToLower(), s2.ToLower());
            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        private string GenerateNewName(string currentName, string originalName, string newTemplate)
        {
            if (string.IsNullOrWhiteSpace(currentName) || string.IsNullOrWhiteSpace(newTemplate))
                return newTemplate ?? string.Empty;

            string episodeNumber = null;

            // Method 1: Look for Japanese episode patterns first (第01話, 01話)
            var japaneseEpisodeMatch = Regex.Match(currentName, @"第(\d{1,3})話", RegexOptions.IgnoreCase);
            if (japaneseEpisodeMatch.Success)
            {
                episodeNumber = japaneseEpisodeMatch.Groups[1].Value;
            }
            else
            {
                var simpleJapaneseMatch = Regex.Match(currentName, @"(?<![a-zA-Z])(\d{1,3})話", RegexOptions.IgnoreCase);
                if (simpleJapaneseMatch.Success)
                {
                    episodeNumber = simpleJapaneseMatch.Groups[1].Value;
                }
            }

            // Method 2: Look for Season/Episode style (S01E01, S1E5, etc.) - most explicit
            if (episodeNumber == null)
            {
                var seasonEpisodeMatch = Regex.Match(currentName, @"[Ss](\d{1,2})[Ee](\d{1,3})", RegexOptions.IgnoreCase);
                if (seasonEpisodeMatch.Success)
                {
                    episodeNumber = seasonEpisodeMatch.Groups[2].Value;
                }
            }

            // Method 3: Look for dash-style episode numbers (- 01, - 02, etc.)
            if (episodeNumber == null)
            {
                var dashMatch = Regex.Match(currentName, @"-\s*(\d{1,3})(?:\s|\(|$)", RegexOptions.IgnoreCase);
                if (dashMatch.Success)
                {
                    int epNum = int.Parse(dashMatch.Groups[1].Value);
                    if (epNum >= 1 && epNum <= 999) // Reasonable episode range
                    {
                        episodeNumber = dashMatch.Groups[1].Value;
                    }
                }
            }

            // Method 4: Remove brackets and look for patterns in clean text
            if (episodeNumber == null)
            {
                // Remove all bracketed/parenthesized content first
                string cleanName = Regex.Replace(currentName, @"[\[\(][^\[\]\(\)]*[\]\)]", " ");
                cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();

                // Now look for episode patterns in the clean text
                var patterns = new[]
                {
                    @"-\s*(\d{1,3})(?:\s|v\d+|\.|$)",           // - 01, - 01v2, - 01.
                    @"_\s*(\d{1,3})(?:\s|v\d+|\.|$)",           // _01, _01v2
                    @"\.(\d{1,3})(?:\s|v\d+|\.|$)",             // .01, .01v2
                    @"\s(\d{1,3})(?:\s|v\d+|\.|$)",             // space 01
                    @"[Ee]pisode\s*(\d{1,3})",                  // Episode 01
                    @"[Ee]p\s*(\d{1,3})",                       // Ep 01
                    @"第(\d{1,3})話",                           // Japanese: 第01話 (dai 01 wa)
                    @"(\d{1,3})話",                             // Japanese: 01話
                    @"#(\d{1,3})(?:\s|$)",                      // #01
                    @"No\.?\s*(\d{1,3})(?:\s|$)",               // No.01, No 01
                    @"Chapter\s*(\d{1,3})(?:\s|$)",             // Chapter 01
                    @"Part\s*(\d{1,3})(?:\s|$)",                // Part 01
                    @"Vol\.?\s*(\d{1,3})(?:\s|$)",              // Vol.01, Vol 01
                    @"V(\d{1,3})(?:\s|$)",                      // V01
                    @"\[(\d{1,3})\]",                           // [01]
                    @"\((\d{1,3})\)",                           // (01)
                    @"~(\d{1,3})(?:\s|$)",                      // ~01
                    @"(\d{1,3})(?:st|nd|rd|th)(?:\s|$)",        // 1st, 2nd, 3rd, 4th
                    @"x(\d{1,3})(?:\s|$)",                      // x01 (for 1x01 format)
                    @"(\d{1,3})\s*of\s*\d{1,3}(?:\s|$)",       // 01 of 12
                };

                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(cleanName, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int epNum = int.Parse(match.Groups[1].Value);
                        if (epNum >= 1 && epNum <= 999)
                        {
                            episodeNumber = match.Groups[1].Value;
                            break;
                        }
                    }
                }
            }

            // Method 5: Last resort - find any reasonable number in the filename
            if (episodeNumber == null)
            {
                // Look for standalone numbers that could be episode numbers
                var numberMatches = Regex.Matches(currentName, @"\b(\d{1,3})\b");
                foreach (Match match in numberMatches)
                {
                    int num = int.Parse(match.Groups[1].Value);
                    // Skip obvious non-episode numbers (years, resolutions, etc.)
                    if (num >= 1 && num <= 999 &&
                        num != 720 && num != 1080 && num != 480 && // Skip resolutions
                        (num < 1900 || num > 2030)) // Skip years
                    {
                        episodeNumber = match.Groups[1].Value;
                        break;
                    }
                }
            }

            // If no episode number found, return just the template
            if (string.IsNullOrEmpty(episodeNumber))
                return newTemplate;

            // Format the episode number with leading zeros (01, 02, etc.)
            string formattedEpisode = episodeNumber.PadLeft(2, '0');

            // Build the new name: Template + formatted episode
            return $"{newTemplate} - {formattedEpisode}";
        }


        private void DisplayPreview()
        {
            lstPreview.Items.Clear();

            if (renameOperations.Count == 0)
            {
                lstPreview.Items.Add("No matching files found in the directory.");
                lstPreview.Items.Add("");
                lstPreview.Items.Add("Tips:");
                lstPreview.Items.Add("• Make sure there are similar files in the same folder");
                lstPreview.Items.Add("• Try a different filename pattern");
                lstPreview.Items.Add("• Check if files have common prefixes or suffixes");
                return;
            }

            lstPreview.Items.Add($"Preview: {renameOperations.Count} files will be renamed");
            lstPreview.Items.Add("".PadRight(80, '─'));

            foreach (var operation in renameOperations)
            {
                lstPreview.Items.Add($"➤ {operation.OriginalName}");
                lstPreview.Items.Add($"  → {operation.NewName}");
                lstPreview.Items.Add("");
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (var fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
        }

        private void BtnRename_Click(object sender, EventArgs e)
        {
            if (renameOperations.Count == 0)
            {
                MessageBox.Show("No files to rename. Please generate a preview first.", "No Files Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to rename {renameOperations.Count} files?\n\nThis action cannot be undone.",
                                        "Confirm Batch Rename", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;


            try
            {
                btnRename.Enabled = false;
                btnRename.Text = "Renaming...";
                lblStatus.Text = "🔄 Renaming files...";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                Application.DoEvents();

                int successful = 0;
                int failed = 0;
                var errors = new List<string>();

                foreach (var operation in renameOperations)
                {
                    try
                    {
                        if (IsFileLocked(operation.OriginalPath))
                        {
                            errors.Add($"File in use: {operation.OriginalName}");
                            failed++;
                            continue;
                        }

                        if (File.Exists(operation.NewPath))
                        {
                            errors.Add($"Target already exists: {operation.NewName}");
                            failed++;
                            continue;
                        }

                        File.Move(operation.OriginalPath, operation.NewPath);
                        successful++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed: {operation.OriginalName} - {ex.Message}");
                        failed++;
                    }
                }

                if (failed > 0)
                {
                    string errorMsg = $"Completed with issues:\n✓ {successful} successful\n❌ {failed} failed\n\nErrors:\n";
                    errorMsg += string.Join("\n", errors.Take(5));
                    if (errors.Count > 5) errorMsg += $"\n... and {errors.Count - 5} more errors";

                    MessageBox.Show(errorMsg, "Rename Complete with Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = $"⚠ Completed: {successful} successful, {failed} failed";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                }
                else
                {
                    MessageBox.Show($"Successfully renamed {successful} files!", "Rename Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblStatus.Text = $"Successfully renamed {successful} files";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                }

                if (successful > 0)
                {
                    // Clear and reset
                    lstPreview.Items.Clear();
                    lstPreview.Items.Add("Rename complete! Select another file to continue...");
                    txtOriginalName.Text = "";
                    txtNewName.Text = "";
                    selectedFilePath = "";
                    renameOperations.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error during rename:\n{ex.Message}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Critical error occurred";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
            }
            finally
            {
                btnRename.Enabled = false;
                btnRename.Text = "Apply Rename";
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(currentTheme))
            {
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    currentTheme = settingsForm.SelectedTheme;
                    ApplyTheme(currentTheme);
                }
                // else: Cancel, do nothing
            }
        }

        private void ApplyTheme(ThemeMode mode)
        {
            if (mode == ThemeMode.System)
            {
                ApplySystemTheme();
                return;
            }

            if (mode == ThemeMode.Dark)
            {
                this.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
                foreach (Control ctrl in this.Controls)
                {
                    ctrl.ForeColor = System.Drawing.Color.White;
                    ctrl.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
                }
            }
            else // Light
            {
                this.BackColor = System.Drawing.SystemColors.Control;
                foreach (Control ctrl in this.Controls)
                {
                    ctrl.ForeColor = System.Drawing.Color.Black;
                    ctrl.BackColor = System.Drawing.SystemColors.Control;
                }
            }
        }

        private void ApplySystemTheme()
        {
            try
            {
                var theme = Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme", 1);

                if (theme is int value && value == 0)
                {
                    // Dark mode
                    this.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
                    foreach (Control ctrl in this.Controls)
                    {
                        ctrl.ForeColor = System.Drawing.Color.White;
                        ctrl.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
                    }
                }
                else
                {
                    // Light mode
                    this.BackColor = System.Drawing.SystemColors.Control;
                    foreach (Control ctrl in this.Controls)
                    {
                        ctrl.ForeColor = System.Drawing.Color.Black;
                        ctrl.BackColor = System.Drawing.SystemColors.Control;
                    }
                }
            }
            catch
            {
                ApplyTheme(ThemeMode.Light); // Fallback to light if registry access fails
            }
        }

        // Event handlers for Folder Mode buttons
        private void BtnFolderModeToggle_Click(object sender, EventArgs e)
        {
            isSingleFileMode = !isSingleFileMode;
            var btn = (Button)sender;
            
            // Clear any existing operations when switching modes
            renameOperations.Clear();
            selectedFilePath = "";
            
            if (isSingleFileMode)
            {
                lblStatus.Text = "Single Files Mode enabled.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                toolTip.SetToolTip(btn, "Switch to Folder Mode");
                btnSelectFile.Visible = true;
                btnSelectFolder.Visible = false;
                btnRename.Enabled = false;
            }
            else
            {
                lblStatus.Text = "Folder Mode enabled - Select a folder to begin.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                toolTip.SetToolTip(btn, "Switch to Single Files Mode");
                btnSelectFile.Visible = false;
                btnSelectFolder.Visible = true;
                btnSelectFolder.Location = btnSelectFile.Location;
                btnRename.Enabled = false;
            }
            

        }

        // Aggressive mode tooltip handler
        private void AggressiveModeToggle(object sender, EventArgs e)
        {
            if (aggressiveMode.Checked)
            {
                toolTip.SetToolTip(aggressiveMode, "Currently ON - Matches files of all types\nUncheck to match only same extension");
                aggressiveModeLabel.Text = aggressiveModeLabel.Text.TrimEnd() + "Aggressive Mode: ON";
                aggressiveModeLabel.ForeColor = System.Drawing.Color.FromArgb(255, 70, 0);
            }
            else
            {
                toolTip.SetToolTip(aggressiveMode, "Currently OFF - Only matches files with same extension\nCheck to match all file types");
                aggressiveModeLabel.Text = aggressiveModeLabel.Text.Replace("Aggressive Mode ON", "");
                aggressiveModeLabel.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
            }
        }

        private void DisplayFolderBatchPreview(string folderPath)
        {
            lstPreview.Items.Clear();
            renameOperations.Clear();

            try
            {
                var allFiles = Directory.GetFiles(folderPath);
                var fileGroups = new Dictionary<string, List<string>>();

                // Group files by predicted name
                foreach (var file in allFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string predicted = PredictCleanName(fileName);

                    if (!fileGroups.ContainsKey(predicted))
                        fileGroups[predicted] = new List<string>();

                    fileGroups[predicted].Add(file);
                }

                // Generate rename operations for groups with multiple files
                int totalOperations = 0;
                foreach (var group in fileGroups.Where(g => g.Value.Count > 1))
                {
                    string predictedBaseName = group.Key;
                    var filesInGroup = group.Value.OrderBy(f => Path.GetFileName(f)).ToList();
                    
                    int counter = 1;
                    foreach (var file in filesInGroup)
                    {
                        string extension = Path.GetExtension(file);
                        string newName = $"{predictedBaseName} - {counter:D2}";
                        string newFileName = SanitizeFilename(newName) + extension;

                        renameOperations.Add(new FileRenameOperation
                        {
                            OriginalPath = file,
                            OriginalName = Path.GetFileName(file),
                            NewName = newFileName,
                            NewPath = Path.Combine(folderPath, newFileName)
                        });

                        counter++;
                        totalOperations++;
                    }
                }

                // Ensure unique filenames across all operations
                EnsureUniqueFilenames(renameOperations);

                // Display preview
                if (renameOperations.Count == 0)
                {
                    lstPreview.Items.Add("No batch rename groups found in this folder.");
                    lstPreview.Items.Add("");
                    lstPreview.Items.Add("Folder Mode looks for files that can be grouped together");
                    lstPreview.Items.Add("based on similar naming patterns and renames them with");
                    lstPreview.Items.Add("sequential numbering.");
                    lstPreview.Items.Add("");
                    lstPreview.Items.Add("Try using Single Files Mode with 'Add All' for randomly named single files.");
                    
                    btnRename.Enabled = false;
                    lblStatus.Text = "No renameable groups found in folder";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(255, 140, 0);
                }
                else
                {
                    lstPreview.Items.Add($"Folder Mode Preview: {renameOperations.Count} files in {fileGroups.Count(g => g.Value.Count > 1)} groups");
                    lstPreview.Items.Add("".PadRight(80, '─'));

                    var groupedOperations = renameOperations
                        .GroupBy(op => PredictCleanName(Path.GetFileNameWithoutExtension(op.OriginalName)))
                        .ToList();

                    foreach (var group in groupedOperations)
                    {
                        lstPreview.Items.Add($"Group: {group.Key} ({group.Count()} files)");
                        
                        foreach (var operation in group.Take(3)) // Show first 3 files in each group
                        {
                            lstPreview.Items.Add($"  → {operation.OriginalName}");
                            lstPreview.Items.Add($"    → {operation.NewName}");
                        }
                        
                        if (group.Count() > 3)
                        {
                            lstPreview.Items.Add($"    ... and {group.Count() - 3} more files");
                        }
                        
                        lstPreview.Items.Add("");
                    }

                    btnRename.Enabled = true;
                    lblStatus.Text = $"Ready to rename {renameOperations.Count} files in {groupedOperations.Count} groups";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(46, 125, 50);
                }
            }
            catch (Exception ex)
            {
                lstPreview.Items.Clear();
                lstPreview.Items.Add($"Error scanning folder: {ex.Message}");
                btnRename.Enabled = false;
                lblStatus.Text = "Error occurred while scanning folder";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
            }
        }
    }

    public class FileRenameOperation
    {
        public string OriginalPath { get; set; }
        public string OriginalName { get; set; }
        public string NewName { get; set; }
        public string NewPath { get; set; }
    }
}
