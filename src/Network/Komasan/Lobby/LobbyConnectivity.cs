using System.Net.Http;
using AmongUs.Data;
using InnerNet;
using Lotus.Network.Komasan.DTO;
using Lotus.Network.Komasan.RestClient;
using Lotus.Utilities;
using VentLib.Networking;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Network.Komasan.Lobby;

public class LobbyConnectivity
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LobbyConnectivity));
    private static FixedUpdateLock _requestLock = new(1.5f);
    private const string UnresolvedLobbyUrl = $"{NetConstants.Host}{NetConstants.CreateLobbyEndpoint}";

    private static bool lobbyOpen;
    private static long refreshId;

    [QuickPrefix(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
    public static void SendCreateLobbyRequest(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost || !_requestLock.AcquireLock() || lobbyOpen) return;
        log.Info($"Lobby Created: {AmongUsClient.Instance.GameId}", "ModdedLobbyCheck");
        if (!NetworkRules.AllowRoomDiscovery) return;
        log.Info("Posting Room to Public", "RoomDiscovery");

        Async.ExecuteThreaded(() =>
        {
            Komajiro komajiro = Komajiro.Instance;
            RestResponse<KomasanLobbyResponse> lobbyResponse = komajiro.Execute<KomasanLobbyResponse>(HttpMethod.Put, UnresolvedLobbyUrl, req => req
                .AppendPath(AmongUsClient.Instance.GameId.ToString())
                .Body(new KomasanLobbyCreatedRequest
                {
                    HostName = DataManager.Player.customization.name,
                    Region = GetCurrentRegion(),
                    ModName = ProjectLotus.ModName,
                    ModVersion = ProjectLotus.Instance.CurrentVersion.ToSimpleName(),
                    Players = PlayerControl.AllPlayerControls.Count,
                    SecondsRemaining = 600-(int)__instance.countDownTimer
                }).Build());

            if (lobbyResponse.IsError())
            {
                log.Warn($"Failed to send lobby create request. (ErrorCode={lobbyResponse.Status}, Response={lobbyResponse.GetRawBody()})");
                return;
            }

            KomasanLobbyResponse lr = lobbyResponse.GetBody();
            long currentRefresh = refreshId;
            Async.ScheduleThreaded(() => RefreshLobby(currentRefresh), lr.RefreshIn);
        });
    }

    [QuickPrefix(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
    private static void SendLobbyClosedRequest(InnerNetClient __instance, DisconnectReasons reason)
    {
        if (reason is DisconnectReasons.NewConnection || !__instance.AmHost || !lobbyOpen) return;
        lobbyOpen = false;
        log.Trace("Sending lobby closed status");

        Komajiro.Instance.ExecuteThreaded<KomasanLobbyResponse>(HttpMethod.Put, UnresolvedLobbyUrl, req => req
            .AppendPath(AmongUsClient.Instance.GameId.ToString())
            .Body(new KomasanLobbyClosedRequest("Host Disconnected")).Build());
    }

    private static void RefreshLobby(long currentRefresh)
    {
        if (currentRefresh != refreshId) return;
    }



    private static AURegion GetCurrentRegion()
    {
        string regionName = ServerManager.Instance.CurrentRegion.Name;
        return regionName switch
        {
            "North America" => AURegion.NORTH_AMERICA,
            "Europe" => AURegion.EUROPE,
            "Asia" => AURegion.ASIA,
            _ => ResolveModdedRegion()
        };

        AURegion ResolveModdedRegion()
        {
            if (regionName.Contains("Asia")) return AURegion.MODDED_ASIA;
            return regionName.Contains("Europe") ? AURegion.MODDED_EUROPE : AURegion.NORTH_AMERICA;
        }
    }
}