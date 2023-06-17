using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Utilities;
using Lotus.Victory;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles.Romantics;

public class Romantic: Subrole
{
    public static Color RomanticColor = new(1f, 0.28f, 0.47f);
    private static VengefulRomantic _vengefulRomantic = new();
    private static RuthlessRomantic _ruthlessRomantic = new();

    private bool targetKnowsRomantic;
    private Cooldown protectionCooldown = null!;
    private Cooldown protectionDuration = null!;

    private bool partnerLockedIn;
    private byte partner = byte.MaxValue;
    private IRemote? winDelegateRemote;
    private RomanticFaction myFaction = null!;
    private IFaction originalFaction = null!;

    public override string Identifier() => "♥";

    protected override void PostSetup()
    {
        DisplayOrder = 100;
        winDelegateRemote = Game.GetWinDelegate().AddSubscriber(InterceptWinCondition);
        CustomRole myRole = MyPlayer.GetCustomRole();
        originalFaction = myRole.Faction;
        myRole.Faction = myFaction = new RomanticFaction(originalFaction);
    }

    [RoleAction(RoleActionType.OnPet, priority: Priority.VeryHigh)]
    public void HandlePet(ActionHandle handle)
    {
        PlayerControl? love = Players.FindPlayerById(partner);
        if (love == null || !love.IsAlive()) return;
        if (protectionCooldown.NotReady() || protectionDuration.NotReady()) return;
        handle.Cancel();
        protectionDuration.Start();
        Async.Schedule(() => protectionCooldown.Start(), protectionDuration.Duration);
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    public void SendMeetingMessage()
    {
        if (partner != byte.MaxValue) return;
        Async.Schedule(() => RHandler(Translations.RomanticMessage).Send(MyPlayer), 1.5f);
    }

    [RoleAction(RoleActionType.AnyInteraction)]
    public void InterceptActions(PlayerControl _, PlayerControl target, Interaction interaction, ActionHandle handle)
    {
        if (protectionDuration.IsReady()) return;
        if (interaction.Intent is not IFatalIntent) return;
        if (target.PlayerId == partner) handle.Cancel();
    }

    [RoleAction(RoleActionType.AnyDeath)]
    private void CheckPartnerDeath(PlayerControl dead, PlayerControl killer)
    {
        if (dead.PlayerId != partner) return;
        MyPlayer.GetSubroles().Remove(this);
        winDelegateRemote?.Delete();
        if (RoleUtils.RandomSpawn(_ruthlessRomantic)) MatchData.AssignSubrole(MyPlayer, _ruthlessRomantic);
        else
        {
            MatchData.AssignSubrole(MyPlayer, _vengefulRomantic);
            CustomRole playerRole = MyPlayer.GetCustomRole();
            playerRole.Faction = FactionInstances.Neutral;
            playerRole.RoleFlags |= RoleFlag.CannotWinAlone;
            MyPlayer.GetSubrole<VengefulRomantic>()?.SetupVengeful(killer, originalFaction);
        }
    }

    [RoleAction(RoleActionType.MyVote, priority: Priority.High)]
    public void SetPartner(Optional<PlayerControl> votedPlayer, ActionHandle handle)
    {
        if (!partnerLockedIn) handle.Cancel();
        else return;
        if (!votedPlayer.Exists())
        {
            RHandler(Translations.NoSkipMessage).Send(MyPlayer);
            return;
        }

        PlayerControl player = votedPlayer.Get();

        byte votedId = player.PlayerId;

        if (votedId == MyPlayer.PlayerId) return;

        if (votedId != partner)
        {
            partner = votedId;
            RHandler(Translations.RomanticSelectMessage.Formatted(player.name)).Send(MyPlayer);
            return;
        }

        partnerLockedIn = true;
        myFaction.Partner = votedId;
        RHandler(Translations.ConfirmedPartnerMessage.Formatted(player.name)).Send(MyPlayer);

        string partnerText = TranslationUtil.Colorize(Translations.PartnerIndicator.Formatted(player.name), RoleColor);
        NameComponent nameComponent = new(new LiveString(partnerText), Game.IgnStates, ViewMode.Replace, MyPlayer);
        player.NameModel().GCH<NameHolder>().Add(nameComponent);
        LiveString protectionIndicator = new(() => protectionDuration.NotReady() ? RoleColor.Colorize(Identifier()) : "");
        player.NameModel().GCH<IndicatorHolder>().Add(new IndicatorComponent(protectionIndicator, GameState.Roaming, ViewMode.Additive, MyPlayer));

        if (!targetKnowsRomantic) return;

        string myText = TranslationUtil.Colorize(Translations.PartnerIndicator.Formatted(MyPlayer.name), RoleColor);
        nameComponent = new NameComponent(new LiveString(myText), Game.IgnStates, ViewMode.Replace, player);
        MyPlayer.NameModel().GCH<NameHolder>().Add(nameComponent);
        RHandler(Translations.NotifyPartnerMessage.Formatted(MyPlayer.name)).Send(player);
    }

    [RoleAction(RoleActionType.MeetingEnd)]
    public void KillIfUndecided()
    {
        if (partner == byte.MaxValue) ProtectedRpc.CheckMurder(MyPlayer, MyPlayer);
    }

    private void InterceptWinCondition(WinDelegate winDelegate)
    {
        Players.PlayerById(partner).IfPresent(p => p.GetSubroles().Insert(0, new Partner()));
        if (winDelegate.GetWinners().All(w => w.PlayerId != partner))
        {
            winDelegate.RemoveWinner(MyPlayer);
            return;
        }
        if (!Players.FindPlayerById(partner)?.IsAlive() ?? true) return;
        winDelegate.AddAdditionalWinner(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Notify Target of Romance", Translations.Options.TargetKnowsRomantic)
                .AddOnOffValues()
                .BindBool(b => targetKnowsRomantic = b)
                .Build())
            .SubOption(sub => sub.KeyName("Protection Cooldown", Translations.Options.ProtectionCooldown)
                .AddFloatRange(0, 120, 2.5f, 12, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(protectionCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.KeyName("Protection Duration", Translations.Options.ProtectionDuration)
                .AddFloatRange(0, 120, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(protectionDuration.SetDuration)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(RomanticColor)
            .LinkedRoles(_vengefulRomantic, _ruthlessRomantic);

    private ChatHandler RHandler(string message) => new ChatHandler().Title(t => t.PrefixSuffix(Identifier()).Color(RoleColor).Text(RoleName).Build()).LeftAlign().Message(message);

    [Localized(nameof(Romantic))]
    private static class Translations
    {
        [Localized(nameof(PartnerText))]
        public static string PartnerText = "Partner ♥";

        public static string PartnerIndicator = "{0} ♥::0";

        [Localized(nameof(RomanticMessage))]
        public static string RomanticMessage = "You are a Romantic. You must select a partner by the end of this meeting or die! To select a partner, vote them twice! Afterwards, you may vote normally.";

        [Localized(nameof(RomanticSelectMessage))]
        public static string RomanticSelectMessage = "You have selected {0} to be your partner. To confirm, vote them again, otherwise vote a different player.";

        [Localized(nameof(ConfirmedPartnerMessage))]
        public static string ConfirmedPartnerMessage = "You have confirmed {0} to be your partner. You may now vote normally";

        [Localized(nameof(NotifyPartnerMessage))]
        public static string NotifyPartnerMessage = "You have been selected by {0} to be their romantic partner! Congrats!";

        [Localized(nameof(NoSkipMessage))]
        public static string NoSkipMessage = "You may not skip until you have chosen a partner!";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(TargetKnowsRomantic))]
            public static string TargetKnowsRomantic = "Notify Target of Romance";

            [Localized(nameof(ProtectionCooldown))]
            public static string ProtectionCooldown = "Protection Cooldown";

            [Localized(nameof(ProtectionDuration))]
            public static string ProtectionDuration = "Protection Duration";
        }
    }
}