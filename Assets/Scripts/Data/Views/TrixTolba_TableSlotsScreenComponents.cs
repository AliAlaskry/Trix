using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TrixTolba_TableSlotsScreenComponents : ScreenComponents
{
    #region Fields
    [SerializeField] TrixTolbaTableSlotDisplayItem KobaSlot;
    [SerializeField] TrixTolbaTableSlotDisplayItem BastonySlot;
    [SerializeField] TrixTolbaTableSlotDisplayItem DenarySlot;
    [SerializeField] TrixTolbaTableSlotDisplayItem SanakSlot;
    #endregion

    #region Props
    public TrixTolbaTableSlotDisplayItem this[int index]
    {
        get
        {
            switch ((CardSuit)index)
            {
                case CardSuit.Koba:
                    return KobaSlot;

                case CardSuit.Bastony:
                    return BastonySlot;

                case CardSuit.Denary:
                    return DenarySlot;

                default:
                    return SanakSlot;
            }
        }
    }
    #endregion

    #region Unity Fns
    private void Start()
    {
       ResetInitialState();
    }
    #endregion

    #region Fns
    public void Display(Card card, RectTransform startParent)
    {
        switch (card.Suit)
        {
            case CardSuit.Koba:
                KobaSlot.Display(card, startParent);
                break;

            case CardSuit.Bastony:
                KobaSlot.Display(card, startParent);
                break;

            case CardSuit.Denary:
                KobaSlot.Display(card, startParent);
                break;

            case CardSuit.Sanak:
                KobaSlot.Display(card, startParent);
                break;
        }
    }

    void OnCurrentTrickUpdated(Trick trick)
    {
        Card[] cards = GameState.Cards;

        foreach (var card in cards)
        {
            if (card.Dropped())
            {
                TrixTolbaTableSlotDisplayItem item = this[((int)card.Suit)];
                int index = item.Cards.FindIndex(o => o.IsTheSameCard(card));
                if (index == -1)
                {
                    item.Display(card, item.GetContainer());
                }
            }
            else
            {
                TrixTolbaTableSlotDisplayItem item = this[((int)card.Suit)];
                int index = item.Cards.FindIndex(o => o.IsTheSameCard(card));
                if (index != -1)
                {
                    item.Hide(card);
                }
            }
        }
    }

    void OnPreviousTrickUpdated(Trick trick)
    {

    }
    #endregion

    #region Inherited Fns
    public override void ResetInitialState()
    {
        base.ResetInitialState();

        KobaSlot.HideAll();
        BastonySlot.HideAll();
        DenarySlot.HideAll();
        SanakSlot.HideAll();
    }
    public override void Subscribe()
    {
        base.Subscribe();

        GameState.CurrentTrickUpdated += OnCurrentTrickUpdated;
        GameState.PreviousTrickUpdated += OnPreviousTrickUpdated;
    }

    public override void UnSubscribe()
    {
        base.UnSubscribe();

        GameState.CurrentTrickUpdated += OnCurrentTrickUpdated;
        GameState.PreviousTrickUpdated += OnPreviousTrickUpdated;
    }
    #endregion
}

[Serializable]
public class TrixTolbaTableSlotDisplayItem
{
    #region Fields
    public List<Card> Cards = new List<Card>();
    [SerializeField] RectTransform Container;
    #endregion

    #region Calls
    public void Display(CardDisplayItem card)
    {
        if (Cards.Exists(o => o.IsTheSameCard(card.Card))) return;

        int siblingIndex = GetSiblingIndex(card.Card);

        card.Obj.GetComponentInChildren<UnityTrixCard>().Button.enabled = false;

        CardContainerDisplay cardContainer = card.Obj.GetComponent<CardContainerDisplay>();

        Latch lastDroppedLatch = TrixController.GameState.LastDroppedLatch();
        if (!lastDroppedLatch.IsNull() && !lastDroppedLatch.IsEmpty() && card.Card.IsTheSameCard(lastDroppedLatch.Slot.Card))
        {
            //      move card from card obj position to position of child of container at siblingIndex
            //      set parent of card to container
            
            cardContainer.SetContainerParent(Container, 0);
        
            cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;
         
            cardContainer.ResetContainerPosition();

            AnimationController.Instance.Animate(cardContainer.cardTrnasform, AnimationController.LinearAnimation(ScreenComponentsController.Instance.DragCardSpeed),
                Vector3.zero, Vector3.zero, new Vector2(150, 225), null);
        }
        else
        {
            cardContainer.SetContainerParent(Container, siblingIndex);
            cardContainer.ResetAll();
        }

        Cards.Add(card.Card);
        Cards = Cards.OrderBy(o => o.Suit).ThenBy(o => o.Rank).ToList();
    }

    public void Display(Card card, RectTransform startParent)
    {
        if (Cards.Exists(o => o.IsTheSameCard(card))) return;

        int siblingIndex = GetSiblingIndex(card);

        //      create the card in the center of icon
        //      move the card towards position of card at siblingiIndex
        //      increase size while moving 
        //      set parent of card container when reach it

        GameObject UnityTrixCardObj = UnityEngine.Object.Instantiate(ScreenComponentsController.Instance.CardPrefab, startParent);
      
        UnityTrixCardObj.GetComponentInChildren<UnityTrixCard>().Initialize(card, false);
     
        CardContainerDisplay cardContainer = UnityTrixCardObj.GetComponent<CardContainerDisplay>();

        Latch lastDroppedLatch = TrixController.GameState.LastDroppedLatch();
        if (!lastDroppedLatch.IsNull() && !lastDroppedLatch.IsEmpty() && card.IsTheSameCard(lastDroppedLatch.Slot.Card))
        {
            cardContainer.SetContainerParent(Container, 0);
            
            cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;
         
            cardContainer.ResetContainerPosition();

            AnimationController.Instance.Animate(cardContainer.cardTrnasform, AnimationController.LinearAnimation(ScreenComponentsController.Instance.DragCardSpeed),
                Vector3.zero, Vector3.zero, new Vector2(150, 225), null);
        }
        else
        {
            cardContainer.SetContainerParent(Container, siblingIndex);
            cardContainer.ResetAll();
        }

        Cards.Add(card);
        Cards = Cards.OrderByDescending(o => o.Suit).ThenByDescending(o => o.Rank).ToList();
    }

    int GetSiblingIndex(Card card)
    {
        int index = 12;
        for(int i = 0, count = Cards.Count; i < count; i++)
        {
            if (Cards[i].CompareTo(card) > 0)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void Hide(Card card)
    {
        foreach (Transform trans in Container)
        {
            if (trans.GetComponentInChildren<UnityTrixCard>().TrixCard.IsTheSameCard(card))
            {
                UnityEngine.Object.Destroy(trans.gameObject);
                break;
            }
        }
    }

    public void HideAll()
    {
        foreach (Transform trans in Container) UnityEngine.Object.Destroy(trans.gameObject);
    }

    public RectTransform GetContainer()
    {
        return Container;
    }
    #endregion
}
