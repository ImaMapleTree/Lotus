using System;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Units.Impl;

public class TConditionalUser: CommonConditionalUnit
{
    private UserType user;

    public TConditionalUser(object input) : base(input)
    {
        if (!Enum.TryParse(input as string, true, out UserType status)) VentLogger.Warn($"Could not parse \"{input}\" as type \"{nameof(UserType)}\"");
        else user = status;
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyUser(player);
    }

    private bool VerifyUser(PlayerControl player)
    {
        return user switch
        {
            UserType.Everyone => true,
            UserType.Host => player.IsHost(),
            UserType.Admins => player.IsHost(), // TODO
            UserType.Triggerer => player.PlayerId == Template.Triggerer || Template.Triggerer == byte.MaxValue,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum UserType
    {
        Everyone,
        Host,
        Admins,
        Triggerer
    }
}