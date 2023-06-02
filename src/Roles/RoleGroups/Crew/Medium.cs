using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Subroles;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Medium.Translations;
using Object = UnityEngine.Object;

namespace Lotus.Roles.RoleGroups.Crew;

public partial class Medium: Crewmate, IModdable
{
    public static HashSet<Type> MediumBannedModifiers = new() { typeof(Oblivious) };
    public override HashSet<Type> BannedModifiers() => MediumBannedModifiers;

    [NewOnSetup] private Dictionary<byte, Optional<CustomRole>> killerDictionary = new();
    private bool hasArrowsToBodies;

    [UIComponent(UI.Indicator)]
    private string Arrows() => hasArrowsToBodies ? Object.FindObjectsOfType<DeadBody>()
        .Where(b => !Game.MatchData.UnreportableBodies.Contains(b.ParentId))
        .Select(b => RoleUtils.CalculateArrow(MyPlayer, b.TruePosition, RoleColor)).Fuse("") : "";


    [RoleAction(RoleActionType.AnyDeath)]
    private void AnyPlayerDeath(PlayerControl player, IDeathEvent deathEvent)
    {
        killerDictionary[player.PlayerId] = deathEvent.Instigator().Map(p => p.GetCustomRole());
    }

    [RoleAction(RoleActionType.SelfReportBody)]
    private void MediumDetermineRole(GameData.PlayerInfo reported)
    {
        killerDictionary.GetOptional(reported.PlayerId).FlatMap(o => o)
            .IfPresent(killerRole => Async.Schedule(() => MediumSendMessage(killerRole), 2f));
    }

    private void MediumSendMessage(CustomRole killerRole)
    {
        ChatHandler.Of(MediumMessage.Formatted(killerRole.RoleColor.Colorize(killerRole.RoleName)))
            .Title(t => t.Prefix("˖°").Suffix("°˖✧").Color(RoleColor).Text(MediumTitle).Build())
            .Send(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Has Arrows to Bodies", Translations.Options.HasArrowsToBody)
                .AddOnOffValues(false)
                .BindBool(b => hasArrowsToBodies = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor("#A680FF");

    internal static class Translations
    {
        [Localized(nameof(MediumTitle))]
        public static string MediumTitle = "Meditation";

        [Localized(nameof(MediumMessage))]
        public static string MediumMessage =
            "You've reported a body, and after great discussion with its spirits. You've determined the killer's role was {0}.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(HasArrowsToBody))]
            public static string HasArrowsToBody = "Has Arrows to Bodies";
        }
    }
}

