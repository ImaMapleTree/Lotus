using VentLib.Localization;

namespace Lotus.Roles2;

public class DefaultRoleLocalizer: RoleLocalizer
{
    public override string ProvideTranslation(string qualifier, string? untranslatedText)
    {
        return untranslatedText == null ? Localizer.Translate(qualifier, assembly: SourceAssembly) : Localizer.Translate(qualifier, untranslatedText ?? "<{}>", assembly: SourceAssembly, translationCreationOption: TranslationCreationOption.SaveIfNull);
    }
}