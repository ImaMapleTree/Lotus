using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Threading;
using Lotus.API.Odyssey;
using Lotus.API.Stats;
using Lotus.Managers.History;
using Lotus.Roles;
using Lotus.Roles2;
using TMPro;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.GUI.Menus;

public class WinnersMenu
{
    public static List<Statistic> EligibleEndgameStats = DisplayEligibleStats();
    public UnityOptional<GameObject> GameObject;
    private UnityOptional<PoolablePlayer> playerPrefab;
    private UnityOptional<TextMeshPro> textMeshProPrefab;

    private List<PlayerHistory>? lastPlayers = new();
    private List<UnityOptional<PoolablePlayer>> poolablePlayers = new();

    public WinnersMenu(ChatController chatParent)
    {
        GameObject gameObject = new();
        gameObject.transform.SetParent(chatParent.scroller.Inner.transform);

        GameObject = UnityOptional<GameObject>.Of(gameObject);
        playerPrefab = UnityOptional<PoolablePlayer>.Of(Object.Instantiate(HudManager.Instance.IntroPrefab.PlayerPrefab, gameObject.transform));
        playerPrefab.IfPresent(p => p.gameObject.SetActive(false));

        playerPrefab.IfPresent(p =>
        {
            p.cosmetics.nameText.text = $"Test Player\n{Color.red.Colorize("Impostor")}\nKills: 5";
            p.gameObject.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            p.gameObject.transform.localPosition += new Vector3(-3.1f, 0.925f);
            p.cosmetics.nameText.transform.localPosition -= new Vector3(0f, 1.15f);
            p.cosmetics.nameText.transform.localScale += new Vector3(1f, 1f);
        });

        HudManager.Instance.GameSettings.sortingOrder = -1;
        chatParent.backgroundImage.sortingOrder = -1;

        textMeshProPrefab = UnityOptional<TextMeshPro>.Of(Object.Instantiate(chatParent.freeChatField.textArea.outputText, gameObject.transform));
        textMeshProPrefab.IfPresent(text =>
        {

            text.font = GameStartManager.Instance.startLabelText.font;
            text.color = Color.white;
            text.m_fontSizeMin = 8f;
            text.transform.localPosition += new Vector3(-0.85f, 2f);
            text.text = "Results";
        });
    }

    public void Open()
    {
        Async.Schedule(() =>
        {
            List<PlayerHistory>? allPlayers = Game.MatchData.GameHistory.PlayerHistory;
            if (lastPlayers != allPlayers) Async.Schedule(() => GeneratePlayers(allPlayers), 0.1f);
            lastPlayers = allPlayers!;
            GameObject.IfPresent(gameObject => gameObject.SetActive(true));
        }, 0.1f);

        //chatController.IfPresent(cc => Async.Schedule(() => cc.scroller.ContentYBounds.max = 5f, 0.1f));
    }

    public void Close()
    {
        GameObject.IfPresent(gameObject => gameObject.SetActive(false));
    }

    public bool Exists() => GameObject.Exists();

    private void GeneratePlayers(List<PlayerHistory>? allPlayers)
    {
        HashSet<byte> winners = Game.MatchData.GameHistory.LastWinners.Select(p => p.MyPlayer.PlayerId).ToHashSet();

        poolablePlayers.ForEach(pw => pw.IfPresent(pp => pp.gameObject.SetActive(false)));
        poolablePlayers.Clear();

        if (!playerPrefab.Exists() || allPlayers == null) return;

        allPlayers = allPlayers.Sorted(p => winners.Contains(p.PlayerId) ? (int)p.Status : 99 + (int)p.Status).ToList();
        PoolablePlayer prefabPlayer = playerPrefab.Get();
        prefabPlayer.InitBody();
        prefabPlayer.UpdateFromLocalPlayer(PlayerMaterial.MaskType.ComplexUI);

        for (int index = 0; index < allPlayers.Count; index++)
        {
            PlayerHistory playerHistory = allPlayers[index];
            UnifiedRoleDefinition role = playerHistory.PrimaryRoleDefinition;
            PoolablePlayer newPlayer = prefabPlayer;//Object.Instantiate(prefabPlayer, prefabPlayer.transform.parent);
            newPlayer.gameObject.SetActive(true);
            newPlayer.UpdateFromPlayerOutfit(playerHistory.Outfit, PlayerMaterial.MaskType.ComplexUI, false, false);

            int row = Mathf.FloorToInt(index / 5f);

            newPlayer.transform.localPosition += new Vector3(1.65f * (index - (row * 5)), -row * 1.27f, 0);


            string bestStat = EligibleEndgameStats
                .Sorted(stat => stat.GetGenericValue(playerHistory.UniquePlayerId) as IComparable ?? -999)
                .Reverse()
                .FirstOrOptional()
                .Map(stat => $"{stat.Name()}: {stat.GetGenericValue(playerHistory.UniquePlayerId)}")
                .OrElse("");

            string name = playerHistory.Name + "\n" + role.RoleColor.Colorize(role.Name) + "\n" + bestStat;
            newPlayer.SetName(name);

            TextMeshPro aboveNameTmp = Object.Instantiate(newPlayer.cosmetics.nameText, newPlayer.transform);
            aboveNameTmp.transform.localPosition += new Vector3(0f, 2.6f, 0f);
            aboveNameTmp.transform.localScale += new Vector3(1f, 1f, 0f);

            string aboveNameText = winners.Contains(playerHistory.PlayerId) ? new Color(1f, 0.83f, 0.24f).Colorize("Winner") : "";
            aboveNameText += "\n<size=1.25>" + StatusColor(playerHistory.Status).Colorize(playerHistory.Status.ToString()) + "</size>";
            aboveNameTmp.text = aboveNameText;

            poolablePlayers.Add(UnityOptional<PoolablePlayer>.Of(newPlayer));
            Thread.Sleep(30);
        }
    }

    private Color StatusColor(PlayerStatus status)
    {
        return status switch
        {
            PlayerStatus.Alive => Color.green,
            PlayerStatus.Exiled => new Color(0.73f, 0.54f, 0.45f),
            PlayerStatus.Disconnected => Color.grey,
            PlayerStatus.Dead => Color.red,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private static List<Statistic> DisplayEligibleStats() => new()
    {
        VanillaStatistics.Kills, VanillaStatistics.BodiesReported, VanillaStatistics.SabotagesCalled,
        VanillaStatistics.SabotagesFixed, VanillaStatistics.PlayersExiled
    };

}