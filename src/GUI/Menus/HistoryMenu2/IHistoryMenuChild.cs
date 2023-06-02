using VentLib.Localization.Attributes;

namespace Lotus.GUI.Menus.HistoryMenu2;

[Localized("GUI.HistoryMenu")]
public interface IHistoryMenuChild
{
    public PassiveButton CreateTabButton(PassiveButton prefab);
    
    public void Open();

    public void Close();
}