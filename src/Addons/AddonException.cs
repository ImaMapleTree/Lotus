#nullable enable
using System;

namespace Lotus.Addons;

public class AddonException : Exception
{
    public AddonException(string? message, Exception? innerException) : base(message, innerException)
    { }
}