using Lotus.Network.Komasan.RestClient;

namespace Lotus.Network.Komasan.Exception;

public class ClientStateException: ClientException
{
    public Komajiro.State State { get; }

    public ClientStateException(Komajiro.State clientState, string? message) : base(message)
    {
        State = clientState;
    }

    public ClientStateException(Komajiro.State clientState, string? message, System.Exception? innerException) : base(message, innerException)
    {
        State = clientState;
    }
}