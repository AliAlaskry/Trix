using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UnityEngine;

[Serializable]
public class Trick : IComparable<Trick>
{
    #region Constructor
    public Trick(int no)
    {
        No = no; 
        FirstLatch = null;
        Latches = new Latch[4]
        {
            new Latch(null, 1),
            new Latch(null, 2),
            new Latch(null, 3),
            new Latch(null, 4),
        };
        FirstLatch = null;
        TakerId = null;
    }
    #endregion

    #region Fields
    [JsonProperty("No")]
    public readonly int No; //Sync
    [JsonProperty("Latches")]
    public readonly Latch[] Latches = new Latch[4];
    [JsonProperty("FirstLatch")]
    Latch firstLatch;

    [JsonProperty("Taker")]
    private string takerId;
    #endregion

    #region Props
    [JsonIgnore]
    public Latch FirstLatch
    {
        get { return firstLatch; }
        private set { firstLatch = value; }
    }

    [JsonIgnore]
    public CardSuit Suit => FirstLatch.Slot.Card.Suit;

    [JsonIgnore]
    public string TakerId
    {
        get => takerId;
        private set => takerId = value;
    }

    [JsonIgnore]
    public TrixPlayer Taker => TrixController.GameState.GetPlayer(takerId);
    #endregion

    #region Fns
    int GetNextLatchIndex()
    {
        for (int i = 0; i < Latches.Length; i++)
        {
            if (Latches[i].IsEmpty())
            {
                return i;
            }
        }

        return -1;
    }

    public int GetNextLatchOrder()
    {
        for (int i = 0; i < Latches.Length; i++)
        {
            if (Latches[i].IsEmpty())
            {
                return Latches[i].Rank;
            }
        }

        return -1;
    }

    public Latch StronggestLatch(CardSuit suit)
    {
        Latch temp = Latches[0];
        temp = temp.Stronger(Latches[1], suit);
        temp = temp.Stronger(Latches[2], suit);
        temp = temp.Stronger(Latches[3], suit);
        return temp;
    }

    public bool ContainsLatchOfCard(Card card)
    {
        return !Array.Find(Latches, o => !o.IsEmpty() && o.Slot.Card.IsTheSameCard(card)).IsNull();
    }

    public void AddLatch(Slot slot)
    {
        int index = GetNextLatchIndex();
        if(index == -1)
        {
            //todo: debug error
            return;
        }

        Latches[index].CardThrowed(slot);
        if (index == 0)
        {
            FirstLatch = Latches[index];
        }
    }

    public void AddTaker(string takerId)
    {
        TakerId = takerId;
    }

    public bool HasCard(Card card)
    {
        foreach(var latch in Latches)
        {
            if (latch.Slot.Card.IsTheSameCard(card))
                return true;
        }

        return false;
    }

    public Latch GetLatchOfCard(Card card)
    {
        foreach (var latch in Latches)
        {
            if (latch.Slot.Card.IsTheSameCard(card))
                return latch;
        }

        return null;
    }

    public Latch GetLatchOfRank(CardRank rank)
    {
        foreach (var latch in Latches)
        {
            if (latch.Slot.Card.Rank == rank)
                return latch;
        }

        return null;
    }

    public Latch GetLatchOfOrder(int order)
    {
        for (int i = 0; i < Latches.Length; i++)
        {
            if (Latches[i].Rank == order)
            {
                return Latches[i];
            }
        }

        return null;
    }

    public Latch GetLatchOfOwner(TrixPlayer owner)
    {
        return Array.Find(Latches, o => !o.IsNull() && !o.IsEmpty() && o.Thrower.PlayerId == owner.PlayerId);
    }

    public bool HasCardOfSuit(CardSuit suit)
    {
        foreach (var latch in Latches)
        {
            if (latch.Slot.Card.Suit == suit)
                return true;
        }

        return false;
    }

    public bool HasCardOfRank(CardRank rank)
    {
        foreach (var latch in Latches)
        {
            if (latch.Slot.Card.Rank == rank)
                return true;
        }

        return false;
    }

    public CardSuit[] GetGirlsSuits()
    {
        return Array.FindAll(Latches, o => !o.IsEmpty() && o.Slot.Card.Rank == CardRank.Girl).Select<Latch, CardSuit>(latch =>
        {
            return latch.Slot.Card.Suit;
        }).ToArray();
    }

    public bool IsEmpty()
    {
        return Array.FindAll(Latches, o => !o.IsEmpty()).Length == 0;
    }

    public bool HasFullLatchesData()
    {
        return Array.FindAll(Latches, o => o.IsEmpty()).Length == 0;
    }

    public bool HasFullData()
    {
        return HasFullLatchesData() && !Taker.IsNull();
    }

    public void ResetTrick()
    {
        foreach(var l in Latches)
        {
            l.ResetLatch();
        }
        FirstLatch = null;
        TakerId = null;
    }

    public int CompareTo(Trick other)
    {
        return No.CompareTo(other.No);
    }
    #endregion
}
