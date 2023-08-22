using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Network.Komasan.DTO;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Network.Komasan.RestClient;

public class Komajiro
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Komajiro));
    public static Komajiro Instance => _instance ??= new Komajiro();
    private static Komajiro? _instance;

    private const string AuthURL = $"{NetConstants.Host}{NetConstants.AuthEndpoint}?ra-token={{0}}";
    private const string FetchAuthURL = $"{NetConstants.Host}{NetConstants.FetchAuthEndpoint}";
    private readonly HttpClient httpClient = new();

    private State clientState = State.Empty;
    private String clientSecret = null!;
    private byte[] clientId;
    private String? authSecret;
    private String raToken = null!;

    private SessionManager sessionManager = new();

    private Komajiro()
    {
        _instance = this;
    }

    public void Initialize(string uniqueClientId, string clientSecret)
    {
        if (clientState is not State.Empty) return;
        Async.ExecuteThreaded(() =>
        {
            this.clientId = Encoding.UTF8.GetBytes(uniqueClientId);
            Array.Resize(ref this.clientId, 16);
            this.clientSecret = clientSecret;
            log.High("Initializing Komajiro");
            clientState = State.Initializing;

            httpClient.DefaultRequestHeaders.Add("ota", Base64StringEncoder.Encode(Encoding.UTF8.GetBytes(this.clientSecret).Concat(clientId).ToArray()));

            string? sessionAuth = sessionManager.LoadSessionToken(this.clientId);
            if (sessionAuth != null)
            {
                DevLogger.Log($"Session Auth: {sessionAuth}");
                bool success = FetchAuthSecret(sessionAuth);
                if (success) return;
            }

            byte[] encodingBytes = clientId.Concat(Encoding.UTF8.GetBytes(clientSecret)).ToArray();
            DevLogger.Log($"Encoding {encodingBytes.Fuse()} for RA-Token");
            raToken = Base64StringEncoder.Encode(encodingBytes);
            MainThreadAnchor.ExecuteOnMainThread(() => Application.OpenURL(AuthURL.Formatted(raToken)));
        });
    }

    public RestResponse<T> Execute<T>(HttpMethod method, string url, Func<RestRequestBuilder, HttpRequestMessage> requestBuilder)
    {
        return Execute<T>(method, url, requestBuilder, false);
    }

    public void ExecuteThreaded<T>(HttpMethod method, string url, Func<RestRequestBuilder, HttpRequestMessage> requestBuilder, Action<RestResponse<T>>? responseConsumer = null)
    {
        Async.ExecuteThreaded(() =>
        {
            RestResponse<T> response = Execute<T>(method, url, requestBuilder, false);
            responseConsumer?.Invoke(response);
        });
    }

    private RestResponse<T> Execute<T>(HttpMethod method, string url, Func<RestRequestBuilder, HttpRequestMessage> requestBuilder, bool ignoreClientState)
    {
        if (!ignoreClientState && clientState is State.Initializing) FetchAuthSecret();
        return ParseMessageResponse<T>(httpClient.Send(requestBuilder(new RestRequestBuilder(method, url))));
    }

    private bool FetchAuthSecret(string? ota = null)
    {
        if (clientState is not State.Initializing) return false;
        clientState = State.Initialized;

        Func<RestRequestBuilder, HttpRequestMessage> requestMessageBuilderFunction = ota == null ? rb => rb.Build() : rb => rb.Header("ota", ota).Build();

        try
        {
            RestResponse<KomasanAuthResponse> restResponse = Execute<KomasanAuthResponse>(HttpMethod.Get, FetchAuthURL, requestMessageBuilderFunction, true);
            if (restResponse.Status is not HttpStatusCode.OK)
            {
                log.Warn($"Failed to authenticate with Komasan. {restResponse.Status}");
                return false;
            }

            KomasanAuthResponse authResponse = restResponse.GetBody();

            httpClient.DefaultRequestHeaders.Add("Cookie", $"JSESSIONID={authSecret = authResponse.AuthSecret}");
            try
            {
                sessionManager.SaveSessionToken(authResponse.AuthSecret, clientId);
            }
            catch (System.Exception exception)
            {
                log.Exception(exception);
            }
            return true;
        }
        catch (System.Exception exception)
        {
            log.Exception(exception);
            clientState = State.Errored;
        }

        return false;
    }

    private static RestResponse<T> ParseMessageResponse<T>(HttpResponseMessage responseMessage)
    {
        return new RestResponse<T>(responseMessage.Headers, responseMessage.StatusCode, responseMessage.Content);
    }

    public enum State
    {
        Empty,
        Initializing,
        Initialized,
        Errored
    }

    private class SessionManager
    {
        private FileInfo sessionFile = PluginDataManager.HiddenDataDirectory.GetFile("session-token.key");

        public string? LoadSessionToken(byte[] clientId)
        {
            if (!sessionFile.Exists) return null;
            try
            {
                byte[] content = System.IO.File.ReadAllBytes(sessionFile.FullName);
                byte[] decodedContent = DecodeAuthToken(content, clientId);
                return Encoding.UTF8.GetString(decodedContent);
            }
            catch (System.Exception exception)
            {
                log.Exception(exception);
                return null;
            }
        }

        public void SaveSessionToken(string authToken, byte[] clientId)
        {
            FileStream fileStream = sessionFile.Open(FileMode.Create);
            try
            {
                EncodeAuthToken(Encoding.UTF8.GetBytes(authToken), clientId).ForEach(fileStream.WriteByte);
            }
            catch (System.Exception exception)
            {
                log.Exception(exception);
            }
            finally
            {
                fileStream.Close();
            }
        }

        private static byte[] EncodeAuthToken(byte[] authToken, byte[] userId)
        {
            int index = 0;
            return authToken.Select(b => (byte)(b + GetByte())).ToArray();

            byte GetByte()
            {
                byte b = userId[index++];
                if (index >= userId.Length) index = 0;
                return b;
            }
        }

        private static byte[] DecodeAuthToken(byte[] token, byte[] userId)
        {
            int index = userId.Length - 1;

            return token.Reverse().Select(b => (byte)(b - GetByte())).Reverse().ToArray();

            byte GetByte()
            {
                byte b = userId[index--];
                if (index < 0) index = userId.Length - 1;
                return b;
            }
        }
    }
}