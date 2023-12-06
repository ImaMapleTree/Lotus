using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.Factions.Neutrals;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Debugger;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2;
using Lotus.Roles2.Definitions;
using Lotus.Roles2.Manager;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Backing;

// ReSharper disable once InconsistentNaming
internal class TUAllRoles
{
    public static string GetAllRoles(bool allowSubroles, bool onlyModifiers = false)
    {
        string? factionName = null;

        OrderedDictionary<string, List<UnifiedRoleDefinition>> rolesByFaction = new();

        string FactionName(UnifiedRoleDefinition roleDefinition)
        {
            if (roleDefinition.Metadata.GetOrEmpty(RoleProperties.Key).Compare(r => r.HasProperty(RoleProperty.IsModifier))) return "Modifiers";
            if (roleDefinition.Faction is not Neutral) return roleDefinition.Faction.Name();

            SpecialType specialType = roleDefinition.Metadata.GetOrDefault(LotusKeys.AuxiliaryRoleType, SpecialType.None);

            return specialType is SpecialType.NeutralKilling ? "Neutral Killers" : "Neutral";
        }

        bool Condition(UnifiedRoleDefinition role)
        {
            if (role is NoOpDefinition) return false;
            if (onlyModifiers && RoleProperties.IsModifier(role)) return true;
            if (onlyModifiers) return false;
            if (allowSubroles) return true;
            return !RoleProperties.IsModifier(role);
        }


        IRoleManager.Current.RoleDefinitions().ForEach(r => rolesByFaction.GetOrCompute(FactionName(r), () => new List<UnifiedRoleDefinition>()).Add(r));

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

            roleNames.Add(r.Name);
        });

        text += roleNames.Fuse();

        return text.TrimStart('\n');
    }
}