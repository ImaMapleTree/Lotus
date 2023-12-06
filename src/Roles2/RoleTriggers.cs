using Lotus.Roles2.Interfaces;

namespace Lotus.Roles2;

public abstract class RoleTriggers<T>: IDefinitionAware<T> where T: RoleDefinition
{
    public T Definition { get; internal set; } = null!;
    public PlayerControl MyPlayer => Definition.MyPlayer;

    public void SetRoleDefinition(T definition)
    {
        Definition = definition;
    }

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => setupHelper.Clone(this);
}