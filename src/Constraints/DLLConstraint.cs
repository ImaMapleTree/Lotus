using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Lotus.Managers;
using Lotus.Logging;
using VentLib.Logging;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Constraints;

[LoadStatic]
public class DLLConstraint
{
    /*private static string DLLValidation = "http://localhost:25565/validate-dll";*/
    private const string DLLValidation = "http://18.219.112.36:8080/validate-dll";

    private static readonly FileInfo LocalFile;

    private const bool Enabled = false;
    
    static DLLConstraint()
    {
        if (!Enabled) return;
        // First Get Assembly Version.
        
        Assembly sourceAssembly = typeof(DLLConstraint).Assembly;
        FileInfo sourceFile = new(sourceAssembly.Location);

        if (!sourceFile.Exists) throw new ConstraintException("Unable to load Lotus DLL. Could properly not establish the location of the assembly.");

        DateTime oneWeekAfterCreation = sourceFile.LastWriteTime.AddDays(7); //
        // If one week from creation happened before now then throw exception
        if (oneWeekAfterCreation.CompareTo(DateTime.Now) < 0) throw new ConstraintException("Unable to load Lotus DLL. DLL is older than 1 week old.");
        string uniqueId = sourceAssembly.ManifestModule.ModuleVersionId + "-" + sourceAssembly.GetName().Version;

        LocalFile = PluginDataManager.HiddenDataDirectory.GetFile("dll-identifier.key");
        StreamReader reader = new(LocalFile.Open(FileMode.OpenOrCreate));
        string key = reader.ReadToEnd();
        reader.Close();

        if (key == uniqueId)
        {
            VentLogger.Debug("DLLConstraint - Passed Locally", "DLLConstraint");
            return;
        }
        
        SendValidationRequest(uniqueId);
        VentLogger.Debug("DLLConstraint - Passed Server Validation", "DLLConstraint");
    }

    private static void SendValidationRequest(string uniqueId)
    {
        //
        HttpClient httpClient = new();
        HttpRequestMessage requestMessage = new();
        requestMessage.RequestUri = new Uri(DLLValidation);
        requestMessage.Method = HttpMethod.Post;
        requestMessage.Headers.Add("dll-identifier", uniqueId);
        HttpResponseMessage responseMessage;
        
        try
        {
            responseMessage = httpClient.Send(requestMessage);
        }
        catch (HttpRequestException requestException)
        {
            if (requestException.StatusCode == null) throw new ConstraintException("Unable to load Lotus DLL. Status code was null.");
            responseMessage = HandleStatusCode(requestException.StatusCode.Value);
        }

        HandleStatusCode(responseMessage.StatusCode);
        StreamReader contentReader = new(responseMessage.Content.ReadAsStream());
        string body = contentReader.ReadToEnd();
        contentReader.Close();
        
        if (body != uniqueId) throw new ConstraintException("Unable to load Lotus DLL. Returned identifier does not match local identifier.");

        StreamWriter writer = new(LocalFile.Open(FileMode.Create));
        writer.Write(body);
        writer.Close();
    }

    private static HttpResponseMessage HandleStatusCode(HttpStatusCode statusCode)
    {
        switch (statusCode)
        {
            case HttpStatusCode.OK:
                break;
            case HttpStatusCode.PreconditionFailed:
                throw new ConstraintException("Unable to load Lotus DLL. DLL Constraint Error (Limit = 1)");
            case HttpStatusCode.InternalServerError:
                throw new ConstraintException("Unable to load Lotus DLL. Server error.");
            default:
                throw new ConstraintException($"Unable to load Lotus DLL. Unknown error from server (StatusCode = {statusCode}).");
        }

        return null!;
    }
    
}

