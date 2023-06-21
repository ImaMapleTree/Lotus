using System;
using Lotus.Extensions;
using VentLib.Logging;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalVitals: CommonConditionalUnit
{
    private PlayerVital playerVital;

    public TConditionalVitals(object input) : base(input)
    {
        if (!Enum.TryParse(input as string, true, out PlayerVital status)) VentLogger.Warn($"Could not parse \"{input}\" as type \"{nameof(PlayerVital)}\"");
        else playerVital = status;
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyStatus(player);
    }

    private bool VerifyStatus(PlayerControl? player)
    {
        if (playerVital is PlayerVital.Any || player == null) return true;
        return playerVital is PlayerVital.Alive ? player.IsAlive() : !player.IsAlive();
    }

    private enum PlayerVital
    {
        Any,
        Dead,
        Alive
    }
}