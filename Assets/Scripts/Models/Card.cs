using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Card : IComparable<Card>
{
    #region Constructor
    public Card(CardSuit suit, CardRank rank)
    {
        Suit = suit;
        Rank = rank;
        doubled = false;
        OwnerId = "";
    }

    public Card(int suit, int rank)
    {
        Suit = (CardSuit)suit;
        Rank = (CardRank)rank;
        doubled = false;
        OwnerId = "";
    }
    #endregion

    #region Fields
    [JsonProperty("Suit")]
    public CardSuit Suit;
    [JsonProperty("Rank")]
    public CardRank Rank;

    [JsonIgnore]
    [SerializeField] private bool canBeDoubled;

    [JsonProperty("Doubled")]
    [SerializeField] private bool doubled; //Sync

    [JsonProperty("Owner")]
    private string ownerId;
    #endregion

    #region Props
    [JsonIgnore]
    public string OwnerId
    {
        get { return ownerId; }
        private set
        {
            if (ownerId == value) return;

            ownerId = value;
        }
    }

    [JsonIgnore]
    public bool CanBeDoubled
    {
        get { return canBeDoubled; }
    }
    [JsonIgnore]
    public bool Doubled
    {
        get { return doubled; }
        set
        {
            if (!CanBeDoubled)
            {
                doubled = false;
                UnDouble();
                return;
            }

            if (doubled == value)
                return;

            doubled = value;

            if (doubled)
            {
                Double();
            }
            else
            {
                UnDouble();
            }
        }
    }

    [JsonIgnore]
    public TrixPlayer Owner => TrixController.GameState.GetPlayer(OwnerId);
    #endregion

    #region Calls
    public void SetOwner(TrixPlayer player)
    {
        if (player.IsNull()) return;

        OwnerId = player.PlayerId;
    }

    void Double()
    {
        Slot temp = new Slot(ownerId, this);
        TrixController.GameState.AddSpecialSlot(temp);
        Owner.DoubledCard?.Invoke(temp);
    }

    public void RefreshDoubleState()
    {
        if (doubled)
        {
            Slot temp = TrixController.GameState.GetSpcialSlot(this);
            if (temp.IsNull())
            {
                Double();
                return;
            }

            Owner.DoubledCard?.Invoke(temp);
        }
    }

    void UnDouble()
    {
        Slot temp = TrixController.GameState.GetSpcialSlot(this);
        if (temp.IsNull()) return;

        TrixController.GameState.RemoveSpecialSlot(temp);
        Owner.HideDoubledCard?.Invoke(temp);
    }

    public void DropCard()
    {
        OwnerId = "";
        doubled = false;
    }

    public bool Dropped()
    {
        return ownerId.IsNull();
    }

    public void ReAssignCanBeDoubled()
    {
        canBeDoubled = (TrixController.GameState.Tolba == TolbaEnum.OldKoba && Suit == CardSuit.Koba && Rank == CardRank.Old) 
            || (TrixController.GameState.Tolba == TolbaEnum.Girls && Rank == CardRank.Girl);
    }

    public bool IsStronger(Card card, CardSuit suit)
    {
        if(Suit == suit && card.Suit == suit)
        {
            return Rank > card.Rank;
        }
        else if(Suit == suit)
        {
            return true;
        }
        return false;
    }
    public Card GetStronger(Card card, CardSuit suit)
    {
        if (IsStronger(card, suit))
            return this;
        return card;
    }

    public bool IsTheSameCard(Card card)
    {
        return card.Suit == Suit && card.Rank == Rank;
    }

    public bool CanBeDropped()
    {
        if(TrixController.GameState.Stage != GameStageEnum.DropCards) return false;
        if (OwnerId.IsNull()) return false;
        if (TrixController.GameState.CurrentTurnPlayer.PlayerId != Owner.PlayerId) return false;

        Trick last = TrixController.GameState.CurrentTrick();
        if (last.IsNull()) return false;

        int index = last.GetNextLatchOrder();

        return IsObyOrderOfTrix() || (TrixController.GameState.Tolba != TolbaEnum.TRS && (index == 1 || Suit == last.Suit || !Owner.HasCardsOfSuit(last.Suit)));
    }

    public bool IsObyOrderOfTrix()
    {
        if (TrixController.GameState.Tolba != TolbaEnum.TRS) return false;

        int rank = ((int)Rank);
        
        if (rank == 11) return true;

        if (rank > 11) if (TrixController.GameState.GetCard(Suit, (CardRank)(rank - 1)).Owner.IsNull()) return true;

        if (rank < 11) if (TrixController.GameState.GetCard(Suit, (CardRank)(rank + 1)).Owner.IsNull()) return true;

        return false;
    }

    public override string ToString()
    {
        return string.Format("[{0},{1}]", Suit, Rank);
    }

    public int CompareTo(Card other)
    {
        if(Suit != other.Suit)
        {
            return ((int)Suit).CompareTo(((int)other.Suit));
        }

        return ((int)Rank).CompareTo(((int)other.Rank));
    }
    #endregion
}