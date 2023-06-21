using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.RPC;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using static Lotus.ModConstants.Palette;
using static Lotus.Roles.RoleGroups.Impostors.IdentityThief.Translations.Options;

namespace Lotus.Roles.RoleGroups.Impostors;

public class IdentityThief : Impostor
{
    private ShiftingType shiftingType;

    private bool shapeshiftedThisRound;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (Relationship(target) is Relation.FullAllies) return false;

        LotusInteraction lotusInteraction = new(new FakeFatalIntent(), this);
        InteractionResult result = MyPlayer.InteractWith(target, lotusInteraction);
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));

        if (result is InteractionResult.Halt) return false;

        if (shiftingType is not ShiftingType.UntilMeeting || !shapeshiftedThisRound) MyPlayer.CRpcShapeshift(target, true);

        if (shiftingType is ShiftingType.KillCooldown) Async.Schedule(() =>
        {
            if (Game.State is GameState.InMeeting) return;
            MyPlayer.CRpcRevertShapeshift(true);
        }, KillCooldown);

        ProtectedRpc.CheckMurder(MyPlayer, target);
        shapeshiftedThisRound = true;
        return true;
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void ClearShapeshiftStatus() => shapeshiftedThisRound = false;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Disguise Settings", DisguiseSettings)
                .Value(v => v.Text(UntilKillCooldown).Value(0).Color(GeneralColor3).Build())
                .Value(v => v.Text(UntilNextKill).Value(1).Color(GeneralColor4).Build())
                .Value(v => v.Text(UntilMeeting).Value(2).Color(GeneralColor5).Build())
                .BindInt(b => shiftingType = (ShiftingType)b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.ShapeshiftDuration, 3000f)
            .OptionOverride(Override.ShapeshiftCooldown, 0.001f);

    [Localized(nameof(IdentityThief))]
    internal static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(DisguiseSettings))]
            public static string DisguiseSettings = "Disguise Settings";

            [Localized(nameof(UntilKillCooldown))]
            public static string UntilKillCooldown = "Kill CD";

            [Localized(nameof(UntilNextKill))]
            public static string UntilNextKill = "Next Kill";

            [Localized(nameof(UntilMeeting))]
            public static string UntilMeeting = "Until Meeting";
        }
    }

    private enum ShiftingType
    {
        KillCooldown,
        NextKill,
        UntilMeeting
    }
}