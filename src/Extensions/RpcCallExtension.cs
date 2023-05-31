namespace Lotus.Extensions;

public static class RpcCallExtension
{
    public static string Name(this RpcCalls rpcCalls)
    {
        return rpcCalls switch
        {
            RpcCalls.PlayAnimation => "PlayAnimation",
            RpcCalls.CompleteTask => "CompleteTask",
            RpcCalls.SyncSettings => "SyncSettings",
            RpcCalls.SetInfected => "SetInfected",
            RpcCalls.Exiled => "Exiled",
            RpcCalls.CheckName => "CheckName",
            RpcCalls.SetName => "SetName",
            RpcCalls.CheckColor => "CheckColor",
            RpcCalls.SetColor => "SetColor",
            RpcCalls.SetHat => "SetHat",
            RpcCalls.SetSkin => "SetSkin",
            RpcCalls.ReportDeadBody => "ReportDeadBody",
            RpcCalls.MurderPlayer => "MurderPlayer",
            RpcCalls.SendChat => "SendChat",
            RpcCalls.StartMeeting => "StartMeeting",
            RpcCalls.SetScanner => "SetScanner",
            RpcCalls.SendChatNote => "SendChatNote",
            RpcCalls.SetPet => "SetPet",
            RpcCalls.SetStartCounter => "SetStartCounter",
            RpcCalls.EnterVent => "EnterVent",
            RpcCalls.ExitVent => "ExitVent",
            RpcCalls.SnapTo => "SnapTo",
            RpcCalls.CloseMeeting => "CloseMeeting",
            RpcCalls.VotingComplete => "VotingComplete",
            RpcCalls.CastVote => "CastVote",
            RpcCalls.ClearVote => "ClearVote",
            RpcCalls.AddVote => "AddVote",
            RpcCalls.CloseDoorsOfType => "CloseDoorsOfType",
            RpcCalls.RepairSystem => "RepairSystem",
            RpcCalls.SetTasks => "SetTasks",
            RpcCalls.ClimbLadder => "ClimbLadder",
            RpcCalls.UsePlatform => "UsePlatform",
            RpcCalls.SendQuickChat => "SendQuickChat",
            RpcCalls.BootFromVent => "BootFromVent",
            RpcCalls.UpdateSystem => "UpdateSystem",
            RpcCalls.SetVisor => "SetVisor",
            RpcCalls.SetNamePlate => "SetNamePlate",
            RpcCalls.SetLevel => "SetLevel",
            RpcCalls.SetHatStr => "SetHatStr",
            RpcCalls.SetSkinStr => "SetSkinStr",
            RpcCalls.SetPetStr => "SetPetStr",
            RpcCalls.SetVisorStr => "SetVisorStr",
            RpcCalls.SetNamePlateStr => "SetNamePlateStr",
            RpcCalls.SetRole => "SetRole",
            RpcCalls.ProtectPlayer => "ProtectPlayer",
            RpcCalls.Shapeshift => "Shapeshift",
            RpcCalls.CheckMurder => "CheckMurder",
            RpcCalls.CheckProtect => "CheckProtect",
            RpcCalls.Pet => "Pet",
            RpcCalls.CancelPet => "CancelPet",
            _ => rpcCalls.ToString()
        };
    }
}