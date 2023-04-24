using TOHTOR.Options.General;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Attributes;

namespace TOHTOR.Options;

[Localized("Options")]
[LoadStatic]
public class GeneralOptions
{
    public static AdminOptions AdminOptions;
    public static DebugOptions DebugOptions;
    public static GameplayOptions GameplayOptions;
    public static MayhemOptions MayhemOptions;
    public static MiscellaneousOptions MiscellaneousOptions;
    public static SabotageOptions SabotageOptions;
    public static VoteOptions VoteOptions;

    static GeneralOptions()
    {
        AdminOptions = new AdminOptions();
        GameplayOptions = new GameplayOptions();
        SabotageOptions = new SabotageOptions();
        MayhemOptions = new MayhemOptions();
        MiscellaneousOptions = new MiscellaneousOptions();
        DebugOptions = new DebugOptions();
    }
}