using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using Lotus.Logging;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Lotus.Network.Komasan.RestClient;

public class RestRequestBuilder
{
    private HttpRequestMessage requestMessage;

    public RestRequestBuilder(HttpMethod method, string url)
    {
        requestMessage = new HttpRequestMessage();
        requestMessage.Method = method;
        DevLogger.Log($"URI: {url}");
        requestMessage.RequestUri = new Uri(url);
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
    }

    public RestRequestBuilder Header(string headerName, string headerValue)
    {
        requestMessage.Headers.Add(headerName ,headerValue);
        return this;
    }

    public RestRequestBuilder AppendPath(string path)
    {
        requestMessage.RequestUri = new Uri(Path.Join(requestMessage.RequestUri!.AbsoluteUri, path));
        return this;
    }

    public RestRequestBuilder Body(Object body)
    {
        requestMessage.Content = JsonContent.Create(body);
        requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        return this;
    }

    public HttpRequestMessage Build()
    {
        return requestMessage;
    }
}