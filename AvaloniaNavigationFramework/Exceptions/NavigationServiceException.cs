namespace NavTest.Exceptions
{
    public class NavigationServiceException : Exception
    {
        public NavigationServiceException(string message) : base(message) { }
        public NavigationServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}