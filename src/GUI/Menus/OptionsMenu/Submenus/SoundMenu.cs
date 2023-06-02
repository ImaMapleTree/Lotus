using System;
using System.Globalization;
using TMPro;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu.Submenus;

[RegisterInIl2Cpp]
public class SoundMenu: MonoBehaviour, IBaseOptionMenuComponent
{
    private TextMeshPro soundHeader;
    private SlideBar sfxSlider;
    private SlideBar musicSlider;

    private TextMeshPro musicText;
    private TextMeshPro sfxText;

    private GameObject anchorObject;

    public SoundMenu(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("Anchor");
        soundHeader = Instantiate(FindObjectOfType<TextMeshPro>(), anchorObject.transform);
        soundHeader.font = CustomOptionContainer.GetGeneralFont();
        soundHeader.transform.localPosition += new Vector3(0.95f, 1.75f);
    }

    public void PassMenu(OptionsMenuBehaviour menuBehaviour)
    {
        musicSlider = Instantiate(menuBehaviour.MusicSlider, anchorObject.transform);
        musicSlider.transform.localScale = new Vector3(1.1f, 1.2f, 1f);
        var musicTitleText = musicSlider.GetComponentInChildren<TextMeshPro>();
        musicTitleText.transform.localPosition += new Vector3(0.7f, 0.25f);
        musicText = Instantiate(musicTitleText, musicSlider.transform);
        musicText.transform.localPosition += new Vector3(4f, 0.25f);
        musicSlider.OnValueChange.AddListener((Action)(() =>
        {
            musicText.text = CalculateVolPercent(musicSlider.Value);
            menuBehaviour.MusicSlider.SetValue(musicSlider.Value);
            menuBehaviour.MusicSlider.OnValidate();
        }));
        musicSlider.transform.localPosition += new Vector3(0.27f, 0.9f);

        sfxSlider = Instantiate(menuBehaviour.SoundSlider, anchorObject.transform);
        sfxSlider.transform.localScale = new Vector3(1.1f, 1.2f, 1f);
        var sfxTitleText = sfxSlider.GetComponentInChildren<TextMeshPro>();
        sfxTitleText.transform.localPosition += new Vector3(0.7f, 0.25f);
        sfxText = Instantiate(sfxTitleText, sfxSlider.transform);
        sfxText.transform.localPosition += new Vector3(4f, 0.25f);
        sfxSlider.OnValueChange.AddListener((Action)(() =>
        {
            sfxText.text = CalculateVolPercent(sfxSlider.Value);
            menuBehaviour.SoundSlider.SetValue(sfxSlider.Value);
            menuBehaviour.SoundSlider.OnValidate();
        }));
        sfxSlider.transform.localPosition += new Vector3(0.27f, -0.1f);

        anchorObject.SetActive(false);
    }

    private void Start()
    {
        soundHeader.text = "Sounds";
    }

    public virtual void Open()
    {
        anchorObject.SetActive(true);

        Async.Schedule(() =>
        {
            musicText.text = CalculateVolPercent(musicSlider.Value);
            sfxText.text = CalculateVolPercent(sfxSlider.Value);
        }, 0.000001f);
    }

    public virtual void Close()
    {
        anchorObject.SetActive(false);
    }

    private static string CalculateVolPercent(float value)
    {
        return Math.Round(value * 100, 0).ToString(CultureInfo.InvariantCulture) + "%";
    }
}