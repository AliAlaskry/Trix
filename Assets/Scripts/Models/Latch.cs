using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class Latch
{
    #region Constructor
    public Latch(Slot slot, int rank)
    {
        Slot = slot;
        Rank = rank;
    }
    #endregion

    #region Fields
    [JsonProperty("Slot")]
    Slot slot;

    [JsonProperty("Rank")]
    public readonly int Rank;
    #endregion

    #region Props
    [JsonIgnore]
    public Slot Slot
    {
        get { return slot; }
        private set { slot = value; }
    }

    [JsonIgnore]
    public TrixPlayer Thrower => slot.Owner;
    #endregion

    #region Fns
    public void CardThrowed(Slot slot)
    {
        Slot = slot;
    }

    public Latch Stronger(Latch latch, CardSuit suit)
    {
        Card temp = slot.Card.GetStronger(latch.slot.Card, suit);
        if (temp.IsTheSameCard(slot.Card))
        {
            return this;
        }

        return latch;
    }

    public bool IsEmpty()
    {
        return slot.IsNull() || slot.IsEmpty();
    }

    public void ResetLatch()
    {
        slot = null;
    }
    #endregion
}