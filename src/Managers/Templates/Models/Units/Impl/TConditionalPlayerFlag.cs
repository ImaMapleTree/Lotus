﻿using System;
using System.Collections;
using System.Collections.Generic;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Units.Impl;

public class TConditionalPlayerFlag: CommonConditionalUnit
{
    private readonly HashSet<PlayerFlag> flags = new();

    public TConditionalPlayerFlag(object input) : base(input)
    {
        if (Input is ICollection collection)
            foreach (object o in collection)
                if (!Enum.TryParse(o as string, true, out PlayerFlag state)) VentLogger.Warn($"Could not parse \"{o}\" as type \"{nameof(PlayerFlag)}\"");
                else flags.Add(state);
        else if (!Enum.TryParse(input as string, true, out PlayerFlag state)) VentLogger.Warn($"Could not parse \"{input}\" as type \"{nameof(PlayerFlag)}\"");
        else flags.Add(state);
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyPlayerFlags(player);
    }

    public bool VerifyPlayerFlags(PlayerControl player)
    {
        foreach (PlayerFlag flag in flags)
            switch (flag)
            {
                case PlayerFlag.HasModifier:
                    if (!player.GetSubroles().IsEmpty()) return true;
                    break;
                case PlayerFlag.HasNoModifier:
                    if (player.GetSubroles().IsEmpty()) return true;
                    break;
                default:
                    VentLogger.Warn($"PlayerFlag {flag} is not properly setup and will not result in a proper conditional validation.", "VerifyPlayerFlags");
                    break;
            }

        return true;
    }

    public enum PlayerFlag
    {
        HasModifier,
        HasNoModifier
    }
}