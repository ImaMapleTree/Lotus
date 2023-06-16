namespace Lotus.Roles.Interfaces;

public interface ISubrole
{
    public string? Identifier();

    public bool IsAssignableTo(PlayerControl player);
}