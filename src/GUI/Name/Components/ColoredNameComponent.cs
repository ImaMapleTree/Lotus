using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using UnityEngine;

namespace TOHTOR.GUI.Name.Components;

public class ColoredNameComponent : NameComponent
{
    public ColoredNameComponent(PlayerControl player, Color color, GameState[] gameStates, Func<List<PlayerControl>>? viewers = null) : base(new LiveString(player.name, color), gameStates, Impl.ViewMode.Replace, viewers)
    {
    }
    public ColoredNameComponent(PlayerControl player, Color color, GameState gameState, Func<List<PlayerControl>>? viewers = null) : base(new LiveString(player.name, color), new []{gameState}, Impl.ViewMode.Replace, viewers)
    {
    }
    public ColoredNameComponent(PlayerControl player, Color color, GameState[] gameStates, params PlayerControl[] viewers) : base(new LiveString(player.name, color), gameStates, Impl.ViewMode.Replace, viewers)
    {
    }
    public ColoredNameComponent(PlayerControl player, Color color, GameState gameState, params PlayerControl[] viewers) : base(new LiveString(player.name, color), gameState, Impl.ViewMode.Replace, viewers)
    {
    }

}