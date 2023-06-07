using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.Options;

public static class DesyncOptions
{
    public static void SyncToAll(IGameOptions options) => Game.GetAllPlayers().Do(p => SyncToPlayer(options, p));

    public static void SyncToPlayer(IGameOptions options, PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (player == null) return;
        if (!player.AmOwner)
        {
            try {
                SyncToClient(options, player.GetClientId());
            }
            catch (Exception exception) {
                VentLogger.Exception(exception, "Error syncing game options to client.");
            }
            return;
        }

        GameOptionsManager.Instance.currentGameOptions = options;

        var normalOptions = options.TryCast<NormalGameOptionsV07>();
        if (normalOptions != null)
            GameManager.Instance.LogicOptions.Cast<LogicOptionsNormal>().GameOptions = normalOptions;
        GameOptionsManager.Instance.currentGameOptions = options;
    }

    public static void SyncToClient(IGameOptions options, int clientId)
    {
        GameOptionsFactory optionsFactory = GameOptionsManager.Instance.gameOptionsFactory;

        MessageWriter messageWriter = MessageWriter.Get(); // Start message writer
        messageWriter.StartMessage(6); // Initial control-flow path for packet receival (Line 1352 InnerNetClient.cs) || This can be changed to "5" and remove line 20 to sync options to everybody
        messageWriter.Write(AmongUsClient.Instance.GameId); // Write 4 byte GameId
        messageWriter.WritePacked(clientId); // Target player ID

        messageWriter.StartMessage(1); // Second control-flow path specifically for changing game options
        messageWriter.WritePacked(GetManagerClientId()); // Packed ID for game manager

        messageWriter.StartMessage(4); // Index of logic component in GameManager (4 is current LogicOptionsNormal)
        optionsFactory.ToNetworkMessageWithSize(messageWriter, options); // Write options to message

        messageWriter.EndMessage(); // Finish message 1
        messageWriter.EndMessage(); // Finish message 2
        messageWriter.EndMessage(); // Finish message 3
        AmongUsClient.Instance.SendOrDisconnect(messageWriter); // Wrap up send
        messageWriter.Recycle(); // Recycle
    }

    public static int GetTargetedClientId(string name)
    {
        int clientId = -1;
        var allClients = AmongUsClient.Instance.allObjectsFast;
        var allClientIds = allClients.Keys;

        foreach (uint id in allClientIds)
            if (clientId == -1 && allClients[id].name.Contains(name))
                clientId = (int)id;
        return clientId;
    }

    // This method is used to find the "GameManager" client which is now needed for synchronizing options
    public static int GetManagerClientId() => GetTargetedClientId("Manager");

    public static IGameOptions GetModifiedOptions(IEnumerable<GameOptionOverride> overrides)
    {
        IGameOptions clonedOptions = AUSettings.StaticOptions.DeepCopy();
        overrides.Where(o => o != null!).Do(optionOverride => optionOverride.ApplyTo(clonedOptions));
        return clonedOptions;
    }

    public static void SendModifiedOptions(IEnumerable<GameOptionOverride> overrides, PlayerControl player)
    {
        SyncToPlayer(GetModifiedOptions(overrides), player);
    }


}