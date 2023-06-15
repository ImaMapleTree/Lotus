using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Statuses;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models.Units.Impl;

public class TConditionStatuses: StringListConditionalUnit
{
    private HashSet<string>? statusesLower;

    public TConditionStatuses(object input) : base(input)
    {
    }

    public override bool Evaluate(object? data)
    {
        return data is not PlayerControl player || VerifyStatus(player);
    }

    public bool VerifyStatus(PlayerControl? player)
    {
        if (player == null) return true;
        statusesLower ??= Values.Select(r => r.ToLower()).ToHashSet();
        return statusesLower.Any(status => Game.MatchData.Statuses.GetOrCompute(player.PlayerId, () => new RemoteList<IStatus>()).Any(st => st.Name.ToLower().Equals(status.ToLower())));
    }
}