using System.Collections.Generic;
using Lotus.Extensions;
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
    public bool ExplodeOnSkip;
    
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
        
        AllOptions.Add(Builder("Explode on Skip")
            .Name(MeetingOptionTranslations.ExplodeOnSkip)
            .AddOnOffValues(false)
            .BindBool(b => ExplodeOnSkip = b)
            .BuildAndRegister());
        
        additionalOptions.ForEach(o =>
        {
            o.Register();
            AllOptions.Add(o);
        });
    }
    
    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor);
    
    private static class MeetingOptionTranslations
    {
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

        [Localized(nameof(ExplodeOnSkip))]
        public static string ExplodeOnSkip = "Explode On Skip";
    }
}

public enum ResolveTieMode
{
    None,
    Random,
    KillAll
}