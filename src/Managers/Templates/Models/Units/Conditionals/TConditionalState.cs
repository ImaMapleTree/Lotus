using System;
using System.Collections;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using VentLib.Logging;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalState: CommonConditionalUnit
{
    private readonly HashSet<State> states = new();

    public TConditionalState(object input) : base(input)
    {
        if (Input is ICollection collection)
            foreach (object o in collection)
                if (!Enum.TryParse(o as string, true, out State state)) VentLogger.Warn($"Could not parse \"{o}\" as type \"State\"");
                else states.Add(state);
        else if (!Enum.TryParse(input as string, true, out State state)) VentLogger.Warn($"Could not parse \"{input}\" as type \"State\"");
        else states.Add(state);
    }

    public override bool Evaluate(object? _)
    {
        foreach (State state in states)
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (state)
            {
                case State.Lobby when Game.State is GameState.InLobby:
                    return true;
                case State.Game when Game.State is GameState.Roaming:
                    return true;
                case State.Meeting when Game.State is GameState.InMeeting:
                    return true;
            }

        return false;
    }

    private enum State
    {
        Lobby,
        Game,
        Meeting
    }
}