using IDEPython.Modelo;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
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
        private Process? terminalProcess;
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
            btnStop.Visibility = Visibility.Collapsed;
            txtConsole.Visibility = Visibility.Collapsed;
            txtConsoleSeparator.Visibility = Visibility.Collapsed;
            spConsoleInput.Visibility = Visibility.Collapsed;

            lblProjectName.Content = this.projectName;
            //this.Topmost = true;

            CargarCursosEstudiante();

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

        private async void CargarCursosEstudiante()
        {

            List<Object> courses = new List<Object>
                {
                };

            string answer = await api.GetAsync(
                "/listarCursosEstudiante"
            );

            if (answer == null)
            {
                MessageBox.Show("No se obtuvo respuesta de parte del servidor, intente de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                CoursesResponse? coursesResponse =
                    JsonSerializer.Deserialize<CoursesResponse>(answer);

                if (coursesResponse == null)
                {
                    MessageBox.Show("Respuesta inválida de parte del servidor");
                    return;
                }

                if (coursesResponse.exito)
                {
                    foreach (CourseInfo courseInfo in coursesResponse.cursos)
                    {
                        Course course = new Course
                        {
                            Code = courseInfo.Codigo,
                            Name = courseInfo.Nombre,
                            EmailProfessor = courseInfo.CorreoUsuarioProfesor
                        };
                        courses.Add(course);
                    }

                    

                }
                else
                {
                    MessageBox.Show("Hubo un error al cargar los cursos");
                    return;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                lbCursos.ItemsSource = courses;
            }
        }

        private void BtnEntregarTareaDesdeEnunciado_Click(object sender, RoutedEventArgs e) { }


        private async void CargarTareasPorCurso(Course curso)
        {
            if (curso == null) return;

            try
            {
                // 1. Construir el endpoint utilizando las propiedades exactas de tu modelo
                string endpoint = "/listarTareasEstudiante";
                endpoint += $"?nombreCurso={Uri.EscapeDataString(curso.Name)}";
                endpoint += $"&correoProfesor={Uri.EscapeDataString(curso.EmailProfessor)}";

                // 2. Realizar la petición HTTP a la API
                string answer = await api.GetAsync(endpoint);

                if (answer == null)
                {
                    MessageBox.Show("No se obtuvo respuesta de parte del servidor, intente de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3. Deserializar usando tus clases estructurales: AssignmentsResponse y AssignmentInfo
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                AssignmentsResponse? assignmentsResponse = JsonSerializer.Deserialize<AssignmentsResponse>(answer, options);

                if (assignmentsResponse == null)
                {
                    MessageBox.Show("Respuesta inválida de parte del servidor", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                List<Assignment> assignments = new List<Assignment>();

                // 4. Si la respuesta fue exitosa, mapear al control visual icTareas
                if (assignmentsResponse.exito)
                {
                    foreach (AssignmentInfo assignmentInfo in assignmentsResponse.tareas)
                    {
                        Assignment assignment = new Assignment
                        {
                            Id = int.Parse(assignmentInfo.idEnunciado),
                            Title = assignmentInfo.Titulo,
                            Description = assignmentInfo.Descripcion
                        };
                        assignments.Add(assignment);
                    }

                    // Asignar los datos reales al ItemsControl del bloque derecho del IDE
                    icTareas.ItemsSource = assignments;
                }
                else
                {
                    MessageBox.Show("Hubo un error al cargar las tareas de este curso", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    icTareas.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Excepción al cargar tareas");
            }
        }
        private void LbCursos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbCursos.SelectedItem is Course cursoSeleccionado)
            {
                lblCursoSeleccionadoTitulo.Text = $"Tareas: {cursoSeleccionado.Name}";

                pnlCursosSeccion.Visibility = Visibility.Collapsed;
                pnlTareasSeccion.Visibility = Visibility.Visible;

                CargarTareasPorCurso(cursoSeleccionado);
            }
        }


        private void BtnVolverCursos_Click(object sender, RoutedEventArgs e)
        {
            lbCursos.SelectedIndex = -1;

            pnlTareasSeccion.Visibility = Visibility.Collapsed;
            pnlCursosSeccion.Visibility = Visibility.Visible;
        }

        private void stopActiveProcesses()
        {
            if (currentPythonProcess != null && !currentPythonProcess.HasExited)
            {
                currentPythonProcess.Kill();
                currentPythonProcess.Dispose();
                currentPythonProcess = null;
            }

            if (terminalProcess != null && !terminalProcess.HasExited)
            {
                terminalProcess.Kill();
                terminalProcess.Dispose();
                terminalProcess = null;
            }

            
            Dispatcher.Invoke(() => {
                txtConsole.Clear();
                txtConsole.Foreground = Brushes.White;
            });
        }

        private void openPythonTerminal()
        {
            stopActiveProcesses();

            txtConsole.Visibility = Visibility.Visible;
            txtConsoleSeparator.Visibility = Visibility.Visible;
            spConsoleInput.Visibility = Visibility.Visible;
            txtConsole.Foreground = Brushes.LightGreen;
            txtConsole.AppendText("--- Terminal de Python ---\n");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "-u -i",
                WorkingDirectory = !string.IsNullOrEmpty(currentProjectPath)
                                   ? currentProjectPath
                                   : AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            terminalProcess = new Process { StartInfo = startInfo };

            terminalProcess.OutputDataReceived += (s, args) =>
            {
                if (args.Data != null)
                    Dispatcher.Invoke(() =>
                    {
                        txtConsole.Foreground = Brushes.White;
                        txtConsole.AppendText(args.Data + Environment.NewLine);
                        txtConsole.ScrollToEnd();
                    });
            };

            terminalProcess.ErrorDataReceived += (s, args) =>
            {
                if (args.Data != null)
                    Dispatcher.Invoke(() =>
                    {
                        bool isWelcomePrompt = args.Data.StartsWith(">>>")
                                                || args.Data.StartsWith("...")
                                                || args.Data.StartsWith("Python ")
                                                || args.Data.StartsWith("Type \"");

                        txtConsole.Foreground = isWelcomePrompt ? Brushes.Cyan : Brushes.Red;
                        txtConsole.AppendText(args.Data + Environment.NewLine);
                        txtConsole.ScrollToEnd();
                    });
            };

            terminalProcess.Start();
            terminalProcess.BeginOutputReadLine();
            terminalProcess.BeginErrorReadLine();

            txtConsoleInput.Focus();
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
                string text = txtConsoleInput.Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    txtConsole.Foreground = Brushes.Yellow;
                    txtConsole.AppendText($">>> {text}{Environment.NewLine}");
                    txtConsole.ScrollToEnd();

                    if (currentPythonProcess != null && !currentPythonProcess.HasExited)
                    {
                        currentPythonProcess.StandardInput.WriteLine(text);
                        currentPythonProcess.StandardInput.Flush();
                    }
                    else if (terminalProcess != null && !terminalProcess.HasExited)
                    {
                        terminalProcess.StandardInput.WriteLine(text);
                        terminalProcess.StandardInput.Flush();
                    }
                }
                else
                {
                    if (currentPythonProcess != null && !currentPythonProcess.HasExited)
                    {
                        currentPythonProcess.StandardInput.WriteLine("");
                        currentPythonProcess.StandardInput.Flush();
                    }
                    else if (terminalProcess != null && !terminalProcess.HasExited)
                    {
                        terminalProcess.StandardInput.WriteLine("");
                        terminalProcess.StandardInput.Flush();
                    }
                }
                txtConsoleInput.Clear();
                txtConsoleInput.Focus();
            }
            catch (Exception ex)
            {
                txtConsole.AppendText($"Error: {ex.Message}{Environment.NewLine}");
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
                    string contenidoDisco = File.ReadAllText(path, Encoding.UTF8);
                    string nombreArchivo = Path.GetFileName(path);

                    if (IDEPython.Decorator.ScriptSigned.IsAlreadySigned(contenidoDisco))
                    {
                        string[] lineas = contenidoDisco.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        string codigoLimpioParaUsuario = string.Join(Environment.NewLine, lineas.Skip(1));
                        txtEditor.Text = codigoLimpioParaUsuario;
                    }
                    else
                    {
                        IDEPython.Decorator.IScript scriptBase = new IDEPython.Decorator.Script(contenidoDisco, nombreArchivo);
                        IDEPython.Decorator.ScriptSigned scriptDecorado = new IDEPython.Decorator.ScriptSigned(scriptBase);

                        File.WriteAllText(path, scriptDecorado.GetContent(), Encoding.UTF8);

                        scriptDecorado.RegistrarEnCsv(nombreArchivo);

                        txtEditor.Text = contenidoDisco;
                    }

                    ActualizarNumerosLinea();
                    lblProjectName.Content = this.projectName + " - " + Path.GetFileName(path);
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
                    
                    string nombreArchivo = Path.GetFileName(currentFilePath);
                    string contenidoEditor = txtEditor.Text;

                    IDEPython.Decorator.IScript scriptBase = new IDEPython.Decorator.Script(contenidoEditor, nombreArchivo);
                    IDEPython.Decorator.ScriptSigned scriptDecorado = new IDEPython.Decorator.ScriptSigned(scriptBase);

                    File.WriteAllText(currentFilePath, scriptDecorado.GetContent(), Encoding.UTF8);

                    scriptDecorado.RegistrarEnCsv(nombreArchivo);

                    if (string.IsNullOrEmpty(currentProjectPath))
                        currentProjectPath = Path.GetDirectoryName(currentFilePath);

                    lblProjectName.Content = this.projectName + " - " + Path.GetFileName(currentFilePath);
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
                    string nombreArchivo = Path.GetFileName(newPath);
                    string contenidoEditor = txtEditor.Text;

                    IDEPython.Decorator.IScript scriptBase = new IDEPython.Decorator.Script(contenidoEditor, nombreArchivo);
                    IDEPython.Decorator.ScriptSigned scriptDecorado = new IDEPython.Decorator.ScriptSigned(scriptBase);

                    File.WriteAllText(newPath, scriptDecorado.GetContent(), Encoding.UTF8);

                    scriptDecorado.RegistrarEnCsv(nombreArchivo);

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

                if (projectSelected) btnReturn_Click(sender, e);
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
            e.CancelCommand();
        }

        private void txtEditor_Copying(object sender, DataObjectCopyingEventArgs e)
        {
            e.CancelCommand();
        }

        // --- EJECUCIÓN DEL SCRIPT DE PYTHON ---
        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            // Auto-guardar antes de correr para evitar desfases de código
            if (!string.IsNullOrEmpty(currentFilePath)) SaveCurrentFile();

            stopActiveProcesses();
            lblProjectName.Content = this.projectName + " - Running";
            this.Topmost = false;

            btnRun.IsEnabled = false;
            btnRun.Visibility = Visibility.Hidden;
            btnStop.IsEnabled = true;
            btnStop.Visibility = Visibility.Visible;
            txtConsoleSeparator.Visibility = Visibility.Visible;
            txtConsole.Visibility = Visibility.Visible;
            txtConsole.AppendText($"--- Ejecutando: {Path.GetFileName(currentFilePath)} ---\n");

            string code = txtEditor.Text;
            this.running = true;

            await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo start = new ProcessStartInfo
                    {
                        FileName = "python.exe",
                        Arguments = $"-u \"{currentFilePath}\"", // Se usa el archivo físico para evitar límites de Base64
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(currentFilePath) ?? AppDomain.CurrentDomain.BaseDirectory
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
                        //this.Topmost = true;
                        lblProjectName.Content = this.projectName;
                        btnRun.Visibility = Visibility.Visible;
                        btnStop.Visibility = Visibility.Collapsed;
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
                btnReturn_Click(sender, e);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
            {
                e.Handled = true;
                openPythonTerminal();
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

        private async void btnUploadAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentProjectPath))
            {
                MessageBox.Show("No hay proyecto abierto para subir.");
                return;
            }
            try
            {
                string zipPath = Path.GetTempFileName() + ".zip";

                ZipFile.CreateFromDirectory(currentProjectPath, zipPath);

                MessageBox.Show($"Proyecto comprimido en: {zipPath}");

                byte[] contenido = File.ReadAllBytes(zipPath);

                string fileName = Path.GetFileName(currentProjectPath);

                var datos = new
                {
                    nombreArchivo = fileName,
                    idEnunciado = 2,
                    contenido = Convert.ToBase64String(contenido)
                };

                string respuesta =
                    await api.PostAsync(
                        "/createSubmission",
                        datos
                    );

                MessageBox.Show("Respuesta del servidor: " + respuesta);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer o subir el proyecto: " + ex.Message);
            }
        }
        private void btnReturn_Click(object sender, RoutedEventArgs e)
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

                colFiles.MinWidth = 0;

                btnShowFiles.ToolTip = "Show Files";
            }
            else
            {
                spFiles.Visibility = Visibility.Visible;
                filesSplitter.Visibility = Visibility.Visible;

                colPadding.Width = new GridLength(20);
                colFiles.Width = new GridLength(340);
                colSplitter.Width = new GridLength(3);

                colFiles.MinWidth = 340;

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

        private void btnTerminal_Click(object sender, RoutedEventArgs e)
        {
            openPythonTerminal();
        }

        //Signature

        
    }
}
