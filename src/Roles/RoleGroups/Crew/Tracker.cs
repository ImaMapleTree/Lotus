using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Trackers;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Tracker: Crewmate
{
    private TrackBodyValue canTrackBodies;
    private bool canTrackUnreportableBodies;
    private float arrowUpdateRate;

    private Cooldown trackBodyCooldown;
    private Cooldown trackBodyDuration;

    private byte trackedPlayer = byte.MaxValue;
    private MeetingPlayerSelector meetingPlayerSelector = new();
    private FixedUpdateLock fixedUpdateLock;

    private string arrowCache = "";

    [UIComponent(UI.Indicator)]
    public string DisplayArrow()
    {
        PlayerControl? tracker = Players.FindPlayerById(trackedPlayer);
        if (tracker == null) return "";
        if (arrowUpdateRate == 0 || fixedUpdateLock.AcquireLock())
            return arrowCache = $"<size=3>{RoleUtils.CalculateArrow(MyPlayer, tracker, RoleColor)}</size>";
        return arrowCache;
    }

    [UIComponent(UI.Indicator)]
    public string DisplayDeadBodies()
    {
        if (canTrackBodies is TrackBodyValue.Never) return "";
        if (canTrackBodies is TrackBodyValue.OnPet && trackBodyDuration.IsReady()) return "";
        return Object.FindObjectsOfType<DeadBody>()
            .Where(db => canTrackUnreportableBodies || !Game.MatchData.UnreportableBodies.Contains(db.ParentId))
            .Select(db => RoleUtils.CalculateArrow(MyPlayer, db.TruePosition, Color.gray))
            .Fuse();
    }

    [UIComponent(UI.Cooldown)]
    public string TrackBodyCooldown() => trackBodyDuration.Duration > 0 ? trackBodyCooldown.Format("{0}", true) : "";

    protected override void PostSetup()
    {
        fixedUpdateLock = new FixedUpdateLock(arrowUpdateRate);
    }

    [RoleAction(RoleActionType.MyVote)]
    public void SelectTrackedPlayer(Optional<PlayerControl> player, ActionHandle handle)
    {
        VoteResult result = meetingPlayerSelector.CastVote(player);
        if (result.VoteResultType is not VoteResultType.None) handle.Cancel();
        if (result.VoteResultType is VoteResultType.Confirmed) trackedPlayer = result.Selected;
        if (result.VoteResultType is not VoteResultType.Skipped)
            result.Message().Title(RoleColor.Colorize(RoleName)).Send(MyPlayer);
    }

    [RoleAction(RoleActionType.OnPet)]
    public void TrackDeadBodies()
    {
        if (canTrackBodies is not TrackBodyValue.OnPet) return;
        if (trackBodyCooldown.NotReady() || trackBodyDuration.NotReady()) return;
        trackBodyDuration.Start();
        Async.Schedule(() => trackBodyCooldown.Start(), trackBodyDuration.Duration);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    public void ResetTrackedPlayer()
    {
        meetingPlayerSelector.Reset();
        Async.Schedule(() => ChatHandler.Of(Translations.TrackerMessage, RoleColor.Colorize(RoleName)).Send(MyPlayer), 2f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Arrow Update Rate", Translations.Options.ArrowUpdateRate)
                .Value(v => v.Text(Translations.Options.RealtimeText).Value(0f).Build())
                .AddFloatRange(0.25f, 10, 0.25f, 4, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => arrowUpdateRate = f)
                .Build())
            .SubOption(sub => sub.KeyName("Track Bodies", Translations.Options.CanTrackBodies)
                .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(0).Build())
                .Value(v => v.Text(Translations.Options.OnPetText).Color(new Color(0.73f, 0.58f, 1f)).Value(1).Build())
                .Value(v => v.Text(GeneralOptionTranslations.AlwaysText).Color(Color.green).Value(2).Build())
                .BindInt(i => canTrackBodies = (TrackBodyValue)i)
                .ShowSubOptionPredicate(i => (int)i == 1)
                .SubOption(sub2 => sub2.KeyName("Can Track Unreportable Bodies", Translations.Options.CanTrackUnreportableBodies)
                    .BindBool(b => canTrackUnreportableBodies = b)
                    .AddOnOffValues()
                    .Build())
                .SubOption(sub2 => sub2.KeyName("Track Body Duration", Translations.Options.TrackBodyDuration)
                    .AddFloatRange(2.5f, 120f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                    .BindFloat(trackBodyDuration.SetDuration)
                    .Build())
                .SubOption(sub2 => sub2.KeyName("Track Body Cooldown", Translations.Options.TrackBodyCooldown)
                    .AddFloatRange(0, 120f, 2.5f, 16, GeneralOptionTranslations.SecondsSuffix)
                    .BindFloat(trackBodyCooldown.SetDuration)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.82f, 0.24f, 0.82f));

    private static class Translations
    {
        [Localized(nameof(TrackerMessage))]
        public static string TrackerMessage ="You are a Tracker. Select a player each meeting (by voting them twice) to track them. After meeting, you will have an arrow point towards your tracked player.";

        public static class Options
        {
            [Localized(nameof(ArrowUpdateRate))]
            public static string ArrowUpdateRate = "Arrow Update Rate";

            [Localized(nameof(RealtimeText))]
            public static string RealtimeText = "Realtime";

            [Localized(nameof(CanTrackBodies))]
            public static string CanTrackBodies = "Can Track Bodies";

            [Localized(nameof(CanTrackUnreportableBodies))]
            public static string CanTrackUnreportableBodies = "Can Track Unreportable Bodies";

            [Localized(nameof(TrackBodyCooldown))]
            public static string TrackBodyCooldown = "Track Body Cooldown";

            [Localized(nameof(TrackBodyDuration))]
            public static string TrackBodyDuration = "Track Body Duration";

            [Localized(nameof(OnPetText))]
            public static string OnPetText = "On Pet";
        }
    }

    private enum TrackBodyValue
    {
        Never,
        OnPet,
        Always
    }
}