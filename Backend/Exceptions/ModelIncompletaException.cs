namespace Backend.Exceptions
{
    public class ModelIncompletaException : Exception
    {
        public ModelIncompletaException(string message)
        : base(message) { }

        public ModelIncompletaException(string message, Exception inner)
            : base(message, inner) { }
    }
}
