using System;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Attributes;

namespace TOHTOR.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class GraphicsMenu: MonoBehaviour, IBaseOptionMenuComponent
{
    private PassiveButton fullscreenButton;
    private PassiveButton vsyncButton;
    private PassiveButton screenshakeButton;
    private PassiveButton fpsLimitButton;
    private PassiveButton applyButton;


    public GraphicsMenu(IntPtr intPtr) : base(intPtr)
    {
    }

    public void PassMenu(OptionsMenuBehaviour optionsMenuBehaviour)
    {

        VentLogger.Fatal("Graphics Menu:");
    }

    public virtual void Open()
    {
    }

    public virtual void Close()
    {
    }
}