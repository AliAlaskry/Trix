using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[Serializable]
public class TrixState
{
    #region Constants
    const string WaitForPlayersTimer = "W_5_S_F_P_J_G";
    const string TakeTrickWaitTimer = "T_T_W_T";
    #endregion

    #region Constructor
    public TrixState(bool isCombine)
    {        
        IsTeams = isCombine;

        UpdateLocalStage(GameStageEnum.WaitingForPlayers);

        LoyalNo = 0;
        TolbaNO = 0;

        ownerOfLoyalId = null;

        turn = null;

        Teams = isCombine ? new TrixTeam[2] : new TrixTeam[4];

        TRSTolbaPlayerIdsOrder = new List<string>();

        specialSlots = new Slot[4];

        GenerateStaticCards();
        GenerateStaticTricks();
    }
    #endregion

    #region Variables
    [JsonProperty("IsTeams")]
    public readonly bool IsTeams;

    [JsonProperty("Stage")]
    private GameStageEnum stage; // Sync
    [JsonIgnore]
    public GameStageEnum Stage
    {
        get { return stage; }
    }
    [JsonProperty("LoyalNo")]
    [Range(1, 5)] public byte LoyalNo; //Sync
    [JsonProperty("TolbaNO")]
    [Range(1, 5)] public int TolbaNO; //Sync //bitwise operation
    [JsonProperty("OwnerOfLoyalId")]
    private string ownerOfLoyalId; //Sync
    [JsonProperty("Tolba")]
    public TolbaEnum Tolba; //Sync 
    [JsonProperty("Turn")]
    private string turn; //Sync
    
    [JsonProperty("TRS Tolba Player Order")]
    public List<string> TRSTolbaPlayerIdsOrder;
    
    [JsonProperty("Teams")]
    public TrixTeam[] Teams; //Sync

    [JsonProperty("Special Slots")]
    private Slot[] specialSlots = new Slot[4];

    [JsonProperty("Cards")]
    public Card[] Cards = new Card[52]; //Sync
    [JsonProperty("Tricks")]
    public Trick[] Tricks = new Trick[13]; //Sync
    #endregion

    #region Props
    [JsonIgnore]
    public TrixPlayer[] Players
    {
        get
        {
            TrixPlayer[] temp = new TrixPlayer[4] { null, null, null, null};
            int index = 0;
            foreach (var team in Teams)
            {
                if (!team.IsNull())
                {
                    foreach (var player in team.Players)
                    {
                        temp[index] = player;
                        index++;
                    }
                }
            }
            return temp;
        }
    }

    [JsonIgnore]
    public string Turn
    {
        get { return turn; }
        /*todo: private*/ set
        {
            if (turn == value) return;

            turn = value;
            TurnUpdated?.Invoke(CurrentTurnPlayer);
        }
    }

    [JsonIgnore]
    public string OwnerOfLoyalId
    {
        get { return ownerOfLoyalId; }
        set
        {
            if (ownerOfLoyalId == value) return;

            ownerOfLoyalId = value;
            LoyalOwnerUpdated?.Invoke(OwnerOfLoyal);
        }
    }

    [JsonIgnore]
    public TrixTeam LocalTeam => Array.Find(Teams, o => !o.IsNull() && (o.Player_1.IsLocal || o.Player_2.IsLocal));

    [JsonIgnore]
    public TrixPlayer LocalPlayer => Array.Find(Players, o => !o.IsNull() && o.IsLocal);
    [JsonIgnore]
    Counters Counters => Counters.Instance;
    [JsonIgnore]
    public TrixPlayer OwnerOfLoyal { get { return GetPlayer(OwnerOfLoyalId); } }
    [JsonIgnore]
    public TrixPlayer CurrentTurnPlayer => GetPlayer(Turn);
    [JsonIgnore]
    public Slot[] SpecialSlots => specialSlots;
    #endregion

    #region Actions
    [JsonIgnore]
    public Action<GameStageEnum> StageUpdated;

    [JsonIgnore]
    public Action ProccessingAndLoadingData;

    [JsonIgnore]
    public Action LoadedData;

    [JsonIgnore]
    public Action WaitingForPlayers;

    [JsonIgnore]
    public Action PlayersJoinedGame;

