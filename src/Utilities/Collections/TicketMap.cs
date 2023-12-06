using System.Collections.Generic;
using System.Data;
using UnityEngine;
using VentLib.Utilities.Extensions;

namespace Lotus.Utilities.Collections;

public class TicketMap<T>
{
    private List<T> specialTickets = new();
    private List<Ticket> tickets = new();
    private int ticketCount;
    private int nextTicketID;

    public bool WasRemoved { get; private set; }

    public int AddEntry(T item, int count = 1, int maximumPulls = 1) => AddEntry(nextTicketID++, item, count, maximumPulls);

    public int AddEntry(int ticketID, T item, int count = 1, int maximumPulls = 1)
    {
        if (count >= 100) specialTickets.Add(item);
        else
        {
            ticketCount += count;
            tickets.Add(new Ticket(ticketID, item, count, maximumPulls));
        }

        return ticketID;
    }

    public T Next()
    {
        if (IsEmpty()) throw new ConstraintException("Map is empty");
        WasRemoved = false;

        if (!specialTickets.IsEmpty())
        {
            WasRemoved = true;
            return specialTickets.PopRandom();
        }

        int ticketNumber = Random.RandomRangeInt(1, ticketCount + 1);
        int threshold = 0;

        for (int i = 0; i < tickets.Count; i++)
        {
            Ticket ticket = tickets[i];
            threshold += ticket.Count;
            if (ticketNumber > threshold) continue;

            ticket.Pulls += 1;

            WasRemoved = ticket.Count == 0 || ticket.Pulls >= ticket.MaximumPulls;
            if (!WasRemoved) return ticket.Item;

            tickets.RemoveAt(i); // Safe because we only call this when we're returning a ticket item

            int j = 0;
            while (j < tickets.Count)
            {
                Ticket removal = tickets[j];
                if (removal.ID == ticket.ID)
                {
                    tickets.RemoveAt(j);
                    ticketCount -= removal.Count;
                }
                else j++;
            }

            ticketCount -= ticket.Count;

            return ticket.Item;
        }

        throw new ConstraintException("Error in ticket calculation");
    }

    public bool IsEmpty() => tickets.IsEmpty() && specialTickets.IsEmpty();

    private class Ticket
    {
        public int ID;
        public T Item;
        public int Count;
        public int Pulls;
        public int MaximumPulls;

        public Ticket(int id, T item, int count, int maximumPulls)
        {
            ID = id;
            Item = item;
            Count = count;
            MaximumPulls = maximumPulls;
        }
    }
}