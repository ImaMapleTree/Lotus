using System;
using UnityEngine;
using VentLib.Utilities.Attributes;

namespace TOHTOR.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class SoundMenu: MonoBehaviour, IBaseOptionMenuComponent
{
    public SoundMenu(IntPtr intPtr) : base(intPtr)
    {
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}