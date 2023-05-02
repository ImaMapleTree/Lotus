using System;
using UnityEngine;
using VentLib.Utilities.Attributes;

namespace TOHTOR.GUI.Menus.OptionsMenu.Components;

[RegisterInIl2Cpp]
public class TiledToggleButton: MonoBehaviour
{
    private GameObject anchorObject;
    private GameObject leftButtonObject;
    private GameObject rightButtonObject;

    private MonoToggleButton leftButton;
    private MonoToggleButton rightButton;

    private string leftButtonText = "LEFT";
    private string rightButtonText = "RIGHT";

    public bool state;

    private Action toggleOnAction = () => { };
    private Action toggleOffAction = () => { };

    public TiledToggleButton(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = new GameObject("Anchor");
        anchorObject.transform.SetParent(transform);
        anchorObject.transform.localScale = new Vector3(1f, 1f, 1f);

        leftButtonObject = new GameObject("Left Button");
        leftButtonObject.transform.SetParent(anchorObject.transform);
        leftButtonObject.transform.localScale = new Vector3(1f, 1f, 1f);

        rightButtonObject = new GameObject("Right Button");
        rightButtonObject.transform.SetParent(anchorObject.transform);
        rightButtonObject.transform.localScale = new Vector3(1f, 1f, 1f);

        leftButton = leftButtonObject.AddComponent<MonoToggleButton>();
        leftButton.SetToggleOnAction(SetOffState);
        leftButton.SetToggleOffAction(() => leftButton.SetState(true, true));

        rightButton = rightButtonObject.AddComponent<MonoToggleButton>();
        rightButton.SetToggleOnAction(SetOnState);
        rightButton.SetToggleOffAction(() => rightButton.SetState(true, true));

        leftButtonObject.transform.localPosition = new Vector3(-1.25f, 0f, 0f);
        rightButtonObject.transform.localPosition = new Vector3(1.25f, 0f, 0f);
    }

    private void Start()
    {
        SetLeftButtonText(leftButtonText);
        SetRightButtonText(rightButtonText);
        SetState(state);
    }

    public void SetLeftButtonText(string text)
    {
        leftButtonText = text;
        leftButton.SetOffText(text);
        leftButton.SetOnText(text);
    }

    public void SetRightButtonText(string text)
    {
        rightButtonText = text;
        rightButton.SetOffText(text);
        rightButton.SetOnText(text);
    }

    public void SetState(bool onState)
    {
        state = onState;
        if (state) SetOnState();
        else SetOffState();
    }

    private void SetOnState()
    {
        leftButton.SetState(false, true);
        rightButton.SetState(true, true);
        toggleOnAction();
    }

    private void SetOffState()
    {
        leftButton.SetState(true, true);
        rightButton.SetState(false, true);
        toggleOffAction();
    }

    public void SetToggleOnAction(Action action) => toggleOnAction = action;

    public void SetToggleOffAction(Action action) => toggleOffAction = action;
}