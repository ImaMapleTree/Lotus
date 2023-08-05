using System;

namespace Lotus.Roles.Interfaces;

public interface IRoleInitializer<T>: IRoleInitializer where T: CustomRole
{
    Type IRoleInitializer.TargetType => typeof(T);

    void IRoleInitializer.PreSetup(CustomRole role)
    {
        PreSetup((T)role);
    }

    public void PreSetup(T role)
    {
    }

    void IRoleInitializer.PostModify(CustomRole role, AbstractBaseRole.RoleModifier roleModifier)
    {
        PostModify((T)role, roleModifier);
    }

    public void PostModify(T role, AbstractBaseRole.RoleModifier roleModifier)
    {
    }

    CustomRole IRoleInitializer.PostSetup(CustomRole role) => PostSetup((T)role);

    public T PostSetup(T role) => role;
}

public interface IRoleInitializer
{
    public Type TargetType { get; }

    public void PreSetup(CustomRole role)
    {
    }

    public void PostModify(CustomRole role, AbstractBaseRole.RoleModifier roleModifier)
    {
    }

    public CustomRole PostSetup(CustomRole role) => role;
}