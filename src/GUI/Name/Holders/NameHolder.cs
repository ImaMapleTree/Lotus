using Lotus.GUI.Name.Components;

namespace Lotus.GUI.Name.Holders;

public class NameHolder : ComponentHolder<NameComponent>
{
    public NameHolder(int line = 0) : base(line)
    {
        Spacing = 1;
    }
}