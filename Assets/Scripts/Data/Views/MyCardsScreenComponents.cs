using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MyCardsScreenComponents : ScreenComponents
{
    #region Fields
    [SerializeField] PlayerScreenComponents PlayerDisplay;

    [SerializeField] AnimationCurve CardsArrowCurve;
    [SerializeField] AnimationCurve CardsPositionCurve;

    [SerializeField] RectTransform CardsContainer;
    #endregion

    #region Props
    TrixPlayer Player => TrixController.GameState.LocalPlayer;
    #endregion

    #region Unity Fns
    private void Start()
    {
        ResetInitialState();
    }
    #endregion

    #region Fns
    void ArrangeCards()
    {
        List<CardContainerDisplay> activeCards = new List<CardContainerDisplay>();
        foreach (Transform cardSubContainer in CardsContainer)
        {
            activeCards.Add(cardSubContainer.GetComponent<CardContainerDisplay>());
        }

        // mid = (count - 1) / 2
        // dis = abs : mid - i
        // time = i < mid ? 7 - dis : 7 + dis

        float mid = activeCards.Count / 2f;
        for (int i = 0, count = activeCards.Count; i < count; i++)
        {
            float distance = (i + 1) - mid;
            float time = 6.5f + distance;

            Vector3 targetPos = Vector3.up * CardsPositionCurve.Evaluate(time);
            Vector3 targetRot = Vector3.forward * CardsArrowCurve.Evaluate(time);
          
            AnimationController.Instance.Animate(activeCards[i].cardTrnasform, AnimationController.LinearAnimation(2.5f), targetPos, targetRot, new Vector3(150, 225), null);
        }
    }

    [ContextMenu("Rearrange Cards")]
    void ArrangeCards_Immediatly()
    {
        List<CardContainerDisplay> activeCards = new List<CardContainerDisplay>();
        foreach (Transform cardSubContainer in CardsContainer)
        {
            activeCards.Add(cardSubContainer.GetComponent<CardContainerDisplay>());
        }

        // mid = (count - 1) / 2
        // dis = abs : mid - i
        // time = i < mid ? 7 - dis : 7 + dis

        float mid = activeCards.Count / 2f;
        for (int i = 0, count = activeCards.Count; i < count; i++)
        {
            float distance = (i + 1) - mid;
            float time = 6.5f + distance;

            activeCards[i].cardTrnasform.anchoredPosition3D = Vector3.up * CardsPositionCurve.Evaluate(time);
            activeCards[i].cardTrnasform.localEulerAngles = Vector3.forward * CardsArrowCurve.Evaluate(time);
            activeCards[i].cardTrnasform.sizeDelta = new Vector3(150, 225);
        }
    }
    #endregion

    #region Listeners
    public CardDisplayItem DisplayCard(Card card)
    {
        CardDisplayItem item = PlayerDisplay.GetCardDisplayItem(card);
        if (!item.IsNull()) return item;

        GameObject cardObj = Instantiate(ScreenController.CardPrefab, ScreenController.TrixTolbaTableSlots.transform);
        
        UnityTrixCard unityTrixCard = cardObj.GetComponentInChildren<UnityTrixCard>();
        unityTrixCard.Initialize(card, false);
        unityTrixCard.SetButtonListener(() => { TrixController.GameState.ClickCard(unityTrixCard); });

        CardContainerDisplay cardContainer = cardObj.GetComponent<CardContainerDisplay>();

        int siblingIndex;
        for (siblingIndex = 0; siblingIndex < CardsContainer.childCount; siblingIndex++)
        {
            int compareIndicator = card.CompareTo(CardsContainer.GetChild(siblingIndex).GetComponentInChildren<UnityTrixCard>().TrixCard);
            if(compareIndicator > 0)
            {
                break;
            }
        }

        cardContainer.SetContainerParent(CardsContainer, siblingIndex);
        cardContainer.SetCardRectSize(new Vector2(150, 225));
        cardContainer.ResetAll();

        ArrangeCards_Immediatly();
        
        item = new(card, cardObj);
        PlayerDisplay.DisplayedCards.Add(item);

        return item;
    }

    public void DisplayCards(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            CardDisplayItem item = PlayerDisplay.GetCardDisplayItem(card);
            if (!item.IsNull()) return;

            GameObject cardObj = Instantiate(ScreenController.CardPrefab, ScreenController.TrixTolbaTableSlots.transform);

            UnityTrixCard unityTrixCard = cardObj.GetComponentInChildren<UnityTrixCard>();
            unityTrixCard.Initialize(card, false);
            unityTrixCard.SetButtonListener(() => { TrixController.GameState.ClickCard(unityTrixCard); });

            CardContainerDisplay cardContainer = cardObj.GetComponent<CardContainerDisplay>();

            int siblingIndex;
            for (siblingIndex = 0; siblingIndex < CardsContainer.childCount; siblingIndex++)
            {
                int compareIndicator = card.CompareTo(CardsContainer.GetChild(siblingIndex).GetComponentInChildren<UnityTrixCard>().TrixCard);
                if (compareIndicator > 0)
                {
                    break;
                }
            }

            cardContainer.SetContainerParent(CardsContainer, siblingIndex);
            cardContainer.cardTrnasform.anchoredPosition3D += cardContainer.containerTransform.anchoredPosition3D;

            cardContainer.ResetContainerPosition();
            cardContainer.SetCardRectSize(new Vector2(100, 150));

            item = new(card, cardObj);
            PlayerDisplay.DisplayedCards.Add(item);
        }

        if (cards.Count > 0)
            ArrangeCards();
    }

    void EnableInteractCards(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            CardDisplayItem item = PlayerDisplay.GetCardDisplayItem(card);
            if (item.IsNull())
                item = DisplayCard(card);

            item.EnableInteract();
        }
    }

    void DisableInteractCards(List<Card> cards)
    {
        foreach(Card card in cards)
        {
            CardDisplayItem item = PlayerDisplay.GetCardDisplayItem(card);
            if (item.IsNull())
                item = DisplayCard(card);

            item.DisableInteract();
        }
    }

    public bool TryDropCard(Card card)
    {
        CardDisplayItem cardDisplayItem = PlayerDisplay.GetCardDisplayItem(card);
        if (cardDisplayItem.IsNull())
        {
            Debug.LogWarning($"no exist card of {card.ToString()} on my hand");
            return false;
        }

        if (GameState.Tolba != TolbaEnum.TRS)
            ScreenController.TableSlots[0].Display(cardDisplayItem);
        else
            ScreenController.TrixTolbaTableSlots[0].Display(cardDisplayItem);

        PlayerDisplay.DisplayedCards.Remove(cardDisplayItem);

        ArrangeCards_Immediatly();

        return true;
    }

    void HideCardSuddenly(Card card)
    {
        CardDisplayItem cardDisplayItem = PlayerDisplay.GetCardDisplayItem(card);
        Destroy(cardDisplayItem.Obj);
        PlayerDisplay.DisplayedCards.Remove(cardDisplayItem);
    }

    public void HideAll()
    {
        IEnumerable<Card> cards = PlayerDisplay.DisplayedCards.Select((CardDisplayItem item) =>
        {
            return item.Card;
        });
        foreach (Card card in cards)
        {
            HideCardSuddenly(card);
        }

        PlayerDisplay.DisplayedCards = new List<CardDisplayItem>();
    }
    #endregion

    #region Inherited Fns
    public override void ResetInitialState()
    {
        base.ResetInitialState();

        foreach (Transform child in CardsContainer)
            Destroy(child.gameObject);
    }
    public override void Subscribe()
    {
        base.Subscribe();

        if (Player.IsNull())
        {
            Subscribed = false;
            return;
        }

        Player.EnableInteractCards += EnableInteractCards;
        Player.DisableInteractCards += DisableInteractCards;
    
        Player.HideCardSuddenly += HideCardSuddenly;
    }

    public override void UnSubscribe()
    {
        base.UnSubscribe();
   
        Player.EnableInteractCards -= EnableInteractCards;
        Player.DisableInteractCards -= DisableInteractCards;
   
        Player.HideCardSuddenly -= HideCardSuddenly;
    }
    #endregion
}

