using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class TableSlotsScreenComponents : ScreenComponents
{
    #region Fields
    [Tooltip("[0] down player slot\n[1] right player slot\n[2] up player slot\n[3] left player slot")]
    [SerializeField] TableSlotDisplayItem[] TableSlots;
    #endregion

    #region Props
    public TableSlotDisplayItem this[int index]
    {
        get
        {
            return TableSlots[index];
        }
    }

    MyCardsScreenComponents MyCards => ScreenController.PlayersOnScreen[0].MyCards;
    #endregion

    #region Unity Fns
    private void Start()
    {
        ResetInitialState();
    }
    #endregion

    #region Listeners
    void OnCurrentTrickUpdated(Trick trick)
    {
        if (trick.IsNull())
        {
            for (int i = 0, count = TableSlots.Length; i < count; i++)
            {
                TableSlots[i].Hide();
            }
            return;
        }

        for(int i = 0, count = TableSlots.Length; i < count; i++)
        {
            Latch latch = trick.GetLatchOfOwner(GameState.GetPlayer(i));
            if (latch.IsNull() || latch.IsEmpty())
            {
                TableSlots[i].Hide();
            }
            else if (!latch.Slot.Card.IsTheSameCard(TableSlots[i].Card))
            {
                if (latch.Thrower.IsLocal && !MyCards.TryDropCard(latch.Slot.Card))
                    TableSlots[i].Display(latch.Slot.Card, ScreenController.PlayersOnScreen[latch.Thrower.Order].CenterOfIcon);
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

        foreach (var slot in TableSlots)
        {
            slot.Hide();
        }
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
public class TableSlotDisplayItem
{
    #region Fields
    public Card Card;
    [SerializeField] RectTransform Container;
    #endregion

    #region Calls
    public void Display(CardDisplayItem card)
    {
        if (Card.IsTheSameCard(card.Card)) return;

        Hide();

        Card = card.Card;

        card.Obj.GetComponentInChildren<UnityTrixCard>().Button.enabled = false;
        CardContainerDisplay cardContainer = card.Obj.GetComponent<CardContainerDisplay>();

        Latch lastDroppedLatch = TrixController.GameState.LastDroppedLatch();
        if (!lastDroppedLatch.IsNull() && !lastDroppedLatch.IsEmpty() && card.Card.IsTheSameCard(lastDroppedLatch.Slot.Card))
        {
            // animate card from its current position to center container
            cardContainer.SetContainerParent(Container, 0);

            cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;
            
            cardContainer.ResetContainerPosition();

            AnimationController.Instance.Animate(cardContainer.cardTrnasform, AnimationController.LinearAnimation(ScreenComponentsController.Instance.DragCardSpeed),
                Vector3.zero, Vector3.zero, cardContainer.cardTrnasform.sizeDelta, null);
        }
        else
        {
            cardContainer.SetContainerParent(Container, 0);
            cardContainer.ResetAll();
        }
    }

    public void Display(Card card, RectTransform startParent)
    {
        if (Card.IsTheSameCard(card)) return;

        Hide();

        Card = card;

        //      create the card in the center of icon
        //      move the card towards own slot in the middle of board
        //      increase size while moving 
        //      set parent of card player board slot when reach it

        GameObject UnityTrixCardObj = UnityEngine.Object.Instantiate(ScreenComponentsController.Instance.CardPrefab, startParent);
      
        UnityTrixCardObj.GetComponentInChildren<UnityTrixCard>().Initialize(Card, false);
   
        CardContainerDisplay cardContainer = UnityTrixCardObj.GetComponent<CardContainerDisplay>();

        Latch lastDroppedLatch = TrixController.GameState.LastDroppedLatch();
        if (!lastDroppedLatch.IsNull() && !lastDroppedLatch.IsEmpty() && card.IsTheSameCard(lastDroppedLatch.Slot.Card))
        {
            cardContainer.SetContainerParent(Container, 0);

            cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;
           
            cardContainer.ResetContainerPosition();

            cardContainer.SetCardRectSize(Vector2.zero);

            AnimationController.Instance.Animate(cardContainer.cardTrnasform, AnimationController.LinearAnimation(ScreenComponentsController.Instance.DragCardSpeed),
                Vector3.zero, Vector3.zero, cardContainer.containerTransform.sizeDelta, null);
        }
        else
        {
            cardContainer.SetContainerParent(Container, 0);
            cardContainer.ResetAll();
        }
    }

    public void Hide()
    {
        if (Container.childCount == 0) return;

        if (TrixController.GameState.IsNull())
        {
            foreach (Transform trans in Container)
            {
                UnityEngine.Object.Destroy(trans.gameObject);
            }
            return;
        }

        Trick previous = TrixController.GameState.PreviousTrick();

        if (previous.IsNull())
        {
            foreach (Transform trans in Container)
            {
                UnityEngine.Object.Destroy(trans.gameObject);
            }
            return;
        }

        // move to taker icon
        int order = previous.Taker.Order;
        RectTransform parent = ScreenComponentsController.Instance.PlayersOnScreen[order].CenterOfIcon;

        foreach(Transform trans in Container)
        {
            CardContainerDisplay cardContainer = trans.GetComponent<CardContainerDisplay>();

            cardContainer.SetContainerParent(parent, 0);
           
            cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;
           
            cardContainer.ResetContainerPosition();

            GameObject obj = trans.gameObject;
            AnimationController.Instance.Animate(cardContainer.cardTrnasform, AnimationController.LinearAnimation(0.8f), Vector3.zero, Vector3.zero, Vector3.zero, () =>
            {
                UnityEngine.Object.Destroy(obj);
            });
        }
    }

    public RectTransform GetContainer()
    {
        return Container;
    }
    #endregion
}