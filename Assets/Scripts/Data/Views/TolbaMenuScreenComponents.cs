using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TolbaMenuScreenComponents : ScreenComponents
{
    #region Fields
    [SerializeField] TolbaDisplayItem[] Tolbas;
    #endregion

    #region Initialize
    public void Initalize(int tolbaNo)
    {
        foreach (var item in Tolbas)
        {
            bool interactable = (((int)item.Tolba) & tolbaNo) == 0;
            item.Button.enabled = interactable;
        }
    }
    #endregion

    #region Inherited Fns
    public override void Subscribe()
    {
        base.Subscribe();

        for (int i = 0, count = Tolbas.Length; i < count; i++)
        {
            Tolbas[i].Initialize(GameState.OnSelectNextTolba);
        }
    }
    #endregion
}

[Serializable]
public class TolbaDisplayItem
{
    #region Constructor
    public void Initialize(UnityAction<int> OnClick)
    {
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(() => { OnClick(((int)Tolba)); });
    }
    #endregion

    #region Fields
    public TolbaEnum Tolba;
    public Button Button;
    #endregion
}