[Serializable]
public class CardDisplayItem
{
    #region Constructor
    public CardDisplayItem(Card card, GameObject obj)
    {
        this.Card = card;
        this.Obj = obj;

        if (!obj.IsNull())
        {
            button = obj.GetComponentInChildren<Button>();
            CardImage = obj.GetComponentInChildren<Image>();
        }
    }
    #endregion

    #region Fields
    public Card Card;
    public GameObject Obj;
    Button button;
    Image CardImage;
    #endregion

    #region Props
    Color ActiveColor => ScreenComponentsController.Instance.ActiveColor;
    Color DeactiveColor => ScreenComponentsController.Instance.DeactiveColor;
    #endregion

    #region Fns
    public void EnableInteract()
    {
        if (button.IsNull()) return;

        RectTransform card = Obj.GetComponent<CardContainerDisplay>().cardTrnasform;
        ObjToBeAnimatedItem item = AnimationController.Instance.TryGetAnimatedItem(card);
        if (!item.IsNull())
        {
            item.AtFinish += () =>
            {
                button.enabled = true;
                CardImage.color = ActiveColor;
            };
        }
        else
        {
            button.enabled = true;
            CardImage.color = ActiveColor;
        }
    }

    public void DisableInteract()
    {
        if (button.IsNull()) return;

        RectTransform card = Obj.GetComponent<CardContainerDisplay>().cardTrnasform;
        ObjToBeAnimatedItem item = AnimationController.Instance.TryGetAnimatedItem(card);
        if (!item.IsNull())
        {
            item.AtFinish += () =>
            {
                button.enabled = false;
                CardImage.color = DeactiveColor;
            };
        }
        else
        {
            button.enabled = false;
            CardImage.color = DeactiveColor;
        }
    }
    #endregion
}