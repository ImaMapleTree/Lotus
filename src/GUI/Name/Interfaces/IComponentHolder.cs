using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using VentLib.Utilities.Collections;

namespace TOHTOR.GUI.Name.Interfaces;

public interface IComponentHolder<T> : IComponentHolder where T: INameModelComponent
{
    List<INameModelComponent> IComponentHolder.Components() => this.Components().Cast<INameModelComponent>().ToList();

    public new RemoteList<T> Components();
}

public interface IComponentHolder
{
    public List<INameModelComponent> Components();

    public void SetSize(float size);

    public void SetLine(int line);

    public int Line();

    public void SetSpacing(int spacing);

    public string Render(PlayerControl player, GameState state);

    public bool Updated(byte playerId);
}