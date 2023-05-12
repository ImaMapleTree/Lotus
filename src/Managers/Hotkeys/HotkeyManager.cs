using System.Collections.Generic;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace TOHTOR.Managers.Hotkeys;

public class HotkeyManager
{
    private static readonly List<Hotkey> Hotkeys = new();
    
    [QuickPostfix(typeof(ControllerManager), nameof(ControllerManager.Update))]
    private static void DoHotkeyCheck()
    {
        Hotkeys.ForEach(h => h.Update());
    }

    public static Hotkey AddHokey(Hotkey hotkey)
    {
        Hotkeys.Add(hotkey);
        return hotkey;
    }

    public static Hotkey Bind(params KeyCode[] keyCodes)
    {
        return AddHokey(Hotkey.When(keyCodes));
    }
}

