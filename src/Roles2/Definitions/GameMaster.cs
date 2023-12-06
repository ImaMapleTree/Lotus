using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2.Attributes.Roles;
using Lotus.Roles2.Components;
using UnityEngine;
using VentLib.Options.Game.Interfaces;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2.Definitions;

[RoleTriggers]
public class GameMaster: RoleDefinition, IMetadataEditor
{
    public static Color GMColor = new(1f, 0.4f, 0.4f);

    public override RoleTypes Role => RoleTypes.Crewmate;
    public override Color RoleColor { get; set; } = GMColor;
    public override IFaction Faction { get; set; } = FactionInstances.Neutral;
    public override IGameOptionTab OptionTab => DefaultTabs.HiddenTab;

    [RoleAction(LotusActionType.RoundStart)]
    public void ExileGameMaster(bool roundStart)
    {
        if (!roundStart) return;
        MyPlayer.RpcExileV2(false);

        Players.GetPlayers().Where(p => p.PlayerId != MyPlayer.PlayerId)
            .SelectMany(p => p.NameModel().ComponentHolders())
            .ForEach(holders =>
                {
                    holders.AddListener(component => component.AddViewer(MyPlayer));
                    holders.Components().ForEach(components => components.AddViewer(MyPlayer));
                }
            );

        MyPlayer.NameModel().Render(force: true);
    }

    public void ModifyMetadata(RoleMetadata metadata)
    {
        metadata.Get(RoleProperties.Key).AddAll(RoleProperty.CannotWinAlone, RoleProperty.IsApparition);
        metadata.Set(LotusKeys.GloballyManagedRole, true);
    }
}