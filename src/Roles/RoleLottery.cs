using System;
using System.Collections;
using System.Collections.Generic;
using Lotus.Managers;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles;

public class RoleLottery: IEnumerator<CustomRole>, IEnumerable<CustomRole>
{
    private UuidList<CustomRole> roles = new();
    private List<Ticket> tickets = new();
    private List<Ticket> priorityTickets = new();

    private uint batchNumber;
    private CustomRole defaultRole;
    private CustomRole? current;

    private Dictionary<int, int> roleLimitTracker = new();

    public RoleLottery(CustomRole defaultRole)
    {
        this.defaultRole = defaultRole;
        roles.Add(defaultRole);
    }

    public void AddRole(CustomRole role, bool useSubsequentChance = false)
    {
        int chance = useSubsequentChance ? role.AdditionalChance : role.Chance;
        if (chance == 0 || role.RoleFlags.HasFlag(RoleFlag.Unassignable)) return;
        uint id = roles.Add(role);
        uint batch = batchNumber++;

        int roleId = CustomRoleManager.GetRoleId(role);


        // If the role chance is at 100, we move it into the priority list
        if (chance >= 100)
        {
            priorityTickets.Add(new Ticket { Id = id, Batch = batch, RoleId = roleId});
            return;
        }

        // Add tickets for the new role first
        for (int i = 0; i < chance; i++) tickets.Add(new Ticket { Id = id, Batch = batch, RoleId = roleId});

        // Add tickets for the new role second
        for (int i = 0; i < 100 - chance; i++) tickets.Add(new Ticket { Id = 0, Batch = batch, RoleId = roleId});
    }


    public bool MoveNext()
    {
        current = Next();
        return tickets.Count > 0 || priorityTickets.Count > 0;
    }

    public void Reset()
    {
    }

    public CustomRole Next()
    {
        while (true)
        {
            current = null;
            // Infinite loop break condition (bc we'll exhaust the ticket pool eventually)
            if (priorityTickets.Count == 0 && tickets.Count == 0) return defaultRole;

            Ticket ticket = priorityTickets.Count > 0 ? priorityTickets.PopRandom() : tickets.PopRandom();
            tickets.RemoveAll(t => t.Batch == ticket.Batch);

            CustomRole associatedRole = roles.Get(ticket.Id);
            int count = roleLimitTracker.GetOrCompute(ticket.RoleId, () => 0);
            if (count >= associatedRole.Count) continue;

            roleLimitTracker[ticket.RoleId] += 1;
            AddRole(associatedRole, true);
            return associatedRole;
        }
    }

    public CustomRole Current => current ??= Next();

    object IEnumerator.Current => Current;

    private struct Ticket
    {
        public uint Id;
        public uint Batch;
        public int RoleId;

        public override bool Equals(object? obj)
        {
            if (obj is not Ticket ticket) return false;
            return ticket.Batch == Batch;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Batch);
    }

    public void Dispose()
    {
    }

    public IEnumerator<CustomRole> GetEnumerator() => this;

    IEnumerator IEnumerable.GetEnumerator() => this;
}