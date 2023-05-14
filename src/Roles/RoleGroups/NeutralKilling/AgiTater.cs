using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Impl;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.Internals.InteractionResult;
using static Lotus.Roles.RoleGroups.NeutralKilling.AgiTater.AgitaterTranslations;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

[Localized("Roles")]
public class AgiTater: NeutralKillingBase
{
    private ExplodeCondition Condition;
    private Cooldown bombDuration = null!;
    private int bombsPerRound;

    [NewOnSetup] private RemoteList<AgiBomb> bombs = null!;
    [NewOnSetup] private Dictionary<byte, int> BombCounts = null!;
    [NewOnSetup] private FixedUpdateLock fixedUpdateLock = new(0.1f);

    private int currentBombs;

    public override bool CanSabotage() => false;

    [UIComponent(UI.Counter, ViewMode.Additive, GameState.Roaming)]
    private string BombCounter() => bombsPerRound == -1 ? "" : RoleUtils.Counter(currentBombs, bombsPerRound, RoleColor);
    
    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (currentBombs == 0) return false;
        bool canBomb = MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is Proceed;
        if (!canBomb) return false;
        currentBombs--;

        int count = BombCounts.Compose(target.PlayerId, i => i + 1, () => 1);
        if (count >= 2 && Condition.HasFlag(ExplodeCondition.DoubleBomb))
        {
            bombs.FirstOrDefault(b => b.Owner == target.PlayerId)?.Explode();
            return false;
        }

        AgiBomb agiBomb = new(target.PlayerId, this, Condition.HasFlag(ExplodeCondition.Duration) ? bombDuration.Clone() : null);
        agiBomb.SetRemote(bombs.Add(agiBomb));
        MyPlayer.RpcGuardAndKill(target);
        return false;
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void AgitaterBombReset() => currentBombs = bombsPerRound;

