#nullable enable
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Archangel : CustomRole
{
    [UIComponent(UI.Cooldown)]
    private Cooldown protectCooldown;
    private Cooldown protectDuration; // I think this is for the amount of time the player is protected?

    // We can make these public as needed but it's usually better to stay private otherwise
    private bool TargetKnowsGaExists;
    private bool GaKnowsTargetRole;
    private GARoleChange roleChangeWhenTargetDies;
    private PlayerControl? target;

    [UIComponent(UI.Text)]
    private string TargetDisplay() => target == null ? "" : RoleColor.Colorize("Target: ") + Color.white.Colorize(target.name);

    protected override void Setup(PlayerControl player)
    {
        // Since "MyPlayer" is ALWAYS an Archangel we don't need to check for playerId
        List<PlayerControl> eligiblePlayers = Players.GetPlayers().Where(p => p.GetCustomRole() is not Archangel).ToList();
        if (eligiblePlayers.Any())
            target = eligiblePlayers.GetRandom();
        protectCooldown.Start(10f);

        if (target == null) return;
        target.NameModel().GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(protectDuration, GameState.Roaming, viewers: MyPlayer));
    }

    [RoleAction(RoleActionType.RoundStart)]
    public void Restart(bool gameStart) => protectCooldown.Start();

    [RoleAction(RoleActionType.OnPet)]
    public void OnPet()
    {
        if (protectCooldown.NotReady() || target == null) return;
        protectCooldown.Start();
        protectDuration.Start();
        SendProtection();
    }

    [RoleAction(RoleActionType.SuccessfulAngelProtect)]
    public void AnyProtect(PlayerControl targeted)
    {
        if (target == null || targeted.PlayerId != target.PlayerId || protectDuration.IsReady()) return;
        SendProtection();
    }

    [RoleAction(RoleActionType.AnyDeath)]
    public void Death(PlayerControl killed, PlayerControl killer)
    {
        if (!MyPlayer.IsAlive() || target == null || target.PlayerId != killed.PlayerId) return;
        target = null;
        if (roleChangeWhenTargetDies is GARoleChange.None) return;

        switch (roleChangeWhenTargetDies)
        {
            case GARoleChange.Jester:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Jester);
                break;
            case GARoleChange.Opportunist:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Opportunist);
                break;
            case GARoleChange.SchrodingerCat:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Copycat);
                break;
            case GARoleChange.Crewmate:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Crewmate);
                break;
            case GARoleChange.None:
            default:
                break;
        }
    }

    private void SendProtection()
    {
        GameOptionOverride[] overrides = { new(Override.GuardianAngelDuration, protectDuration.Duration) };
        if (target == null) return;
        target.GetCustomRole().SyncOptions(overrides);
        target.RpcProtectPlayer(target, 0);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .Name("Protect Duration")
                .BindFloat(v => protectDuration.Duration = v)
                .AddFloatRange(2.5f, 180f, 2.5f, 11, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Protect Cooldown")
                .BindFloat(v => protectCooldown.Duration = v)
                .AddFloatRange(2.5f, 180f, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Target Knows They have A GA")
                .Bind(v => TargetKnowsGaExists = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("GA Knows Target Role")
                .Bind(v => GaKnowsTargetRole = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("Role Change When Target Dies")
                .BindInt(v => roleChangeWhenTargetDies = (GARoleChange)v)
                .Value(v => v.Text("Jester").Value(1).Color(new Color(0.93f, 0.38f, 0.65f)).Build())
                .Value(v => v.Text("Opportunist").Value(2).Color(Color.green).Build())
                .Value(v => v.Text("Schrodinger's Cat").Value(3).Color(Color.black).Build())
                .Value(v => v.Text("Crewmate").Value(4).Color(new Color(0.71f, 0.94f, 1f)).Build())
                .Value(v => v.Text("Off").Value(0).Color(Color.red).Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .SpecialType(SpecialType.Neutral)
            .RoleColor("#B3FFFF"); // RoleColor takes a string too and automatically converts

    private enum GARoleChange
    {
        None,
        Jester,
        Opportunist,
        SchrodingerCat,
        Crewmate
    }
}