using System;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalUser: CommonConditionalUnit
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TConditionalUser));

    private UserType user;

    public TConditionalUser(object input) : base(input)
    {
        if (!Enum.TryParse(input as string, true, out UserType status)) log.Warn($"Could not parse \"{input}\" as type \"{nameof(UserType)}\"");
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
            UserType.Triggerer => player.PlayerId == TemplateUnit.Triggerer,
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