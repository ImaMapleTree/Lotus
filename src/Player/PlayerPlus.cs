using Lotus.GUI.Name.Impl;
using Lotus.GUI.Name.Interfaces;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.GUI;

namespace Lotus.Player;

public class PlayerPlus
{
    public PlayerControl MyPlayer;
    public PlayerState State;
    public INameModel NameModel;
    public CustomRole Role;

    public PlayerPlus(PlayerControl player)
    {
        this.MyPlayer = player;
        this.State = PlayerState.Alive;
        this.NameModel = new SimpleNameModel(player);
        this.Role = player.GetCustomRole();
    }
}