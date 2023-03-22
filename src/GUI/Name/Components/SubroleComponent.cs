using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Roles.Subroles;

namespace TOHTOR.GUI.Name.Components;

public class SubroleComponent : NmComponent
{
    public SubroleComponent(Subrole subrole, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.SetMainText(new LiveString(subrole.Identifier() ?? "", subrole.RoleColor));
    }

    public SubroleComponent(Subrole subrole, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : this(subrole, new []{gameState}, viewMode, viewers)
    {
    }
}