    [RoleAction(RoleActionType.FixedUpdate)]
    private void AgitaterFixedUpdate()
    {
        if (!fixedUpdateLock.AcquireLock()) return;
        bombs.ToArray().ForEach(b => b.DoUpdate());
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    private void KillPlayersBeforeMeeting()
    {
        if (Condition.HasFlag(ExplodeCondition.Meetings))
            bombs.ToArray().ForEach(b => b.Explode());
    }

    [RoleAction(RoleActionType.Disconnect)]
    [RoleAction(RoleActionType.AnyDeath)]
    private void RemoveDeadPlayerBomb(PlayerControl player)
    {
        bombs.ToArray().Where(b => b.Owner == player.PlayerId).ForEach(b => b.DeleteBomb());
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream), AgitaterTranslations.AgiOptions.PlaceBombCooldown, "Place Bomb Cooldown")
            .SubOption(sub => sub.KeyName("Explode On Meeting", AgitaterTranslations.AgiOptions.ExplodeOnMeetings)
                .AddOnOffValues()
                .BindBool(FlagSetter(ExplodeCondition.Meetings))
                .ShowSubOptionPredicate(o => (bool)o)
                .Build())
            .SubOption(sub => sub.KeyName("Explode After Duration", AgitaterTranslations.AgiOptions.ExplodeAfterDuration)
                .AddOnOffValues(false)
                .BindBool(FlagSetter(ExplodeCondition.Duration))
                .ShowSubOptionPredicate(b => (bool)b)
                .SubOption(sub2 => sub2.KeyName("Bomb Duration", AgitaterTranslations.AgiOptions.BombDuration)
                    .AddFloatRange(2.5f, 120f, 2.5f, 7, "s")
                    .BindFloat(bombDuration.SetDuration)
                    .Build())
                .Build())
            .SubOption(sub => sub.KeyName("Explode When Bombed Twice", AgitaterTranslations.AgiOptions.ExplodeDoubleBombed)
                .AddOnOffValues(false)
                .BindBool(FlagSetter(ExplodeCondition.DoubleBomb))
                .Build())
            .SubOption(sub2 => sub2.KeyName("Bombs per Round", AgitaterTranslations.AgiOptions.BombsPerRound)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 15, 1, 3)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
                .BindInt(i => bombsPerRound = i)
                .Build());


    private Action<bool> FlagSetter(ExplodeCondition flag)
    {
        return b =>
        {
            if (b) Condition |= flag;
            else Condition &= ~flag;
        };
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.96f, 0.64f, 0.38f))
            .OptionOverride(new IndirectKillCooldown(() => KillCooldown));


    private class AgiBomb
    {
        public byte Owner;
        public Cooldown? Duration;
        
        private AgiTater agitater;
        private Remote<AgiBomb>? remote;
        private Remote<TextComponent>? bombText;
        private FixedUpdateLock fixedUpdateLock = new(0.8f);
        
        public AgiBomb(byte owner, AgiTater source, Cooldown? duration)
        {
            Owner = owner;
            agitater = source;
            Duration = duration;
            duration?.Start();
            Utils.PlayerById(owner).IfPresent(p => bombText = SetBombText(p));
        }

        public bool Explode(PlayerControl? owner = null)
        {
            owner ??= Utils.GetPlayerById(Owner);
            if (owner == null || agitater.MyPlayer == null) return false;
            
            // ReSharper disable once Unity.NoNullPropagation
            VentLogger.Trace($"AgiTater Bomb Exploding (AgiTater={agitater.MyPlayer.name}, target={owner.name})", "AgiTater::Explode");
            BombedEvent bombedEvent = new(owner, agitater.MyPlayer);
            FatalIntent fatalIntent = new(true, () => bombedEvent);
            
            bool success = agitater.MyPlayer.InteractWith(owner, new IndirectInteraction(fatalIntent, agitater)) is Proceed;
            if (success) Game.MatchData.GameHistory.AddEvent(new BombedEvent(owner, agitater.MyPlayer));
            agitater.BombCounts[owner.PlayerId] = 0;
            agitater.bombs.ToArray().Where(b => b.Owner == owner.PlayerId).ForEach(b => b.DeleteBomb());
            return true;
        }

        public bool DoUpdate()
        {
            if (!fixedUpdateLock.AcquireLock()) return false;
            PlayerControl? owner = Utils.GetPlayerById(Owner);
            if (owner == null) return DeleteBomb();

            // The timer has run out thus the bomb has exploded
            if (Duration?.IsReady() ?? false) return Explode();
            
            List<PlayerControl> inRangePlayers = owner.GetPlayersInAbilityRangeSorted();
            return !inRangePlayers.IsEmpty() && Transfer(inRangePlayers[0]);
        }
        
        public bool DeleteBomb()
        {
            remote?.Delete();
            bombText?.Delete();
            return true;
        }

        private bool Transfer(PlayerControl newPlayer)
        {
            VentLogger.Trace($"Transferring AgiTater Bomb {Utils.GetPlayerById(Owner)?.name} => {newPlayer.name}", "AgiTater::Transfer");
            bombText?.Delete();
            if (agitater.BombCounts.ContainsKey(Owner)) agitater.BombCounts[Owner]--;
            Owner = newPlayer.PlayerId;
            bombText = SetBombText(newPlayer);
            int count = agitater.BombCounts.Compose(Owner, v => v + 1, () => 1);
            if (count == 2 && agitater.Condition.HasFlag(ExplodeCondition.DoubleBomb)) return Explode();
            return false;
        }

        private Remote<TextComponent> SetBombText(PlayerControl player)
        {
            LiveString indicator = new(IndicatorString);
            TextComponent text = new(indicator, GameState.Roaming, ViewMode.Additive, Game.GetDeadPlayers().AddItem(player).ToArray());
            return player.NameModel().GetComponentHolder<TextHolder>().Add(text);
        }
        
        public void SetRemote(Remote<AgiBomb> bombRemote)
        {
            this.remote = bombRemote;
        }

        private string IndicatorString()
        {
            if (Duration == null) return new Color(0.71f, 0.58f, 0.27f).Colorize(PassTheBombText);
            if (Duration.IsReady()) return "";
            if (Duration.TimeRemaining() > 20) return Color.green.Colorize(HoldingBombLevel1);
            if (Duration.TimeRemaining() > 10) return Color.yellow.Colorize(HoldingBombLevel2);
            return Color.red.Colorize(HoldingBombLevel3);
        }
    }
    
    
    [Flags]
    public enum ExplodeCondition
    {
        Meetings = 1,
        Duration = 2,
        DoubleBomb = 4
    }
    
    [Localized(nameof(AgiTater))]
    internal static class AgitaterTranslations
    {
        [Localized(nameof(PassTheBombText))]
        public static string PassTheBombText = "Pass The Bomb";
        
        [Localized(nameof(HoldingBombLevel1))]
        public static string HoldingBombLevel1 = "Holding Bomb!";
        
        [Localized(nameof(HoldingBombLevel2))]
        public static string HoldingBombLevel2 = "Holding Bomb!!!";
        
        [Localized(nameof(HoldingBombLevel3))]
        public static string HoldingBombLevel3 = "Holding Bomb!!!!!";

        [Localized("Options")]
        public static class AgiOptions
        {
            [Localized(nameof(PlaceBombCooldown))]
            public static string PlaceBombCooldown = "Place Bomb Cooldown";
            
            [Localized(nameof(ExplodeOnMeetings))]
            public static string ExplodeOnMeetings = "Explode On Meetings";

            [Localized(nameof(ExplodeAfterDuration))]
            public static string ExplodeAfterDuration = "Explode After Duration";
            
            [Localized(nameof(ExplodeDoubleBombed))]
            public static string ExplodeDoubleBombed = "Explode When Bombed Twice";

            [Localized(nameof(BombDuration))]
            public static string BombDuration = "Bomb Duration";

            [Localized(nameof(BombsPerRound))]
            public static string BombsPerRound = "Bombs per Round";
        }
    }
}