    [JsonIgnore]
    public Action<TrixPlayer> LoyalOwnerUpdated;

    [JsonIgnore]
    public Action<TrixPlayer> TurnUpdated;

    [JsonIgnore]
    public Action<TolbaEnum> TolbaSelected;

    [JsonIgnore]
    public Action<Trick> PreviousTrickUpdated;
    [JsonIgnore]
    public Action<Trick> CurrentTrickUpdated;
    #endregion

    #region Main Fns
    void GenerateStaticCards()
    {
        Cards = new Card[52];
        for (byte i = 1; i < 5; i++)
        {
            for (byte j = 2; j < 15; j++)
            {
                Cards[13 * i + j - 15] = new Card(i, j);
            }
        }
    }

    void GenerateStaticTricks()
    {
        Tricks = new Trick[13];
        for (int i = 1; i < 14; i++)
        {
            Trick temp = new Trick(i - 1);
            Tricks[i - 1] = temp;
        }
    }

    void UpdateLocalStage(GameStageEnum st)
    {
        stage = st;
        StageUpdated?.Invoke(stage);
    }

    void UpdateRemoteStage(GameStageEnum st)
    {
        stage = st;
        ImplementNewStage();
        StageUpdated?.Invoke(stage);
    }

    void ImplementNewStage()
    {
        switch (stage)
        {
            case GameStageEnum.WaitingForPlayers:
                OnWaiting();
                break;

            case GameStageEnum.SelectOwnerOfLoyal:
                SelectOwnerOfLoyal();
                break;

            case GameStageEnum.SelectTolba:
                SelectTolba();
                break;

            case GameStageEnum.DoublingCards:
                if (AllPlayersReadyToPlay())
                {
                    UpdateLocalStage(GameStageEnum.DropCards);
                    SendUpdate();
                    return;
                }
                ReAssignCanDoubleCard();
                DoublingCards();
                break;

            case GameStageEnum.DropCards:
                DropCard();
                break;

            case GameStageEnum.TakeTrick:
                TakeTrick();
                break;
            case GameStageEnum.EndOfTolba:
                EndOfTolba();
                break;
            case GameStageEnum.EndOfGame:
                EndOfGame();
                break;
        }
    }

    public void SendUpdate()
    {
        Debug.Log("update\n" + this.ToString());
        TrixController.GameState.RecieveState(this);
        //todo: clear comments
        //string data = JsonConvert.SerializeObject(this);
        //DewaniaHostController.UpdateGameState(data);
    }
    #endregion

    #region Secondary Fns
    public TrixTeam GetTeam(int teamOrder)
    {
        return Array.Find(Teams, o => !o.IsNull() && o.Order == teamOrder);
    }

    public TrixPlayer GetPlayer(string id)
    {
        return Array.Find(Players, o => !o.IsNull() && o.PlayerId == id);
    }

    public TrixPlayer GetPlayer(int order)
    {
        return Array.Find(Players, o => !o.IsNull() && o.Order == order);
    }

    public Card GetCard(CardSuit suit, CardRank rank)
    {
        return Array.Find(Cards, o => o.Suit == suit && o.Rank == rank);
    }

    public Card GetCard(Card card)
    {
        return Array.Find(Cards, o => o.IsTheSameCard(card));
    }

    public Latch LastDroppedLatch()
    {
        Trick currentTrick = CurrentTrick();
        if (currentTrick.IsNull() || currentTrick.IsEmpty())
        {
            currentTrick = PreviousTrick();
        }

        if (currentTrick.IsNull() || currentTrick.IsEmpty())
            return null;

        int index = currentTrick.GetNextLatchOrder();

        if (index == -1) return currentTrick.GetLatchOfOrder(4);
        else return currentTrick.GetLatchOfOrder(index - 1);
    }

    public bool IsDropped(Card card)
    {
        Trick current = CurrentTrick();
        if (current.IsNull()) return false;

        int startIndex = current.No;
        for (int i = startIndex; i >= 0; i--)
            if (Tricks[i].HasCard(card))
                return true;

        return false;
    }

    void ReAssignCanDoubleCard()
    {
        foreach (var item in Cards)
        {
            item.ReAssignCanBeDoubled();
        }
    }

    bool AllPlayersJoinedGame()
    {
        return Array.FindAll(Players, o => o.JoinedGame).Length >= 4;
    }

