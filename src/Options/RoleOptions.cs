using Lotus.Options.LotusImpl.Roles;
using VentLib.Localization.Attributes;

namespace Lotus.Options;

[Localized(ModConstants.Options)]
public class RoleOptions
{
    public static LotusImpostorOptions ImpostorOptions = new LotusImpostorOptions();
    public static LotusCrewmateOption CrewmateOptions = new LotusCrewmateOption();
    public static LotusMadmateOptions MadmateOptions = new LotusMadmateOptions();
    public static LotusNeutralOptions NeutralOptions = new LotusNeutralOptions();
    public static LotusModifierOptions SubroleOptions = new LotusModifierOptions();
}