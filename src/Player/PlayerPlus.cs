using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.GUI.Name.Interfaces;
using TOHTOR.Roles;

namespace TOHTOR.Player;

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