    bool AllPlayersReadyToPlay()
    {
        return Array.FindAll(Players, o => o.ReadyToPlay).Length >= 4;
    }

    public Trick PreviousTrick()
    {
        for (int i = 0, count = Tricks.Length - 1; i < count; i++)
        {
            if (!Tricks[i + 1].HasFullData() && Tricks[i].HasFullData())
                return Tricks[i];
        }

        return null;
    }

    public Trick CurrentTrick()
    {
        for (int i = 0, count = Tricks.Length; i < count; i++)
        {
            if (!Tricks[i].HasFullData())
                return Tricks[i];
        }
        //todo: show error 
        return null;
    }

    public void ResetTricks()
    {
        foreach (Trick trick in Tricks)
        {
            trick.ResetTrick();
        }
    }

    public bool AllGirlsDropped()
    {
        Card[] girls = new Card[4]
        {
            GetCard(CardSuit.Koba, CardRank.Girl),
            GetCard(CardSuit.Bastony, CardRank.Girl),
            GetCard(CardSuit.Denary, CardRank.Girl),
            GetCard(CardSuit.Sanak, CardRank.Girl),
        };

        bool AllGirlsExist = true;
        AllGirlsExist &= !Array.Find(Tricks, o => o.ContainsLatchOfCard(girls[0])).IsNull();
        AllGirlsExist &= !Array.Find(Tricks, o => o.ContainsLatchOfCard(girls[1])).IsNull();
        AllGirlsExist &= !Array.Find(Tricks, o => o.ContainsLatchOfCard(girls[2])).IsNull();
        AllGirlsExist &= !Array.Find(Tricks, o => o.ContainsLatchOfCard(girls[3])).IsNull();

        return AllGirlsExist;
    }

    public bool PlayersAdded()
    {
        foreach (TrixPlayer player in Players)
        {
            if (player.IsNull()) return false;
        }
        return true;
    }

    public void AddSpecialSlot(Slot slot)
    {
        if(slot.IsEmpty())
        {
            Debug.Log("try to add empty special slot");
            return;
        }

        if (!slot.Card.Doubled)
        {
            Debug.Log("try to add none special slot as spcial slot");
            return;
        }

        if (Array.Exists(specialSlots, o => !o.IsNull() && o.IsTheSameSlot(slot))) return;

        int index = Array.FindIndex(specialSlots, o => o.IsNull() || o.IsEmpty());

        Debug.Log("add special slot at " + index);
        
        specialSlots[index] = slot;
    }

    public void RemoveSpecialSlot(Slot slot)
    {
        if (slot.IsEmpty())
        {
            Debug.Log("try to remove empty special slot");
            return;
        }

        if (slot.Card.Doubled)
        {
            Debug.Log("try to remove card which is special");
            return;
        }

        int index = Array.FindIndex(specialSlots, o => !o.IsNull() && o.IsTheSameSlot(slot));
        if(index != -1) specialSlots[index] = null;
    }

    public void RemoveSpecialSlot(Card card)
    {
        if (card.IsNull())
        {
            Debug.Log("try to remove empty card");
            return;
        }

        if (card.Doubled)
        {
            Debug.Log("try to remove card which is special");
            return;
        }

        int index = Array.FindIndex(specialSlots, o => !o.IsNull() && o.Card.IsTheSameCard(card));
        if (index != -1) specialSlots[index] = null;
    }

    public Slot GetSpcialSlot(Card card)
    {
        return Array.Find(specialSlots, o => !o.IsNull() && o.Card.IsTheSameCard(card));
    }

    [ContextMenu("Check Spcial Slot Data")]
    void CheckSpecialSlots()
    {
        foreach(var card in Cards)
        {
            Slot special = GetSpcialSlot(card);
            if (card.Doubled && (special.IsNull() || special.IsEmpty()))
            {
                Debug.Log("card is doubled but none exist, review your code senarios");
            }
        }
    }

    void DebugSpecialSlots()
    {
        string log = "";
        if(!specialSlots[0].IsNull()) log += $"[0]:{specialSlots[0].ToString()}";
        if(!specialSlots[1].IsNull()) log += $"[1]:{specialSlots[1].ToString()}";
        if(!specialSlots[2].IsNull()) log += $"[2]:{specialSlots[2].ToString()}";
        if(!specialSlots[3].IsNull()) log += $"[3]:{specialSlots[3].ToString()}";
        Debug.Log(log);
    }
    #endregion

