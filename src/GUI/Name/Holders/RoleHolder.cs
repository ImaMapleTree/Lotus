using Lotus.GUI.Name.Components;

namespace Lotus.GUI.Name.Holders;

public class RoleHolder : ComponentHolder<RoleComponent>
{
    public RoleHolder(int line = 0) : base(line)
    {
        Spacing = 1;
    }
}