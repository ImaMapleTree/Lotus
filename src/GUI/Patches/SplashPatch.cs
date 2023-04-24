using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using TMPro;
using TOHTOR.Extensions;
using TOHTOR.Utilities;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace TOHTOR.GUI.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
class SplashPatch
{
    public static GameObject AmongUsLogo = null!;
    private static UnityOptional<GameObject> _customSplash = UnityOptional<GameObject>.Null();
    internal static bool FriendListButtonHasBeenMoved;

    private static GameObject howToPlayButton;

    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    static void Postfix(MainMenuManager __instance)
    {
        Application.targetFrameRate = 165;
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
        tohLogo.transform.position = Vector3.up;
        tohLogo.transform.localScale *= 1.2f;
        var renderer = tohLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Utils.LoadSprite("TOHTOR.assets.tohtor-logo-rold.png", 300f);

        _customSplash.OrElseSet(InitializeSplash);
        PlayerParticles particles = Object.FindObjectOfType<PlayerParticles>();
        particles.gameObject.SetActive(false);
    }

    private static GameObject InitializeSplash()
    {
        GameObject splashArt = new("SplashArt");
        splashArt.transform.position = new Vector3(0, 0.40f, 600f);
        var spriteRenderer = splashArt.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Utils.LoadSprite("TOHTOR.assets.TOHTORBackground.png", 200f);
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