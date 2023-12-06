namespace Lotus.Roles2.Interfaces;

public interface IRoleComponent
{
    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player);
}