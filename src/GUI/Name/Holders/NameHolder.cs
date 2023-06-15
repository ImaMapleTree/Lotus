using Lotus.GUI.Name.Components;
using VentLib.Utilities.Collections;

namespace Lotus.GUI.Name.Holders;

public class NameHolder : ComponentHolder<NameComponent>
{
    public NameComponent? TrueName;

    public NameHolder(int line = 0) : base(line)
    {
        Spacing = 1;
    }

    public override Remote<NameComponent> Add(NameComponent component)
    {
        TrueName = component;
        return base.Add(component);
    }
}