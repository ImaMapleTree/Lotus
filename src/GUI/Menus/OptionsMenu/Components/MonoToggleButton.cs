using System;
using System.Linq;
using TMPro;
using TOHTOR.Utilities;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Logging;
using VentLib.Utilities.Attributes;
using Action = System.Action;

namespace TOHTOR.GUI.Menus.OptionsMenu.Components;

[RegisterInIl2Cpp]
public class MonoToggleButton: MonoBehaviour
{
    private PassiveButton enabledButton;
    private SpriteRenderer enabledRender;
    private PassiveButton disabledButton;
    private SpriteRenderer disabledRender;

    private string enabledText = "On";
    private string disabledText = "Off";

    private TextMeshPro enabledTextTMP;
    private TextMeshPro disabledTextTMP;
    public bool state;

    private Action toggleOnAction = () => { };
    private Action toggleOffAction = () => { };

    public MonoToggleButton()
    {
        PassiveButton template = FindObjectsOfType<PassiveButton>(true).First(pb => pb.name == "CensorChatButton");
        enabledButton = Instantiate(template, transform);
        enabledButton.OnClick = new Button.ButtonClickedEvent();
        enabledButton.OnClick.AddListener((Action)(() => SetOffState()));

        enabledRender = enabledButton.GetComponentInChildren<SpriteRenderer>();
        enabledRender.color = Color.white;
        enabledRender.sprite = OptionMenuResources.ButtonOnSprite;

        disabledButton = Instantiate(template, transform);
        disabledButton.OnClick = new Button.ButtonClickedEvent();
        disabledButton.OnClick.AddListener((Action)(() => SetOnState()));

        disabledRender = disabledButton.GetComponentInChildren<SpriteRenderer>();
        disabledRender.color = Color.white;
        disabledRender.sprite = OptionMenuResources.ButtonOffSprite;

        enabledTextTMP = enabledButton.GetComponentInChildren<TextMeshPro>();
        enabledTextTMP.font = CustomOptionContainer.GetGeneralFont();
        enabledTextTMP.color = new Color(0.11f, 0.51f, 0.6f);
        enabledTextTMP.text = enabledText;
        enabledTextTMP.enableWordWrapping = false;

        disabledTextTMP = disabledButton.GetComponentInChildren<TextMeshPro>();
        disabledTextTMP.font = CustomOptionContainer.GetGeneralFont();
        disabledTextTMP.color = new Color(0.34f, 0.36f, 0.37f);
        disabledTextTMP.text = disabledText;
        disabledTextTMP.enableWordWrapping = false;
    }

    private void Start() => SetState(state);

    public void Toggle() => SetState(state = !state);

    public void SetState(bool isEnabled, bool noAction = false)
    {
        state = isEnabled;
        if (state) SetOnState(noAction);
        else SetOffState(noAction);
    }

    private void SetOnState(bool noAction = false)
    {
        state = true;
        enabledButton.gameObject.SetActive(true);
        disabledButton.gameObject.SetActive(false);
        if (!noAction) toggleOnAction();
    }

    private void SetOffState(bool noAction = false)
    {
        state = false;
        enabledButton.gameObject.SetActive(false);
        disabledButton.gameObject.SetActive(true);
        if (!noAction) toggleOffAction();
    }

    public void SetOnText(string onText) => enabledTextTMP.text = onText;

    public void SetOffText(string offText) => disabledTextTMP.text = offText;

    public void SetToggleOnAction(Action action) => toggleOnAction = action;

    public void SetToggleOffAction(Action action) => toggleOffAction = action;

    public void ConfigureAsPressButton(string text, Action action)
    {
        SetOffText(text);
        toggleOffAction = action;
        toggleOnAction = () => SetOffState();
    }
}