using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.Crew;

public class Tracker: Crewmate
{
    private bool canTrackBodies;
    
    private byte trackedPlayeer;
    
    
    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier)
            .RoleColor(new Color(0.82f, 0.24f, 0.82f));
}