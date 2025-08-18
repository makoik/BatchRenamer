using System;
using System.Windows.Forms;

namespace BatchRenamer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());  // Note: Form1, not MainForm
        }
    }
}