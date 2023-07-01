using VentLib.Localization.Attributes;

namespace Lotus.Options.LotusImpl;

public class LotusMeetingOptions: LotusOptionModel
{
    public SkipVoteMode NoVoteMode;
    public ResolveTieMode ResolveTieMode;

    public int MeetingButtonPool = -1;
    public bool SyncMeetingButtons => MeetingButtonPool != -1;

    [Localized("Meetings")]
    public static class MeetingOptionTranslations
    {
        [Localized(nameof(ButtonsRemainingMessage))]
        public static string ButtonsRemainingMessage = "There are {0} emergency buttons remaining.";

        [Localized(nameof(RandomPlayer))]
        public static string RandomPlayer = "Random Player";

        [Localized(nameof(KillAll))]
        public static string KillAll = "Kill All";

        [Localized("Meeting Options")]
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

public enum SkipVoteMode
{
    None,
    Random,
    Reverse,
    Explode
}

public enum ResolveTieMode
{
    None,
    Random,
    KillAll
}