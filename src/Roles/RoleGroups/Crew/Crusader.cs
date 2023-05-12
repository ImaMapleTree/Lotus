using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Overrides;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;
using static TOHTOR.Roles.RoleGroups.Crew.Crusader.CrusaderTranslations.CrusaderOptions;
using static TOHTOR.Utilities.TranslationUtil;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Crusader: Crewmate, ISabotagerRole
{
    private Optional<byte> protectedPlayer = Optional<byte>.Null();
    private bool protectAgainstHelpfulInteraction;
    private bool protectAgainstNeutralInteraction;

    [RoleAction(RoleActionType.Attack)]
    private void SelectTarget(PlayerControl target)
    {
        if (MyPlayer.InteractWith(target, DirectInteraction.HelpfulInteraction.Create(this)) == InteractionResult.Halt) return;
        protectedPlayer = Optional<byte>.NonNull(target.PlayerId);
        MyPlayer.RpcGuardAndKill(target);
        Game.MatchData.GameHistory.AddEvent(new ProtectEvent(MyPlayer, target));
    }

    [RoleAction(RoleActionType.AnyInteraction)]
    private void AnyPlayerTargeted(PlayerControl killer, PlayerControl target, Interaction interaction, ActionHandle handle)
    {
        if (Game.State is not GameState.Roaming) return;
        if (!protectedPlayer.Exists()) return;
        if (target.PlayerId != protectedPlayer.Get()) return;
        Intent intent = interaction.Intent();

        switch (intent)
        {
            case IHelpfulIntent when !protectAgainstHelpfulInteraction:
            case INeutralIntent when !protectAgainstNeutralInteraction:
            case IFatalIntent fatalIntent when fatalIntent.IsRanged():
                return;
        }

        if (interaction is IDelayedInteraction or IRangedInteraction or IIndirectInteraction) return;

        handle.Cancel();
        RoleUtils.SwapPositions(target, MyPlayer);
        bool killed = MyPlayer.InteractWith(killer, DirectInteraction.FatalInteraction.Create(this)) is InteractionResult.Proceed;
        Game.MatchData.GameHistory.AddEvent(new PlayerSavedEvent(target, MyPlayer, killer));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, killer, killed));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Protect against Beneficial Interactions", Colorize(BeneficialInteractionProtection, ModConstants.Palette.PassiveColor))
                .BindBool(b => protectAgainstHelpfulInteraction = b)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub.KeyName("Protect against Neutral Interactions", Colorize(NeutralInteractionProtection, ModConstants.Palette.NeutralColor))
                .BindBool(b => protectAgainstNeutralInteraction = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .DesyncRole(RoleTypes.Impostor)
            .RoleColor(new Color(0.78f, 0.36f, 0.22f))
            .OptionOverride(new IndirectKillCooldown(() => AUSettings.KillCooldown()));

    public bool CanSabotage() => false;

    [Localized(nameof(Crusader))]
    internal static class CrusaderTranslations
    {
        [Localized("Options")]
        public static class CrusaderOptions
        {
            [Localized(nameof(BeneficialInteractionProtection))]
            public static string BeneficialInteractionProtection = "Protect against Beneficial::0 Interactions";
            
            [Localized(nameof(NeutralInteractionProtection))]
            public static string NeutralInteractionProtection = "Protect against Neutral::0 Interactions";
        }
    }
}