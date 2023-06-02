using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using Lotus.Roles.Subroles;
using Lotus.API;

namespace Lotus.GUI.Name.Components;

public class SubroleComponent : SimpleComponent
{
    public SubroleComponent(Subrole subrole, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.SetMainText(new LiveString(subrole.Identifier() ?? "", subrole.RoleColor));
    }

    public SubroleComponent(Subrole subrole, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : this(subrole, new []{gameState}, viewMode, viewers)
    {
    }
}