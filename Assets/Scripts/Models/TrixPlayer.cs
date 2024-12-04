using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class TrixPlayer
{
    #region Constructor
    public TrixPlayer(string id, int teamOrder, string username, Sprite avatar, Sprite frame, int points, int order, bool botPlay, bool joinedGame, bool readyToPlay)
    {
        PlayerId = id;
        TeamOrder = teamOrder;
        UserName = username;
        
        Avatar = avatar;
        Frame = frame;

        canBeDroppedCards = new List<Card>();

        IsLocal = id == DewaniaSession.LocalPlayerId;
        Order = order;
        JoinedGame = joinedGame;

        Points = points;
        ReadyToPlay = readyToPlay;
        BotPlay = botPlay;

        PlayerDataChanaged?.Invoke(this);
    }
    #endregion

    #region Variables
    [JsonProperty("PlayerId")]
    public readonly string PlayerId;

    [JsonProperty("Team Order")]
    public readonly int TeamOrder;

    [JsonIgnore]
    public readonly string UserName; //Sync
    
    [JsonIgnore]
    public readonly Sprite Avatar; //Sync
    [JsonIgnore]
    public readonly Sprite Frame; //Sync
    
    [JsonProperty("Points")]
    private int points; //Sync
   
    [JsonProperty("Order")]
    public int Order; //Sync
    [JsonProperty("BotPlay")]
    private bool botPlay; //Sync
   
    [JsonIgnore]
    public readonly bool IsLocal; //Sync

    [JsonProperty("Joined Game")]
    public bool JoinedGame;
    [JsonProperty("ReadyToPlay")]
    private bool readyToPlay; //Sync
    [JsonIgnore]
    private List<Card> canBeDroppedCards;
    #endregion

    #region Props
    [JsonIgnore]
    public bool ReadyToPlay
    {
        get => readyToPlay;
        set
        {
            if (readyToPlay == value) return;

            readyToPlay = value;
            PlayerIsReady?.Invoke();
        }
    }
    
    [JsonIgnore]
    public int Points
    {
        get { return points; }
        set
        {
            if (points == value) return;

            points = value;
            PointsUpdated?.Invoke(value);
        }
    }

    [JsonIgnore]
    public bool BotPlay
    {
        get { return botPlay; }
        set
        {
            if(botPlay == value) return;

            botPlay = value;
            if(value) BecomeBot?.Invoke();
            else BecomePlayer?.Invoke();    

            if (botPlay && TrixController.GameState.Turn == PlayerId)
            {
                this.Bot_DropCard();
            }
        }
    }

    [JsonIgnore]
    public List<Card> Cards => TrixController.GameState.Cards.ToList().FindAll(o => o.OwnerId == PlayerId);

    [JsonIgnore]
    public List<Card> CanBeDroppedCards
    {
        get
        {
            if (canBeDroppedCards.Count == 0)
                ReAssignCanBeDroppedCards();

            return canBeDroppedCards;
        }
    }

    [JsonIgnore]
    Slot[] SpecialSlots => TrixController.GameState.SpecialSlots;

    [JsonIgnore]
    public Slot[] DoubledCards => Array.FindAll(SpecialSlots, o => !o.IsNull() && o.OwnerId == PlayerId);
    #endregion

    #region Actions
    [JsonIgnore]
    public Action<TrixPlayer> PlayerDataChanaged;

    [JsonIgnore]
    public Action<List<Card>> DisplayCards;
   
    [JsonIgnore]
    public Action<List<Card>> DisableInteractCards;
    [JsonIgnore]
    public Action<List<Card>> EnableInteractCards;

    [JsonIgnore]
    public Action<Slot> DoubledCard;
   
    [JsonIgnore]
    public Action<Slot> HideDoubledCard;

    [JsonIgnore]
    public Action<Card> DropCardToTrick;

    [JsonIgnore]
    public Action<Card> HideCardSuddenly;

    [JsonIgnore]
    public Action<int> PointsUpdated;

    [JsonIgnore]
    public Action PlayerIsReady;

    [JsonIgnore]
    public Action BecomeBot;

    [JsonIgnore]
    public Action BecomePlayer;
    #endregion

    #region Fns
    public void RefreshCards()
    {
        List<Card> cards = Cards;

        DisplayCards?.Invoke(cards);
    }

    public void ReAssignCanBeDroppedCards()
    {
        canBeDroppedCards = new List<Card>();
        List<Card> cantBeDroppedCards = new List<Card>();
        Cards.ForEach(card =>
        {
            if (card.CanBeDropped())
            {
                canBeDroppedCards.Add(card);
            }
            else
            {
                cantBeDroppedCards.Add(card);
            }
        });

        EnableInteractCards?.Invoke(canBeDroppedCards);
        DisableInteractCards?.Invoke(cantBeDroppedCards);
    }

    public bool HasCard(Card card)
    {
        return Cards.Exists(o => o.IsTheSameCard(card));
    }

    public bool HasGirls()
    {
        foreach (Card card in Cards)
        {
            if (card.Rank == CardRank.Girl) return true;
        }
        return false;
    }

    public bool HasGirls(out List<Card> girls)
    {
        girls = new List<Card>();
        foreach(Card card in Cards)
        {
            if (card.Rank == CardRank.Girl) girls.Add(card);
        }
        return girls.Count > 0;
    }

    public bool HasOldKoba()
    {
        return !Cards.Find(o => o.Suit == CardSuit.Koba && o.Rank == CardRank.Old).IsNull();
    }

    public bool HasOldKoba(out Card card)
    {
        card = Cards.Find(o => o.Suit == CardSuit.Koba && o.Rank == CardRank.Old);
        return !card.IsNull();
    }

    public bool HasCardsOfSuit(CardSuit suit)
    {
        return !Cards.Find(o => o.Suit == suit).IsNull();
    }

    public void DropCard(Card card)
    {
        if (!Cards.Exists(o => o.IsTheSameCard(card))) return;
        
        Cards.Remove(card);

        card.DropCard();
        DropCardToTrick?.Invoke(card);
    }
    #endregion

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
