using System;
using System.Collections.Generic;
using Lotus.Managers.History.Events;
using Lotus.Utilities;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.GUI.Menus;

public class HistoryMenu
{
    private UnityOptional<ChatController> chatOptional;

    public UnityOptional<Scroller> Scroller() => chatOptional.UnityMap(c => c.scroller);
    public UnityOptional<SpriteRenderer> Background() => chatOptional.UnityMap(c => c.BackgroundImage);
    public UnityOptional<ObjectPoolBehavior> ChatBubblePool() => chatOptional.UnityMap(c => c.chatBubPool);

    public UnityOptional<PassiveButton> historyTab;
    public UnityOptional<PassiveButton> winnersTab;
    public CustomOptional<WinnersMenu> winnersMenu;

    private List<IHistoryEvent> historyEvents = new();

    private int activeTab;

    private HistoryMenu(ChatController chatController, HudManager hudManager)
    {
        this.chatOptional = UnityOptional<ChatController>.Of(Object.Instantiate(chatController, hudManager.transform));
        chatController = this.chatOptional.Get();
        chatController.ChatButton.SetActive(false);

        var transform = chatController.transform;
        transform.localPosition += new Vector3(0.2f, -0.6f); // -1.4
        transform.localScale += new Vector3(0.3f, 0f);
        chatController.scroller.transform.localPosition += new Vector3(0.5f, -0.5f); //0.5

        var background = Background().Get();
        background.flipX = true;
        background.flipY = true;

        winnersMenu = CustomOptional<WinnersMenu>.Of(new WinnersMenu(chatController), menu => menu.Exists());

        PassiveButton parentButton = chatController.Content.FindChild<PassiveButton>("OpenKeyboardButton");
        PassiveButton historyTabButton = Object.Instantiate(parentButton, parentButton.transform.parent);
        PassiveButton winnersTabButton = Object.Instantiate(parentButton, parentButton.transform.parent);

        historyTabButton.GetComponentsInChildren<SpriteRenderer>().ForEach(sr => sr.sprite = Utils.LoadSprite("Lotus.assets.HistoryTab.png"));
        historyTabButton.transform.localPosition += new Vector3(-6f, 4.1f);
        historyTab = UnityOptional<PassiveButton>.Of(historyTabButton);


        winnersTabButton.GetComponentsInChildren<SpriteRenderer>().ForEach(sr => sr.sprite = Utils.LoadSprite("Lotus.assets.Winners.png"));
        winnersTabButton.transform.localPosition += new Vector3(-5.15f, 4.1f);
        winnersTab = UnityOptional<PassiveButton>.Of(winnersTabButton);

        historyTabButton.OnClick = new Button.ButtonClickedEvent();
        historyTabButton.OnClick.AddListener((Action)(() =>
        {
            Refresh();
            winnersMenu.IfPresent(wm => wm.Close());
        }));
        winnersTabButton.OnClick.AddListener((Action)(() =>
        {
            ChatBubblePool().IfPresent(bp => bp.ReclaimAll());
            winnersMenu.IfPresent(wm => wm.Open());
        }));

        InitChatPool();
    }

    public static CustomOptional<HistoryMenu> Create()
    {
        return CustomOptional<HistoryMenu>.From(new UnityOptional<HudManager>(HudManager.Instance)
            .UnityMap(m => m.Chat)
            .Map(c => new HistoryMenu(c, HudManager.Instance)),
            menu => menu.Exists());
    }

    public void Open(bool openWinnersMenu = false)
    {
        if (!chatOptional.Exists()) return;
        ChatController chatController = chatOptional.Get();
        ControllerManager.Instance.OpenOverlayMenu(chatController.name, chatController.BackButton, chatController.DefaultButtonSelected, chatController.ControllerSelectable);
        chatController.Content.SetActive(true);
        chatController.ChatButton.SetActive(false);
        chatController.TextArea.gameObject.SetActive(false);

        GameStartManager.Instance.StartButton.enabled = false;
        GameStartManager.Instance.startLabelText.enabled = false;
        GameStartManager.Instance.PlayerCounter.gameObject.SetActive(false);

        try
        {
            chatController.Content.FindChild<Transform>("TypingArea").gameObject.SetActive(false);
            chatController.Content.FindChild<Transform>("BanMenuButton").gameObject.SetActive(false);
            chatController.Content.FindChild<PassiveButton>("OpenKeyboardButton").transform.localPosition += new Vector3(100f, 100f);
            chatController.Content.FindChild<PassiveButton>("QuickChatButton").gameObject.SetActive(false);
        }
        catch
        {
            chatController.TypingArea.gameObject.SetActive(false);
            chatController.BanButton.gameObject.SetActive(false);
            chatController.OpenKeyboardButton.SetActive(false);
            chatController.ChatButton.SetActive(false);
            // ignored
        }


        Refresh();

        AddLog(PlayerControl.LocalPlayer.PlayerId, "Hello World!");
        chatController.scroller.ScrollToTop();

        winnersMenu.IfPresent(wm =>
        {
            if (!openWinnersMenu) wm.Close();
            else
            {
                ChatBubblePool().IfPresent(bp => bp.ReclaimAll());
                wm.Open();
            }
        });
    }

