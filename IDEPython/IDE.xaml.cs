using IDEPython.Modelo;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDEPython
{
    public partial class IDE : Window
    {
        private bool isModified = false;
        private Process? currentPythonProcess;
        private User user;
        private bool running;
        private string projectName;
        private string? currentProjectPath;
        private string? currentFilePath;
        private ApiService api;

        public IDE(User user, ApiService api)
        {
            this.api = api;
            this.user = user;
            this.running = false;
            this.projectName = "Assignment #1";

            InitializeComponent();

            txtEditor.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(txtEditor_ScrollChanged));
            ActualizarNumerosLinea();

            btnStop.IsEnabled = false;
            btnStop.Visibility = Visibility.Hidden;
            txtConsole.Visibility = Visibility.Collapsed;
            txtConsoleSeparator.Visibility = Visibility.Collapsed;
            spConsoleInput.Visibility = Visibility.Collapsed;

            lblProjectName.Content = this.projectName;
            this.Topmost = true;

            List<Assignment> enunciados = new List<Assignment>();

            enunciados.Add(new Assignment
            {
                Id = 101,
                Title = "Tarea 1",
                Deadline = DateTime.Now.AddDays(7)
            });

            enunciados.Add(new Assignment
            {
                Title = "Tareíta 2",
                Description = "Serie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que calcule la serie de Fibonaccerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que caerie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que ca",
                Id = 33
            });
            enunciados.Add(new Assignment
            {
                Title = "Tareíta 2",
                Description = "Serie Fibonacci: \nCree un programa que calcule la serie de Fibonacci.Serie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que calcule la serie de FibonaccSerie Fibonacci: \nCree un programa que calcule la serie de Fibonacc",
                Id = 33
            });

            icTareas.ItemsSource = enunciados;
        }

        // Constructor secundario con ruta de proyecto
        public IDE(User user, string projectPath, ApiService api) : this(user, api)
        {
            if (!string.IsNullOrWhiteSpace(projectPath) && Directory.Exists(projectPath))
            {
                LoadProject(projectPath);
            }
            else
            {
                var projectsRoot = Utils.GetUserProjectsRoot(this.user);
                Directory.CreateDirectory(projectsRoot);
                LoadProject(projectsRoot);
            }
        }

        private void btnConsoleSend_Click(object sender, RoutedEventArgs e) => SendConsoleInput();

        private void txtConsoleInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SendConsoleInput();
            }
        }

        private void SendConsoleInput()
        {
            try
            {
                if (currentPythonProcess != null && !currentPythonProcess.HasExited)
                {
                    string text = txtConsoleInput.Text ?? "";
                    currentPythonProcess.StandardInput.WriteLine(text);
                    txtConsole.AppendText($">>> {text}" + Environment.NewLine);
                    txtConsoleInput.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo enviar la entrada: " + ex.Message);
            }
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
            if (tvFiles.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                if (item.Header is TextBox txt) txt.SelectAll();
                else StartRename(item, path);
            }
        }

        private void tvFiles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
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

                var miDelete = new MenuItem { Header = "Eliminar" };
                miDelete.Click += (s, ev) => ctxDelete_Click(s, ev);

                cm.Items.Add(miRename);
                cm.Items.Add(miNewFile);
                cm.Items.Add(miNewFolder);
                cm.Items.Add(new Separator());
                cm.Items.Add(miDelete);

                item.ContextMenu = cm;
            }
            else
            {
                e.Handled = true;
            }
        }

        private bool FindAndSelectNode(TreeViewItem parent, string path)
        {
            tvFiles.Foreground = Brushes.White;

            if (parent.Tag is string t && string.Equals(t, path, StringComparison.OrdinalIgnoreCase))
            {
                parent.IsSelected = true;
                parent.BringIntoView();
                return true;
            }

            foreach (var item in parent.Items)
            {
                if (item is TreeViewItem node && FindAndSelectNode(node, path))
                {
                    parent.IsExpanded = true;
                    return true;
                }
            }
            return false;
        }

        private void txtEditor_Click(object sender, RoutedEventArgs e)
        {
            if (txtEditor.Text == "Puedes escribir código de prueba aquí..")
            {
                txtEditor.Text = "";
            }
            e.Handled = true;
        }

        private void RefreshTreeView()
        {
            if (string.IsNullOrEmpty(currentProjectPath)) return;
            tvFiles.Items.Clear();
            var rootNode = new TreeViewItem() { Header = Path.GetFileName(currentProjectPath), Tag = currentProjectPath, IsExpanded = true };
            BuildTree(rootNode, currentProjectPath);
            tvFiles.Items.Add(rootNode);
        }

        private void LoadProject(string projectPath)
        {
            try
            {
                this.currentProjectPath = projectPath;
                this.projectName = Path.GetFileName(projectPath.TrimEnd(Path.DirectorySeparatorChar));
                lblProjectName.Content = this.projectName;

                RefreshTreeView();

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
            if (tvFiles.SelectedItem is TreeViewItem node && node.Tag is string path && File.Exists(path))
            {
                try
                {
                    currentFilePath = path;
                    txtEditor.Text = File.ReadAllText(path);
                    ActualizarNumerosLinea();
                    lblProjectName.Content = $"{this.projectName} - {Path.GetFileName(path)}";
                    isModified = false;
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

        // Drag & Drop
        private Point _startPoint;

        private void tvFiles_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void tvFiles_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
               (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (sender is TreeView tree && tree.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is string path)
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
            if (!e.Data.GetDataPresent("FilePath") || string.IsNullOrEmpty(currentProjectPath)) return;

            string? sourcePath = e.Data.GetData("FilePath") as string;
            if (string.IsNullOrEmpty(sourcePath)) return;

            var pos = e.GetPosition(tvFiles);
            var element = tvFiles.InputHitTest(pos) as UIElement;
            var targetItem = FindAncestor<TreeViewItem>(element);

            string targetFolder = currentProjectPath;
            if (targetItem != null && targetItem.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag) ?? currentProjectPath;
            }

            try
            {
                string destPath = Path.Combine(targetFolder, Path.GetFileName(sourcePath));
                if (!destPath.Equals(sourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(sourcePath)) File.Move(sourcePath, destPath);
                    else if (Directory.Exists(sourcePath)) Directory.Move(sourcePath, destPath);

                    LoadProject(currentProjectPath);
                    if (tvFiles.Items.Count > 0 && tvFiles.Items[0] is TreeViewItem root)
                    {
                        FindAndSelectNode(root, destPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error moviendo: " + ex.Message);
            }
        }

        private void tvFiles_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && tvFiles.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                StartRename(item, path);
                e.Handled = true;
            }
        }

        private void StartRename(TreeViewItem item, string path)
        {
            var textBox = new TextBox
            {
                Text = item.Header is TextBox t ? t.Text : item.Header.ToString(),
                Width = 200,
                IsEnabled = true
            };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) FinishRename(item, path, textBox.Text);
                else if (e.Key == Key.Escape)
                {
                    item.Header = Path.GetFileName(path);
                    textBox.IsEnabled = false;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (textBox.IsEnabled) FinishRename(item, path, textBox.Text);
            };

            item.Header = textBox;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void FinishRename(TreeViewItem item, string oldPath, string newName)
        {
            if (string.IsNullOrEmpty(currentProjectPath)) return;
            try
            {
                string newPath = Path.Combine(Path.GetDirectoryName(oldPath) ?? "", newName);
                if (File.Exists(oldPath))
                {
                    if (!Path.GetExtension(newPath).Equals(Path.GetExtension(oldPath), StringComparison.OrdinalIgnoreCase))
                        newPath = Path.ChangeExtension(newPath, Path.GetExtension(oldPath));

                    File.Move(oldPath, newPath);
                }
                else if (Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                }

                if (oldPath.Equals(currentProjectPath, StringComparison.OrdinalIgnoreCase)) currentProjectPath = newPath;

                LoadProject(currentProjectPath);
                if (tvFiles.Items.Count > 0 && tvFiles.Items[0] is TreeViewItem root)
                {
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
                    if (string.IsNullOrEmpty(currentProjectPath)) currentProjectPath = Path.GetDirectoryName(currentFilePath);
                    lblProjectName.Content = $"{this.projectName} - {Path.GetFileName(currentFilePath)}";
                    SetSelectedNodeItalic(false);
                    isModified = false;
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
                    RefreshTreeView();
                    if (tvFiles.Items.Count > 0 && tvFiles.Items[0] is TreeViewItem root)
                    {
                        FindAndSelectNode(root, newPath);
                    }
                    lblProjectName.Content = $"{this.projectName} - {Path.GetFileName(newPath)}";
                    isModified = false;
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

        private void btnNewFile_Click(object sender, RoutedEventArgs e) => CreateNewFileOrFolder(true);
        private void btnNewFolder_Click(object sender, RoutedEventArgs e) => CreateNewFileOrFolder(false);

        private void CreateNewFileOrFolder(bool isFile)
        {
            if (string.IsNullOrEmpty(currentProjectPath))
            {
                MessageBox.Show("No project open.");
                return;
            }

            string targetFolder = currentProjectPath;
            if (tvFiles.SelectedItem is TreeViewItem node && node.Tag is string tag)
            {
                if (Directory.Exists(tag)) targetFolder = tag;
                else if (File.Exists(tag)) targetFolder = Path.GetDirectoryName(tag) ?? currentProjectPath;
            }

            string newPath;
            int i = 1;
            do
            {
                newPath = Path.Combine(targetFolder, isFile ? $"NewFile_{i}.py" : $"NewFolder_{i}");
                i++;
            } while (isFile ? File.Exists(newPath) : Directory.Exists(newPath));

            if (isFile) File.WriteAllText(newPath, "# new file\n");
            else Directory.CreateDirectory(newPath);

            LoadProject(currentProjectPath);
            if (tvFiles.Items.Count > 0 && tvFiles.Items[0] is TreeViewItem root)
            {
                FindAndSelectNode(root, newPath);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(tvFiles.SelectedItem is TreeViewItem node) || node.Tag == null) return;
            string? path = node.Tag as string;
            if (string.IsNullOrEmpty(path)) return;

            string displayName = Path.GetFileName(path);
            bool projectSelected = string.Equals(path, currentProjectPath, StringComparison.OrdinalIgnoreCase);
            string message = projectSelected ? $"Vas a eliminar todo el proyecto '{displayName}'. ¿Continuar?" : $"¿Eliminar '{displayName}'?";

            var result = MessageBox.Show(message, "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                if (File.Exists(path)) File.Delete(path);
                else if (Directory.Exists(path)) Directory.Delete(path, true);

                if (projectSelected) btnReturn_Cick(sender, e);
                else LoadProject(currentProjectPath ?? "");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message);
            }
        }

        private void ctxRename_Click(object sender, RoutedEventArgs e) => tvFiles_PreviewKeyDown(sender, new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this)!, 0, Key.F2) { RoutedEvent = Keyboard.KeyDownEvent });
        private void ctxNewFile_Click(object sender, RoutedEventArgs e) => btnNewFile_Click(sender, e);
        private void ctxNewFolder_Click(object sender, RoutedEventArgs e) => btnNewFolder_Click(sender, e);
        private void ctxDelete_Click(object sender, RoutedEventArgs e) => btnDelete_Click(sender, e);

        private void txtEditor_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            //e.CancelCommand();
        }

        private void txtEditor_Copying(object sender, DataObjectCopyingEventArgs e)
        {
            //e.CancelCommand();
        }

        // --- EJECUCIÓN DEL SCRIPT DE PYTHON ---
        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            // Auto-guardar antes de correr para evitar desfases de código
            if (!string.IsNullOrEmpty(currentFilePath)) SaveCurrentFile();

            txtConsole.Clear();
            txtConsole.Foreground = Brushes.White;
            lblProjectName.Content = this.projectName + " - Running";
            this.Topmost = false;

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
                        RedirectStandardInput = true,
                        CreateNoWindow = true
                    };

                    currentPythonProcess = Process.Start(start);
                    Dispatcher.Invoke(() => spConsoleInput.Visibility = Visibility.Visible);

                    if (currentPythonProcess != null)
                    {
                        currentPythonProcess.OutputDataReceived += (s, args) =>
                            Dispatcher.Invoke(() => { if (args.Data != null) txtConsole.AppendText(args.Data + Environment.NewLine); });

                        currentPythonProcess.ErrorDataReceived += (s, args) =>
                            Dispatcher.Invoke(() =>
                            {
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
                    Dispatcher.Invoke(() => txtConsole.AppendText("Error de ejecución: " + ex.Message + Environment.NewLine));
                }
                finally
                {
                    if (currentPythonProcess != null)
                    {
                        currentPythonProcess.Dispose();
                        currentPythonProcess = null;
                    }
                    this.running = false;

                    Dispatcher.Invoke(() =>
                    {
                        spConsoleInput.Visibility = Visibility.Collapsed;
                        this.Topmost = true;
                        lblProjectName.Content = this.projectName;
                        btnRun.Visibility = Visibility.Visible;
                        btnStop.Visibility = Visibility.Hidden;
                        btnRun.IsEnabled = true;
                        btnStop.IsEnabled = false;
                    });
                }
            });
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
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                e.Handled = true;
                SaveCurrentFile();
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R) || e.Key == Key.F5)
            {
                e.Handled = true;
                btnRun_Click(sender, e);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Q)
            {
                e.Handled = true;
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
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= lineCount; i++)
            {
                sb.AppendLine(i.ToString());
            }
            txtLineNumbers.Text = sb.ToString();
        }

        private void btnReturn_Cick(object sender, RoutedEventArgs e)
        {

            // If there are unsaved changes, prompt the user before closing
            if (isModified)
            {
                var dlg = new SaveChangesDialog(Path.GetFileName(currentFilePath ?? "Sin nombre: Se creará como Untitled"));
                dlg.Owner = this;
                var dlgRes = dlg.ShowDialog();

                if (dlg.Result == SaveChangesDialog.DialogResultOption.Cancel)
                {
                    return; // User canceled, do not close
                }
                else if (dlg.Result == SaveChangesDialog.DialogResultOption.Save)
                {
                    SaveCurrentFile();
                }
            }
            
            VistaCursos cursos = new VistaCursos(this.user, api);
            cursos.Show();
            this.Close();
        }


        private void btnShowFiles_Click(object sender, RoutedEventArgs e)
        {
            if (spFiles.Visibility == Visibility.Visible)
            {
                spFiles.Visibility = Visibility.Collapsed;
                filesSplitter.Visibility = Visibility.Collapsed;

                colPadding.Width = new GridLength(0);
                colFiles.Width = new GridLength(0);
                colSplitter.Width = new GridLength(0);

                btnShowFiles.ToolTip = "Show Files";
            }
            else
            {
                spFiles.Visibility = Visibility.Visible;
                filesSplitter.Visibility = Visibility.Visible;

                colPadding.Width = new GridLength(20);
                colFiles.Width = new GridLength(258);
                colSplitter.Width = new GridLength(3);

                btnShowFiles.ToolTip = "Hide Files";
            }

            txtEditor.Focus();
        }

        private void txtEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarNumerosLinea();
            // mark as modified when the user edits content (ignore placeholder)            
            isModified = txtEditor.Text != "Puedes escribir código de prueba aquí..";

            if (txtEditor.IsFocused && !string.IsNullOrEmpty(currentFilePath) && isModified)
            {
                string shortFileName = Path.GetFileName(currentFilePath);
                lblProjectName.Content = $"{this.projectName} - {shortFileName} - Cambios sin guardar*";
                SetSelectedNodeItalic(true);
            }
        }

        private void BtnCerrarEnunciado_Click(object sender, RoutedEventArgs e)
        {
            pnlEnunciado.Visibility = Visibility.Collapsed;
            homeWorklist.Visibility = Visibility.Visible;
        }

        private void BtnVerEnunciado_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tarea = btn?.Tag as Assignment;
            if (tarea != null)
            {
                mostrarEnunciado(tarea);
                homeWorklist.Visibility = Visibility.Collapsed;
            }
            else
            {
                homeWorklist.Visibility = Visibility.Collapsed;
                pnlEnunciado.Visibility = Visibility.Visible;
            }
        }

        private void mostrarEnunciado(Assignment tarea)
        {
            if (tarea == null)
            {
                pnlEnunciado.Visibility = Visibility.Collapsed;
                return;
            }

            lblEnunciadoTitulo.Text = string.IsNullOrWhiteSpace(tarea.Title) ? "(Sin título)" : tarea.Title;

            if (tarea.Deadline != DateTime.MinValue)
            {
                lblEnunciadoDeadline.Text = "Entrega: " + tarea.Deadline.ToString("g");
            }
            else
            {
                lblEnunciadoDeadline.Text = string.Empty;
            }

            lblEnunciadoDescripcion.Text = string.IsNullOrWhiteSpace(tarea.Description) ? "(Sin descripción)" : tarea.Description;

            pnlEnunciado.Visibility = Visibility.Visible;
        }
    }
}
