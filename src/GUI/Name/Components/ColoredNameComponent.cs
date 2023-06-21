using System;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using UnityEngine;

namespace Lotus.GUI.Name.Components;

public class ColoredNameComponent : NameComponent
{
    public ColoredNameComponent(PlayerControl player, Color color, GameState[] gameStates, Func<List<PlayerControl>>? viewers = null) : base(new LiveString(player.name, color), gameStates, Name.ViewMode.Replace, viewers)
    {
    }
    public ColoredNameComponent(PlayerControl player, Color color, GameState gameState, Func<List<PlayerControl>>? viewers = null) : base(new LiveString(player.name, color), new []{gameState}, Name.ViewMode.Replace, viewers)
    {
    }
    public ColoredNameComponent(PlayerControl player, Color color, GameState[] gameStates, params PlayerControl[] viewers) : base(new LiveString(player.name, color), gameStates, Name.ViewMode.Replace, viewers)
    {
    }
    public ColoredNameComponent(PlayerControl player, Color color, GameState gameState, params PlayerControl[] viewers) : base(new LiveString(player.name, color), gameState, Name.ViewMode.Replace, viewers)
    {
    }

}