using System;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Utilities.Attributes;

namespace TOHTOR.GUI.Menus.OptionsMenu.Components;

[RegisterInIl2Cpp]
public class SimpleDropdownButton: MonoBehaviour
{
    private GameObject anchorObject;
    private Dropdown dropdown;

    public SimpleDropdownButton(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = new GameObject("Anchor");
        anchorObject.transform.localScale = new Vector3(10f, 10f, 10f);
        anchorObject.transform.SetParent(transform);
        dropdown = anchorObject.AddComponent<Dropdown>();
    }

    private void Start()
    {
        Il2CppSystem.Collections.Generic.List<string> options = new();
        options.Add("480 x 1080");
        options.Add("900 x 3280");

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }
}