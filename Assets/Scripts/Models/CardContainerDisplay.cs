using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CardContainerDisplay : MonoBehaviour
{
    #region Fields
    public RectTransform containerTransform;
    public RectTransform cardTrnasform;
    #endregion

    #region Fns

    #region Container
    public void SetContainerParent(RectTransform parent, int siblingIndex)
    {
        transform.SetParent(parent);
        transform.SetSiblingIndex(siblingIndex);

        containerTransform.sizeDelta = parent.sizeDelta;

        containerTransform.localScale = Vector3.one;
        cardTrnasform.localEulerAngles = Vector3.zero;
    }

    public void SetContainerPosition(Vector2 position)
    {
        containerTransform.anchoredPosition = position;
    }

    public void ResetContainerPosition()
    {
        containerTransform.anchoredPosition = Vector2.zero;
        containerTransform.anchorMin = Vector2.one / 2;
        containerTransform.anchorMax = Vector2.one / 2;
        containerTransform.pivot = Vector2.one / 2;
    }
    #endregion

    #region Card
    public void SetCardPosition(Vector3 position)
    {
        cardTrnasform.anchoredPosition3D = position;
    }

    public void SetCardRectSize(Vector2 size)
    {
        cardTrnasform.sizeDelta = size;
    }
    
    public void FitCardSizeToParent()
    {
        cardTrnasform.sizeDelta = containerTransform.sizeDelta;
    }

    public void ResetCardTransfrom()
    {        
        ResetCardPositionToZero();
        ResetCardRotationToZero();
    }

    public void ResetCardPositionToZero()
    {
        cardTrnasform.anchoredPosition = Vector2.zero;
        cardTrnasform.anchorMin = Vector2.one / 2;
        cardTrnasform.anchorMax = Vector2.one / 2;
        cardTrnasform.pivot = Vector2.one / 2;
    }

    public void ResetCardRotationToZero()
    {
        cardTrnasform.localEulerAngles = Vector3.zero;
    }
    
    #endregion

    public void ResetAll()
    {
        ResetContainerPosition();
        ResetCardTransfrom();
    }

    #endregion
}
