using IDEPython.Modelo;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDEPython
{
    public partial class IDE : Window
    {
        private bool allowClose = false;
        User user;
        Boolean running;
        String script;
        String projectName;
        String consoleOutput;
        string currentProjectPath;
        string currentFilePath;

        public IDE(User user, int n=-1)
        {

            InitializeComponent();
            txtEditor.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(txtEditor_ScrollChanged));

            ActualizarNumerosLinea();

            this.running = false;
            btnStop.IsEnabled = false;
            txtConsole.Visibility = Visibility.Collapsed;
            txtConsoleSeparator.Visibility = Visibility.Collapsed;
            //Recibir como parametro o asignar nombre predeterminado si es nuevo
            this.projectName = "Assignment #1";
            lblProjectName.Content = this.projectName;
            this.user = user;
        }

        private bool FindAndSelectNode(TreeViewItem parent, string path)
        {
            TreeView tvFiles = (TreeView)this.FindName("tvFiles");
            tvFiles.Foreground = Brushes.White;

            if (parent.Tag is string t && string.Equals(t, path, StringComparison.OrdinalIgnoreCase))
            {
                parent.IsSelected = true;
                parent.BringIntoView();
                return true;
            }

            foreach (var item in parent.Items)
            {
                if (item is TreeViewItem node)
                {
                    if (FindAndSelectNode(node, path)) 
                        return true;
                }
            }

            return false;
        }

        // Constructor with project path parameter
        public IDE(User user, string projectPath) : this(user, -1)
        {
            if (!string.IsNullOrWhiteSpace(projectPath) && Directory.Exists(projectPath))
            {
                LoadProject(projectPath);
            }
            else
            {
                // If null, open Projects folder by default
                var projectsRoot = Path.Combine(AppContext.BaseDirectory, "Projects");
                Directory.CreateDirectory(projectsRoot);
                LoadProject(projectsRoot);
            }
        }


        private void txtEditor_Click(object sender, RoutedEventArgs e)
        {
            if (txtEditor.Text == "Write your code here...")
        private void LoadProject(string projectPath)
        {
            try
            {
                this.currentProjectPath = projectPath;
                this.projectName = Path.GetFileName(projectPath.TrimEnd(Path.DirectorySeparatorChar));
                lblProjectName.Content = this.projectName;

                // Get .py files
                var files = Directory.GetFiles(projectPath, "*.py", SearchOption.AllDirectories);
                // Construir TreeViewItems por estructura de carpetas
                tvFiles.Items.Clear();
                var rootNode = new TreeViewItem() { Header = Path.GetFileName(projectPath), Tag = projectPath, IsExpanded = true };
                BuildTree(rootNode, projectPath);
                tvFiles.Items.Add(rootNode);

                // Clear current file and show placeholder
                currentFilePath = null;
                txtEditor.Text = "Select a file to edit";
                txtLineNumbers.Text = "1";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading project: " + ex.Message);
            }
        }

        private void tvFiles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = tvFiles.SelectedItem as TreeViewItem;
            if (node != null && node.Tag is string path && File.Exists(path))
            {
                try
                {
                    currentFilePath = path;
                    txtEditor.Text = File.ReadAllText(path);
                    ActualizarNumerosLinea();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening file: " + ex.Message);
                }
            }
        }

        private void BuildTree(TreeViewItem parent, string folder)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(folder))
                {
                    var dirNode = new TreeViewItem() { Header = Path.GetFileName(dir), Tag = dir };
                    parent.Items.Add(dirNode);
                    BuildTree(dirNode, dir);
                }

                foreach (var file in Directory.GetFiles(folder, "*.py"))
                {
                    var fileNode = new TreeViewItem() { Header = Path.GetFileName(file), Tag = file };
                    parent.Items.Add(fileNode);
                }
            }
            catch { }
        }
        }
        }
        }
            {
                txtEditor.Text = "";
            }
        }

        private void txtEditor_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();


        }

        private void txtEditor_Copying(object sender, DataObjectCopyingEventArgs e)
        {
            e.CancelCommand();
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Clear();
            txtConsole.Foreground = Brushes.Lime;
            lblProjectName.Content = this.projectName + " - Running";
            btnRun.IsEnabled = false;
            btnStop.IsEnabled = true;

            txtConsole.Visibility = Visibility.Visible;
            txtConsoleSeparator.Visibility = Visibility.Visible;

            string code = txtEditor.Text;

            await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo start = new ProcessStartInfo
                    {
                        FileName = "python.exe",
                        Arguments = $"-u -c \"{code.Replace("\"", "\\\"")}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(start))
                    {

                        process.OutputDataReceived += (s, args) =>
                            Dispatcher.Invoke(() => txtConsole.AppendText(args.Data + Environment.NewLine));

                        process.ErrorDataReceived += (s, args) =>
                            Dispatcher.Invoke(() => {
                                txtConsole.Foreground = Brushes.White;
                                txtConsole.AppendText(args.Data + Environment.NewLine);
                            });

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => txtConsole.AppendText("Error de ejecución: " + ex.Message));
                }
            });

            lblProjectName.Content = this.projectName;
            btnStop.IsEnabled = false;
            btnRun.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.running = false;
            btnRun.IsEnabled = true;
            btnStop.IsEnabled = false;
            lblProjectName.Content = this.projectName;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                //Implementar guardado del archivo
                e.Handled = true;

                lblProjectName.Content = this.projectName + " - Saved";
                
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
            {
                e.Handled = true;
                btnRun_Click(sender, e);
            }

            if (e.Key == Key.F5)
            {
                e.Handled = true;
                btnRun_Click(sender, e);
            }
        }

        private void txtEditor_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                txtLineNumbers.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }

 
        private void ActualizarNumerosLinea()
        {
            int lineCount = txtEditor.LineCount;
            string lines = "";
            for (int i = 1; i <= lineCount; i++)
            {
                lines += i + Environment.NewLine;
            }
            txtLineNumbers.Text = lines;
        }

        private void txtEditor_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            ActualizarNumerosLinea();
        }

        private void btnReturn_Cick(object sender, RoutedEventArgs e)
        {
            VistaCursos cursos = new VistaCursos(this.user);
            cursos.Show();
            // Allow closing only via the return button
            this.allowClose = true;
            this.Close();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!allowClose)
            {
                // Prevent closing/minimizing via system commands
                e.Cancel = true;
            }
        }
    }
}
