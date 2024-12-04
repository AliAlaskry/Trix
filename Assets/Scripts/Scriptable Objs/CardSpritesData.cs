using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Card Sprites", menuName = "Trix/Cards")]
public class CardSpritesData : ScriptableObject
{
    #region Fields
    [Header("Put them in order 2, 3, 4, 5, 6, 7, 8, 9, 10, J, G, K, A")]
    [Space(10)]
    [SerializeField] Sprite FlippedCard;
    [SerializeField] List<Sprite> KobaSprites;
    [SerializeField] List<Sprite> DenarySprites;
    [SerializeField] List<Sprite> BastonySprites;
    [SerializeField] List<Sprite> SanakSprites;
    #endregion

    #region Fns
    public Sprite GetCardSprite(Card card)
    {
        switch (card.Suit)
        {
            case CardSuit.Koba:
                return KobaSprites[((int)card.Rank) - 2];

            case CardSuit.Denary:
                return DenarySprites[((int)card.Rank) - 2];

            case CardSuit.Bastony:
                return BastonySprites[((int)card.Rank) - 2];

            default:
                return SanakSprites[((int)card.Rank) - 2];
        }
    }

    public Sprite GetFlippedCard()
    {
        return FlippedCard;
    }
    #endregion
}
