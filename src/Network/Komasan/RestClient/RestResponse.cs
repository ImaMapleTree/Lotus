using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Lotus.Logging;

namespace Lotus.Network.Komasan.RestClient;

public class RestResponse<T>
{
    public HttpHeaders Headers { get; }
    public HttpStatusCode Status { get; }
    public HttpContent Content { get; }

    internal RestResponse(HttpHeaders headers, HttpStatusCode status, HttpContent content)
    {
        Headers = headers;
        Status = status;
        Content = content;
        DevLogger.Log($"Response {status}");
    }

    public T GetBody()
    {
        return JsonSerializer.Deserialize<T>(GetRawBody())!;
    }

    public string GetRawBody()
    {
        StreamReader reader = new(Content.ReadAsStream());
        string content = reader.ReadToEnd();
        reader.Close();
        return content;
    }

    public bool IsOK()
    {
        return (int)Status >= 200 && (int)Status <= 299;
    }

    public bool IsError()
    {
        return (int)Status >= 300;
    }

    // ReSharper disable once InconsistentNaming
    public bool Is4xx5xxError()
    {
        return (int)Status >= 400;
    }
}