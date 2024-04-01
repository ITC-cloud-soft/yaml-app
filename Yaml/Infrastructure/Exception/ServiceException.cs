namespace Yaml.Infrastructure.Exception;

public class ServiceException : System.Exception
{
    
    public ServiceException() : base()
    {
    }

    public ServiceException(string message)
        : base(message)
    {
    }

    public ServiceException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public ServiceException(string name, object key) : base($"Service Exception  \"{name}\" ({key}) ")
    {
    }
    
}