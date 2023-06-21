using System.Collections.Generic;
using System.Linq;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.OptionBuilders;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using static Lotus.Roles.RoleGroups.Crew.Investigator.Translations.Options;

namespace Lotus.Roles.RoleGroups.Crew;

public class Investigator : Crewmate
{
    [NewOnSetup] private List<byte> investigated = null!;

    public DynamicRoleOptionBuilder DynamicRoleOptionBuilder = new(new List<DynamicOptionPredicate>
    {
        new(r => r.SpecialType is SpecialType.NeutralKilling, "Neutral Killing Are Red", TranslationUtil.Colorize(NeutralKillingRed, Color.red, ModConstants.Palette.NeutralColor, ModConstants.Palette.KillingColor)),
        new(r => r.SpecialType is SpecialType.Neutral, "Neutral Passive Are Red", TranslationUtil.Colorize(NeutralPassiveRed, Color.red, ModConstants.Palette.NeutralColor, ModConstants.Palette.PassiveColor)),
        new(r => r.Faction is Factions.Impostors.Madmates, "Madmates Are Red", TranslationUtil.Colorize(MadmateRed, Color.red, ModConstants.Palette.MadmateColor))
    });

    [UIComponent(UI.Cooldown)]
    private Cooldown abilityCooldown = null!;

    [RoleAction(RoleActionType.OnPet)]
    private void Investigate()
    {
        if (abilityCooldown.NotReady()) return;
        List<PlayerControl> players = MyPlayer.GetPlayersInAbilityRangeSorted().Where(p => !investigated.Contains(p.PlayerId)).ToList();
        if (players.Count == 0) return;

        abilityCooldown.Start();
        PlayerControl player = players[0];
        if (MyPlayer.InteractWith(player, LotusInteraction.NeutralInteraction.Create(this)) is InteractionResult.Halt) return;

        investigated.Add(player.PlayerId);
        CustomRole role = player.GetCustomRole();
        Color color =  DynamicRoleOptionBuilder.IsAllowed(role) ? Color.green : Color.red;

        NameComponent nameComponent = new(new LiveString(player.name, color), Game.IgnStates, ViewMode.Replace, MyPlayer);
        player.NameModel().GetComponentHolder<NameHolder>().Add(nameComponent);
    }

    // This is the most complicated options because of all the individual settings
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        DynamicRoleOptionBuilder.Decorate(base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Investigate Cooldown", InvestigateCooldown)
                .BindFloat(abilityCooldown.SetDuration)
                .AddFloatRange(2.5f, 120, 2.5f, 10, GeneralOptionTranslations.SecondsSuffix)
                .Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(1f, 0.79f, 0.51f));

    [Localized(nameof(Investigator))]
    internal static class Translations
    {
        [Localized(ModConstants.Options)]
        internal static class Options
        {
            [Localized(nameof(InvestigateCooldown))]
            public static string InvestigateCooldown = "Investigate Cooldown";

            [Localized(nameof(NeutralKillingRed))]
            public static string NeutralKillingRed = "Neutral::1 Killing::2 Are Red::0";

            [Localized(nameof(NeutralPassiveRed))]
            public static string NeutralPassiveRed = "Neutral::1 Passive::2 Are Red::0";

            [Localized(nameof(MadmateRed))]
            public static string MadmateRed = "Madmates::1 Are Red::0";
        }
    }

}