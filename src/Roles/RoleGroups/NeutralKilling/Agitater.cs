using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public class AgiTater : NeutralKillingBase
{
    private bool explodeOnMeeting;
    private bool explodeAfterCondition;
    private ExplodeCondition condition;

    private float bombTimer;
    private int bombsPerRound;
    private int currentBombs;
    private bool bombsPersist;

    private List<AgiBomb> bombs = new();
    private Dictionary<byte, int> bombCounts = new();

    private ExplodeCondition Condition => !explodeAfterCondition ? ExplodeCondition.None : condition;
    private bool BombsPersist => !explodeAfterCondition && Condition is ExplodeCondition.DoubleBomb && bombsPersist;

    [UIComponent(UI.Counter)]
    private string BombCounter() => RoleUtils.Counter(currentBombs, bombsPerRound);

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        bombs = new();
        bombCounts = new Dictionary<byte, int>();
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void SetBombCount() => currentBombs = !explodeAfterCondition ? 1 : bombsPerRound;

    [RoleAction(RoleActionType.Attack)]
    private bool SetBomb(PlayerControl target)
    {
        if (currentBombs <= 0) return false;
        currentBombs--;
        if (MyPlayer.InteractWith(target, new SimpleInteraction(new EfIntent(), this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcGuardAndKill(target);
        AgiBomb bomb = new(MyPlayer, new Cooldown(bombTimer));
        bombs.Add(bomb);
        bomb.Transfer(target, bombCounts);
        if (Condition.HasFlag(ExplodeCondition.Duration)) bomb.StartTimer();
        AddPassEvent(MyPlayer, target);
        return false;
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void MeetingCalled()
    {
        if (BombsPersist) return;
        if (!explodeOnMeeting)
        {
            bombs.Clear();
            return;
        }
        foreach (AgiBomb bomb in bombs) ExplodeBomb(bomb.Owner, -1);
        bombs.Clear();
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void BombFixedUpdate()
    {
        for (int i = 0; i < bombs.Count; i++)
        {
            AgiBomb bomb = bombs[i];
            if (!bomb.Owner.IsAlive())
            {
                bombs.RemoveAt(i--);
                continue;
            }
            if (Condition.HasFlag(ExplodeCondition.Duration) && bomb.CheckTimer()) ExplodeBomb(bomb.Owner, i--);
            if (DateTime.Now.Subtract(bomb.LastTransferred).TotalSeconds < 1.5) continue;

            Optional<PlayerControl> closest = bomb.Owner.GetPlayersInAbilityRangeSorted().FirstOrOptional();
            if (!closest.Exists()) continue;

            PlayerControl t = closest.Get();
            AddPassEvent(bomb.Owner, t);
            bomb.Transfer(t, bombCounts);
            if (bombCounts[t.PlayerId] > 1 && Condition.HasFlag(ExplodeCondition.DoubleBomb)) ExplodeBomb(t, i--);
        }
    }

    private void ExplodeBomb(PlayerControl t, int bombIndex)
    {
        MyPlayer.InteractWith(t, new IndirectInteraction(new FatalIntent(true, () => new BombedEvent(t, MyPlayer)), this));
        Game.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, t, $"A bomb from {ModConstants.HColor1.Colorize(MyPlayer.UnalteredName())} blew up {ModConstants.HColor2.Colorize(t.UnalteredName())}."));
        if (bombIndex >= 0) bombs.RemoveAt(bombIndex);
    }

    private void AddPassEvent(PlayerControl passer, PlayerControl receiver)
    {
        Game.GameHistory.AddEvent(new GenericTargetedEvent(passer, receiver, $"{ModConstants.HColor1.Colorize(passer.UnalteredName())} passed a bomb to {ModConstants.HColor2.Colorize(receiver.UnalteredName())}."));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Explode On Meeting")
                .AddOnOffValues()
                .BindBool(b => explodeOnMeeting = b)
                .ShowSubOptionPredicate(o => (bool)o)
                .Build())
            .SubOption(sub => sub.Name("Explode After Condition")
                .AddOnOffValues()
                .BindBool(b => explodeAfterCondition = b)
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(sub2 => sub2.Name("Explode Condition")
                    .Value(v => v.Text("Duration").Value((int)ExplodeCondition.Duration).Build())
                    .Value(v => v.Text("Double Bombed").Value((int)ExplodeCondition.DoubleBomb).Build())
                    .Value(v => v.Text("Both").Value((int)ExplodeCondition.Both).Build())
                    .ShowSubOptionPredicate(o => ((ExplodeCondition)o).HasFlag(ExplodeCondition.Duration))
                    .SubOption(sub3 => sub3.Name("Bomb Timer")
                        .AddFloatRange(10f, 60f, 2.5f, 2, "s")
                        .BindFloat(f => bombTimer = f)
                        .Build())
                    .BindInt(i => condition = (ExplodeCondition)i)
                    .Build())
                .SubOption(sub2 => sub2.Name("Bombs per Round")
                    .AddIntRange(1, 10, 1, 1)
                    .BindInt(i => bombsPerRound = i)
                    .Build())
                .SubOption(sub2 => sub2.Name("Bombs Persist After Meeting")
                    .AddOnOffValues()
                    .BindBool(b => bombsPersist = b)
                    .Build())
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.96f, 0.64f, 0.38f)).OptionOverride(Override.KillCooldown, () => KillCooldown * 2);

    [Flags]
    public enum ExplodeCondition
    {
        None = 1,
        Duration = 2,
        DoubleBomb = 4,
        Both = 6
    }

    private class AgiBomb
    {
        public DateTime LastTransferred = DateTime.Now;
        public PlayerControl Owner;
        public bool Active = true;
        private Cooldown fuse;
        private Remote<TextComponent>? componentRemote;

        public AgiBomb(PlayerControl owner, Cooldown timer)
        {
            this.Owner = owner;
            this.fuse = timer;
        }

        public void StartTimer() => this.fuse.Start();

        public void Transfer(PlayerControl target, Dictionary<byte, int> bombCounts)
        {
            componentRemote?.Delete();
            this.LastTransferred = DateTime.Now;
            bombCounts[this.Owner.PlayerId] = bombCounts.GetOptional(this.Owner.PlayerId).Transform(i => i - 1, () => 0);
            bombCounts[target.PlayerId] = bombCounts.GetOptional(target.PlayerId).Transform(i => i + 1, () => 1);
            this.Owner = target;
            LiveString liveString = new(() => IndicatorString(target.PlayerId));
            componentRemote = this.Owner.NameModel().GetComponentHolder<TextHolder>().Add(new TextComponent(liveString, GameState.Roaming, viewers: this.Owner));
        }

        private string IndicatorString(byte targetId)
        {
            if (!Active || this.Owner.PlayerId != targetId || !this.Owner.IsAlive()) return "";
            if (fuse.IsReady()) return new Color(0.71f, 0.58f, 0.27f).Colorize("Pass the Bomb");
            if (fuse.TimeRemaining() > 20) return Color.green.Colorize("Holding Bomb!");
            if (fuse.TimeRemaining() > 10) return Color.yellow.Colorize("Holding Bomb!!!");
            return Color.red.Colorize("Holding Bomb!!!!!");
        }

        public bool CheckTimer() => fuse.IsReady();
    }

}