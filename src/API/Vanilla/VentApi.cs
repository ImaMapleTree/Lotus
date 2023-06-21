using System.Collections.Generic;
using System.Linq;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Vanilla;

public class VentApi
{
    public static void ForceNoVenting(PlayerControl player, int ventId = -1)
    {
        if (player.NetTransform == null) return;
        Vector2 originalPos = player.GetTruePosition();
        if (ventId == -1)
        {
            List<Vent> vents = Object.FindObjectsOfType<Vent>().ToList();
            if (vents.Count == 0) return;
            ventId = vents.Sorted(v => Vector2.Distance(originalPos, v.transform.position)).First().Id;
        }
        
        
        RpcV3.Mass()
            .Start(player.MyPhysics.NetId, RpcCalls.EnterVent).WritePacked(ventId).End()
            .Start(player.MyPhysics.NetId, RpcCalls.BootFromVent).WritePacked(ventId).End()
            .Send();
        
        Async.Schedule(() => Utils.Teleport(player.NetTransform, originalPos), 0.3f);
    }
}