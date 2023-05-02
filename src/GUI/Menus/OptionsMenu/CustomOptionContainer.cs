using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using TOHTOR.Extensions;
using TOHTOR.GUI.Menus.OptionsMenu.Components;
using TOHTOR.GUI.Menus.OptionsMenu.Submenus;
using TOHTOR.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VentLib.Logging;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.GUI.Menus.OptionsMenu;

[RegisterInIl2Cpp]
public class CustomOptionContainer: MonoBehaviour
{
    public static UnityOptional<TMP_FontAsset> CustomOptionFont = UnityOptional<TMP_FontAsset>.Null();

    public SpriteRenderer background;

    public PassiveButton generalButton;
    public PassiveButton graphicButton;
    public PassiveButton soundButton;
    public PassiveButton ventLibButton;
    public PassiveButton addonsButton;

    public PassiveButton returnButton;
    public PassiveButton exitButton;

    private GeneralMenu generalMenu;
    private GraphicsMenu graphicsMenu;
    private SoundMenu soundMenu;
    private VentLibMenu ventLibMenu;

    private MonoToggleButton monoToggleButton;

    private PassiveButton? template;
    private const int ButtonPpu = 600;

    private List<(PassiveButton, IBaseOptionMenuComponent)> boundButtons = new();

    public CustomOptionContainer(IntPtr intPtr): base(intPtr)
    {
        VentLogger.Fatal("Ctor");
        transform.localPosition += new Vector3(1f, 0f);
        background = gameObject.AddComponent<SpriteRenderer>();
        background.sprite = OptionMenuResources.BackgroundSprite;


        generalMenu = gameObject.AddComponent<GeneralMenu>();
        graphicsMenu = gameObject.AddComponent<GraphicsMenu>();
        soundMenu = gameObject.AddComponent<SoundMenu>();
        ventLibMenu = gameObject.AddComponent<VentLibMenu>();
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        generalMenu.PassMenu(menuBehaviour);
        graphicsMenu.PassMenu(menuBehaviour);
        soundMenu.PassMenu(menuBehaviour);
        ventLibMenu.PassMenu(menuBehaviour);

        var buttonFunc = CreateButton(menuBehaviour);
        generalButton = buttonFunc(OptionMenuResources.GeneralButton);
        generalButton.transform.localPosition += new Vector3(2.6f, 0f);

        graphicButton = buttonFunc(OptionMenuResources.GraphicsButton);
        graphicButton.transform.localPosition += new Vector3(2.6f, -0.5f);

        soundButton = buttonFunc(OptionMenuResources.SoundButton);
        soundButton.transform.localPosition += new Vector3(2.6f, -1f);

        ventLibButton = buttonFunc(OptionMenuResources.VentLibButton);
        ventLibButton.transform.localPosition += new Vector3(2.6f, -1.5f);

        addonsButton = buttonFunc(OptionMenuResources.AddonsButton);
        addonsButton.transform.localPosition += new Vector3(2.6f, -2f);

        returnButton = buttonFunc(OptionMenuResources.ReturnButton);
        returnButton.transform.localPosition += new Vector3(2.6f, -4f);
        returnButton.OnClick.AddListener(((Action)((menuBehaviour.Close))));

        exitButton = buttonFunc(OptionMenuResources.ExitButton);
        exitButton.transform.localPosition += new Vector3(2.6f, -4.5f);
        menuBehaviour.FindChildOrEmpty<PassiveButton>("LeaveGameButton").Handle(exitPassiveButton =>
        {
            exitPassiveButton.gameObject.SetActive(false);
            exitButton.OnClick.AddListener(((Action)(exitPassiveButton.ReceiveClickDown)));
        }, () =>
        {
            exitButton.gameObject.SetActive(false);
            returnButton.gameObject.SetActive(false);
        });

        menuBehaviour.FindChild<PassiveButton>("Background").OnClick = new Button.ButtonClickedEvent();
        menuBehaviour.Background.enabled = false;

        menuBehaviour.Tabs.ForEach(t => t.gameObject.SetActive(false));
        menuBehaviour.Tabs[0].Content.SetActive(false);
        menuBehaviour.Tabs[0].Content.transform.localPosition += new Vector3(0f, 1000f);
        menuBehaviour.BackButton.transform.localPosition += new Vector3(-1.2f, 0.17f);
        CreateButtonBehaviour();
        generalMenu.Open();
    }

    private void CreateButtonBehaviour()
    {
        boundButtons = new List<(PassiveButton, IBaseOptionMenuComponent)>
        {
            (generalButton, generalMenu), (graphicButton, graphicsMenu), (soundButton, soundMenu), (ventLibButton, ventLibMenu)
        };

        Func<int, UnityAction> actionFunc = i => (Action)(() =>
        {
            boundButtons.ForEach((b, i1) =>
            {
                if (i == i1) b.Item2.Open();
                else b.Item2.Close();
            });
        });

        generalButton.OnClick.AddListener(actionFunc(0));
        graphicButton.OnClick.AddListener(actionFunc(1));
        soundButton.OnClick.AddListener(actionFunc(2));
        ventLibButton.OnClick.AddListener(actionFunc(3));
    }



    private Func<Sprite, PassiveButton> CreateButton(OptionsMenuBehaviour menuBehaviour)
    {
        return sprite =>
        {

            PassiveButton button = Instantiate(template ??= menuBehaviour.GetComponentsInChildren<PassiveButton>().Last(), transform);
            button.GetComponentInChildren<TextMeshPro>().gameObject.SetActive(false);
            SpriteRenderer render = button.GetComponentInChildren<SpriteRenderer>();
            render.sprite = sprite;
            render.color = Color.white;

            button.OnClick = new Button.ButtonClickedEvent();

            var buttonTransform = button.transform;
            buttonTransform.localScale -= new Vector3(0.33f, 0f, 0f);
            buttonTransform.localPosition += new Vector3(-4.6f, 2.5f, 0f);

            return button;
        };
    }

    public static TMP_FontAsset GetGeneralFont()
    {
        return CustomOptionFont.OrElseSet(() =>
        {
            string path = Font.GetPathsToOSFonts()
                .FirstOrOptional(f => f.Contains("ARLRDBD"))
                .OrElseGet(() =>
                    Font.GetPathsToOSFonts().FirstOrOptional(f => f.Contains("ARIAL"))
                        .OrElseGet(() => Font.GetPathsToOSFonts()[0])
                );

            return TMP_FontAsset.CreateFontAsset(new Font(path));
        });
    }
}