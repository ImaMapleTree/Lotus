namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

public interface IBaseOptionMenuComponent
{
    public void PassMenu(OptionsMenuBehaviour menuBehaviour);

    public void Open();

    public void Close();
}