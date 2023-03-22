using System.Collections.Generic;
using TOHTOR.API;

namespace TOHTOR.GUI.Name.Interfaces;

public interface INameModel
{
    public string Unaltered();

    public PlayerControl MyPlayer();

    public void Render(List<PlayerControl> players) => players.ForEach(p => this.RenderFor(p));

    public string Render(GameState? state = null, bool sendToPlayer = true, bool force = false);

    public string RenderFor(PlayerControl player, GameState? state = null, bool sendToPlayer = true, bool force = false);

    public List<IComponentHolder> ComponentHolders();

    public T GetComponentHolder<T>() where T : IComponentHolder;
}