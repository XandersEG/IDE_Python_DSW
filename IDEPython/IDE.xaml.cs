using IDEPython.Modelo;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDEPython
{
    public partial class IDE : Window
    {
        private bool allowClose = false;
        private Process currentPythonProcess;
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
            btnStop.Visibility = Visibility.Hidden;
        }

        // Using Italic for unsaved files
        private void SetSelectedNodeItalic(bool isItalic)
        {
            if (tvFiles.SelectedItem is TreeViewItem selectedItem)
            {
                selectedItem.FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal;
            }
        }

        private void tvFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Start rename on second click if a file/folder is selected
            if (tvFiles.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                StartRename(item, path);
            }
        }

        private void tvFiles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Select the item under right-click so context actions operate on it
            var element = e.OriginalSource as DependencyObject;
            var item = FindAncestor<TreeViewItem>(element);
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
                e.Handled = true;
            }
        }

        private void tvFiles_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (tvFiles.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                ContextMenu cm = new ContextMenu();

                var miRename = new MenuItem { Header = "Renombrar" };
                miRename.Click += (s, ev) => StartRename(item, path);

                var miNewFile = new MenuItem { Header = "Nuevo archivo" };
                miNewFile.Click += (s, ev) => ctxNewFile_Click(s, ev);

                var miNewFolder = new MenuItem { Header = "Nueva carpeta" };
                miNewFolder.Click += (s, ev) => ctxNewFolder_Click(s, ev);

                var sep = new Separator();

                var miDelete = new MenuItem { Header = "Eliminar" };
                miDelete.Click += (s, ev) => ctxDelete_Click(s, ev);

                cm.Items.Add(miRename);
                cm.Items.Add(miNewFile);
                cm.Items.Add(miNewFolder);
                cm.Items.Add(sep);
                cm.Items.Add(miDelete);

                item.ContextMenu = cm;
            }
            else
            {
                e.Handled = true; // prevent empty context menu
            }
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
                    {
                        // Make sure the ancestor nodes are expanded so the selected element is visible in the tree.
                        parent.IsExpanded = true;
                        return true;
                }
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
            // Prevent editing placeholder
            if (txtEditor.Text == "Puedes escribir código de prueba aquí..")
            {
                // Delete placeholder
                txtEditor.Text = "";
            }
            e.Handled = true;
            return;
        }

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
                txtEditor.Text = "Puedes escribir código de prueba aquí..";
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
                    lblProjectName.Content = $"{this.projectName} - {Path.GetFileName(path)}";
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

        // Drag & Drop and inline rename support
        private Point _startPoint;

        private void tvFiles_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void tvFiles_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                TreeView tree = sender as TreeView;
                if (tree.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is string path)
                {
                    DataObject dragData = new DataObject("FilePath", path);
                    DragDrop.DoDragDrop(tree, dragData, DragDropEffects.Move);
                }
            }
        }

        private void tvFiles_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent("FilePath"))
            {
                var pos = e.GetPosition(tvFiles);
                var element = tvFiles.InputHitTest(pos) as UIElement;
                var item = FindAncestor<TreeViewItem>(element);
                if (item != null && item.Tag is string tag && Directory.Exists(tag))
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
            e.Handled = true;
        }

        private void tvFiles_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("FilePath")) return;

            string sourcePath = e.Data.GetData("FilePath") as string;
            var pos = e.GetPosition(tvFiles);
            var element = tvFiles.InputHitTest(pos) as UIElement;
            var targetItem = FindAncestor<TreeViewItem>(element);

            string targetFolder = currentProjectPath;
            if (targetItem != null && targetItem.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag);
            }

            try
            {
                if (File.Exists(sourcePath))
                {
                    string destPath = Path.Combine(targetFolder, Path.GetFileName(sourcePath));
                    if (!destPath.Equals(sourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(sourcePath, destPath);
                    }
                }
                else if (Directory.Exists(sourcePath))
                {
                    string destPath = Path.Combine(targetFolder, Path.GetFileName(sourcePath));
                    if (!destPath.Equals(sourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Move(sourcePath, destPath);
                    }
                }

                // Reload project and select moved node
                LoadProject(currentProjectPath);
                if (tvFiles.Items.Count > 0)
                {
                    var root = tvFiles.Items[0] as TreeViewItem;
                    FindAndSelectNode(root, Path.Combine(targetFolder, Path.GetFileName(sourcePath)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error moviendo: " + ex.Message);
            }
        }

        private void tvFiles_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // F2 to rename
            if (e.Key == Key.F2)
            {
                if (tvFiles.SelectedItem is TreeViewItem item && item.Tag is string path)
                {
                    StartRename(item, path);
                }
                e.Handled = true;
            }
        }

        private void StartRename(TreeViewItem item, string path)
        {
            var textBox = new TextBox
            {
                Text = item.Header.ToString(),
                Width = 200
            };

            textBox.LostFocus += (s, e) => FinishRename(item, path, textBox.Text);
            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    FinishRename(item, path, textBox.Text);
                }
                else if (e.Key == Key.Escape)
                {
                    // cancel
                    item.Header = Path.GetFileName(path);
                }
            };

            item.Header = textBox;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void FinishRename(TreeViewItem item, string oldPath, string newName)
        {
            try
            {
                string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                if (File.Exists(oldPath))
                {
                    if (Path.GetExtension(newPath) == string.Empty)
                        newPath += Path.GetExtension(oldPath);
                    File.Move(oldPath, newPath);
                }
                else if (Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                }

                // Reload and select renamed item
                LoadProject(currentProjectPath);
                if (tvFiles.Items.Count > 0)
                {
                    var root = tvFiles.Items[0] as TreeViewItem;
                    FindAndSelectNode(root, newPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error renombrando: " + ex.Message);
                item.Header = Path.GetFileName(oldPath);
            }
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void SaveCurrentFile()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    File.WriteAllText(currentFilePath, txtEditor.Text);

                    if (string.IsNullOrEmpty(currentProjectPath))
                    {
                        currentProjectPath = Path.GetDirectoryName(currentFilePath);
                    }

                    string pathToSelect = currentFilePath;
                    LoadProject(currentProjectPath);
                    if (tvFiles.Items.Count > 0)
                    {
                        var root = tvFiles.Items[0] as TreeViewItem;
                        if (root != null && !string.IsNullOrEmpty(pathToSelect))
                        {
                            FindAndSelectNode(root, pathToSelect);
                        }
                    }
                    lblProjectName.Content = $"{this.projectName} - {Path.GetFileName(pathToSelect)}";
                    SetSelectedNodeItalic(false);

                }
                else if (!string.IsNullOrEmpty(currentProjectPath))
                {
                    int i = 1;
                    string newPath;
                    do
                    {
                        newPath = Path.Combine(currentProjectPath, $"untitled_{i}.py");
                        i++;
                    } while (File.Exists(newPath));

                    File.WriteAllText(newPath, txtEditor.Text);
                    LoadProject(currentProjectPath);
                    if (tvFiles.Items.Count > 0)
                    {
                        var root = tvFiles.Items[0] as TreeViewItem;
                        if (root != null)
                        {
                            FindAndSelectNode(root, newPath);
                        }
                    }
                    lblProjectName.Content = $"{this.projectName} - {Path.GetFileName(newPath)}";
                }
                else
                {
                    MessageBox.Show("No project path available to save the file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message);
            }
        }

        private void btnNewFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentProjectPath))
            {
                MessageBox.Show("No project open.");
                return;
            }
            // Determine target folder: if a folder node selected, use it; if a file selected, use its parent folder
            string targetFolder = currentProjectPath;
            var node = tvFiles.SelectedItem as TreeViewItem;
            if (node != null && node.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag);
            }

            string newPath;
            int i = 1;
            do
            {
                newPath = Path.Combine(targetFolder, $"NewFile_{i}.py");
                i++;
            } while (File.Exists(newPath));

            File.WriteAllText(newPath, "# new file\n");
            LoadProject(currentProjectPath);
            // select and open
            if (tvFiles.Items.Count > 0)
            {
                var root = tvFiles.Items[0] as TreeViewItem;
                FindAndSelectNode(root, newPath);
            }
        }

        private void btnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentProjectPath))
            {
                MessageBox.Show("No project open.");
                return;
            }

            // Determine target folder: if a folder node selected, use it; if a file selected, use its parent folder
            string targetFolder = currentProjectPath;
            var node = tvFiles.SelectedItem as TreeViewItem;
            if (node != null && node.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag);
            }

            string newPath;
            int i = 1;
            do
            {
                newPath = Path.Combine(targetFolder, $"NewFolder_{i}");
                i++;
            } while (Directory.Exists(newPath));

            Directory.CreateDirectory(newPath);
            LoadProject(currentProjectPath);

            // select and open
            if (tvFiles.Items.Count > 0)
            {
                var root = tvFiles.Items[0] as TreeViewItem;
                FindAndSelectNode(root, newPath);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var node = tvFiles.SelectedItem as TreeViewItem;
            if (node == null || node.Tag == null) return;

            var path = node.Tag as string;
            if (string.IsNullOrEmpty(path)) return;
            // If deleting the whole project folder, emphasize this to the user
            string displayName = Path.GetFileName(path);
            string caption = "Confirmar eliminación";
            string message;
            if (string.Equals(path, currentProjectPath, StringComparison.OrdinalIgnoreCase))
            {
                message = $"Vas a eliminar todo el proyecto '{displayName}'. ¿Continuar?";
            }
            else
            {
                message = $"¿Eliminar '{displayName}'?";
            }

            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                if (File.Exists(path)) File.Delete(path);
                else if (Directory.Exists(path)) Directory.Delete(path, true);
                LoadProject(currentProjectPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message);
            }
        }

        // Context menu handlers
        private void ctxRename_Click(object sender, RoutedEventArgs e)
        {
            if (tvFiles.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                StartRename(item, path);
            }
        }

        private void ctxNewFile_Click(object sender, RoutedEventArgs e)
        {
            // Reuse btnNewFile logic but target selected folder
            string targetFolder = currentProjectPath;
            var node = tvFiles.SelectedItem as TreeViewItem;
            if (node != null && node.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag);
            }

            string newPath;
            int i = 1;
            do
            {
                newPath = Path.Combine(targetFolder, $"newfile_{i}.py");
                i++;
            } while (File.Exists(newPath));

            File.WriteAllText(newPath, "# new file\n");
            LoadProject(currentProjectPath);
            if (tvFiles.Items.Count > 0)
            {
                var root = tvFiles.Items[0] as TreeViewItem;
                FindAndSelectNode(root, newPath);
            }
        }

        private void ctxNewFolder_Click(object sender, RoutedEventArgs e)
        {
            string targetFolder = currentProjectPath;
            var node = tvFiles.SelectedItem as TreeViewItem;
            if (node != null && node.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag);
        }

            string newPath;
            int i = 1;
            do
        {
                newPath = Path.Combine(targetFolder, $"NewFolder_{i}");
                i++;
            } while (Directory.Exists(newPath));

            Directory.CreateDirectory(newPath);
            LoadProject(currentProjectPath);
        }

        private void ctxDelete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete_Click(sender, e);
        }
        private void txtEditor_Pasting(object sender, DataObjectPastingEventArgs e)
        {
           //e.CancelCommand();
        }

        private void txtEditor_Copying(object sender, DataObjectCopyingEventArgs e)
        {
            e.CancelCommand();
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Clear();
            txtConsole.Foreground = Brushes.White;
            lblProjectName.Content = this.projectName + " - Running";
            btnRun.IsEnabled = false;
            btnRun.Visibility = Visibility.Hidden;
            btnStop.IsEnabled = true;
            btnStop.Visibility = Visibility.Visible;

            txtConsoleSeparator.Visibility = Visibility.Visible;
            txtConsole.Visibility = Visibility.Visible;

            string code = txtEditor.Text;
            this.running = true;

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

                    currentPythonProcess = Process.Start(start);

                    if (currentPythonProcess != null)
                    {
                        currentPythonProcess.OutputDataReceived += (s, args) =>
                            Dispatcher.Invoke(() => {
                                if (args.Data != null) txtConsole.AppendText(args.Data + Environment.NewLine);
                            });

                        currentPythonProcess.ErrorDataReceived += (s, args) =>
                            Dispatcher.Invoke(() => {
                                if (args.Data != null)
                                {
                                    txtConsole.Foreground = Brushes.Red;
                                    txtConsole.AppendText(args.Data + Environment.NewLine);
                                }
                            });

                        currentPythonProcess.BeginOutputReadLine();
                        currentPythonProcess.BeginErrorReadLine();
                        currentPythonProcess.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => txtConsole.AppendText("Error de ejecución: " + ex.Message));
                }
                finally
                {
                    if (currentPythonProcess != null)
                    {
                        currentPythonProcess.Dispose();
                        currentPythonProcess = null;
                    }
                }
            });

            lblProjectName.Content = this.projectName;
            btnRun.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Hidden;
            btnRun.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.running = false;
            try
            {
                if (currentPythonProcess != null && !currentPythonProcess.HasExited)
                {
                    currentPythonProcess.Kill();

                    txtConsole.Foreground = Brushes.Yellow;
                    txtConsole.AppendText(">>> Ejecución detenida por el usuario." + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo detener el proceso: " + ex.Message);
            }


            btnRun.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Hidden;
            btnRun.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                e.Handled = true;
                SaveCurrentFile();
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

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Q)
            {
                btnReturn_Cick(sender, e);
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
        
        private void btnShowFiles_Click(object sender, RoutedEventArgs e)
        {
            if(spFiles.Visibility == Visibility.Collapsed)
            {
                spFiles.Visibility = Visibility.Visible;
                btnShowFiles.ToolTip = "Hide Files";
        }
            else
            {
                spFiles.Visibility = Visibility.Collapsed;
                btnShowFiles.ToolTip = "Show Files";
        }

        }

        private void txtEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarNumerosLinea();
            if (txtEditor.IsFocused && !string.IsNullOrEmpty(currentFilePath) && txtEditor.Text != "Puedes escribir código de prueba aquí..")
            {
                string shortFileName = Path.GetFileName(currentFilePath);

                lblProjectName.Content = $"{this.projectName} - {shortFileName} - Cambios sin guardar*";
                SetSelectedNodeItalic(true);
            }
            
        }   
    }
}
