using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IScreenComponents
{
    void Subscribe();
    void UnSubscribe();
}

public class ScreenComponents : MonoBehaviour, IScreenComponents
{
    #region Special Fields
    public virtual bool Subscribed
    {
        get; set;
    } = false;
    #endregion

    #region Props
    protected ScreenComponentsController ScreenController => ScreenComponentsController.Instance;
    protected TrixState GameState => TrixController.GameState;
    #endregion

    #region Listeners
    public virtual void ResetInitialState()
    {

    }

    public virtual void Subscribe()
    {
        Subscribed = true;
    }

    public virtual void UnSubscribe()
    {
        Subscribed = false;

        ResetInitialState();
    }
    #endregion
}
