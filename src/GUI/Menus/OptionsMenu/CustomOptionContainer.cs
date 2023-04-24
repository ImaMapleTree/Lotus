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

    private GeneralMenu generalMenu;
    private GraphicsMenu graphicsMenu;

    private MonoToggleButton monoToggleButton;

    private PassiveButton? template;
    private const int ButtonPpu = 600;

    private List<(PassiveButton, IBaseOptionMenuComponent)> boundButtons = new();

    public CustomOptionContainer(IntPtr intPtr): base(intPtr)
    {
        VentLogger.Fatal("Ctor");
        transform.localPosition += new Vector3(1f, 0f);
        background = gameObject.AddComponent<SpriteRenderer>();
        background.sprite = Utils.LoadSprite("TOHTOR.assets.Settings.MenuBackground.png", 525);

        generalMenu = gameObject.AddComponent<GeneralMenu>();
        graphicsMenu = gameObject.AddComponent<GraphicsMenu>();

        gameObject.AddComponent<Components.SimpleDropdownButton>();
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        generalMenu.PassMenu(menuBehaviour);
        graphicsMenu.PassMenu(menuBehaviour);

        menuBehaviour.GetComponentsInChildren<PassiveButton>().Select(c => (c.name, c.TypeName())).Join().DebugLog();

        var buttonFunc = CreateButton(menuBehaviour);
        generalButton = buttonFunc(Utils.LoadSprite("TOHTOR.assets.Settings.GeneralButton.png", ButtonPpu));
        generalButton.transform.localPosition += new Vector3(2.6f, 0f);

        graphicButton = buttonFunc(Utils.LoadSprite("TOHTOR.assets.Settings.GraphicsButton.png", ButtonPpu));
        graphicButton.transform.localPosition += new Vector3(2.6f, -0.5f);

        soundButton = buttonFunc(Utils.LoadSprite("TOHTOR.assets.Settings.SoundButton.png", ButtonPpu));
        soundButton.transform.localPosition += new Vector3(2.6f, -1f);

        ventLibButton = buttonFunc(Utils.LoadSprite("TOHTOR.assets.Settings.VentButton.png", ButtonPpu));
        ventLibButton.transform.localPosition += new Vector3(2.6f, -1.5f);

        addonsButton = buttonFunc(Utils.LoadSprite("TOHTOR.assets.Settings.AddonButton.png", ButtonPpu));
        addonsButton.transform.localPosition += new Vector3(2.6f, -2f);
        addonsButton.GetComponentsInChildren<Component>().Select(c => (c.name, c.TypeName())).Join().DebugLog();

        //menuBehaviour.GetComponentsInChildren<PassiveButton>().ForEach(b => b.OnClick = new Button.ButtonClickedEvent());
        menuBehaviour.FindChild<PassiveButton>("Background").OnClick = new Button.ButtonClickedEvent();
        menuBehaviour.Background.enabled = false;
        //menuBehaviour.Tabs.ForEach(t => t.gameObject.SetActive(false));

        menuBehaviour.SoundSlider.gameObject.SetActive(false);
        menuBehaviour.GetComponentsInChildren<TextMeshPro>().Select(c => (c.name)).Join().DebugLog();
        TextMeshPro[] meshPros = menuBehaviour.GetComponentsInChildren<TextMeshPro>();
        meshPros[0].gameObject.SetActive(false);
        meshPros[1].gameObject.SetActive(false);

        menuBehaviour.BackButton.transform.localPosition += new Vector3(-1f, 1f);
        CreateButtonBehaviour();
        generalMenu.Open();
    }

    private void CreateButtonBehaviour()
    {
        boundButtons = new List<(PassiveButton, IBaseOptionMenuComponent)> { (generalButton, generalMenu), (graphicButton, graphicsMenu) };

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