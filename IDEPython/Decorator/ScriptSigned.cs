using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace IDEPython.Decorator
{
    internal class ScriptSigned : ScriptDecorator
    {
        private readonly string _hash;
        private static readonly string CsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".local_scripts", "firmas.csv");

        // El constructor calcula el Hash a partir del contenido del script base
        public ScriptSigned(IScript inner) : base(inner)
        {
            _hash = ComputeSha256(inner.GetContent());
        }

        public string Hash => _hash;

        // REQUERIMIENTO: Inyecta la firma en la primera línea física
        public override string GetContent()
        {
            var sb = new StringBuilder();
            sb.AppendLine(_hash);
            sb.Append(_inner.GetContent());
            return sb.ToString();
        }

        // Valida de manera estricta si el texto crudo del archivo inicia con un Hash de 64 caracteres
        public static bool IsAlreadySigned(string rawDiskContent)
        {
            if (string.IsNullOrWhiteSpace(rawDiskContent)) return false;

            string[] lines = rawDiskContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length > 0)
            {
                string firstLine = lines[0].Trim();
                if (firstLine.Length == 64 && firstLine.All(c => "0123456789abcdefABCDEF".Contains(c)))
                {
                    return true;
                }
            }
            return false;
        }

        // Registra de manera persistente en la bitácora CSV
        public void RegistrarEnCsv(string nombreArchivo)
        {
            string directorio = Path.GetDirectoryName(CsvPath);
            if (!Directory.Exists(directorio)) Directory.CreateDirectory(directorio);

            if (File.Exists(CsvPath) && File.ReadLines(CsvPath).Any(line => line.StartsWith($"{nombreArchivo},{_hash}")))
                return;

            using (var sw = new StreamWriter(CsvPath, true, Encoding.UTF8))
            {
                sw.WriteLine($"{nombreArchivo},{_hash},{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
        }

        public static string ComputeSha256(string input)
        {
            if (string.IsNullOrEmpty(input)) input = string.Empty;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                var hex = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    hex.AppendFormat("{0:x2}", b);
                }
                return hex.ToString();
            }
        }
    }
}