    public void Close()
    {
        GameStartManager.Instance.StartButton.enabled = true;
        GameStartManager.Instance.startLabelText.enabled = true;
        GameStartManager.Instance.PlayerCounter.gameObject.SetActive(true);

        if (!chatOptional.Exists()) return;
        ChatController chatController = chatOptional.Get();
        ControllerManager.Instance.CloseOverlayMenu(chatController.name);
        chatController.Content.SetActive(false);
    }

    public void AddLog(byte playerId, string chatText)
    {
        if (!ChatBubblePool().Exists()) return;
        HistoryMenuIntermediate.StoreOutfits();
        ObjectPoolBehavior chatBubblePool = ChatBubblePool().Get();
        ChatController chatController = chatOptional.Get();

        if (chatBubblePool.NotInUse == 0) chatBubblePool.ReclaimOldest();
        ChatBubble bubble = chatBubblePool.Get<ChatBubble>();
        bubble.Background.transform.localScale = new Vector3(1.5f, 1f, 1f);
        bubble.TextArea.transform.localScale = new Vector3(0.7f, 1f, 1f);
        bubble.SetLeft();

        Transform transform;
        (transform = bubble.transform).SetParent(Scroller().Get().Inner);
        transform.localScale = Vector3.one;
        bubble.SetCosmetics(PlayerControl.LocalPlayer.Data);
        chatController.SetChatBubbleName(bubble, PlayerControl.LocalPlayer.Data, false, false, PlayerNameColor.Get(PlayerControl.LocalPlayer.Data));
        bubble.TextArea.richText = true;
        bubble.SetText(chatText);

        bubble.NameText.gameObject.SetActive(false);
        bubble.TextArea.transform.localPosition += new Vector3(0f, 0.2f);
        bubble.TextArea.GetComponent<RectTransform>().sizeDelta = new Vector2(6.8f, 0f);
        if (bubble.TextArea.GetNotDumbRenderedHeight() < 0.3f) bubble.Player.transform.localPosition = new Vector3(0f, 0.1f);

        bubble.Background.size = new Vector2(5.52f, 0.2f + bubble.TextArea.GetNotDumbRenderedHeight());
        bubble.MaskArea.size = bubble.Background.size - new Vector2(0.0f, 0.03f);
        var playerTransform = bubble.Player.transform;
        playerTransform.localScale = new Vector3(0.3f, 0.4f, 1f);
        playerTransform.localPosition += new Vector3(0.1f, 0f);

        /*bubble.MaskArea.material.SetInt(PlayerMaterial.MaskLayer, 0);
        bubble.Background.material.SetInt(PlayerMaterial.MaskLayer, 0);*/
        HistoryMenuIntermediate.GetOutfit(playerId).IfPresent(outfit => bubble.Player.UpdateFromPlayerOutfit(outfit, PlayerMaterial.MaskType.ScrollingUI, false, false));

        bubble.TextArea.overrideColorTags = false;
        bubble.AlignChildren();
        chatController.AlignAllBubbles();

        var bubbleTransform = bubble.Background.transform;
        var bubblePosition = bubbleTransform.localPosition;
        bubbleTransform.localPosition = new Vector3(4.2f, bubblePosition.y);
    }

    public void SetHistoryEvents(List<IHistoryEvent> historyEvents)
    {
        List<IHistoryEvent> filteredEvents = new();
        IHistoryEvent lastEvent = null!;
        foreach (IHistoryEvent historyEvent in historyEvents)
        {
            filteredEvents.Add(historyEvent);
            if (lastEvent is IDeathEvent deathEvent)
            {
                if (historyEvent is not KillEvent killEvent) continue;
                if (deathEvent.Player().PlayerId == killEvent.Target().PlayerId) filteredEvents.Remove(historyEvent);
            }

            lastEvent = historyEvent;
        }

        this.historyEvents = filteredEvents;

    }

    public void Refresh()
    {
        InitChatPool();
        historyEvents.ForEach(he => AddLog(he.Player().PlayerId, he.GenerateMessage()));
    }

    private void InitChatPool()
    {
        ChatBubblePool().IfPresent(pool =>
        {
            pool.ReclaimAll();
            pool.InitPool(pool.Prefab);
            for (int i = 0; i < 80; i++) pool.CreateOneInactive(pool.Prefab);
        });
    }

    public bool Exists() => chatOptional.Exists();
}