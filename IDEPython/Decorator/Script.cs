using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDEPython.Decorator
{
    public class Script : IScript
    {

        private readonly string _content;
        private readonly string _fileName;

        public Script(string content, string fileName)
        {
            _content = content ?? string.Empty;
            _fileName = fileName ?? "untitled.py";
        }

        public string GetContent() => _content;
        public string GetFileName() => _fileName;

    }
}
