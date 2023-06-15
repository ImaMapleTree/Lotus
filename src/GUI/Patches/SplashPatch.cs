using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.GUI.Menus;
using Lotus.GUI.Menus.OptionsMenu;
using Lotus.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.GUI.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
class SplashPatch
{
    public static GameObject AmongUsLogo = null!;
    private static UnityOptional<GameObject> _customSplash = UnityOptional<GameObject>.Null();
    internal static ModUpdateMenu ModUpdateMenu;
    internal static UnityOptional<GameObject> UpdateButton = UnityOptional<GameObject>.Null();

    private static GameObject howToPlayButton;


    [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
    static void Prefix(MainMenuManager __instance)
    {
        if ((AmongUsLogo = GameObject.Find("bannerLogo_AmongUs")) != null)
        {
            AmongUsLogo.transform.localScale *= 0.4f;
            AmongUsLogo.transform.position += Vector3.up * 0.25f;
        }

        /*GameObject playOnlineButton = GameObject.Find("PlayOnlineButton");
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
        AccountTabPatch.ModifyAccountTabLocation(accountTab);*/


        /*AdjustBottomButtons(__instance, GameObject.Find("BottomButtons"));*/

        DevLogger.Log("??");
        AccountManager.Instance.accountTab.FindChildOrEmpty<Transform>("GameHeader").IfPresent(g => g.gameObject.SetActive(false));
        DevLogger.Log("?????");


        __instance.screenTint.gameObject.transform.localPosition += new Vector3(1000f, 0f);
        __instance.screenTint.enabled = false;
        __instance.rightPanelMask.SetActive(true);
        // The background texture (large sprite asset)
        __instance.mainMenuUI.FindChild<SpriteRenderer>("BackgroundTexture").transform.gameObject.SetActive(false);
        // The glint on the Among Us Menu
        __instance.mainMenuUI.FindChild<SpriteRenderer>("WindowShine").transform.gameObject.SetActive(false);
        __instance.mainMenuUI.FindChild<Transform>("ScreenCover").gameObject.SetActive(false);

        GameObject leftPanel =__instance.mainMenuUI.FindChild<Transform>("LeftPanel").gameObject;
        GameObject rightPanel =__instance.mainMenuUI.FindChild<Transform>("RightPanel").gameObject;
        rightPanel.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        GameObject maskedBlackScreen = rightPanel.FindChild<Transform>("MaskedBlackScreen").gameObject;
        maskedBlackScreen.GetComponent<SpriteRenderer>().enabled = false;
        Transform accountButtons =  maskedBlackScreen.FindChild<Transform>("AccountButtons", true);
        accountButtons.gameObject.FindChild<Transform>("Divider", true).localPosition += new Vector3(1000f, 0f);
        accountButtons.gameObject.FindChild<Transform>("Header", true).localPosition += new Vector3(1000f, 0f);
        maskedBlackScreen.transform.localPosition = new Vector3(-3.345f, -2.05f);
        maskedBlackScreen.transform.localScale = new Vector3(7.35f, 4.5f, 4f);

        leftPanel.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        leftPanel.gameObject.FindChild<SpriteRenderer>("Divider").enabled = false;
        leftPanel.GetComponentsInChildren<SpriteRenderer>(true).Where(r => r.name == "Shine").ForEach(r => r.enabled = false);

        PassiveButton inventoryButton = MakeIconButton(__instance.inventoryButton,new Vector3(0.25f, 1.15f, 1f), sprite: AssetLoader.LoadSprite("main_menu.InventoryIconRedone.png", 100));
        inventoryButton.transform.localPosition = new Vector3(5.6f, -1.96f, 0f);

        PassiveButton discordButton = Object.Instantiate(inventoryButton, __instance.transform);
        discordButton.inactiveSprites.GetComponent<SpriteRenderer>().sprite = AssetLoader.LoadSprite("main_menu.discord_button_icon.png", 100);
        discordButton.transform.localPosition = new Vector3(0.34f, -2.4f, 0f);
        discordButton.transform.localScale = new Vector3(0.17f, 0.90f, 1f);
        discordButton.Modify(() => Application.OpenURL(ModConstants.DiscordInvite));

        PassiveButton shopButton = MakeIconButton(__instance.shopButton, new Vector3(0.25f, 1.05f, 1f), sprite: AssetLoader.LoadSprite("main_menu.ShopIconRedone.png", 100));
        shopButton.transform.localPosition = new Vector3(6.75f, -1.975f, 0f);

        PassiveButton newsButton = MakeIconButton(__instance.newsButton, new Vector3(0.22f, 1.44f, 1f), sprite: AssetLoader.LoadSprite("main_menu.AnnouncementIconRedone.png", 100));
        newsButton.transform.localPosition = new Vector3(7.89f, -1.8675f, 0f);

        __instance.playButton.transform.localPosition -= new Vector3(0f, 1.4f);

        SpriteRenderer activeSpriteRender = __instance.playButton.activeSprites.GetComponent<SpriteRenderer>();
        activeSpriteRender.color = new Color(1f, 0f, 0.62f);

        SpriteRenderer inactiveSpriteRender = __instance.playButton.inactiveSprites.GetComponent<SpriteRenderer>();
        inactiveSpriteRender.color = new Color(1f, 0f, 0.35f);
        inactiveSpriteRender.sprite = activeSpriteRender.sprite;

        __instance.playButton.activeTextColor = Color.white;
        __instance.playButton.inactiveTextColor = Color.white;
        __instance.playButton.OnClick = __instance.PlayOnlineButton.OnClick;
        Async.Schedule(() => __instance.playButton.buttonText.text = "Play Online", 0.001f);

        PassiveButton playLocalButton = Object.Instantiate(__instance.playButton, __instance.transform);
        playLocalButton.transform.localPosition -= new Vector3(3.4f, 1.5f);
        playLocalButton.transform.localScale -= new Vector3(0.16f, 0.2f);
        playLocalButton.activeSprites.GetComponent<SpriteRenderer>().color = activeSpriteRender.color;
        playLocalButton.inactiveSprites.FindChild<SpriteRenderer>("Icon", true).sprite = AssetLoader.LoadSprite("main_menu.LittleDudeIcon.png", 50);
        playLocalButton.activeSprites.FindChild<SpriteRenderer>("Icon", true).sprite = AssetLoader.LoadSprite("main_menu.LittleDudeIcon.png", 50);
        playLocalButton.OnClick = __instance.playLocalButton.OnClick;
        Async.Schedule(() => playLocalButton.buttonText.text = "Play Local", 0.001f);


        __instance.myAccountButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0f, 1f);
        __instance.myAccountButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0.85f);
        __instance.myAccountButton.activeTextColor = Color.white;
        __instance.myAccountButton.inactiveTextColor = Color.white;