    #region Stage Implementation Fns
    void OnWaiting()
    {
        WaitingForPlayers?.Invoke();

        if (!LocalPlayer.JoinedGame)
        {
            LocalPlayer.JoinedGame = true;
            SendUpdate();
        }

        if (!Counters.HasCountDown(WaitForPlayersTimer))
        {
            Counters.AddCountDown(WaitForPlayersTimer, 5, () =>
            {
                PlayersJoinedGame?.Invoke();

                foreach (var player in Players)
                    if (!player.JoinedGame)
                        player.BotPlay = true;

                UpdateLocalStage(GameStageEnum.SelectOwnerOfLoyal);

                SendUpdate();
            });
            Counters.StartCountDown(WaitForPlayersTimer);
        }
        else
        {
            if (!Counters.IsCounterDownActive(WaitForPlayersTimer))
            {
                Counters.ModifyCountdownValue(WaitForPlayersTimer, 5);
                Counters.StartCountDown(WaitForPlayersTimer);
            }
        }

        if (AllPlayersJoinedGame())
        {
            PlayersJoinedGame?.Invoke();

            Counters.RemoveCountDown_WaitUntillEndOfUpdateExcusion(WaitForPlayersTimer);

            UpdateLocalStage(GameStageEnum.SelectOwnerOfLoyal);

            SendUpdate();
        }
    }

    void SelectOwnerOfLoyal()
    {
        PlayersJoinedGame?.Invoke();

        ProccessingAndLoadingData?.Invoke();

        TolbaNO = 0;
        LoyalNo++;

        if (OwnerOfLoyalId.IsNull())
        {
            OwnerOfLoyalId = Players[UnityEngine.Random.Range(0, Players.Length)].PlayerId;
            //todo: remove next statement
            OwnerOfLoyalId = Players[0].PlayerId;
        }
        else
        {
            int order = OwnerOfLoyal.Order + 1;
            order %= Players.Length;

            OwnerOfLoyalId = Array.Find(Players, o => o.Order == order).PlayerId;
        }

        LoyalOwnerUpdated?.Invoke(OwnerOfLoyal);
        LoadedData?.Invoke();

        UpdateLocalStage(GameStageEnum.SelectTolba);

        SendUpdate();
    }

    void SelectTolba()
    {
        ResetTricks();

        specialSlots = new Slot[4];

        foreach (var player in Players) player.ReadyToPlay = false;

        DistributeCards();

        if (OwnerOfLoyal.BotPlay)
        {
            OwnerOfLoyal.Bot_SelectTolba();
        }
    }

