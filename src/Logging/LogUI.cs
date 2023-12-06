using System.Collections.Generic;
using Il2CppSystem;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using IntPtr = System.IntPtr;

namespace Lotus.Logging;

[RegisterInIl2Cpp]
public class LogUI: MonoBehaviour
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LogUI));
    private FreeChatInputField chatInputField;
    private TextMeshPro title;
    private GameObject anchorObject;

    public LogUI(IntPtr intPtr) : base(intPtr)
    {
        anchorObject = gameObject.CreateChild("Anchor", new Vector3(0.3f, 2.8f), new Vector3(0.5f, 0.5f));
        title = anchorObject.QuickComponent<TextMeshPro>("Title", new Vector3(8.6f, -3.8f));
        title.fontSize = 3;
        anchorObject.SetActive(false);
    }

    public void PassRequirements(HudManager hudManager)
    {
        object i = 0;
        int c = 200;
        bool truth = (int)c > (int)i;

               chatInputField = Instantiate(hudManager.Chat.chatScreen.GetComponentInChildren<FreeChatInputField>(), anchorObject.transform);
        chatInputField.charCountText.transform.localPosition += new Vector3(0f, 100f, 0f);
        chatInputField.OnSubmitEvent = (Action)Submit;
        chatInputField.gameObject.transform.localPosition = new Vector3(-0.24f, -2.08f, -5f);
        chatInputField.background.transform.localScale = new Vector3(0.6f, 1f, 1f);
    }

    private void Start()
    {
        Async.Schedule(() =>
        {
            chatInputField.submitButton.text.text = "Submit";
            chatInputField.submitButton.transform.localPosition = new Vector3(0.845f, 0.02f, -0.1f);
            chatInputField.textArea.transform.localPosition = new Vector3(-1.755f, 0f, -0.1f);
        }, 0.01f);
        Async.Schedule(() => title.text = "Enter Log Name", 0.01f);
    }

    public void Open()
    {
        anchorObject.SetActive(true);
        Async.Schedule(() =>
        {
            chatInputField.submitButton.text.text = "Submit";
            chatInputField.submitButton.transform.localPosition = new Vector3(0.855f, 0.02f, -0.1f);
            chatInputField.textArea.transform.localPosition = new Vector3(-1.755f, 0f, -0.1f);
            chatInputField.textArea.GiveFocus();
        }, 0.01f);
    }

    public void Submit()
    {
        anchorObject.SetActive(false);
        OnTextSubmit?.Invoke(chatInputField.Text);
        chatInputField.Clear();
    }

    public event TextSubmitEvent? OnTextSubmit;

    public delegate void TextSubmitEvent(string text);
}