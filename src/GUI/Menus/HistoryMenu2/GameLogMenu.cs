using System;
using System.Linq;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Menus.HistoryMenu2;

[RegisterInIl2Cpp]
public class GameLogMenu : MonoBehaviour, IHistoryMenuChild
{
    private GameObject tabIconObject;
    private PassiveButton tabButton = null!;
    private SpriteRenderer tabButtonRenderer = null!;
    
    public GameLogMenu(IntPtr intPtr) : base(intPtr)
    {
    }

    public PassiveButton CreateTabButton(PassiveButton prefab)
    {
        tabButton = Instantiate(prefab, tabIconObject.transform);
        tabButtonRenderer = tabButton.GetComponentsInChildren<SpriteRenderer>().Last();
        tabButtonRenderer.sprite = AssetLoader.LoadLotusSprite("HistoryMenu.ResultsIcon.png", 100, true);
        tabButton.transform.localPosition += new Vector3(-6.9f, 4.102f);
        return tabButton;
    }

    public void Open()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }
}