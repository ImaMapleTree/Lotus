using System;
using System.Text;

namespace Lotus.Network;

/// <summary>
/// Encodes and Decodes strings using Base64 URL encoding.
/// </summary>
public static class Base64StringEncoder
{
    #region Constants

    private const char   Base64Character62        = '+';
    private const char   Base64Character63        = '/';
    private const string Base64DoublePadCharacter = "==";
    private const char   Base64PadCharacter       = '=';
    private const char   Base64UrlCharacter62     = '-';
    private const char   Base64UrlCharacter63     = '_';

    #endregion

    #region Methods

    /// <summary>
    /// Converts the specified Base64 URL encoded string to a UTF8 string.
    /// </summary>
    /// <param name="s">The Base64 URL encoded string to convert</param>
    /// <returns>A UTF8 string</returns>
    public static string Decode(string s)
    {
        return Encoding.UTF8.GetString(DecodeBytes(s));
    }

    /// <summary>
    /// Converts the specified Base64 URL encoded string to a byte array.</summary>
    /// <param name="s">The Base64 URL encoded string to convert</param>
    /// <returns>A byte array</returns>
    public static byte[] DecodeBytes(string s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        // Replace - with +
        s = s.Replace(Base64UrlCharacter62, Base64Character62);

        // Replace _ with /
        s = s.Replace(Base64UrlCharacter63, Base64Character63);

        // Check padding.
        switch (s.Length % 4)
        {
            case 0: // No pad characters.
                break;
            case 2: // Two pad characters.
                s += Base64DoublePadCharacter;
                break;
            case 3: // One pad character.
                s += Base64PadCharacter;
                break;
            default:
                throw new FormatException("Invalid Base64 URL encoding.");
        }

        return Convert.FromBase64String(s);
    }

    /// <summary>
    /// Converts the specified UTF8 string into a Base64 URL encoded string.
    /// </summary>
    /// <param name="s">The UTF8 string to convert</param>
    /// <returns>A Base64 URL encoded string</returns>
    public static string Encode(string s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        return Encode(Encoding.UTF8.GetBytes(s));
    }

    /// <summary>
    /// Converts the specified byte array to a Base64 URL encoded string.
    /// </summary>
    /// <param name="bytes">The byte array to convert</param>
    /// <returns>A Base64 URL encoded string</returns>
    public static string Encode(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));

        string s = Convert.ToBase64String(bytes, 0, bytes.Length);
        s        = s.Split(Base64PadCharacter)[0];                     // Remove trailing padding i.e. = or ==
        s        = s.Replace(Base64Character62, Base64UrlCharacter62); // Replace + with -
        s        = s.Replace(Base64Character63, Base64UrlCharacter63); // Replace / with _

        return s;
    }

    /// <summary>
    /// Converts the specified byte array to a Base64 URL encoded string.
    /// </summary>
    /// <param name="bytes">The byte array to convert</param>
    /// <param name="offset">The byte array offset</param>
    /// <param name="length">The number of elements in the byte array to convert</param>
    /// <returns>A Base64 URL encoded string</returns>
    public static string Encode(byte[] bytes, int offset, int length)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));

        string s = Convert.ToBase64String(bytes, offset, length);
        s        = s.Split(Base64PadCharacter)[0];                     // Remove trailing padding i.e. = or ==
        s        = s.Replace(Base64Character62, Base64UrlCharacter62); // Replace + with -
        s        = s.Replace(Base64Character63, Base64UrlCharacter63); // Replace / with _

        return s;
    }

    #endregion
}