using System;
using UnityEngine.UI;

namespace Lotus.Extensions;

public static class PassiveButtonExtension
{
    public static void Modify(this PassiveButton passiveButton, Action action)
    {
        if (passiveButton == null) return;
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(action);
    } 
}