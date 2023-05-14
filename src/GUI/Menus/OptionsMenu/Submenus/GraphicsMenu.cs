using System;
using AmongUs.Data;
using Lotus.GUI.Menus.OptionsMenu.Components;
using Lotus.Options;
using Lotus.Options.Client;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using static Lotus.Utilities.GameObjectUtils;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class GraphicsMenu: MonoBehaviour, IBaseOptionMenuComponent
{
    private TextMeshPro graphicsTitle;
    private MonoToggleButton fullscreenButton;
    private MonoToggleButton vsyncButton;
    private MonoToggleButton screenshakeButton;
    private MonoToggleButton applyButton;
    private SlideBar resolutionSlider;
    private SlideBar fpsSlider;
    private TextMeshPro fpsText;
    private TextMeshPro resolutionText;

    private OptionsMenuBehaviour optionsMenuBehaviour;
    private TabGroup tab;
    private GameObject anchor;

    private bool opening;
    private int temporaryResolutionIndex;
    private int temporaryFps;

    public GraphicsMenu(IntPtr intPtr) : base(intPtr)
    {
        anchor = CreateGameObject("Anchor", transform);
        graphicsTitle = Instantiate(FindObjectOfType<TextMeshPro>(), anchor.transform);
        graphicsTitle.font = CustomOptionContainer.GetGeneralFont();
        graphicsTitle.transform.localPosition += new Vector3(0.95f, 1.75f);
    }

    public void PassMenu(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        this.optionsMenuBehaviour = optionsMenuBehaviour;
        tab = optionsMenuBehaviour.Tabs[1];

        ResolutionSlider graphicsContent = tab.Content.GetComponentInChildren<ResolutionSlider>(true);


        GameObject applyGameObject = anchor.CreateChild("Apply Button", new Vector3(4f, -1.8f));
        applyButton = applyGameObject.AddComponent<MonoToggleButton>();
        applyButton.ConfigureAsPressButton("Apply", () =>
        {
            ResolutionUtils.ResolutionIndex = temporaryResolutionIndex;
            SetResolution(fullscreenButton.state);

            Application.targetFrameRate = ClientOptions.VideoOptions.TargetFps = temporaryFps;
            SetFpsText();
            applyGameObject.SetActive(false);
        });
        applyGameObject.SetActive(false);


        GameObject fullscreenGameObject = anchor.CreateChild("Fullscreen Button", new Vector3(0.5f, 0.25f));
        fullscreenButton = fullscreenGameObject.AddComponent<MonoToggleButton>();
        fullscreenButton.SetOnText("Fullscreen: ON");
        fullscreenButton.SetOffText("Fullscreen: OFF");
        fullscreenButton.SetState(Screen.fullScreen);
        fullscreenButton.SetToggleOnAction(() => applyGameObject.SetActive(!opening));
        fullscreenButton.SetToggleOffAction(() => applyGameObject.SetActive(!opening));
        graphicsContent.Fullscreen.gameObject.SetActive(false);


        resolutionSlider = graphicsContent.slider;
        resolutionSlider.transform.localScale = new Vector3(1.1f, 1.2f, 1f);
        resolutionSlider.transform.localPosition += new Vector3(0.31f, -0.35f);
        resolutionSlider.GetComponentInChildren<TextMeshPro>().transform.localPosition += new Vector3(1f, 0.2f);
        resolutionText = graphicsContent.FindChild<TextMeshPro>("ResolutionText_TMP");
        resolutionSlider.OnValueChange = new UnityEvent();
        resolutionSlider.OnValueChange.AddListener(((Action)(() =>
        {
            temporaryResolutionIndex = Mathf.RoundToInt(resolutionSlider.Value * 9);
            (int width, int height) = ResolutionUtils.ResolutionsSixteenNine[temporaryResolutionIndex];
            resolutionText.text = $"{width} x {height}";
            if (!opening) applyGameObject.SetActive(true);
        })));
        resolutionText.transform.localPosition += new Vector3(2f, 0.625f);


        fpsSlider = Instantiate(resolutionSlider, anchor.transform);
        fpsSlider.transform.localScale = new Vector3(1.1f, 1.2f, 1f);
        //fpsSlider.transform.localPosition += new Vector3(0.2f, -1f);
        TextMeshPro fpsTextLabel = fpsSlider.GetComponentInChildren<TextMeshPro>();
        fpsTextLabel.transform.localPosition += new Vector3(1f, 0.2f);
        fpsSlider.transform.localPosition -= new Vector3(0f, 0.65f);
        fpsSlider.OnValueChange = new UnityEvent();
        fpsSlider.OnValueChange.AddListener(((Action)(() =>
        {
            int index = Mathf.RoundToInt(fpsSlider.Value * 7);
            int fps = (int)VideoOptions.FpsLimits[index];
            temporaryFps = fps;
            SetFpsText();
            if (!opening) applyGameObject.SetActive(true);
        })));
        fpsText = Instantiate(resolutionText, fpsSlider.transform);
        fpsText.transform.localPosition += new Vector3(2f, 0.625f);

        GameObject vsyncObject = anchor.CreateChild("VSync Button", new Vector3(0.5f, -0.25f));
        vsyncButton = vsyncObject.AddComponent<MonoToggleButton>();
        vsyncButton.SetOnText("VSync: ON");
        vsyncButton.SetOffText("VSync: OFF");
        vsyncButton.SetToggleOnAction(() =>
        {
            DataManager.Settings.Video.VSync = true;
            QualitySettings.vSyncCount = 1;
        });
        vsyncButton.SetToggleOffAction(() =>
        {
            DataManager.Settings.Video.VSync = false;
            QualitySettings.vSyncCount = 0;
        });
        vsyncButton.SetState(DataManager.Settings.Video.VSync);
        graphicsContent.VSync.gameObject.SetActive(false);



        GameObject screenShake = anchor.CreateChild("Screenshake Button", new Vector3(0.5f, -0.75f));
        screenshakeButton = screenShake.AddComponent<MonoToggleButton>();
        screenshakeButton.SetOnText("Screenshake: ON");
        screenshakeButton.SetOffText("Screenshake: OFF");
        screenshakeButton.SetToggleOnAction(() =>
        {
            DataManager.Settings.Gameplay.ScreenShake = true;
            DataManager.Settings.Save();
        });
        screenshakeButton.SetToggleOffAction(() =>
        {
            DataManager.Settings.Gameplay.ScreenShake = false;
            DataManager.Settings.Save();
        });
        screenshakeButton.SetState(DataManager.Settings.Gameplay.ScreenShake);
        graphicsContent.Screenshake.gameObject.SetActive(false);


        graphicsContent.FindChild<PassiveButton>("ApplyButton").gameObject.SetActive(false);
        graphicsContent.Fullscreen.gameObject.SetActive(false);
        anchor.gameObject.SetActive(false);
    }

    private void SetResolution(bool fullscreen)
    {
        (int width, int height) = ResolutionUtils.ResolutionsSixteenNine[ResolutionUtils.ResolutionIndex];
        ResolutionManager.SetResolution(width, height, fullscreen);
    }

    private void SetFpsText()
    {
        int targetFps = temporaryFps;
        string fpsString = targetFps != int.MaxValue ? targetFps.ToString() : "Uncapped";
        fpsText.text = $"{fpsString}";
    }

    private void Start()
    {
        fpsSlider.GetComponentInChildren<TextMeshPro>().text = "Max Framerate";
        graphicsTitle.text = "Display";
    }

    public virtual void Open()
    {
        opening = true;

        temporaryResolutionIndex = ResolutionUtils.ResolutionIndex;
        temporaryFps = ClientOptions.VideoOptions.TargetFps;

        tab.Content.gameObject.SetActive(true);
        anchor.gameObject.SetActive(true);
        applyButton.gameObject.SetActive(false);
        resolutionSlider.SetValue(ResolutionUtils.ResolutionIndex / 9f);
        resolutionSlider.OnValidate();

        int fpsIndex = VideoOptions.FpsLimits.IndexOf(i => ClientOptions.VideoOptions.TargetFps == (int)i);
        fpsSlider.SetValue((fpsIndex != -1 ? fpsIndex : VideoOptions.FpsLimits.Length - 1) / 7f);


        applyButton.gameObject.SetActive(false);

        SetFpsText();
        Async.Schedule(() =>
        {
            fpsSlider.GetComponentInChildren<TextMeshPro>().text = "Max Framerate";
            (int width, int height) = ResolutionUtils.ResolutionsSixteenNine[ResolutionUtils.ResolutionIndex];
            resolutionText.text = $"{width} x {height}";
        }, 0.0001f);
        Async.Schedule(() => opening = false, 0.1f);
    }

    public virtual void Close()
    {
        tab.Content.gameObject.SetActive(false);
        anchor.gameObject.SetActive(false);
    }
}