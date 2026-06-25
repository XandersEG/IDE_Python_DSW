namespace IDEPython.Decorator
{
    public abstract class ScriptDecorator : IScript
    {
        protected readonly IScript _inner;

        protected ScriptDecorator(IScript inner)
        {
            _inner = inner;
        }

        public virtual string GetContent() => _inner.GetContent();
        public virtual string GetFileName() => _inner.GetFileName();
    }
}
