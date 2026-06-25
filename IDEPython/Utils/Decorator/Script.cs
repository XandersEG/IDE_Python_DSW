namespace IDEPython.Utils.Decorator
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
