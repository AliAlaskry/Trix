using System.Collections.Generic;
using UnityEngine;
using static DewaniaSession.DewaniaGameData;

public class Callback : MonoBehaviour, ICallback
{
    private void OnEnable()
    {
        Callbacks.AddCallback(this);
    }

    private void OnDisable()
    {
        Callbacks.RemoveCallback(this);
    }
}

public class OnConnectedCallback : Callback, IOnConnectedCallbacks
{
    public virtual void OnConnectedToHost()
    {

    }
}

public class OnConnectingCallback : Callback, IOnConnectingCallbacks
{
    public virtual void OnConnecting()
    {

    }
}

public class OnConnectingFailedCallback : Callback, IOnConnectingFailedCallbacks
{
    public virtual void OnConnectFail()
    {

    }
}

public class OnDisconnectCallback : Callback, IOnDisconnectCallbacks
{
    public virtual void OnDisconnectFromHost()
    {
    }
}

public class ConnectionCallbacks : OnConnectedCallback, IOnDisconnectCallbacks
{
    public virtual void OnDisconnectFromHost()
    {
    }
}

public class ReceivedGameStateCallback : Callback, IReceiveGameStateCallbacks
{
    public virtual void OnReceivedGameState(string data)
    {

    }
}

public class OnErrorCallback : Callback, IOnErrorCallback
{
    public virtual void OnError(Error error)
    {

    }
}

public class OnPlayerLeftCallback : Callback, IOnPlayerLeftCallback
{
    public virtual void OnPlayerLeft(DewaniaPlayer player)
    {
    }
}

public class PlayersJoinLeftCallback : Callback, IPlayersCallbacks
{
    public virtual void OnPlayerJoined(DewaniaPlayer player)
    {
    }

    public virtual void OnPlayerLeft(DewaniaPlayer player)
    {
    }
}

public class NewMessageCallback : Callback, IGetNewMessage
{
    public virtual void ClearFailedMessages(List<Message> messages)
    {

    }

    public virtual void OnGetMessage(Message message)
    {

    }
}