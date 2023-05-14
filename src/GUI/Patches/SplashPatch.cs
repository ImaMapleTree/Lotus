using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Lotus.Utilities;
using TMPro;
using Lotus.Extensions;
using Lotus.GUI.Menus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.GUI.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
class SplashPatch
{
    public static GameObject AmongUsLogo = null!;
    private static UnityOptional<GameObject> _customSplash = UnityOptional<GameObject>.Null();
    internal static bool FriendListButtonHasBeenMoved;
    internal static ModUpdateMenu ModUpdateMenu;
    internal static UnityOptional<GameObject> UpdateButton;

    private static GameObject howToPlayButton;


    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    static void Postfix(MainMenuManager __instance)
    {
        if ((AmongUsLogo = GameObject.Find("bannerLogo_AmongUs")) != null)
        {
            AmongUsLogo.transform.localScale *= 0.4f;
            AmongUsLogo.transform.position += Vector3.up * 0.25f;
        }

        GameObject playOnlineButton = GameObject.Find("PlayOnlineButton");
        playOnlineButton.transform.localPosition = new Vector3(-4.15f, -0.65f); // 1.25 is good

        GameObject playLocalButton = GameObject.Find("PlayLocalButton");
        playLocalButton.transform.localPosition = new Vector3(-4.15f, -1.65f); // 2.25 is good

        howToPlayButton = GameObject.Find("HowToPlayButton");
        howToPlayButton.transform.localPosition = new Vector3(-4.15f, -2.5f);
        Async.Schedule(() => howToPlayButton.GetComponentInChildren<TextMeshPro>().text = "Settings", 0.1f);

        GameObject freePlayButton = GameObject.Find("FreePlayButton");
        freePlayButton.gameObject.SetActive(false); // TODO: make discord button

        GameObject quitButton = GameObject.Find("/MainUI/ExitGameButton");
        quitButton.transform.localPosition = new Vector3(4.5f, -2.5f);

        AccountTab accountTab = Object.FindObjectOfType<AccountTab>();
        AccountTabPatch.ModifyAccountTabLocation(accountTab);

        AdjustBottomButtons(GameObject.Find("BottomButtons"));

        FriendListButtonHasBeenMoved = true;

        var tohLogo = new GameObject("titleLogo_TOH");
        tohLogo.transform.position = new Vector3(4.55f, -1.5f);
        tohLogo.transform.localScale = new Vector3(1f, 1f, 1f);
        var renderer = tohLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Utils.LoadSprite("Lotus.assets.LotusBanner.png", 1000f);

        _customSplash.OrElseSet(InitializeSplash);
        PlayerParticles particles = Object.FindObjectOfType<PlayerParticles>();
        particles.gameObject.SetActive(false);
        
        ModUpdateMenu = __instance.gameObject.AddComponent<ModUpdateMenu>();
        
        GameObject updateButton = Object.Instantiate(playLocalButton, __instance.transform);
        Async.Schedule(() =>
        {
            TextMeshPro tmp = updateButton.GetComponentInChildren<TextMeshPro>();
            tmp.text = "Update Found!";
            tmp.enableWordWrapping = true;
        }, 0.1f);
        updateButton.transform.localPosition += new Vector3(0f, 1.85f);
        updateButton.transform.localScale -= new Vector3(0f, 0.25f);
        updateButton.GetComponentInChildren<ButtonRolloverHandler>().OutColor = ModConstants.Palette.GeneralColor5;
        updateButton.GetComponentInChildren<SpriteRenderer>().color = ModConstants.Palette.GeneralColor5;
        Button.ButtonClickedEvent buttonClickedEvent = new();
        updateButton.GetComponentInChildren<PassiveButton>().OnClick = buttonClickedEvent;
        buttonClickedEvent.AddListener((UnityAction)(Action)(() => ModUpdateMenu.Open()));
        
        UpdateButton = UnityOptional<GameObject>.Of(updateButton);
        
        if (!ProjectLotus.ModUpdater.HasUpdate) updateButton.gameObject.SetActive(false);
        
    }

    private static GameObject InitializeSplash()
    {
        GameObject splashArt = new("SplashArt");
        splashArt.transform.position = new Vector3(0, 0.40f, 600f);
        var spriteRenderer = splashArt.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Utils.LoadSprite("Lotus.assets.TOHTORBackground.png", 200f);
        return splashArt;
    }

    private static void AdjustBottomButtons(GameObject buttonContainer)
    {
        buttonContainer.FindChild<PassiveButton>("StatsButton").gameObject.SetActive(false);

        PassiveButton optionsButton = buttonContainer.FindChild<PassiveButton>("OptionsButton");
        optionsButton.gameObject.SetActive(false);

        PassiveButton settingsPassiveButton = howToPlayButton.GetComponentInChildren<PassiveButton>();
        settingsPassiveButton.OnClick = new Button.ButtonClickedEvent();
        settingsPassiveButton.OnClick.AddListener(((Action)(() => optionsButton.ReceiveClickDown())));

        buttonContainer.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        buttonContainer.gameObject.transform.localPosition = new Vector3(2.3f, -2.5f, 0f);
    }
}