        __instance.settingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0.85f);
        __instance.settingsButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0.85f);
        __instance.settingsButton.activeTextColor = Color.white;
        __instance.settingsButton.inactiveTextColor = Color.white;

        var tohLogo = new GameObject("titleLogo_TOH");
        tohLogo.transform.position = new Vector3(4.55f, -2.1f);
        tohLogo.transform.localScale = new Vector3(1f, 1f, 1f);
        var renderer = tohLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Utils.LoadSprite("Lotus.assets.Lotus_Icon.png", 700f);

        _customSplash.OrElseSet(InitializeSplash);
        PlayerParticles particles = Object.FindObjectOfType<PlayerParticles>();
        particles.gameObject.SetActive(false);

        ModUpdateMenu = __instance.gameObject.AddComponent<ModUpdateMenu>();
        ModUpdateMenu.AnchorObject.transform.localPosition += new Vector3(0f, 0f, -9f);

        /*GameObject updateButton = Object.Instantiate(playLocalButton, __instance.transform);*/
        /*Async.Schedule(() =>
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
        buttonClickedEvent.AddListener((UnityAction)(Action)(() => ModUpdateMenu.Open()));*/

        /*UpdateButton = UnityOptional<GameObject>.Of(updateButton);

        if (!ProjectLotus.ModUpdater.HasUpdate) updateButton.gameObject.SetActive(false);*/
        FriendsListManager.Instance.StopPolling();
        FriendsListManager.Instance.OnSignOut();
    }

    private static GameObject InitializeSplash()
    {
        GameObject splashArt = new("SplashArt");
        splashArt.transform.position = new Vector3(0, 0.40f, 600f);
        var spriteRenderer = splashArt.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Utils.LoadSprite("Lotus.assets.TOHTORBackground.png", 200f);
        return splashArt;
    }

    private static PassiveButton MakeIconButton(PassiveButton passiveButton, Vector3 scaling, Sprite? sprite = null)
    {
        SpriteRenderer icon = passiveButton.FindChild<SpriteRenderer>("Icon");
        SpriteRenderer buttonRender = passiveButton.inactiveSprites.GetComponent<SpriteRenderer>();
        if (sprite != null) icon.sprite = sprite;
        buttonRender.sprite = icon.sprite;
        passiveButton.activeSprites = null;
        passiveButton.GetComponentInChildren<TextMeshPro>().enabled = false;
        passiveButton.transform.localScale = scaling;
        passiveButton.OnMouseOver.AddListener((Action)(() => buttonRender.color = Color.green));
        passiveButton.OnMouseOut.AddListener((Action)(() => buttonRender.color = Color.white));
        icon.enabled = false;

        // Button Specific Things
        // News Button Icon
        NewsCountButton? newsCountButton = passiveButton.GetComponentInChildren<NewsCountButton>();
        if (newsCountButton != null)
        {
            newsCountButton.FindChild<Transform>("NewItem", true).localScale = new Vector3(4f, 0.6f, 1f);
            passiveButton.transform.localScale += new Vector3(0f, 0.2f);
        }

        // Shop Button Icon
        if (passiveButton.name == "ShopButton") passiveButton.FindChild<SpriteRenderer>("Sprite", true).transform.localScale = new Vector3(3.55f, 0.95f, 1f);

        passiveButton.Debug();
        return passiveButton;
    }

    [QuickPostfix(typeof(MainMenuManager), nameof(MainMenuManager.OpenGameModeMenu))]
    private static void InterceptPlayClick(MainMenuManager __instance)
    {

    }

    [QuickPostfix(typeof(PassiveButton), nameof(PassiveButton.ReceiveMouseOver))]
    private static void InterceptMouseOver(PassiveButton __instance)
    {
        if (__instance.activeSprites != null) return;
        if (__instance.inactiveSprites != null) __instance.inactiveSprites.SetActive(true);
    }
}