namespace Lotus.Network.Komasan.Exception;

public class ClientException: System.Exception
{
    public ClientException()
    {
    }

    public ClientException(string? message) : base(message)
    {
    }

    public ClientException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}