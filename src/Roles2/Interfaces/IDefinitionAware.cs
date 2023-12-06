namespace Lotus.Roles2.Interfaces;

public interface IDefinitionAware<in T>: IDefinitionAware where T: RoleDefinition
{
    public void SetRoleDefinition(T definition);

    void IDefinitionAware.SetRoleDefinition(RoleDefinition definition)
    {
        SetRoleDefinition((T)definition);
    }
}

public interface IDefinitionAware: IRoleComponent
{
    public void SetRoleDefinition(RoleDefinition definition);
}