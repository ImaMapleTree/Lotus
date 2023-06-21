using System.Collections.Generic;
using Lotus.API.Odyssey;

namespace Lotus.GUI.Name.Interfaces;

public interface INameModel
{
    public bool Updated();

    public void Render(List<PlayerControl> players) => players.ForEach(p => this.RenderFor(p));

    public string Render(GameState? state = null, bool sendToPlayer = true, bool force = false);

    public string RenderFor(PlayerControl player, GameState? state = null, bool sendToPlayer = true, bool force = false);

    public void RenderForAll(GameState? state = null, bool sendToPlayer = true, bool force = false);

    public List<IComponentHolder> ComponentHolders();

    public T GetComponentHolder<T>() where T : IComponentHolder;

    // ReSharper disable once InconsistentNaming
    public T GCH<T>() where T : IComponentHolder;
}