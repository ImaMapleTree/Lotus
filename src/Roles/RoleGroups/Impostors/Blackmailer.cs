using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Chat;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Impostors.Blackmailer.Translations;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Blackmailer: Shapeshifter
{
    private Remote<TextComponent>? blackmailingText;
    private Optional<PlayerControl> blackmailedPlayer = Optional<PlayerControl>.Null();

    private bool showBlackmailedToAll;

    private int warnsUntilKick;
    private int currentWarnings;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(RoleActionType.Shapeshift)]
    public void Blackmail(PlayerControl target, ActionHandle handle)
    {
        if (target.PlayerId == MyPlayer.PlayerId) return;
        handle.Cancel();
        blackmailingText?.Delete();
        blackmailedPlayer = Optional<PlayerControl>.NonNull(target);
        TextComponent textComponent = new(new LiveString(BlackmailedText, Color.red), GameStates.IgnStates, viewers: MyPlayer);
        blackmailingText = target.NameModel().GetComponentHolder<TextHolder>().Add(textComponent);
    }

    [RoleAction(RoleActionType.RoundStart, triggerAfterDeath: true)]
    public void ClearBlackmail()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        currentWarnings = 0;
        blackmailingText?.Delete();
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    public void NotifyBlackmailed()
    {
        List<PlayerControl> allPlayers = showBlackmailedToAll
            ? Game.GetAllPlayers().ToList()
            : blackmailedPlayer.Transform(p => new List<PlayerControl> { p, MyPlayer }, () => new List<PlayerControl> { MyPlayer });
        if (!blackmailingText?.IsDeleted() ?? false) blackmailingText?.Get().SetViewerSupplier(() => allPlayers);
        blackmailedPlayer.IfPresent(p =>
        {
            string message = $"{RoleColor.Colorize(MyPlayer.name)} blackmailed {p.GetRoleColor().Colorize(p.name)}.";
            Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, p, message));
            ChatHandler.Of(BlackmailedMessage, RoleColor.Colorize(RoleName)).Send(p);
        });
    }

    [RoleAction(RoleActionType.SelfExiled)]
    [RoleAction(RoleActionType.MyDeath)]
    private void BlackmailerDies()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        blackmailingText?.Delete();
    }

    [RoleAction(RoleActionType.Chat)]
    public void InterceptChat(PlayerControl speaker, GameState state, bool isAlive)
    {
        if (!isAlive || state is not GameState.InMeeting) return;
        if (!blackmailedPlayer.Exists() || speaker.PlayerId != blackmailedPlayer.Get().PlayerId) return;
        if (currentWarnings++ < warnsUntilKick)
        {
            ChatHandler.Of(WarningMessage, RoleColor.Colorize(RoleName)).Send(speaker);
            return;
        }

        VentLogger.Trace($"Blackmailer Killing Player: {speaker.name}");
        MyPlayer.InteractWith(speaker, new UnblockedInteraction(new FatalIntent(), this));
    }

    public override void HandleDisconnect() => ClearBlackmail();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Warnings Until Death", Translations.Options.WarningsUntilDeath)
                .AddIntRange(0, 5, 1)
                .BindInt(i => warnsUntilKick = i)
                .Build())
            .SubOption(sub => sub.KeyName("Show Blackmailed to All", Translations.Options.ShowBlackmailedToAll)
                .AddOnOffValues()
                .BindBool(b => showBlackmailedToAll = b)
                .Build());

    [Localized(nameof(Blackmailer))]
    internal static class Translations
    {
        [Localized(nameof(BlackmailedMessage))]
        public static string BlackmailedMessage = "You have been blackmailed! Sending a chat message will kill you.";

        [Localized(nameof(WarningMessage))]
        public static string WarningMessage = "You are not allowed to speak! If you speak again you may be killed.";

        [Localized(nameof(BlackmailedText))]
        public static string BlackmailedText = "BLACKMAILED";

        [Localized(ModConstants.Options)]
        internal static class Options
        {
            [Localized(nameof(WarningsUntilDeath))]
            public static string WarningsUntilDeath = "Warnings Until Death";

            [Localized(nameof(ShowBlackmailedToAll))]
            public static string ShowBlackmailedToAll = "SHow Blackmailed to All";
        }
    }
}