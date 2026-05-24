using System.Windows;

namespace IDEPython
{
    public partial class SaveChangesDialog : Window
    {
        public enum DialogResultOption { Save, DontSave, Cancel }
        public DialogResultOption Result { get; private set; } = DialogResultOption.Cancel;

        public SaveChangesDialog(string fileName)
        {
            InitializeComponent();
            txtFileName.Text = fileName;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultOption.Save;
            this.DialogResult = true;
            this.Close();
        }

        private void btnDontSave_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultOption.DontSave;
            this.DialogResult = false;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultOption.Cancel;
            this.DialogResult = null;
            this.Close();
        }
    }
}
