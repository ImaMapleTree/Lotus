namespace Lotus.Network;

public class NetConstants
{
#if !DEBUG
    public const string Host = "http://18.219.112.36:8080/";
#endif
#if DEBUG
    public const string Host = "http://localhost:8080";
#endif

    public const string AuthEndpoint = "/auth/oauth2/appdirect/discord";
    public const string FetchAuthEndpoint = "/auth/oauth2/fetch";
    public const string CreateLobbyEndpoint = "/api/among-us/lobbies";

}