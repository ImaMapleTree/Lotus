using Lotus.API.Vanilla.Sabotages;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Reactive.HookEvents;

public class SabotageFixHookEvent : SabotageHookEvent
{
    public Optional<PlayerControl> Fixer;

    public SabotageFixHookEvent(PlayerControl? fixer, ISabotage sabotage) : base(sabotage)
    {
        Fixer = fixer == null ? Optional<PlayerControl>.Null() : Optional<PlayerControl>.Of(fixer);
    }
}