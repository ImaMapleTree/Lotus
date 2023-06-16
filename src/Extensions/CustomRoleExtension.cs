using Lotus.Roles;
using VentLib.Utilities;

namespace Lotus.Extensions;

public static class CustomRoleExtension
{
    public static string ColoredRoleName(this AbstractBaseRole abr) => abr.RoleColorGradient?.Apply(abr.RoleName) ?? abr.RoleColor.Colorize(abr.RoleName);
}