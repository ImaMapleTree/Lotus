using System;
using System.Linq;
using AmongUs.Data;
using Lotus.Extensions;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.Logging;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

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
    private MonoToggleButton languageButton;

    private TiledToggleButton controlScheme;
    private LanguageSetter languageSetter;

    private GameObject anchorObject;
    private bool languageSetterExists;

    public GeneralMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = new GameObject();
        anchorObject.transform.SetParent(transform);
        anchorObject.transform.localPosition += new Vector3(2f, 2f);
        anchorObject.transform.localScale = new Vector3(1f, 1f, 1);
    }


    private void Start()
    {
        title.text = "General";
        title.gameObject.SetActive(false);
        controlsText.text = "Controls";
    }

    private void Update()
    {
        if (!languageSetterExists) return;
        if (languageButton != null) languageButton.SetOffText(DataManager.Settings.language.language);
    }

    public void PassMenu(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        GameObject textGameObject = gameObject.CreateChild("Title", new Vector3(8.6f, -1.8f));
        title = textGameObject.AddComponent<TextMeshPro>();
        title.font = CustomOptionContainer.GetGeneralFont();
        title.fontSize = 5.35f;
        title.transform.localPosition += new Vector3(-3.3f, 0.2f);
        title.gameObject.layer = LayerMask.NameToLayer("UI");

        languageSetterExists = false;
        GameObject censorGameObject = new("Censor Button");
        censorGameObject.transform.SetParent(anchorObject.transform);
        censorGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        censorChatButton = censorGameObject.AddComponent<MonoToggleButton>();
        censorChatButton.SetOnText("Censor Chat: ON");
        censorChatButton.SetOffText("Censor Chat: OFF");
        censorChatButton.SetToggleOnAction(() => DataManager.Settings.Multiplayer.CensorChat = true);
        censorChatButton.SetToggleOffAction(() => DataManager.Settings.Multiplayer.CensorChat = false);
        censorChatButton.SetState(DataManager.Settings.Multiplayer.CensorChat);
        censorGameObject.transform.localPosition += new Vector3(0.5f, 0.25f);

        GameObject fIGameObject = new("Friend & Invite Button");
        fIGameObject.transform.SetParent(anchorObject.transform);
        fIGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        friendInviteButton = fIGameObject.AddComponent<MonoToggleButton>();
        friendInviteButton.SetOnText("Friend & Lobby Invites: ON");
        friendInviteButton.SetOffText("Friend & Lobby Invites: OFF");
        friendInviteButton.SetToggleOnAction(() => DataManager.Settings.Multiplayer.AllowFriendInvites = true);
        friendInviteButton.SetToggleOffAction(() => DataManager.Settings.Multiplayer.AllowFriendInvites = false);
        friendInviteButton.SetState(DataManager.Settings.Multiplayer.AllowFriendInvites);
        fIGameObject.transform.localPosition += new Vector3(0.5f, -1.25f);

        optionsMenuBehaviour.EnableFriendInvitesButton.gameObject.SetActive(false);

        GameObject colorblindGameObject = new("Colorblind Button");
        colorblindGameObject.transform.SetParent(anchorObject.transform);
        colorblindGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        colorblindTextButton = colorblindGameObject.AddComponent<MonoToggleButton>();
        colorblindTextButton.SetOnText("Colorblind Mode: ON");
        colorblindTextButton.SetOffText("Colorblind Mode: OFF");
        colorblindTextButton.SetToggleOnAction(() => DataManager.Settings.Accessibility.ColorBlindMode = true);
        colorblindTextButton.SetToggleOffAction(() => DataManager.Settings.Accessibility.ColorBlindMode = false);
        colorblindTextButton.SetState(DataManager.Settings.Accessibility.ColorBlindMode);
        colorblindGameObject.transform.localPosition += new Vector3(0.5f, -0.25f);

        optionsMenuBehaviour.ColorBlindButton.gameObject.SetActive(false);

        GameObject streamerGameObject = new("Streamer Mode Button");
        streamerGameObject.transform.SetParent(anchorObject.transform);
        streamerGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        streamerModeButton = streamerGameObject.AddComponent<MonoToggleButton>();
        streamerModeButton.SetOnText("Streamer Mode: ON");
        streamerModeButton.SetOffText("Streamer Mode: OFF");
        streamerModeButton.SetToggleOnAction(() => DataManager.Settings.Gameplay.StreamerMode = true);
        streamerModeButton.SetToggleOffAction(() => DataManager.Settings.Gameplay.StreamerMode = false);
        streamerModeButton.SetState(DataManager.Settings.Gameplay.StreamerMode);
        streamerGameObject.transform.localPosition += new Vector3(0.5f, -0.75f);

        optionsMenuBehaviour.StreamerModeButton.gameObject.SetActive(false);


        GameObject controlGameObject = new("Control Scheme Button");
        controlsText = Instantiate(title, anchorObject.transform);
        controlsText.transform.localPosition += new Vector3(-1.3f, -0.2f);

        controlGameObject.transform.SetParent(anchorObject.transform);
        controlGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        controlScheme = controlGameObject.AddComponent<TiledToggleButton>();
        controlScheme.SetLeftButtonText("Mouse");
        controlScheme.SetRightButtonText("Mouse & Keyboard");
        controlScheme.SetState(DataManager.Settings.input.inputMode is ControlTypes.Keyboard);

        PassiveButton joystickModeButton = optionsMenuBehaviour.MouseAndKeyboardOptions.FindChild<PassiveButton>("JoystickModeButton");
        PassiveButton touchModeButton = optionsMenuBehaviour.MouseAndKeyboardOptions.FindChild<PassiveButton>("TouchModeButton");

        controlScheme.SetToggleOffAction(() => joystickModeButton.ReceiveClickDown());
        controlScheme.SetToggleOnAction(() => touchModeButton.ReceiveClickDown());
        controlGameObject.transform.localPosition += new Vector3(2.25f, 2f);
        optionsMenuBehaviour.MouseAndKeyboardOptions.gameObject.SetActive(false);
        optionsMenuBehaviour.MouseAndKeyboardOptions.gameObject.GetComponentsInChildren<Component>(true).ForEach(c => c.gameObject.SetActive(false));

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
            optionsMenuBehaviour.KeyboardOptions.GetComponentInChildren<PassiveButton>(true).ReceiveClickDown();
            changeKeyBindingButton.SetState(false, true);
        });
        keybindingObject.transform.localPosition += new Vector3(3.5f, 1.5f);
        optionsMenuBehaviour.KeyboardOptions.SetActive(false);
        optionsMenuBehaviour.MouseAndKeyboardOptions.GetComponentsInChildren<Component>().ForEach(c => c.gameObject.SetActive(false));




        // =======================================
        //          Language Button
        // =========================================
        LanguageSetter languageSetterPrefab = FindObjectOfType<LanguageSetter>(true);
        if (languageSetterPrefab == null) return;

        languageSetter = Instantiate(languageSetterPrefab, anchorObject.transform);
        languageSetter.transform.localPosition -= new Vector3(2.5f, 1.88f);

        GameObject languageButtonObject = anchorObject.CreateChild("Language Button", new Vector3(1f, -1.75f));
        languageButton = languageButtonObject.AddComponent<MonoToggleButton>();
        languageButton.SetOffText(DataManager.Settings.language.language);
        languageButton.SetToggleOnAction(() =>
        {
            languageButton.SetState(false);
            languageSetter.Open();
        });
        languageSetterExists = true;
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