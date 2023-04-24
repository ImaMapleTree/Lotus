using System;
using TMPro;
using TOHTOR.GUI.Menus.OptionsMenu.Components;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class GeneralMenu : MonoBehaviour, IBaseOptionMenuComponent
{
    private TextMeshPro title;
    private TextMeshPro controlsText;


    private MonoToggleButton censorChatButton;
    private MonoToggleButton friendInviteButton;
    private MonoToggleButton colorblindTextButton;
    private MonoToggleButton streamerModeButton;

    private MonoToggleButton mouseMovementButton;
    private MonoToggleButton changeKeyBindingButton;

    private TiledToggleButton controlScheme;

    private GameObject anchorObject;

    public GeneralMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = new GameObject();
        anchorObject.transform.SetParent(transform);
        anchorObject.transform.localPosition += new Vector3(2f, 2f);
        anchorObject.transform.localScale = new Vector3(1f, 1f, 1);

        title = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        title.font = CustomOptionContainer.GetGeneralFont();

        title.transform.localPosition += new Vector3(-3.3f, 0.2f);
    }

    private void Awake()
    {
        title.text = "General";
    }

    private void Start()
    {
        title.text = "General";
        title.gameObject.SetActive(false);
        controlsText.text = "Controls";
    }

    public void PassMenu(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        GameObject censorGameObject = new("Censor Button");
        censorGameObject.transform.SetParent(anchorObject.transform);
        censorGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        censorChatButton = censorGameObject.AddComponent<MonoToggleButton>();
        censorChatButton.SetOnText("Censor Chat: ON");
        censorChatButton.SetOffText("Censor Chat: OFF");
        censorChatButton.SetToggleOnAction(() => optionsMenuBehaviour.CensorChatButton.UpdateText(true));
        censorChatButton.SetToggleOffAction(() => optionsMenuBehaviour.CensorChatButton.UpdateText(false));
        censorChatButton.SetState(optionsMenuBehaviour.CensorChatButton.onState);
        censorGameObject.transform.localPosition += new Vector3(0.5f, 0.25f);

        optionsMenuBehaviour.CensorChatButton.gameObject.SetActive(false);

        GameObject fIGameObject = new("Friend & Invite Button");
        fIGameObject.transform.SetParent(anchorObject.transform);
        fIGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        friendInviteButton = fIGameObject.AddComponent<MonoToggleButton>();
        friendInviteButton.SetOnText("Friend & Lobby Invites: ON");
        friendInviteButton.SetOffText("Friend & Lobby Invites: OFF");
        friendInviteButton.SetToggleOnAction(() => optionsMenuBehaviour.EnableFriendInvitesButton.UpdateText(true));
        friendInviteButton.SetToggleOffAction(() => optionsMenuBehaviour.EnableFriendInvitesButton.UpdateText(false));
        friendInviteButton.SetState(optionsMenuBehaviour.EnableFriendInvitesButton.onState);
        fIGameObject.transform.localPosition += new Vector3(0.5f, -1.25f);

        optionsMenuBehaviour.EnableFriendInvitesButton.gameObject.SetActive(false);

        GameObject colorblindGameObject = new("Colorblind Button");
        colorblindGameObject.transform.SetParent(anchorObject.transform);
        colorblindGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        colorblindTextButton = colorblindGameObject.AddComponent<MonoToggleButton>();
        colorblindTextButton.SetOnText("Colorblind Mode: ON");
        colorblindTextButton.SetOffText("Colorblind Mode: OFF");
        colorblindTextButton.SetToggleOnAction(() => optionsMenuBehaviour.ColorBlindButton.UpdateText(true));
        colorblindTextButton.SetToggleOffAction(() => optionsMenuBehaviour.ColorBlindButton.UpdateText(false));
        colorblindTextButton.SetState(optionsMenuBehaviour.ColorBlindButton.onState);
        colorblindGameObject.transform.localPosition += new Vector3(0.5f, -0.25f);

        optionsMenuBehaviour.ColorBlindButton.gameObject.SetActive(false);

        GameObject streamerGameObject = new("Streamer Mode Button");
        streamerGameObject.transform.SetParent(anchorObject.transform);
        streamerGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        streamerModeButton = streamerGameObject.AddComponent<MonoToggleButton>();
        streamerModeButton.SetOnText("Streamer Mode: ON");
        streamerModeButton.SetOffText("Streamer Mode: OFF");
        streamerModeButton.SetToggleOnAction(() => optionsMenuBehaviour.StreamerModeButton.UpdateText(true));
        streamerModeButton.SetToggleOffAction(() => optionsMenuBehaviour.StreamerModeButton.UpdateText(false));
        streamerModeButton.SetState(optionsMenuBehaviour.StreamerModeButton.onState);
        streamerGameObject.transform.localPosition += new Vector3(0.5f, -0.75f);

        optionsMenuBehaviour.StreamerModeButton.gameObject.SetActive(false);


        GameObject controlGameObject = new GameObject("Control Scheme Button");
        controlsText = Instantiate(title, controlGameObject.transform);
        controlsText.transform.localPosition += new Vector3(-1.3f, -0.2f);

        controlGameObject.transform.SetParent(anchorObject.transform);
        controlGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        controlScheme = controlGameObject.AddComponent<TiledToggleButton>();
        controlScheme.SetLeftButtonText("Mouse");
        controlScheme.SetRightButtonText("Mouse & Keyboard");
        controlGameObject.transform.localPosition += new Vector3(2.25f, 2f);
        optionsMenuBehaviour.MouseAndKeyboardOptions.gameObject.SetActive(false);

        // ==========================================
        //     Mouse Movement Button
        // =============================================
        GameObject mouseMovementObject = new("Mouse Movement Button");
        mouseMovementObject.transform.SetParent(anchorObject.transform);
        mouseMovementObject.transform.localScale = new Vector3(1f, 1f, 1f);
        mouseMovementButton = mouseMovementObject.AddComponent<MonoToggleButton>();
        mouseMovementButton.SetOnText("Mouse Movement: ON");
        mouseMovementButton.SetOffText("Mouse Movement: OFF");
        mouseMovementButton.SetToggleOnAction(() => optionsMenuBehaviour.DisableMouseMovement.UpdateText(true));
        mouseMovementButton.SetToggleOffAction(() => optionsMenuBehaviour.DisableMouseMovement.UpdateText(false));
        mouseMovementButton.SetState(optionsMenuBehaviour.DisableMouseMovement.onState);
        mouseMovementObject.transform.localPosition += new Vector3(1f, 1.5f);
        optionsMenuBehaviour.DisableMouseMovement.gameObject.SetActive(false);


        // ==========================================
        //     Keybinding Button
        // =============================================
        GameObject keybindingObject = new("Keybinding Button");
        keybindingObject.transform.SetParent(anchorObject.transform);
        keybindingObject.transform.localScale = new Vector3(1f, 1f, 1f);
        changeKeyBindingButton = keybindingObject.AddComponent<MonoToggleButton>();
        changeKeyBindingButton.SetOnText("Change Keybindings");
        changeKeyBindingButton.SetOffText("Change Keybindings");
        changeKeyBindingButton.SetToggleOnAction(() =>
        {
            optionsMenuBehaviour.KeyboardOptions.GetComponentInChildren<PassiveButton>().ReceiveClickDown();
            changeKeyBindingButton.SetState(false, true);
        });
        keybindingObject.transform.localPosition += new Vector3(3.5f, 1.5f);
        optionsMenuBehaviour.KeyboardOptions.SetActive(false);
        optionsMenuBehaviour.MouseAndKeyboardOptions.GetComponentsInChildren<Component>().ForEach(c => c.gameObject.SetActive(false));
    }


    public void Open()
    {
        VentLogger.Fatal("Opening!!");
        anchorObject.SetActive(true);
    }

    public void Close()
    {
        anchorObject.SetActive(false);
    }
}