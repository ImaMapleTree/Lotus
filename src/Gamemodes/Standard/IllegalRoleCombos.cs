using System.Collections.Generic;
using System.IO;
using System.Linq;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard;

public static class IllegalRoleCombos
{
    private static FileInfo file = PluginDataManager.HiddenDataDirectory.GetFile("illegal_combos.json");
    private static IllegalRoleJson _illegalRoleJson;

    static IllegalRoleCombos()
    {
        _illegalRoleJson = JsonUtils.ReadJson<IllegalRoleJson>(file).OrElseGet(() => new IllegalRoleJson());
        _illegalRoleJson.UpdateIllegalComboList();
    }

    public static void AddIllegalCombo(List<CustomRole> roles)
    {
        _illegalRoleJson.IllegalCombos.Add(roles);
        _illegalRoleJson.UpdateIllegalRoleNameList();
    }

    public static void RemoveIllegalCombo(int index)
    {
        _illegalRoleJson.IllegalCombos.RemoveAt(index);
        _illegalRoleJson.UpdateIllegalRoleNameList();
    }

    public static List<List<string>> GetCurrentCombos()
    {
        return _illegalRoleJson.IllegalCombos.Select(list => list.Select(r => r.RoleName).ToList()).ToList();
    }

    private class IllegalRoleJson
    {
        public List<List<string>> IllegalCombosRoleNames { get; set; } = new();
        internal List<List<CustomRole>> IllegalCombos = new();

        public void UpdateIllegalComboList()
        {
            IllegalCombos =
                IllegalCombosRoleNames.Select(list => list.Select(CustomRoleManager.GetRoleFromName).ToList()).ToList();
        }

        public void UpdateIllegalRoleNameList()
        {
            IllegalCombosRoleNames = IllegalCombos.Select(list => list.Select(r => r.EnglishRoleName).ToList()).ToList();
            JsonUtils.WriteJson(this, file);
        }
    }
}