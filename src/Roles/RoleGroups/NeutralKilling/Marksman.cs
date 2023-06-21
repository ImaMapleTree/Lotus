using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Marksman : NeutralKillingBase
{
    private const int KillDistanceMax = 3;

    private bool canVent;
    private bool impostorVision;

    private int killsBeforeIncrease;
    private int currentKills;

    [UIComponent(UI.Counter)]
    private string GetKillCount() => RoleUtils.Counter(currentKills, killsBeforeIncrease, new Color(1f, 0.36f, 0.07f));

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        MyPlayer.RpcMark(target);
        InteractionResult result = MyPlayer.InteractWith(target, new LotusInteraction(new FatalIntent(true), this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        if (result is InteractionResult.Halt) return false;

        if (++currentKills >= killsBeforeIncrease)
        {
            currentKills = 0;
            KillDistance = Mathf.Clamp(++KillDistance, 0, KillDistanceMax);
        }

        SyncOptions();
        return true;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.Name("Kills Before Distance Increase")
                .AddIntRange(0, 5, 1, 2)
                .BindInt(i => killsBeforeIncrease = i)
                .Build())
            .SubOption(sub => sub.Name("Starting Kill Distance")
                .Value(v => v.Text(GeneralOptionTranslations.GlobalText).Value(-1).Color(new Color(1f, 0.61f, 0.33f)).Build())
                .AddIntRange(0, 3)
                .BindInt(i => KillDistance = i)
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .BindBool(v => canVent = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("Can Sabotage")
                .BindBool(v => canSabotage = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Name("Impostor Vision")
                .BindBool(v => impostorVision = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.14f, 0.92f, 0.56f))
            .CanVent(canVent)
            .OptionOverride(new IndirectKillCooldown(KillCooldown))
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod(), () => !impostorVision)
            .OptionOverride(Override.KillDistance, () => KillDistance);
}