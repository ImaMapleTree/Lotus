using TOHTOR.Options.Roles;
using VentLib.Localization.Attributes;

namespace TOHTOR.Options;

[Localized("Options")]
public class RoleOptions
{
    public static MadmateOptions MadmateOptions;
    public static NeutralOptions NeutralOptions;

    internal static MadmateOptions LoadMadmateOptions() => MadmateOptions ??= new MadmateOptions();

    internal static NeutralOptions LoadNeutralOptions() => NeutralOptions ??= new NeutralOptions();
}