using Lotus.Roles2.Interfaces;
using Lotus.Utilities;

namespace Lotus.Roles2.GUI;

public interface RoleGUI: IRoleComponent
{
    public RoleButton PetButton(RoleButtonEditor petButton) => petButton.Default(false);

    public RoleButton UseButton(RoleButtonEditor useButton) => useButton.Default(false);

    public RoleButton ReportButton(RoleButtonEditor reportButton) => reportButton.Default(false);

    public RoleButton VentButton(RoleButtonEditor ventButton) => ventButton.Default(false);

    public RoleButton KillButton(RoleButtonEditor killButton) => killButton.Default(false);

    public RoleButton AbilityButton(RoleButtonEditor abilityButton) => abilityButton.Default(false);

    public void SpriteBindings(AssetRegistry registry) {}

    public void UpdateGUI(GUIProvider guiProvider) { }
}