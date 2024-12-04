using System.Collections.Generic;
using static DewaniaSession.DewaniaGameData;
public interface ICallback { }

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnConnectedCallbacks : ICallback
{
    void OnConnectedToHost();
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnConnectingCallbacks : ICallback
{
    void OnConnecting();
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnConnectingFailedCallbacks : ICallback
{
    void OnConnectFail();
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnDisconnectCallbacks : ICallback
{
    void OnDisconnectFromHost();
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnConnectionCallbacks : IOnConnectingCallbacks, IOnDisconnectCallbacks { }

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IReceiveGameStateCallbacks : ICallback
{
    void OnReceivedGameState(string data);
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnErrorCallback : ICallback
{
    void OnError(Error error);
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnPlayerJoinedCallback : ICallback
{
    void OnPlayerJoined(DewaniaPlayer player);
}

/// <summary>
/// Don't forget to call <br/>
/// <c>Callbacks.AddListener</c> OnEnable (Unity) .<br/>
/// <c>Callbacks.RemoveListener</c> OnDisable (Unity).
/// </summary>
public interface IOnPlayerLeftCallback : ICallback
{
    void OnPlayerLeft(DewaniaPlayer player);
}

public interface IPlayersCallbacks : IOnPlayerJoinedCallback, IOnPlayerLeftCallback { }

public interface IGetNewMessage : ICallback
{
    void OnGetMessage(Message message);
    void ClearFailedMessages(List<Message> messages);
}