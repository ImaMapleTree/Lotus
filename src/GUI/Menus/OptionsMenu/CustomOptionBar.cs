using System;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Utilities.Attributes;

namespace TOHTOR.GUI.Menus.OptionsMenu;

[RegisterInIl2Cpp]
public class CustomOptionBar: MonoBehaviour
{
    public PassiveButton GeneralSettings;
    public PassiveButton DisplaySettings;
    public PassiveButton SoundSettings;
    public PassiveButton VentLibSettings;
    public PassiveButton AddonSettings;

    public GameObject GameObject;

    private const int ButtonPpu = 350;

    public CustomOptionBar(IntPtr intPtr) : base(intPtr)
    {
        GeneralSettings = gameObject.AddComponent<PassiveButton>();
        GeneralSettings.GetComponentInChildren<SpriteRenderer>().sprite = Utils.LoadSprite("TOHTOR.assets.Settings.GeneralButton.png", ButtonPpu);

        DisplaySettings = gameObject.AddComponent<PassiveButton>();
        DisplaySettings.GetComponentInChildren<SpriteRenderer>().sprite = Utils.LoadSprite("TOHTOR.assets.Settings.GraphicsButton.png", ButtonPpu);

        SoundSettings = gameObject.AddComponent<PassiveButton>();
        SoundSettings.GetComponentInChildren<SpriteRenderer>().sprite = Utils.LoadSprite("TOHTOR.assets.Settings.SoundButton.png", ButtonPpu);

        VentLibSettings = gameObject.AddComponent<PassiveButton>();
        VentLibSettings.GetComponentInChildren<SpriteRenderer>().sprite = Utils.LoadSprite("TOHTOR.assets.Settings.VentButton.png", 300);

        AddonSettings = gameObject.AddComponent<PassiveButton>();
        AddonSettings.GetComponentInChildren<SpriteRenderer>().sprite = Utils.LoadSprite("TOHTOR.assets.Settings.AddonButton.png", ButtonPpu);
        DisplaySettings.transform.localPosition += new Vector3(0f, -1f);
    }
}