using System.Collections.Generic;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Extensions;

namespace Lotus.Roles.Interactions;

public class HelpfulIntent : IHelpfulIntent
{
    public void Action(PlayerControl actor, PlayerControl target)
    {
    }

    public void Halted(PlayerControl actor, PlayerControl target)
    {
        actor.RpcMark(actor);
    }

    private Dictionary<string, object?>? meta;
    public object? this[string key]
    {
        get => (meta ?? new Dictionary<string, object?>()).GetValueOrDefault(key);
        set => (meta ?? new Dictionary<string, object?>())[key] = value;
    }
}