
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Overrides;
using Lotus.RPC;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using static Lotus.ModConstants.Palette;
using static Lotus.Roles.RoleGroups.Impostors.IdentityThief.Translations.Options;

namespace Lotus.Roles.RoleGroups.Impostors;

public class IdentityThief : Impostor
{
    private bool unshiftOnCooldown;
    private bool totallySwapIdentity;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (Relationship(target) is Relation.FullAllies) return false;
        DirectInteraction directInteraction = new(new FakeFatalIntent(), this);
        InteractionResult result = MyPlayer.InteractWith(target, directInteraction);
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));

        if (result is InteractionResult.Halt) return false;
        if (!unshiftOnCooldown && totallySwapIdentity) return HandlePermanentShift(target);
        
        MyPlayer.RpcShapeshift(target, true);
        if (unshiftOnCooldown) Async.Schedule(() => MyPlayer.RpcRevertShapeshift(true), KillCooldown);
        ProtectedRpc.CheckMurder(MyPlayer, target);
        return true;
    }
    

    private bool HandlePermanentShift(PlayerControl target)
    {
        GameData.PlayerInfo myInfo = MyPlayer.Data;
        GameData.PlayerOutfit myOutfit = myInfo.DefaultOutfit;
        uint myLevel = myInfo.PlayerLevel;
        
        GameData.PlayerInfo stolenIdentity = target.Data;
        GameData.PlayerOutfit targetOutfit = stolenIdentity.DefaultOutfit;
        uint targetLevel = stolenIdentity.PlayerLevel;
        
        myInfo.SetOutfit(PlayerOutfitType.Default, targetOutfit);
        myInfo.PlayerLevel = targetLevel;
        
        stolenIdentity.SetOutfit(PlayerOutfitType.Default, myOutfit);
        stolenIdentity.PlayerLevel = myLevel;
        
        NameHolder targetNameHolder = target.NameModel().GCH<NameHolder>();
        targetNameHolder.First().SetMainText(new LiveString(MyPlayer.name));
        targetNameHolder.Add(new NameComponent(new LiveString(target.name), Game.IgnStates, ViewMode.Replace, target));
        MyPlayer.NameModel().GCH<NameHolder>().Add(new NameComponent(new LiveString(target.name, Color.white), Game.IgnStates, ViewMode.Replace));
        MyPlayer.name = target.name;
        
        ProtectedRpc.CheckMurder(MyPlayer, target);
        
        Players.SendPlayerData(myInfo);
        Players.SendPlayerData(stolenIdentity);
        
        Async.Schedule(() => MyPlayer.CRpcShapeshift(MyPlayer, true), NetUtils.DeriveDelay(0.05f));
        MyPlayer.NameModel().Render(Game.GetAllPlayers().ToList());
        return true;
    }
    
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Disguise Settings", DisguiseSettings)
                .Value(v => v.Text(UntilKillCooldown).Value(true).Color(GeneralColor4).Build())
                .Value(v => v.Text(UntilNextKill).Value(false).Color(GeneralColor5).Build())
                .BindBool(b => unshiftOnCooldown = b)
                .ShowSubOptionPredicate(b => !(bool)b)
                .SubOption(sub2 => sub2.KeyName("Totally Swap Identity", TotallySwapIdentity)
                    .AddOnOffValues(false)
                    .BindBool(b => totallySwapIdentity = b)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier)
            .OptionOverride(Override.ShapeshiftDuration, 3000f)
            .OptionOverride(Override.ShapeshiftCooldown, 0.001f);

    [Localized(nameof(IdentityThief))]
    internal static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(DisguiseSettings))]
            public static string DisguiseSettings = "Disguise Settings";

            [Localized(nameof(UntilKillCooldown))]
            public static string UntilKillCooldown = "Kill CD";
            
            [Localized(nameof(UntilNextKill))]
            public static string UntilNextKill = "Next Kill";

            [Localized(nameof(TotallySwapIdentity))]
            public static string TotallySwapIdentity = "Totally Swap Identity";
        }
    }
}