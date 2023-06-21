using System.Collections.Generic;
using System.Linq;
using Lotus.Factions.Neutrals;
using Lotus.Roles;
using Lotus.Roles.Debugger;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Subroles;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Backing;

// ReSharper disable once InconsistentNaming
internal class TUAllRoles
{
    public static string GetAllRoles(bool allowSubroles, bool onlyModifiers = false)
    {
        string? factionName = null;

        OrderedDictionary<string, List<CustomRole>> rolesByFaction = new();

        string FactionName(CustomRole role)
        {
            if (role is Subrole || onlyModifiers) return "Modifiers";
            if (role.Faction is not Neutral) return role.Faction.Name();
            return role.SpecialType is SpecialType.NeutralKilling ? "Neutral Killers" : "Neutral";
        }

        bool Condition(CustomRole role)
        {
            if (role is Debugger or IllegalRole or EnforceFunctionOrderingRole) return false;
            if (onlyModifiers && role.RoleFlags.HasFlag(RoleFlag.IsSubrole)) return true;
            if (onlyModifiers) return false;
            if (allowSubroles) return true;
            return !role.RoleFlags.HasFlag(RoleFlag.IsSubrole);
        }


        CustomRoleManager.AllRoles.ForEach(r => rolesByFaction.GetOrCompute(FactionName(r), () => new List<CustomRole>()).Add(r));

        string text = "";

        List<string> roleNames = new();
        rolesByFaction.GetValues().SelectMany(s => s).Where(Condition).ForEach(r =>
        {
            string fName = FactionName(r);
            if (factionName != fName)
            {
                text += roleNames.Fuse();
                roleNames = new List<string>();
                if (factionName == "Modifiers") text += $"\n★ {factionName} ★\n";
                else text += $"\n\n★ {fName} ★\n";
                factionName = fName;
            }

            roleNames.Add(r.RoleName);
        });

        text += roleNames.Fuse();

        return text.TrimStart('\n');
    }
}