using System;
using System.Collections;
using System.Collections.Generic;
using Lotus.Extensions;
using Lotus.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalPlayerFlag: CommonConditionalUnit
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TConditionalPlayerFlag));

    private readonly HashSet<PlayerFlag> flags = new();

    public TConditionalPlayerFlag(object input) : base(input)
    {
        if (Input is ICollection collection)
            foreach (object o in collection)
                if (!Enum.TryParse(o as string, true, out PlayerFlag state)) log.Warn($"Could not parse \"{o}\" as type \"{nameof(PlayerFlag)}\"");
                else flags.Add(state);

        else if (!Enum.TryParse(input as string, true, out PlayerFlag state)) log.Warn($"Could not parse \"{input}\" as type \"{nameof(PlayerFlag)}\"");
        else flags.Add(state);
        DevLogger.Log($"Parsed input: {input}");
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyPlayerFlags(player);
    }

    public bool VerifyPlayerFlags(PlayerControl player)
    {
        DevLogger.Log("Evaluating player flags");
        foreach (PlayerFlag flag in flags)
        {
            DevLogger.Log($"Evaluating flag: {flag}");
            switch (flag)
            {
                case PlayerFlag.HasModifier:
                    if (!player.SecondaryRoles().IsEmpty()) return true;
                    break;
                case PlayerFlag.HasNoModifier:
                    if (player.SecondaryRoles().IsEmpty()) return true;
                    break;
                case PlayerFlag.IsModded:
                    if (player.IsModded()) return true;
                    break;
                case PlayerFlag.IsNotModded:
                    if (!player.IsModded()) return true;
                    break;
                default:
                    log.Warn(
                        $"PlayerFlag {flag} is not properly setup and will not result in a proper conditional validation.",
                        "VerifyPlayerFlags");
                    break;
            }
        }

        return false;
    }

    public enum PlayerFlag
    {
        HasModifier,
        HasNoModifier,
        IsModded,
        IsNotModded
    }
}