using System;
using Lotus.GUI.Menus.OptionsMenu;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Components;

[RegisterInIl2Cpp]
public class LazyProgressBar: MonoBehaviour
{
    public SpriteRenderer FullBar;
    private GameObject FullObject;
    
    public SpriteRenderer Layering;
    private GameObject LayeringObject;

    public ProgressBar ProgressBar;
    
    public LazyProgressBar(IntPtr intPtr) : base(intPtr)
    {
        FullObject = gameObject.CreateChild("Full Bar");
        FullBar = FullObject.AddComponent<SpriteRenderer>();
        FullBar.sprite = OptionMenuResources.ProgressBarFull;
        FullObject.transform.localPosition += new Vector3(0f, 0f, -2f);
        
        LayeringObject = FullObject.CreateChild("Layered Bar");
        Layering = LayeringObject.AddComponent<SpriteRenderer>();
        ProgressBar = FullObject.AddComponent<ProgressBar>();
        Layering.sprite = OptionMenuResources.ProgressBarMask;
        ProgressBar.Mask = Layering;
        ProgressBar.cap = FullBar;
        ProgressBar.Value = 0f;
    }
}