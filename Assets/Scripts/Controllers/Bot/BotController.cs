using LessonEra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public static class BotController
{
    #region Constants
    const string WaitSeconds_BotTakingActionTimerKey = "WSBTATK";
    #endregion

    #region Props
    static Counters Counters => Counters.Instance;
    #endregion

    public static void Bot_SelectTolba(this TrixPlayer player)
    {
        if (TrixController.GameState.OwnerOfLoyalId != player.PlayerId) return;

        Counters.AddCountDown(WaitSeconds_BotTakingActionTimerKey, UnityEngine.Random.Range(1, 2), () =>
        {

            List<int> options = new List<int>();
            foreach (TolbaEnum option in Enum.GetValues(typeof(TolbaEnum)))
            {
                int optionValue = (int)option;
                if ((((int)TrixController.GameState.Tolba) & optionValue) == 0)
                {
                    options.Add(optionValue);
                }
            }

            int selected = options[UnityEngine.Random.Range(0, options.Count)];
            TrixController.GameState.OnSelectNextTolba(selected);

            Debug.Log($"{player.PlayerId} selected tolba as bot to be {selected}");
        });
        Counters.StartCountDown(WaitSeconds_BotTakingActionTimerKey);
    }

    public static void Bot_SelectTolba(this TrixPlayer player, TrixState gameState)
    {
        if (gameState.OwnerOfLoyalId != player.PlayerId) return;

        Counters.AddCountDown(WaitSeconds_BotTakingActionTimerKey, UnityEngine.Random.Range(1, 2), () =>
        {
            List<int> options = new List<int>();
            foreach (var option in Enum.GetValues(typeof(TolbaEnum)))
            {
                int optionValue = (int)option;
                if ((((int)gameState.Tolba) & optionValue) == 0)
                {
                    options.Add(optionValue);
                }
            }

            int selected = options[UnityEngine.Random.Range(0, options.Count)];
            gameState.OnSelectNextTolba(selected);

            Debug.Log($"{player.PlayerId} selected tolba as bot to be {selected}");
        });
        Counters.StartCountDown(WaitSeconds_BotTakingActionTimerKey);
    }

    public static void Bot_DoubleCard(this TrixPlayer player)
    {
        Counters.AddCountDown(WaitSeconds_BotTakingActionTimerKey, UnityEngine.Random.Range(1, 2), () =>
        {
            if (TrixController.GameState.Tolba == TolbaEnum.OldKoba)
            {
                Card oldKoba = TrixController.GameState.GetCard(CardSuit.Koba, CardRank.Old);
                if (player.HasCard(oldKoba) && player.HasCard(TrixController.GameState.GetCard(CardSuit.Koba, CardRank.Ace)))
                {
                    oldKoba.Doubled = true;
                }
            }
            else if (TrixController.GameState.Tolba == TolbaEnum.Girls)
            {
                if (player.HasGirls(out List<Card> ownedGrils))
                {
                    foreach (var girl in ownedGrils)
                    {
                        if (player.HasCard(TrixController.GameState.GetCard(girl.Suit, CardRank.Old)) &&
                            player.HasCard(TrixController.GameState.GetCard(girl.Suit, CardRank.Ace)))
                        {
                            girl.Doubled = true;
                        }
                    }
                }
            }
        });
        Counters.StartCountDown(WaitSeconds_BotTakingActionTimerKey);
    }

    public static void Bot_DoubleCard(this TrixPlayer player, TrixState gameState)
    {
        Counters.AddCountDown(WaitSeconds_BotTakingActionTimerKey, UnityEngine.Random.Range(1, 2), () =>
        {
            if (gameState.Tolba == TolbaEnum.OldKoba)
            {
                Card oldKoba = gameState.GetCard(CardSuit.Koba, CardRank.Old);
                if (player.HasCard(oldKoba) && player.HasCard(gameState.GetCard(CardSuit.Koba, CardRank.Ace)))
                {
                    oldKoba.Doubled = true;
                }
            }
            else if (gameState.Tolba == TolbaEnum.Girls)
            {
                if (player.HasGirls(out List<Card> ownedGrils))
                {
                    foreach (var girl in ownedGrils)
                    {
                        if (player.HasCard(gameState.GetCard(girl.Suit, CardRank.Old)) &&
                            player.HasCard(gameState.GetCard(girl.Suit, CardRank.Ace)))
                        {
                            girl.Doubled = true;
                        }
                    }
                }
            }
        });
        Counters.StartCountDown(WaitSeconds_BotTakingActionTimerKey);
    }

    public static void Bot_DropCard(this TrixPlayer player, TrixState gameState)
    {
        List<Card> CanBeDroppedCards = player.CanBeDroppedCards;

        if (CanBeDroppedCards.Count == 0)
        {
            Debug.LogWarning("no can dropped cards for " + player.PlayerId);
            if (gameState.Tolba == TolbaEnum.TRS)
            {
                int order = gameState.CurrentTurnPlayer.Order + 1;
                order %= gameState.Players.Length;
                gameState.Turn = gameState.GetPlayer(order).PlayerId;

                gameState.SendUpdate();
            }
            return;
        }

        Counters.AddCountDown(WaitSeconds_BotTakingActionTimerKey, UnityEngine.Random.Range(1, 2), () =>
        {
            CanBeDroppedCards = CanBeDroppedCards.OrderBy(o => o.Suit).ThenBy(o => o.Rank).ToList();
            Card card = null;
            if (gameState.Tolba == TolbaEnum.Girls)
            {
                List<Card> CanBeDroppedGirls = CanBeDroppedCards.FindAll(o => o.Rank == CardRank.Girl);
                if (CanBeDroppedGirls.Count > 0)
                {
                    foreach (var girl in CanBeDroppedGirls)
                    {
                        Card OldOfGirl = gameState.GetCard(girl.Suit, CardRank.Old);
                        Card AceOfGirl = gameState.GetCard(girl.Suit, CardRank.Ace);

                        bool OwnOldAndAceOfGirlSuit = CanBeDroppedCards.Contains(OldOfGirl) && CanBeDroppedCards.Contains(AceOfGirl);
                        bool OthersCantDropAceOrOldOfGirlSuit = true;

                        foreach (var temp in gameState.Players)
                        {
                            if ((temp.HasCard(OldOfGirl) && OldOfGirl.CanBeDropped())
                                || (temp.HasCard(AceOfGirl) && AceOfGirl.CanBeDropped()))
                            {
                                OthersCantDropAceOrOldOfGirlSuit = false;
                            }
                        }

                        if (OwnOldAndAceOfGirlSuit || OthersCantDropAceOrOldOfGirlSuit)
                        {
                            card = girl;
                            break;
                        }
                    }
                }
            }
            else if (gameState.Tolba == TolbaEnum.OldKoba)
            {
                Card OldOfKoba = gameState.GetCard(CardSuit.Koba, CardRank.Old);
                if (CanBeDroppedCards.Contains(OldOfKoba))
                {
                    Card AceOfKoba = gameState.GetCard(CardSuit.Koba, CardRank.Ace);

                    bool OwnAceOfKoba = player.HasCard(AceOfKoba);
                    bool OthersCantDropAceOfKoba = true;

                    foreach (var temp in gameState.Players)
                    {
                        if (temp.HasCard(AceOfKoba) && AceOfKoba.CanBeDropped())
                        {
                            OthersCantDropAceOfKoba = false;
                        }
                    }

                    if (OwnAceOfKoba || OthersCantDropAceOfKoba)
                    {
                        card = OldOfKoba;
                    }
                }
            }

            if (card.IsNull())
            {
                card = CanBeDroppedCards[0];
            }

            Debug.Log($"{player.PlayerId} dropped card as bot to be {card.ToString()}");

            gameState.ClickCard(card, true);
        });
        Counters.StartCountDown(WaitSeconds_BotTakingActionTimerKey);
    }

    public static void Bot_DropCard(this TrixPlayer player)
    {
        List<Card> CanBeDroppedCards = player.CanBeDroppedCards;

        if (CanBeDroppedCards.Count == 0)
        {
            Debug.LogWarning("no can dropped cards for " + player.PlayerId);
            if (TrixController.GameState.Tolba == TolbaEnum.TRS)
            {
                int order = TrixController.GameState.CurrentTurnPlayer.Order + 1;
                order %= TrixController.GameState.Players.Length;
                TrixController.GameState.Turn = TrixController.GameState.GetPlayer(order).PlayerId;

                TrixController.GameState.SendUpdate();
            }
            return;
        }

        Counters.AddCountDown(WaitSeconds_BotTakingActionTimerKey, UnityEngine.Random.Range(1, 2), () =>
        {
            CanBeDroppedCards = CanBeDroppedCards.OrderBy(o => o.Suit).ThenBy(o => o.Rank).ToList();
            Card card = null;
            if (TrixController.GameState.Tolba == TolbaEnum.Girls)
            {
                List<Card> CanBeDroppedGirls = CanBeDroppedCards.FindAll(o => o.Rank == CardRank.Girl);
                if (CanBeDroppedGirls.Count > 0)
                {
                    foreach (var girl in CanBeDroppedGirls)
                    {
                        Card OldOfGirl = TrixController.GameState.GetCard(girl.Suit, CardRank.Old);
                        Card AceOfGirl = TrixController.GameState.GetCard(girl.Suit, CardRank.Ace);

                        bool OwnOldAndAceOfGirlSuit = CanBeDroppedCards.Contains(OldOfGirl) && CanBeDroppedCards.Contains(AceOfGirl);
                        bool OthersCantDropAceOrOldOfGirlSuit = true;

                        foreach (var temp in TrixController.GameState.Players)
                        {
                            if ((temp.HasCard(OldOfGirl) && OldOfGirl.CanBeDropped())
                                || (temp.HasCard(AceOfGirl) && AceOfGirl.CanBeDropped()))
                            {
                                OthersCantDropAceOrOldOfGirlSuit = false;
                            }
                        }

                        if (OwnOldAndAceOfGirlSuit || OthersCantDropAceOrOldOfGirlSuit)
                        {
                            card = girl;
                            break;
                        }
                    }
                }
            }
            else if (TrixController.GameState.Tolba == TolbaEnum.OldKoba)
            {
                Card OldOfKoba = TrixController.GameState.GetCard(CardSuit.Koba, CardRank.Old);
                if (CanBeDroppedCards.Contains(OldOfKoba))
                {
                    Card AceOfKoba = TrixController.GameState.GetCard(CardSuit.Koba, CardRank.Ace);

                    bool OwnAceOfKoba = player.HasCard(AceOfKoba);
                    bool OthersCantDropAceOfKoba = true;

                    foreach (var temp in TrixController.GameState.Players)
                    {
                        if (temp.HasCard(AceOfKoba) && AceOfKoba.CanBeDropped())
                        {
                            OthersCantDropAceOfKoba = false;
                        }
                    }

                    if (OwnAceOfKoba || OthersCantDropAceOfKoba)
                    {
                        card = OldOfKoba;
                    }
                }
            }

            if (card.IsNull())
            {
                card = CanBeDroppedCards[0];
            }

            Debug.Log($"{player.PlayerId} dropped card as bot to be {card}");

            TrixController.GameState.ClickCard(card, true);
        });
        Counters.StartCountDown(WaitSeconds_BotTakingActionTimerKey);
    }
}
