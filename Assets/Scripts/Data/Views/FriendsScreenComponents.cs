using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FriendsScreenComponents : ScreenComponents
{
    #region Fields
    [SerializeField] Button AddFriend;
    [SerializeField] Button FriendsMenu;
    #endregion

    #region Unity Fns
    private void OnEnable()
    {
        FriendsMenu.onClick.RemoveAllListeners();
        FriendsMenu.onClick.AddListener(OnClickFriendsMenu);
    }
    private void OnDisable()
    {
        FriendsMenu.onClick.RemoveAllListeners();
    }
    #endregion

    #region Inherited Fns
    public override void Subscribe()
    {
        base.Subscribe();

        AddFriend.onClick.RemoveAllListeners();
        AddFriend.onClick.AddListener(OnClickAddFriend);
    }
    public override void UnSubscribe()
    {
        base.UnSubscribe();

        AddFriend.onClick.RemoveAllListeners();
    }
    #endregion

    #region Listeners
    void OnClickAddFriend()
    {

    }

    void OnClickFriendsMenu()
    {

    }
    #endregion
}
