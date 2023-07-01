using System;
using System.Collections;
using System.Collections.Generic;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles;

public class RoleLottery: IEnumerator<CustomRole>, IEnumerable<CustomRole>
{
    protected UuidList<CustomRole> Roles = new();
    protected List<Ticket> Tickets = new();
    protected List<Ticket> PriorityTickets = new();
    protected uint BatchNumber;

    private CustomRole defaultRole;
    private CustomRole? current;

    private Dictionary<string, int> roleLimitTracker = new();

    public RoleLottery(CustomRole defaultRole)
    {
        this.defaultRole = defaultRole;
        Roles.Add(defaultRole);
    }

    public virtual void AddRole(CustomRole role, bool useSubsequentChance = false)
    {
        int chance = useSubsequentChance ? role.AdditionalChance : role.Chance;
        if (chance == 0 || role.RoleFlags.HasFlag(RoleFlag.Unassignable)) return;
        uint id = Roles.Add(role);
        uint batch = BatchNumber++;

        string roleId = ProjectLotus.RoleManager.GetIdentifier(role);


        // If the role chance is at 100, we move it into the priority list
        if (chance >= 100)
        {
            PriorityTickets.Add(new Ticket { Id = id, Batch = batch, RoleId = roleId});
            return;
        }

        // Add tickets for the new role first
        for (int i = 0; i < chance; i++) Tickets.Add(new Ticket { Id = id, Batch = batch, RoleId = roleId});

        // Add tickets for the new role second
        for (int i = 0; i < 100 - chance; i++) Tickets.Add(new Ticket { Id = 0, Batch = batch, RoleId = roleId});
    }


    public bool MoveNext()
    {
        current = Next();
        return HasNext();
    }

    public bool HasNext()
    {
        return Tickets.Count > 0 || PriorityTickets.Count > 0;
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
            if (PriorityTickets.Count == 0 && Tickets.Count == 0) return defaultRole;

            Ticket ticket = PriorityTickets.Count > 0 ? PriorityTickets.PopRandom() : Tickets.PopRandom();
            Tickets.RemoveAll(t => t.Batch == ticket.Batch);

            CustomRole associatedRole = Roles.Get(ticket.Id);
            int count = roleLimitTracker.GetOrCompute(ticket.RoleId, () => 0);
            if (count >= associatedRole.Count) continue;

            roleLimitTracker[ticket.RoleId] += 1;
            AddRole(associatedRole, true);
            return associatedRole;
        }
    }

    public CustomRole Current => current ??= Next();

    object IEnumerator.Current => Current;

    protected struct Ticket
    {
        public uint Id;
        public uint Batch;
        public string RoleId;

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