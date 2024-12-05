using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnityTrixCard : MonoBehaviour
{
    #region Initialize
    public void Initialize(Card card, bool interactable)
    {
        this.card = card;
        CardImage.sprite = ScreenComponentsController.Instance.CardSpritesData.GetCardSprite(card);
        Button.enabled = interactable;
    }

    public void Initialize_Flipped()
    {
        CardImage.sprite = ScreenComponentsController.Instance.CardSpritesData.GetFlippedCard();
        Button.enabled = false;
    }
    #endregion

    #region Fields
    [SerializeField] private Card card;
    [SerializeField] private Image CardImage;
    [SerializeField] private Button button;
    #endregion

    #region Props
    [HideInInspector] public Card TrixCard => TrixController.GameState.GetCard(card.Suit, card.Rank);
    public Button Button
    {
        get
        {
            if (button.IsNull())
                button = GetComponent<Button>();

            return button;
        }
    }
    #endregion

    #region Calls
    public void SetButtonListener(UnityAction callback)
    {
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(callback);
    }
    #endregion
}
