using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ControlButtonsScreenComponents : ScreenComponents
{
    #region Fields
    [SerializeField] Button Settings;
    [SerializeField] Button Share;
    [SerializeField] Button Chat;
    [SerializeField] Button Leave;
    #endregion

    #region Unity Fns
    private void OnEnable()
    {
        Leave.onClick.RemoveAllListeners();
        Leave.onClick.AddListener(OnClickLeave);
    }

    private void OnDisable()
    {
        Leave.onClick.RemoveAllListeners();
    }
    #endregion

    #region Inherited
    public override void Subscribe()
    {
        base.Subscribe();

        Settings.onClick.RemoveAllListeners();
        Settings.onClick.AddListener(OnClickSettings);

        Share.onClick.RemoveAllListeners();
        Share.onClick.AddListener(OnClickShare);

        Chat.onClick.RemoveAllListeners();
        Chat.onClick.AddListener(OnClickChat);
    }

    public override void UnSubscribe()
    {
        base.UnSubscribe();
   
        Settings.onClick.RemoveAllListeners();
        Share.onClick.RemoveAllListeners();
        Chat.onClick.RemoveAllListeners();
    }
    #endregion

    #region Listeners
    void OnClickSettings()
    {

    }

    void OnClickShare()
    {

    }

    void OnClickChat()
    {

    }

    void OnClickLeave()
    {

    }
    #endregion
}
