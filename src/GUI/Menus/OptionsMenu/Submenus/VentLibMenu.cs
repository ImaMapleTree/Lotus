using System;
using Lotus.GUI.Menus.OptionsMenu.Components;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using UnityEngine.Events;
using VentLib.Logging;
using VentLib.Networking;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class VentLibMenu: MonoBehaviour, IBaseOptionMenuComponent
{
    private MonoToggleButton allowLobbySending;
    private TextMeshPro allowLobbySendingText;

    private SlideBar maxPacketSizeSlider;
    private TextMeshPro maxPacketSizeLabel;
    private TextMeshPro maxPacketSizeValue;

    private TextMeshPro menuTitle;

    private GameObject anchorObject;

    public VentLibMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("Anchor");
        menuTitle = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        menuTitle.font = CustomOptionContainer.GetGeneralFont();
        menuTitle.transform.localPosition += new Vector3(0.95f, 1.75f);
    }


    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        maxPacketSizeSlider = Instantiate(menuBehaviour.MusicSlider, anchorObject.transform);
        maxPacketSizeSlider.transform.localScale = new Vector3(1.1f, 1.2f, 1f);

        maxPacketSizeLabel = maxPacketSizeSlider.GetComponentInChildren<TextMeshPro>();
        maxPacketSizeLabel.transform.localPosition += new Vector3(0.7f, 0.25f);
        maxPacketSizeValue = Instantiate(maxPacketSizeLabel, maxPacketSizeSlider.transform);

        maxPacketSizeValue.transform.localPosition += new Vector3(4f, 0.25f);
        maxPacketSizeSlider.OnValueChange = new UnityEvent();
        maxPacketSizeSlider.OnValueChange.AddListener((Action)(() =>
        {
            int packetSize = NetworkRules.MaxPacketSize = Mathf.FloorToInt((maxPacketSizeSlider.Value * (NetworkRules.AbsoluteMaxPacketSize - NetworkRules.AbsoluteMinPacketSize))) + NetworkRules.AbsoluteMinPacketSize;
            maxPacketSizeValue.text = packetSize.ToString();
        }));
        maxPacketSizeSlider.transform.localPosition += new Vector3(0.27f, 0.9f);

        GameObject lobbyObject = anchorObject.CreateChild("Lobby Object");
        allowLobbySending = lobbyObject.AddComponent<MonoToggleButton>();

        allowLobbySendingText = Instantiate(FindObjectOfType<TextMeshPro>(), allowLobbySending.transform);
        allowLobbySendingText.font = CustomOptionContainer.GetGeneralFont();

        allowLobbySending.SetOnText("ON");
        allowLobbySending.SetOffText("OFF");
        allowLobbySending.SetToggleOnAction(() => NetworkRules.AllowRoomDiscovery = true);
        allowLobbySending.SetToggleOffAction(() => NetworkRules.AllowRoomDiscovery = false);
        allowLobbySendingText.transform.localPosition -= new Vector3(3.5f, 0.5f);
        allowLobbySending.SetState(NetworkRules.AllowRoomDiscovery);
        lobbyObject.transform.localPosition += new Vector3(2.3f, 1.25f);

        anchorObject.SetActive(false);
    }

    private void Start()
    {
        menuTitle.text = "Networking";
    }


    public void Open()
    {
        anchorObject.SetActive(true);
        maxPacketSizeSlider.SetValue((float)NetworkRules.MaxPacketSize / NetworkRules.AbsoluteMaxPacketSize);
        Async.Schedule(() =>
        {
            maxPacketSizeLabel.text = "Max Packet Size";
            maxPacketSizeValue.text = NetworkRules.MaxPacketSize.ToString();
            allowLobbySendingText.text = "Open Rooms To Discovery";
            allowLobbySendingText.color = Color.white;
        }, 0.00001f);
    }

    public void Close()
    {
        anchorObject.SetActive(false);
    }
}