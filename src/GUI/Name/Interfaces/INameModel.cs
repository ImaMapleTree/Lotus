using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API;

namespace Lotus.GUI.Name.Interfaces;

public interface INameModel
{
    public string Unaltered();

    public PlayerControl MyPlayer();

    public void Render(List<PlayerControl> players) => players.ForEach(p => this.RenderFor(p));

    public string Render(GameState? state = null, bool sendToPlayer = true, bool force = false);

    public string RenderFor(PlayerControl player, GameState? state = null, bool sendToPlayer = true, bool force = false);

    public List<IComponentHolder> ComponentHolders();

    public T GetComponentHolder<T>() where T : IComponentHolder;

    // ReSharper disable once InconsistentNaming
    public T GCH<T>() where T : IComponentHolder;
}