    void DistributeCards()
    {
        List<Card> temp = new List<Card>(Cards);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Card card = temp[UnityEngine.Random.Range(0, temp.Count)];
                card.SetOwner(Players[i]);
                temp.Remove(card);
            }
        }
    }

    void DoublingCards()
    {
        bool TolbaGirlsAndDontHave = Tolba == TolbaEnum.Girls && !LocalPlayer.HasGirls();
        bool TolbaKobaAndDontHave = Tolba == TolbaEnum.OldKoba && !LocalPlayer.HasOldKoba();
        if (!LocalPlayer.ReadyToPlay && (TolbaGirlsAndDontHave || TolbaKobaAndDontHave))
        {
            LocalPlayer.ReadyToPlay = true;
            SendUpdate();
        }
    }

    void DropCard()
    {
        if (CurrentTurnPlayer.BotPlay)
        {
            CurrentTurnPlayer.Bot_DropCard();
        }
    }

    void TakeTrick()
    {
        Counters.AddCountDown(TakeTrickWaitTimer, 1.5f, () =>
        {
            Trick current = CurrentTrick();

            Latch stronggeset = current.StronggestLatch(current.Suit);
            current.AddTaker(stronggeset.Thrower.PlayerId);

            PreviousTrickUpdated?.Invoke(current);

            if (current.No == 12
                || (Tolba == TolbaEnum.OldKoba && current.ContainsLatchOfCard(GetCard(CardSuit.Koba, CardRank.Old)))
                || (Tolba == TolbaEnum.Girls && AllGirlsDropped()))
            {
                UpdateLocalStage(GameStageEnum.EndOfTolba);
            }
            else
            {
                Turn = current.TakerId;
                UpdateLocalStage(GameStageEnum.DropCards);
            }

            CurrentTrickUpdated?.Invoke(null);

            SendUpdate();
        });
        Counters.StartCountDown(TakeTrickWaitTimer);
    }

    void EndOfTolba()
    {
        CalculatePoints();
        if (TolbaNO == 31)
        {
            if (LoyalNo >= 4)
            {
                UpdateLocalStage(GameStageEnum.EndOfGame);
            }
            else
            {
                UpdateLocalStage(GameStageEnum.SelectOwnerOfLoyal);
            }
        }
        else
        {
            UpdateLocalStage(GameStageEnum.SelectTolba);
        }
        SendUpdate();
    }

    void CalculatePoints()
    {
        // these variables to avoid name duplication in switch case statement
        Card card;
        Trick cardTrick;

        TrixPlayer firstThrower;
        TrixPlayer CardOwner;

        switch (Tolba)
        {
            case TolbaEnum.Lotoch:
                foreach (var trick in Tricks)
                {
                    GetTeam(trick.Taker.TeamOrder).Points -= 15;
                }
                break;

            case TolbaEnum.OldKoba:
                card = GetCard(CardSuit.Koba, CardRank.Old);
                cardTrick = Array.Find(Tricks, o => o.HasFullData() && o.HasCard(card));

                firstThrower = cardTrick.FirstLatch.Thrower;
                CardOwner = cardTrick.GetLatchOfCard(card).Thrower;

                if (GetSpcialSlot(card).IsNull() ||
                    (CardOwner.PlayerId == cardTrick.TakerId && CardOwner.PlayerId == firstThrower.PlayerId))
                {
                    GetTeam(cardTrick.Taker.TeamOrder).Points -= 75;
                }
                else
                {
                    if (CardOwner.PlayerId == cardTrick.TakerId)
                    {
                        GetTeam(CardOwner.TeamOrder).Points -= 150;
                        GetTeam(firstThrower.TeamOrder).Points += 75;
                    }
                    else
                    {
                        GetTeam(cardTrick.Taker.TeamOrder).Points -= 150;
                        GetTeam(CardOwner.TeamOrder).Points += 75;
                    }
                }
                break;

            case TolbaEnum.Girls:
                Trick[] GirlsTricks = Array.FindAll(Tricks, o => o.HasFullData() && o.HasCardOfRank(CardRank.Girl));

                foreach (var girlTrick in GirlsTricks)
                {
                    CardSuit[] suits = girlTrick.GetGirlsSuits();

                    foreach (var suit in suits)
                    {
                        card = GetCard(suit, CardRank.Girl);
                        cardTrick = girlTrick;

                        firstThrower = cardTrick.FirstLatch.Thrower;
                        CardOwner = cardTrick.GetLatchOfCard(card).Thrower;

                        if (GetSpcialSlot(card).IsNull() ||
                            (CardOwner.PlayerId == cardTrick.TakerId && CardOwner.PlayerId == firstThrower.PlayerId))
                        {
                            GetTeam(cardTrick.Taker.TeamOrder).Points -= 25;
                        }
                        else
                        {
                            if (CardOwner.PlayerId == cardTrick.TakerId)
                            {
                                GetTeam(CardOwner.TeamOrder).Points -= 50;
                                GetTeam(firstThrower.TeamOrder).Points += 25;
                            }
                            else
                            {
                                GetTeam(cardTrick.Taker.TeamOrder).Points -= 50;
                                GetTeam(CardOwner.TeamOrder).Points += 25;
                            }
                        }
                    }
                }
                break;

            case TolbaEnum.Denary:
                Trick[] DenaryTricks = Array.FindAll(Tricks, o => o.HasFullData() && o.HasCardOfSuit(CardSuit.Denary));

                foreach (var trick in DenaryTricks)
                    GetTeam(trick.Taker.TeamOrder).Points -= 10;
                break;

            case TolbaEnum.TRS:
                for (int i = 0; i < 4; i++)
                {
                    GetTeam(GetPlayer(TRSTolbaPlayerIdsOrder[i]).TeamOrder).Points += 200 - (50 * i);
                }
                break;
        }
    }

    void EndOfGame()
    {
        // show results menu
    }
    #endregion

    #region Listeners
    public void RecieveState(TrixState state)
    {
        Debug.Log("received\n" + state.ToString());
        ProccessingAndLoadingData?.Invoke();

        // compare players data [username, avatar, frame, order, team order, id, bot play, joined game, ready to play]
        for (int i = 0; i < state.Teams.Length; i++)
        {
            TrixTeam receivedTeam = state.Teams[i];
            TrixTeam currentTeam = Array.Find(Teams, o => !o.IsNull() && o.Order == receivedTeam.Order);
            if (currentTeam.IsNull())
            {
                TrixPlayer p1 = new TrixPlayer(receivedTeam.Player_1.PlayerId, receivedTeam.Player_1.TeamOrder, receivedTeam.Player_1.UserName, 
                    receivedTeam.Player_1.Avatar, receivedTeam.Player_1.Frame, receivedTeam.Player_1.Points,
                    receivedTeam.Player_1.Order, receivedTeam.Player_1.BotPlay, receivedTeam.Player_1.JoinedGame, receivedTeam.Player_1.ReadyToPlay);

                TrixPlayer p2 = !IsTeams ? null : new TrixPlayer(receivedTeam.Player_2.PlayerId, receivedTeam.Player_2.TeamOrder, receivedTeam.Player_2.UserName, 
                    receivedTeam.Player_2.Avatar, receivedTeam.Player_2.Frame, receivedTeam.Player_2.Points, receivedTeam.Player_2.Order,
                    receivedTeam.Player_2.BotPlay, receivedTeam.Player_2.JoinedGame, receivedTeam.Player_2.ReadyToPlay);
                
                Teams[i] = new TrixTeam(receivedTeam.Order, p1, p2);
            }
            else
            {
                for (int j = 0; j < state.Teams[i].Players.Length; j++)
                {
                    TrixPlayer receivedPlayer = state.Teams[i].Players[j];
                    TrixPlayer currentPlayer = Array.Find(Teams[i].Players, o => !o.IsNull() && o.PlayerId == receivedPlayer.PlayerId);
                    if (currentPlayer.IsNull())
                    {
                        Teams[i].Players[j] = new TrixPlayer(receivedPlayer.PlayerId, receivedPlayer.TeamOrder, receivedPlayer.UserName, 
                            receivedPlayer.Avatar, receivedPlayer.Frame, receivedPlayer.Points,
                            receivedPlayer.Order, receivedPlayer.BotPlay, receivedPlayer.JoinedGame, receivedPlayer.ReadyToPlay);
                    }
                    else
                    {
                        currentPlayer.Order = receivedPlayer.Order;
                        currentPlayer.Points = receivedPlayer.Points;
                        currentPlayer.JoinedGame = receivedPlayer.JoinedGame;
                        currentPlayer.ReadyToPlay = receivedPlayer.ReadyToPlay;

                        currentPlayer.PlayerDataChanaged?.Invoke(currentPlayer);
                    }
                }
            }
        }

        // compare tricks data [latches, slot of each latch, card and owner of each slot]
        for (int i = 0; i < Tricks.Length; i++)
        {
            Trick currentTrick = Tricks[i];
            Trick receivedTrick = state.Tricks.First(o => o.No == currentTrick.No);

            if (receivedTrick.IsEmpty())
            {
                currentTrick.ResetTrick();
                continue;
            }

            for (int j = 0; j < currentTrick.Latches.Length; j++)
            {
                Latch currentLatch = currentTrick.Latches[j];
                Latch receivedLatch = receivedTrick.Latches.First(o => o.Rank == currentLatch.Rank);

                if (receivedLatch.IsEmpty())
                {
                    currentLatch.ResetLatch();
                    continue;
                }

                if (!currentLatch.Thrower.IsNull())
                {
                    if (receivedLatch.Thrower.PlayerId != currentLatch.Thrower.PlayerId ||
                        !receivedLatch.Slot.Card.IsTheSameCard(currentLatch.Slot.Card))
                    {
                        currentLatch.ResetLatch();
                        currentTrick.AddLatch(receivedLatch.Slot);
                    }
                }
                else
                {
                    currentTrick.AddLatch(receivedLatch.Slot);
                }
            }

            if (!receivedTrick.Taker.IsNull())
            {
                if (currentTrick.Taker.IsNull() || receivedTrick.Taker.PlayerId != currentTrick.Taker.PlayerId)
                {
                    currentTrick.AddTaker(receivedTrick.TakerId);
                }
            }
        }

        Latch lastDroppedLatch = state.LastDroppedLatch();
        if (!lastDroppedLatch.IsNull() && !lastDroppedLatch.IsEmpty())
            lastDroppedLatch.Thrower.DropCard(lastDroppedLatch.Slot.Card);

        PreviousTrickUpdated?.Invoke(state.PreviousTrick());
        CurrentTrickUpdated?.Invoke(state.CurrentTrick());

        foreach (Card card in Cards)
        {
            Card recievedCard = state.Cards.First(o => o.IsTheSameCard(card));
            card.SetOwner(recievedCard.Owner);
            card.Doubled = recievedCard.Doubled;
        }

        OwnerOfLoyalId = state.OwnerOfLoyalId;
        LoyalOwnerUpdated?.Invoke(OwnerOfLoyal);

        LoyalNo = state.LoyalNo;

        Tolba = state.Tolba;
        TolbaSelected?.Invoke(Tolba);

        //todo: display tolba icon
        TolbaNO = state.TolbaNO;

        Turn = state.Turn;
        TurnUpdated?.Invoke(CurrentTurnPlayer);

        UpdateRemoteStage(state.stage);

        foreach (TrixPlayer player in Players)
        {
            player.RefreshCards();
            player.ReAssignCanBeDroppedCards();
        }

        LoadedData?.Invoke();
    }

    // for button listener when click on desired tobla
    public void OnSelectNextTolba(int i)
    {
        // hide select tolba menu
        if ((i & TolbaNO) != 0) return;

        Tolba = (TolbaEnum)i;
        TolbaSelected?.Invoke(Tolba);

        Turn = OwnerOfLoyalId;

        if (Tolba == TolbaEnum.OldKoba || Tolba == TolbaEnum.Girls)
        {
            UpdateLocalStage(GameStageEnum.DoublingCards);
        }
        else
        {
            UpdateLocalStage(GameStageEnum.DropCards);
        }
        TolbaNO = ((int)Tolba | TolbaNO);
        
        if(Tolba == TolbaEnum.TRS)
        {
            TRSTolbaPlayerIdsOrder = new List<string>();
        }
        
        SendUpdate();
    }

    // for button listener when click a card in general even if while doubling cards
    public void ClickCard(UnityTrixCard card, bool CheckedObyDropRulesBefore = false)
    {
        if (stage == GameStageEnum.DoublingCards)
        {
            card.TrixCard.Doubled = !card.TrixCard.Doubled;
        }
        else if (CheckedObyDropRulesBefore || card.TrixCard.CanBeDropped())
        {
            Trick current = CurrentTrick();

            int latchOrder = current.GetNextLatchOrder();

            current.AddLatch(new Slot(CurrentTurnPlayer.PlayerId, card.TrixCard));
            CurrentTrickUpdated?.Invoke(current);

            CurrentTurnPlayer.DropCard(card.TrixCard);

            if (Tolba == TolbaEnum.TRS)
            {
                if (CurrentTurnPlayer.Cards.Count == 0)
                {
                    TRSTolbaPlayerIdsOrder.Add(CurrentTurnPlayer.PlayerId);
                }

                if (TRSTolbaPlayerIdsOrder.Count == 4)
                {
                    UpdateLocalStage(GameStageEnum.EndOfTolba);
                }
                else
                {
                    if (latchOrder == 4)
                    {
                        current.AddTaker(current.FirstLatch.Thrower.PlayerId);

                        PreviousTrickUpdated?.Invoke(current);
                    }

                    int order = CurrentTurnPlayer.Order + 1;
                    order %= Players.Length;
                    Turn = GetPlayer(order).PlayerId;
                }
            }
            else
            {
                if (latchOrder == 4)
                {
                    Turn = "";
                    UpdateLocalStage(GameStageEnum.TakeTrick);
                }
                else
                {
                    int order = CurrentTurnPlayer.Order + 1;
                    order %= Players.Length;
                    Turn = GetPlayer(order).PlayerId;
                }
            }

            SendUpdate();
            //disable click any card
        }
    }

    // for ready button to be clicked when double cards
    public void ClickReadyToPlay()
    {
        LocalPlayer.ReadyToPlay = true;

        foreach (var player in Players)
            player.Bot_DoubleCard();

        //hide ready to play button
        SendUpdate();
    }
    #endregion

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    #region Testing
#if UNITY_EDITOR
    public void ClickCard(Card card, bool CheckedObyDropRulesBefore = false)
    {
        if (stage == GameStageEnum.DoublingCards)
        {
            card.Doubled = !card.Doubled;
        }
        else if (CheckedObyDropRulesBefore || card.CanBeDropped())
        {
            Trick current = CurrentTrick();

            int latchOrder = current.GetNextLatchOrder();

            current.AddLatch(new Slot(CurrentTurnPlayer.PlayerId, card));

            CurrentTurnPlayer.DropCard(card);
            CurrentTrickUpdated?.Invoke(current);

            if (Tolba == TolbaEnum.TRS)
            {
                if (CurrentTurnPlayer.Cards.Count == 0)
                {
                    TRSTolbaPlayerIdsOrder.Add(CurrentTurnPlayer.PlayerId);
                }

                if (TRSTolbaPlayerIdsOrder.Count == 4)
                {
                    UpdateLocalStage(GameStageEnum.EndOfTolba);
                }
                else
                {
                    if (latchOrder == 4)
                    {
                        current.AddTaker(current.FirstLatch.Thrower.PlayerId);

                        PreviousTrickUpdated?.Invoke(current);
                    }

                    int order = CurrentTurnPlayer.Order + 1;
                    order %= Players.Length;
                    Turn = GetPlayer(order).PlayerId;
                }
            }
            else
            {
                if (latchOrder == 4)
                {
                    Turn = "";
                    UpdateLocalStage(GameStageEnum.TakeTrick);
                }
                else
                {
                    int order = CurrentTurnPlayer.Order + 1;
                    order %= Players.Length;
                    Turn = GetPlayer(order).PlayerId;
                }
            }

            SendUpdate();
            //disable click any card
        }
    }
    public void ClickCard(TrixPlayer player, Card card, bool CheckedObyDropRulesBefore = false)
    {
        if (stage == GameStageEnum.DoublingCards)
        {
            card.Doubled = !card.Doubled;
        }
        else if (CheckedObyDropRulesBefore || card.CanBeDropped())
        {
            Trick current = CurrentTrick();

            int latchOrder = current.GetNextLatchOrder();

            current.AddLatch(new Slot(player.PlayerId, card));
        
            
            player.DropCard(card);
          
            CurrentTrickUpdated?.Invoke(current);

            if (Tolba == TolbaEnum.TRS)
            {
                if (CurrentTurnPlayer.Cards.Count == 0)
                {
                    TRSTolbaPlayerIdsOrder.Add(player.PlayerId);
                }

                if (TRSTolbaPlayerIdsOrder.Count == 4)
                {
                    UpdateLocalStage(GameStageEnum.EndOfTolba);
                }
                else
                {
                    if (latchOrder == 4)
                    {
                        current.AddTaker(current.FirstLatch.Thrower.PlayerId);

                        PreviousTrickUpdated?.Invoke(current);
                    }

                    int order = player.Order + 1;
                    order %= Players.Length;
                    Turn = GetPlayer(order).PlayerId;
                }
            }
            else 
            {
                if (latchOrder == 4)
                {
                    Turn = "";
                    UpdateLocalStage(GameStageEnum.TakeTrick);
                }
                else
                {
                    int order = player.Order + 1;
                    order %= Players.Length;
                    Turn = GetPlayer(order).PlayerId;
                }
            }

            SendUpdate();
            //disable click any card
        }
    }

    public void ClickReadyToPlay(TrixPlayer player)
    {
        // disable click any card
        player.ReadyToPlay = true;
        //hide ready to play button
        SendUpdate();
    }
#endif
    #endregion

    public void Dispose()
    {
        ScreenComponentsController.Instance?.UnsubscribeAll();
    }
}
