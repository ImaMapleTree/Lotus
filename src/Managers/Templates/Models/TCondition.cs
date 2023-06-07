using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models;

// ReSharper disable once InconsistentNaming
public class TCondition
{
    public HashSet<State>? States { get; set; }
    public PlayerStatus Status { set; get; } = PlayerStatus.Any;
    public UserType User { get; set; } = UserType.Everyone;
    public HashSet<string>? Roles { get; set; }
    public HashSet<string>? EnabledRoles { get; set; }

    private HashSet<string>? enabledRolesLower;
    private HashSet<string>? rolesLower;

    public bool VerifyEnabledRoles()
    {
        if (EnabledRoles == null) return true;
        enabledRolesLower ??= EnabledRoles.Select(r => r.ToLower()).ToHashSet();
        HashSet<string> gameEnabledRoleCache = new();
        bool iterated = false;
        foreach (string role in enabledRolesLower)
        {
            if (gameEnabledRoleCache.Contains(role)) return true;
            if (iterated) continue;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (CustomRole customRole in CustomRoleManager.AllRoles)
            {
                if (!customRole.IsEnabled()) continue;
                gameEnabledRoleCache.Add(customRole.RoleName.ToLower());
                gameEnabledRoleCache.Add(customRole.EnglishRoleName.ToLower());
                if (gameEnabledRoleCache.Contains(role)) return true;
            }

            iterated = true;
        }

        return false;
    }

    public bool VerifyPlayer(PlayerControl player)
    {
        return VerifyRole(player) && VerifyStatus(player);
    }

    public bool VerifyRole(PlayerControl? player)
    {
        if (Roles == null || player == null) return true;
        rolesLower ??= Roles.Select(r => r.ToLower()).ToHashSet();
        return VerifyRole(player.GetCustomRole()) || player.GetSubroles().Any(VerifyRole);
    }

    public bool VerifyRole(CustomRole role)
    {
        if (rolesLower == null) return true;
        string englishRoleName = role.EnglishRoleName.ToLower();
        string anyRoleName = role.RoleName.ToLower();
        return rolesLower.Contains(englishRoleName) || rolesLower.Contains(anyRoleName);
    }

    public bool VerifyState()
    {
        if (States == null) return true;
        foreach (State state in States)
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (state)
            {
                case State.Lobby when Game.State is GameState.InLobby:
                    return true;
                case State.Game when Game.State is GameState.Roaming:
                    return true;
                case State.Meeting when Game.State is GameState.InMeeting:
                    return true;
            }

        return false;
    }

    public bool VerifyStatus(PlayerControl? player)
    {
        if (Status is PlayerStatus.Any || player == null) return true;
        return Status is PlayerStatus.Alive ? player.IsAlive() : !player.IsAlive();
    }

    public bool VerifyUnspecific()
    {
        return VerifyEnabledRoles() && VerifyState();
    }

    public bool VerifyUser(PlayerControl player)
    {
        return User switch
        {
            UserType.Everyone => true,
            UserType.Host => player.IsHost(),
            UserType.Admins => player.IsHost(), // TODO
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public enum State
    {
        Lobby,
        Game,
        Meeting
    }

    public enum PlayerStatus
    {
        Any,
        Dead,
        Alive
    }

    public enum UserType
    {
        Everyone,
        Host,
        Admins
    }
}