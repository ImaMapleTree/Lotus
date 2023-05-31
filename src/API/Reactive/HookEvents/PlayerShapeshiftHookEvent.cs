namespace Lotus.API.Reactive.HookEvents;

public class PlayerShapeshiftHookEvent: IHookEvent
{
    public PlayerControl Player;
    public GameData.PlayerInfo Target;
    public bool Reverted;

    public PlayerShapeshiftHookEvent(PlayerControl player, GameData.PlayerInfo target, bool reverted)
    {
        Player = player;
        Target = target;
        Reverted = reverted;
    }
}