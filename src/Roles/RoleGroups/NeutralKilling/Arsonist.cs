using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public class Arsonist : NeutralKillingBase
{
    private static string[] _douseProgressIndicators = { "◦", "◎", "◉", "●" };

    private int requiredAttacks;
    private bool canIgniteAnyitme;

    private int knownAlivePlayers;
    [NewOnSetup] private HashSet<byte> dousedPlayers;
    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> indicators;
    [NewOnSetup] private Dictionary<byte, int> douseProgress;

    [UIComponent(UI.Counter)]
    private string DouseCounter() => RoleUtils.Counter(dousedPlayers.Count, knownAlivePlayers);

    [UIComponent(UI.Text)]
    private string DisplayWin() => dousedPlayers.Count >= knownAlivePlayers ? RoleColor.Colorize("Press Ignite to Win") : "";

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target)
    {
        bool douseAttempt = MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Proceed;
        if (!douseAttempt) return false;

        int progress = douseProgress[target.PlayerId] = douseProgress.GetValueOrDefault(target.PlayerId) + 1;
        if (progress > requiredAttacks) return false;

        RenderProgress(target, progress);
        if (progress < requiredAttacks) return false;

        dousedPlayers.Add(target.PlayerId);
        MyPlayer.RpcGuardAndKill(target);
        Game.MatchData.GameHistory.AddEvent(new PlayerDousedEvent(MyPlayer, target));

        MyPlayer.NameModel().Render();

        return false;
    }

    private void RenderProgress(PlayerControl target, int progress)
    {
        if (progress > requiredAttacks) return;
        string indicator = _douseProgressIndicators[Mathf.Clamp(Mathf.FloorToInt(progress / (requiredAttacks / (float)_douseProgressIndicators.Length) - 1), 0, 3)];

        Remote<IndicatorComponent> IndicatorSupplier() => target.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent("", GameStates.IgnStates, viewers: MyPlayer));

        Remote<IndicatorComponent> component = indicators.GetOrCompute(target.PlayerId, IndicatorSupplier);
        component.Get().SetMainText(new LiveString(indicator, RoleColor));
    }


    [RoleAction(RoleActionType.OnPet)]
    private void KillDoused() => dousedPlayers.Filter(p => Utils.PlayerById(p)).Where(p => p.IsAlive()).Do(p =>
    {
        if (dousedPlayers.Count < knownAlivePlayers && !canIgniteAnyitme) return;
        FatalIntent intent = new(true, () => new IncineratedDeathEvent(p, MyPlayer));
        IndirectInteraction interaction = new(intent, this);
        MyPlayer.InteractWith(p, interaction);
    });

    [RoleAction(RoleActionType.RoundStart)]
    private void UpdatePlayerCounts()
    {
        knownAlivePlayers = Game.GetAlivePlayers().Count(p => p.PlayerId != MyPlayer.PlayerId && Relationship(p) is not Relation.FullAllies);
        dousedPlayers.RemoveWhere(p => Utils.PlayerById(p).Transform(pp => !pp.IsAlive(), () => true));
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void ArsonistDies() => indicators.Values.ForEach(v => v.Delete());

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Color(RoleColor)
            .SubOption(sub => sub.Name("Attacks to Complete Douse")
                .AddIntRange(3, 100, defaultIndex: 16)
                .BindInt(i => requiredAttacks = i)
                .Build())
            .SubOption(sub => sub.Name("Can Ignite Anytime")
                .AddOnOffValues(false)
                .BindBool(b => canIgniteAnyitme = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(1f, 0.4f, 0.2f)).CanVent(false);


    class PlayerDousedEvent : TargetedAbilityEvent, IRoleEvent
    {
        public PlayerDousedEvent(PlayerControl source, PlayerControl target, bool successful = true) : base(source, target, successful)
        {
        }

        public override string Message() => $"{Game.GetName(Player())} doused {Game.GetName(Target())}.";
    }

    class IncineratedDeathEvent : DeathEvent
    {
        public IncineratedDeathEvent(PlayerControl deadPlayer, PlayerControl? killer) : base(deadPlayer, killer)
        {
        }

        public override string SimpleName() => ModConstants.DeathNames.Incinerated;
    }
}