using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Slot
{
    #region Constructor
    public Slot(string player, Card card)
    {
        OwnerId = player;
        Card = card;   
    }
    #endregion

    #region Variables
    [JsonProperty("Owner")]
    public readonly string OwnerId;

    [JsonProperty("Card")]
    private Card card;
    #endregion

    #region Props
    [JsonIgnore]
    public Card Card
    {
        get { return card; }
        set { card = value; }
    }
    
    [JsonIgnore]
    public TrixPlayer Owner => TrixController.GameState.GetPlayer(OwnerId);
    #endregion

    #region Fns
    public bool IsTheSameSlot(Slot slot)
    {
        return card.IsTheSameCard(slot.card) && OwnerId == slot.OwnerId;
    }

    public bool IsEmpty()
    {
        return card.IsNull() | OwnerId.IsNull();
    }

    public void ResetSlot()
    {
        card = null;
    }
    #endregion
}
