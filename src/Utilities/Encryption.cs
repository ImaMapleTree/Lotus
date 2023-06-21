using System;
using System.IO;
using Lotus.Extensions;
using Lotus.Managers;
using VentLib.Utilities.Extensions;

namespace Lotus.Utilities;

public class Encryption
{
    private const string EncryptionFile = "_encrypt.id";

    public static ulong UniqueKey
    {
        get
        {
            if (_uniqueKey != 0) return _uniqueKey;
            return _uniqueKey = CreateOrGetEncryptKey();
        }
    }

    private static ulong _uniqueKey;

    public static ulong CreateOrGetEncryptKey()
    {
        FileInfo encryptFile = PluginDataManager.HiddenDataDirectory.GetFile(EncryptionFile);
        if (encryptFile.Exists)
        {
            string content;
            using (StreamReader reader = new(encryptFile.Open(FileMode.Open))) content = reader.ReadToEnd();
            if (ulong.TryParse(content, out ulong key)) return key;
        }

        ulong newKey = DateTime.Now.ToUniversalTime().SemiConsistentHash();
        StreamWriter stream = new(encryptFile.Open(FileMode.Create));
        stream.Write(newKey);
        stream.Close();
        return newKey;
    }

    public static ulong Encrypt(ulong input) => unchecked(input + UniqueKey);

    public static long Encrypt(long input) => unchecked(input + ((long)UniqueKey + long.MinValue));

    public static ulong Decrypt(ulong input) => unchecked(input - UniqueKey);

    public static long Decrypt(long input) => unchecked(input - ((long)UniqueKey + long.MinValue));
}