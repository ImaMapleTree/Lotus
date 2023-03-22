using System;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Roles;

namespace TOHTOR.GUI.Name.Components;

public class RoleComponent : NmComponent
{
    private CustomRole role;

    public RoleComponent(CustomRole role, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(LiveString.Empty, gameStates, viewMode, viewers)
    {
        this.role = role;
        this.SetMainText(new LiveString(role.RoleName, role.RoleColor));
    }

    public RoleComponent(CustomRole role, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : this(role, gameStates, viewMode, viewers.ToList)
    {
    }

    public RoleComponent(CustomRole role, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : this(role, new []{gameState}, viewMode, viewers)
    {
    }

    public RoleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public RoleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }
}