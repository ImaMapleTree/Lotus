using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Patches.Systems;
using Lotus.Roles.Events;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Crew;

public class Sheriff : Crewmate
{
    private int totalShots;
    private bool oneShotPerRound;
    private bool canKillCrewmates;
    private bool isSheriffDesync;

    private bool shotThisRound;
    private int shotsRemaining;

    [UIComponent(UI.Cooldown)]
    private Cooldown shootCooldown;

    protected override void Setup(PlayerControl player)
    {
        if (!isSheriffDesync) base.Setup(player);
        shotsRemaining = totalShots;
    }

    private bool HasShots() => !(oneShotPerRound && shotThisRound) && shotsRemaining >= 0;

    [UIComponent(UI.Counter, ViewMode.Additive, GameState.Roaming, GameState.InMeeting)]
    public string RemainingShotCounter() => RoleUtils.Counter(shotsRemaining, totalShots);

    [RoleAction(RoleActionType.RoundStart)]
    public bool RefreshShotThisRound() => shotThisRound = false;

    [RoleAction(RoleActionType.OnPet)]
    public bool TryKillWithPet(ActionHandle handle)
    {
        VentLogger.Trace("Sheriff Shoot Ability (Pet)", "SheriffAbility");
        handle.Cancel();
        if (isSheriffDesync || !shootCooldown.IsReady() || !HasShots()) return false;
        List<PlayerControl> closestPlayers = MyPlayer.GetPlayersInAbilityRangeSorted();
        if (closestPlayers.Count == 0) return false;
        PlayerControl target = closestPlayers[0];
        return TryKill(target, handle);
    }

    [RoleAction(RoleActionType.Attack)]
    public bool TryKill(PlayerControl target, ActionHandle handle)
    {
        handle.Cancel();
        if (!shootCooldown.IsReady() || !HasShots()) return false;
        shotsRemaining--;
        shootCooldown.Start();

        if (Relationship(target) is Relation.FullAllies) return Suicide(target);
        return MyPlayer.InteractWith(target, DirectInteraction.FatalInteraction.Create(this)) is InteractionResult.Proceed;
    }

    private bool Suicide(PlayerControl target)
    {
        MyPlayer.RpcMurderPlayer(MyPlayer);
        if (!canKillCrewmates) return false;

        DeathEvent deathEvent = new MisfiredEvent(MyPlayer);
        DirectInteraction directInteraction = new(new FatalIntent(false, () => deathEvent), this);
        bool killed = MyPlayer.InteractWith(target, directInteraction) is InteractionResult.Proceed;
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, killed));
        return true;
    }
    // OPTIONS

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Color(RoleColor)
            .SubOption(sub => sub
                .Name("Can Kill Crewmates")
                .Bind(v => canKillCrewmates = (bool)v)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Name("Kill Cooldown")
                .BindFloat(this.shootCooldown.SetDuration)
                .AddFloatRange(0, 120, 2.5f, 12, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Total Shots")
                .Bind(v => this.totalShots = (int)v)
                .AddIntRange(1, 60, 1, 4)
                .Build())
            .SubOption(sub => sub
                .Name("One Shot Per Round")
                .Bind(v => this.oneShotPerRound = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("Sheriff Action Button")
                .Bind(v => isSheriffDesync = (bool)v)
                .Value(v => v.Text("Kill Button (legacy)").Value(true).Color(Color.green).Build())
                .Value(v => v.Text("Pet Button").Value(false).Color(Color.cyan).Build())
                .Build());

    // Sheriff is not longer a desync role for simplicity sake && so that they can do tasks
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .DesyncRole(isSheriffDesync ? RoleTypes.Impostor : RoleTypes.Crewmate)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => isSheriffDesync)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod() / 5, () => isSheriffDesync && SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Lights)
            .OptionOverride(Override.KillCooldown, () => shootCooldown.Duration)
            .RoleAbilityFlags(RoleAbilityFlag.CannotVent | RoleAbilityFlag.CannotSabotage | RoleAbilityFlag.IsAbleToKill)
            .RoleColor(new Color(0.97f, 0.8f, 0.27f));
}