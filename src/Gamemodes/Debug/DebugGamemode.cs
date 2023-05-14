using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Utilities;
using Lotus.Victory;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using VentLib.Options;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities;

namespace Lotus.Gamemodes.Debug;

public class DebugGamemode: Gamemode
{
    private List<Option> specificOptions = new();
    private readonly Dictionary<byte, string> _roleAssignments = new();

    internal static GameOptionTab DebugTab = new("Debug Tab", () => Utils.LoadSprite("Lotus.assets.Tabs.Debug_Tab.png"));

    public override string GetName() => "Debug";
    public override IEnumerable<GameOptionTab> EnabledTabs() => new[] { DebugTab };

    public override void Setup()
    {
    }

    public override void AssignRoles(List<PlayerControl> players)
    {
        players.Do(p =>
        {
            VentLogger.Debug($"Assigning {p.name} => {_roleAssignments.GetValueOrDefault(p.PlayerId)}");
            CustomRole? role = CustomRoleManager.AllRoles.FirstOrDefault(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(_roleAssignments.GetValueOrDefault(p.PlayerId)?.ToLower() ?? "HEHEXD"));
            Api.Roles.AssignRole(p, role ?? CustomRoleManager.Special.Debugger, true);
        });
    }

    public override void SetupWinConditions(WinDelegate winDelegate)
    {
        winDelegate.CancelGameWin();
    }

    public override void Activate()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            Option option = new GameOptionBuilder()
                .Name(player.name)
                .IsHeader(true)
                .Bind(v => _roleAssignments[player.PlayerId] = ((string)v).RemoveHtmlTags())
                .Values(CustomRoleManager.AllRoles.Select(s => s.RoleColor.Colorize(s.RoleName)))
                .Tab(DebugTab)
                .Build();
            specificOptions.Add(option);
        }
    }

    public override void Deactivate()
    {
        /*List<Option> allHolders = specificOptions.SelectMany(o => o.GetHoldersRecursive()).ToList();
        TOHPlugin.OptionManager.Options().RemoveAll(p => allHolders.Contains(p));
        TOHPlugin.OptionManager.Options().RemoveAll(p => allHolders.Contains(p));
        allHolders.Do(h =>
        {
            if (h.Tab == null) return;
            h.Tab.GetHolders().Remove(h);
        });*/
    }
}