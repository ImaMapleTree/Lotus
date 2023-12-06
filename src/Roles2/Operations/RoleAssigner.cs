namespace Lotus.Roles2.Operations;

// ReSharper disable once InconsistentNaming
public interface RoleAssigner
{
    public void Assign(UnifiedRoleDefinition definition, PlayerControl player);
}