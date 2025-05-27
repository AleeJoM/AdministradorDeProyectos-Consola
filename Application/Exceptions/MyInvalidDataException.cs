namespace Application.Exceptions
{
    public class MyInvalidDataException : Exception
    {
        public MyInvalidDataException(string message) : base(message) { }
    }
}
