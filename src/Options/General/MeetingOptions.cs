using System.Collections.Generic;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Options.General;

[Localized(ModConstants.Options)]
public class MeetingOptions
{
    private static Color _optionColor = new(0.27f, 0.75f, 1f);
    private static List<GameOption> additionalOptions = new();

    public int MeetingButtonPool = -1;
    public bool SyncMeetingButtons => MeetingButtonPool != -1;
    public ResolveTieMode ResolveTieMode;
    public SkipVoteMode NoVoteMode;

    public List<GameOption> AllOptions = new();

    public MeetingOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(MeetingOptionTranslations.SectionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build());

        AllOptions.Add(Builder("Single Meeting Pool")
            .IsHeader(true)
            .Name(MeetingOptionTranslations.SingleMeetingPool)
            .BindInt(i => MeetingButtonPool= i)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(-1).Build())
            .AddIntRange(1, 30)
            .BuildAndRegister());

        AllOptions.Add(Builder("Resolve Tie Mode")
            .Name(MeetingOptionTranslations.ResolveTieMode)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(0).Build())
            .Value(v => v.Text(MeetingOptionTranslations.RandomPlayer).Color(ModConstants.Palette.InfinityColor).Value(1).Build())
            .Value(v => v.Text(MeetingOptionTranslations.KillAll).Color(ModConstants.Palette.GeneralColor4).Value(2).Build())
            .BindInt(i => ResolveTieMode = (ResolveTieMode)i)
            .BuildAndRegister());

        AllOptions.Add(Builder("No Vote Mode")
            .Name(MeetingOptionTranslations.SkipVoteMode)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(0).Build())
            .Value(v => v.Text(MeetingOptionTranslations.RandomVote).Color(ModConstants.Palette.InfinityColor).Value(1).Build())
            .Value(v => v.Text(MeetingOptionTranslations.ReverseVote).Color(new Color(0.55f, 0.73f, 1f)).Value(2).Build())
            .Value(v => v.Text(MeetingOptionTranslations.ExplodeOnSkip).Color(new Color(1f, 0.4f, 0.2f)).Value(3).Build())
            .BindInt(i => NoVoteMode = (SkipVoteMode)i)
            .BuildAndRegister());

        additionalOptions.ForEach(o =>
        {
            o.Register();
            AllOptions.Add(o);
        });
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor);

    [Localized("Meetings")]
    public static class MeetingOptionTranslations
    {
        [Localized(nameof(ButtonsRemainingMessage))]
        public static string ButtonsRemainingMessage = "There are {0} emergency buttons remaining.";

        [Localized(nameof(RandomPlayer))]
        public static string RandomPlayer = "Random Player";

        [Localized(nameof(KillAll))]
        public static string KillAll = "Kill All";

        [Localized(nameof(MeetingOptions))]
        public static string SectionTitle = "Meeting Options";

        [Localized(nameof(SingleMeetingPool))]
        public static string SingleMeetingPool = "Single Meeting Pool";

        [Localized(nameof(ResolveTieMode))]
        public static string ResolveTieMode = "Resolve Tie Mode";

        [Localized(nameof(SkipVoteMode))]
        public static string SkipVoteMode = "No Vote Mode";

        [Localized(nameof(ExplodeOnSkip))]
        public static string ExplodeOnSkip = "Explode";

        [Localized(nameof(ReverseVote))]
        public static string ReverseVote = "Self";

        [Localized(nameof(NegateVote))]
        public static string NegateVote = "Negate";

        [Localized(nameof(RandomVote))]
        public static string RandomVote = "Random";
    }
}

public enum ResolveTieMode
{
    None,
    Random,
    KillAll
}

public enum SkipVoteMode
{
    None,
    Random,
    Reverse,
    Explode
}