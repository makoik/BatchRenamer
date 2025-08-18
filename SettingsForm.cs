using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace BatchRenamer
{
    public class SettingsForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ThemeMode SelectedTheme { get; private set; }

        public SettingsForm(ThemeMode currentTheme)
        {
            InitializeComponent();

            switch (currentTheme)
            {
                case ThemeMode.Dark: radioDark.Checked = true; break;
                case ThemeMode.Light: radioLight.Checked = true; break;
                default: radioSystem.Checked = true; break;
            }
        }

        private RadioButton radioSystem;
        private RadioButton radioLight;
        private RadioButton radioDark;
        private Button btnOK;
        private Button btnCancel;

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new System.Drawing.Size(300, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var groupTheme = new GroupBox();
            groupTheme.Text = "Theme";
            groupTheme.Location = new System.Drawing.Point(20, 20);
            groupTheme.Size = new System.Drawing.Size(240, 90);

            radioSystem = new RadioButton();
            radioSystem.Text = "System";
            radioSystem.Location = new System.Drawing.Point(10, 20);

            radioLight = new RadioButton();
            radioLight.Text = "Light";
            radioLight.Location = new System.Drawing.Point(10, 40);

            radioDark = new RadioButton();
            radioDark.Text = "Dark";
            radioDark.Location = new System.Drawing.Point(10, 60);

            groupTheme.Controls.Add(radioSystem);
            groupTheme.Controls.Add(radioLight);
            groupTheme.Controls.Add(radioDark);
            this.Controls.Add(groupTheme);

            btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Location = new System.Drawing.Point(60, 160);
            btnOK.Size = new System.Drawing.Size(70, 30);
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(150, 160);
            btnCancel.Size = new System.Drawing.Size(70, 30);
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (radioDark.Checked) SelectedTheme = ThemeMode.Dark;
            else if (radioLight.Checked) SelectedTheme = ThemeMode.Light;
            else SelectedTheme = ThemeMode.System;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}