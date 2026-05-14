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


        private void txtEditor_Click(object sender, RoutedEventArgs e)
        {
            if (txtEditor.Text == "Write your code here...")
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
