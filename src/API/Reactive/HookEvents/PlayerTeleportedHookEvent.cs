using UnityEngine;

namespace Lotus.API.Reactive.HookEvents;

public class PlayerTeleportedHookEvent: PlayerHookEvent
{
    public Vector2 OriginalLocation;
    public Vector2 NewLocation;

    public PlayerTeleportedHookEvent(PlayerControl player, Vector2 originalLocation, Vector2 newLocation) : base(player)
    {
        OriginalLocation = originalLocation;
        NewLocation = newLocation;
    }
}