using System.Collections.Generic;
using Lotus;
using Lotus.Options.LotusImpl;
using LotusTrigger.Options.General;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Attributes;

namespace LotusTrigger.Options;

[Localized(ModConstants.Options)]
[LoadStatic]
public class GeneralOptions
{
    public static AdminOptions AdminOptions;
    public static LotusDebugOptions DebugOptions;
    public static LotusGameplayOptions GameplayOptions;
    public static LotusMayhemOptions MayhemOptions;
    public static LotusMeetingOptions MeetingOptions;
    public static LotusMiscellaneousOptions MiscellaneousOptions;
    public static LotusSabotageOptions SabotageOptions;

    public static List<GameOption> AllOptions = new();
    private static List<GameOption> deferredList = new();

    private static bool finished;

    static GeneralOptions()
    {
        AdminOptions = new AdminOptions();
        GameplayOptions = new LotusGameplayOptions();
        MayhemOptions = new LotusMayhemOptions();
        MeetingOptions = new LotusMeetingOptions();
        MiscellaneousOptions = new LotusMiscellaneousOptions();
        SabotageOptions = new LotusSabotageOptions();
        DebugOptions = new LotusDebugOptions();
    }

    public static void AddOption(GameOption option)
    {
        if (finished) AllOptions.Add(option);
        else deferredList.Add(option);
    }

    public static void AddOptions(IEnumerable<GameOption> options)
    {
        if (finished) AllOptions.AddRange(options);
        else deferredList.AddRange(options);
    }

    public static void Finish()
    {
        finished = true;
        AllOptions.AddRange(GameplayOptions.AllOptions);
        AllOptions.AddRange(SabotageOptions.AllOptions);
        AllOptions.AddRange(MeetingOptions.AllOptions);
        AllOptions.AddRange(MayhemOptions.AllOptions);
        AllOptions.AddRange(MiscellaneousOptions.AllOptions);
        AllOptions.AddRange(DebugOptions.AllOptions);
    }
}