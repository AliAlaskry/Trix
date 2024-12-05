using LessonEra;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerScreenComponents : ScreenComponents
{
    #region Fields
    public int Order;

    [SerializeField] Image Avatar;
    [SerializeField] Image Frame;
    [SerializeField] TMP_Text Username;

    public RectTransform CenterOfIcon;

    [SerializeField] SpecialSlotDisplayItem SpecialSlot;
    public MyCardsScreenComponents MyCards;
   
    public List<CardDisplayItem> DisplayedCards;
    #endregion

    #region Props
    TrixPlayer Player => GameState.GetPlayer(Order);
    #endregion

    #region Unity Fns
    private void Start()
    {
        ResetInitialState();
    }
    #endregion

    #region Listeners
    void PlayerDataChanged(TrixPlayer player)
    {
        if (!player.Avatar.IsNull())
            Avatar.sprite = player.Avatar;
        
        if (!player.Frame.IsNull())
            Frame.sprite = player.Frame;

        Username.text = player.UserName;
    }

    void DisplayCards(List<Card> cards)
    {
        if (Player.IsLocal)
        {
            MyCards.DisplayCards(cards);
        }
        else
        {
            float delay = 0;
            foreach (var card in cards)
            {
                DisplayCard(card, delay);
                delay += 0.1f;
            }
        }
    }

    void DisplayCard(Card card, float delay)
    {
        CardDisplayItem item = GetCardDisplayItem(card);
        if (!item.IsNull()) return;

        //      create the card in the middle of board 
        //      move the card from the middle of board towards player icon
        //      decrease size while moving and destroy after reach player icon

        GameObject cardObj = Instantiate(ScreenController.CardPrefab, ScreenController.TableSlots.transform);

        cardObj.GetComponentInChildren<UnityTrixCard>().Initialize_Flipped();

        CardContainerDisplay cardContainer = cardObj.GetComponent<CardContainerDisplay>();

        cardContainer.SetContainerParent(CenterOfIcon, 0);
        cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;

        cardContainer.ResetContainerPosition();
        cardContainer.SetCardRectSize(new Vector2(150, 225));

        AnimationController.Instance.Animate(cardContainer.cardTrnasform, AnimationController.LinearAnimation(1.5f),
             Vector3.zero, Vector3.zero, Vector3.zero, () =>
             {
                 Destroy(cardObj);
             }, delay);

        DisplayedCards.Add(new CardDisplayItem(card, null));
    }

    public CardDisplayItem GetCardDisplayItem(Card card)
    {
        foreach (CardDisplayItem item in DisplayedCards)
        {
            if (item.Card.IsTheSameCard(card))
                return item;
        }

        return null;
    }
    void PlayerDoubledCard(Slot slot)
    {
        SpecialSlot.DisplaySlot(slot);
    }

    void HideDoubledCard(Slot slot)
    {
        SpecialSlot.HideSlot(slot);
    }   

    void PlayerIdReady()
    {

    }

    void BecomeBot()
    {

    }

    void BecomePlayer()
    {

    }
    
    void DropCard(Card card)
    {
        SpecialSlot.TryHideCard(card);  

        if (Player.IsLocal)
        {
            if (!MyCards.TryDropCard(card))
            {
                if (GameState.Tolba != TolbaEnum.TRS)
                    ScreenController.TableSlots[Order].Display(card, CenterOfIcon);
                else
                    ScreenController.TrixTolbaTableSlots[Order].Display(card, CenterOfIcon);
            }
        }
        else
        {
            if (GameState.Tolba != TolbaEnum.TRS)
                ScreenController.TableSlots[Order].Display(card, CenterOfIcon);
            else
                ScreenController.TrixTolbaTableSlots[Order].Display(card, CenterOfIcon);

            CardDisplayItem item = GetCardDisplayItem(card);
            if (!item.IsNull())
                DisplayedCards.Remove(item);
        }
    }
    #endregion

    #region Inherited Fns
    public override void ResetInitialState()
    {
        base.ResetInitialState();

        DisplayedCards = new List<CardDisplayItem>();
        SpecialSlot.HideAllSlots();
    }

    public override void Subscribe()
    {
        base.Subscribe();

        if (Player.IsNull())
        {
            Subscribed = false;
            return;
        }

        Player.PlayerDataChanaged += PlayerDataChanged;

        Player.DisplayCards += DisplayCards;

        Player.DoubledCard += PlayerDoubledCard;
        Player.HideDoubledCard += HideDoubledCard;

        Player.PlayerIsReady += PlayerIdReady;
        
        Player.BecomeBot += BecomeBot;
        Player.BecomePlayer += BecomePlayer;

        Player.DropCardToTrick += DropCard;

    }

    public override void UnSubscribe()
    {
        base.UnSubscribe();

        Player.PlayerDataChanaged -= PlayerDataChanged;

        Player.DisplayCards -= DisplayCards;

        Player.DoubledCard -= PlayerDoubledCard;
        Player.HideDoubledCard -= HideDoubledCard;

        Player.PlayerIsReady -= PlayerIdReady;

        Player.BecomeBot -= BecomeBot;
        Player.BecomePlayer -= BecomePlayer;

        Player.DropCardToTrick -= DropCard;

    }
    #endregion
}

[Serializable]
public class SpecialSlotDisplayItem
{
    #region Fields
    [SerializeField, SerializeReference] List<Slot> SpecialSlotsData;
    [SerializeField] RectTransform SpecialSlotsContianer;
    #endregion 

    #region Fns
    public void DisplaySlot(Slot slot)
    {
        HideAllSlots();
        Display(slot);
    }

    public void DisplaySlots(List<Slot> slots)
    {
        HideAllSlots();
        foreach (Slot slot in slots)
            Display(slot);
    }

    void Display(Slot slot)
    {
        if(SpecialSlotsData.IsNull()) SpecialSlotsData = new List<Slot>();
        if (SpecialSlotsData.Exists(o => o.IsTheSameSlot(slot))) return;
        
        SpecialSlotsData.Add(slot);
        GameObject cardObj = UnityEngine.Object.Instantiate(ScreenComponentsController.Instance.CardPrefab);

        cardObj.GetComponentInChildren<UnityTrixCard>().Initialize(slot.Card, false);
        cardObj.GetComponent<CardContainerDisplay>().SetContainerParent(SpecialSlotsContianer, 0);

        //todo: move the card from player icon to special slot while increasing it's size till end size
    }

    public void TryHideCard(Card card)
    {
        Slot slot = SpecialSlotsData.Find(o => o.Card.IsTheSameCard(card));
        if (!slot.IsNull())
            HideSlot(slot);
    }

    public void HideSlot(Slot slot)
    {
        foreach (Transform trans in SpecialSlotsContianer)
        {
            if(trans.GetChild(0).TryGetComponent(out UnityTrixCard card))
            {
                if (card.TrixCard.IsTheSameCard(slot.Card))
                {
                    UnityEngine.Object.Destroy(trans.gameObject);
                }
            }
        }

        SpecialSlotsData.Remove(SpecialSlotsData.Find(o => o.IsTheSameSlot(slot)));
    }

    public void HideAllSlots()
    {
        foreach (Transform trans in SpecialSlotsContianer)
        {
            UnityEngine.Object.Destroy(trans.gameObject);
        }

        SpecialSlotsData = new List<Slot>();
    }
    #endregion
}
