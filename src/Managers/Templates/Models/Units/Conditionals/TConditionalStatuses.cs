using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

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
        return statusesLower.Any(status => MatchData.GetStatuses(player)?.Any(st => st.Name.ToLower().Equals(status.ToLower())) ?? false);
    }
}