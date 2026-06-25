using System.IO;
using System.Security.Cryptography;
using System.Text;
using IDEPython.Modelo;

namespace IDEPython
{
    public static class Utils
    {
        public static string GetUserProjectsRoot(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            string key = user.Email ?? (user.FirstName + user.LastName1 + user.LastName2);
            string hash = ComputeSha256Hex(key);
            var projectsRoot = Path.Combine(AppContext.BaseDirectory, "Projects", hash);
            Directory.CreateDirectory(projectsRoot);
            return projectsRoot;
        }

        private static string ComputeSha256Hex(string input)
        {
            if (input == null) input = "";
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.AppendFormat("{0:x2}", b);
                return sb.ToString();
            }
        }
    }
}
