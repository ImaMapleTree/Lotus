using System;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace TOHTOR.GUI.Menus;

public class HistoryMenuButton
{
    private UnityOptional<ReportButton> optionalButton;
    private bool toggle;

    private HistoryMenuButton(ReportButton reportButton, HudManager instance)
    {
        VentLogger.Fatal("Creating button");
        optionalButton = UnityOptional<ReportButton>.Of(Object.Instantiate(reportButton, instance.transform));
        ReportButton button = optionalButton.Get();
        button.graphic.sprite = Utils.LoadSprite("TOHTOR.assets.History.png", 125);
        button.transform.localPosition += new Vector3(-3.8f, -2.45f);
        button.SetActive(true);

        PassiveButton passiveButton = button.GetComponentInChildren<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((Action)OnClick);


        Async.Schedule(() => button.buttonLabelText.text = "History", 0.1f);
    }

    public static CustomOptional<HistoryMenuButton> Create()
    {
        return CustomOptional<HistoryMenuButton>.From(UnityOptional<HudManager>.Of(HudManager.Instance)
                .UnityMap(m => m.ReportButton)
                .Map(r => new HistoryMenuButton(r, HudManager.Instance)), b => b.Exists()
            );
    }

    private void OnClick()
    {
        if (!optionalButton.Exists()) return;
        ReportButton button = optionalButton.Get();
        button.SetActive(toggle);
        toggle = !toggle;
        VentLogger.Fatal($"Exists: {HistoryMenuIntermediate.HistoryMenu.Exists()}");
        HistoryMenu menu = HistoryMenuIntermediate.HistoryMenu.OrElseSet(() => HistoryMenu.Create().Get());
        VentLogger.Info($"Menu: {menu}");
        if (toggle) menu.Open();
        else menu.Close();
    }


    public bool Exists() => optionalButton.Exists();
}