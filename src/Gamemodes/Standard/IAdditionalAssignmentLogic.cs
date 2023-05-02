using System.Collections.Generic;
using TOHTOR.API.Odyssey;

namespace TOHTOR.Gamemodes.Standard;

public interface IAdditionalAssignmentLogic
{
    /// <summary>
    /// Allows for injecting custom role assignment into the standard gamemode.
    /// <br/>
    /// <b>Important</b>
    /// <br/>
    /// You should have a rough understanding of how role assignment works and roles should <b>ALWAYS</b> call <see cref="TOHTOR.Roles.CustomRole.Instantiate"/>
    /// Additionally, the standard-algorithm uses <see cref="Game.AssignRole"/> for actually applying the role to players. You should replicate
    /// this behaviour but DO NOT set sendToClient=true
    /// </summary>
    /// <param name="allPlayers">a list of all the players</param>
    /// <param name="unassignedPlayers">list of players who don't have roles assigned. <b>You are responsible for updating this list</b></param>
    public void AssignRoles(List<PlayerControl> allPlayers, List<PlayerControl> unassignedPlayers);
}