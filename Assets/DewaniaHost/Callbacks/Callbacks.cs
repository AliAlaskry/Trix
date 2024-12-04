using System.Collections.Generic;

public static class Callbacks
{
    public static List<IOnConnectingCallbacks> OnConnectingCallbacks = new List<IOnConnectingCallbacks>();
    public static List<IOnConnectedCallbacks> OnConnectedCallbacks = new List<IOnConnectedCallbacks>();
    public static List<IOnConnectingFailedCallbacks> OnConnectingFailedCallbacks = new List<IOnConnectingFailedCallbacks>();
    public static List<IOnErrorCallback> OnErrorCallbacks = new List<IOnErrorCallback>();
    public static List<IReceiveGameStateCallbacks> OnReceivedGameStateCallbacks = new List<IReceiveGameStateCallbacks>();
    public static List<IOnDisconnectCallbacks> OnDisconnectCallbacks = new List<IOnDisconnectCallbacks>();
    public static List<IOnPlayerJoinedCallback> OnPlayerJoinedCallbacks = new List<IOnPlayerJoinedCallback>();
    public static List<IOnPlayerLeftCallback> OnPlayerLeftCallbacks = new List<IOnPlayerLeftCallback>();
    public static List<IGetNewMessage> OnGetNewMessage = new List<IGetNewMessage>();

    public static void AddCallback<T>(T callback) where T : ICallback
    {
        AddCallback<IOnConnectingCallbacks>(callback, OnConnectingCallbacks);
        AddCallback<IOnConnectedCallbacks>(callback, OnConnectedCallbacks);
        AddCallback<IOnConnectingFailedCallbacks>(callback, OnConnectingFailedCallbacks);
        AddCallback<IOnErrorCallback>(callback, OnErrorCallbacks);
        AddCallback<IReceiveGameStateCallbacks>(callback, OnReceivedGameStateCallbacks);
        AddCallback<IOnDisconnectCallbacks>(callback, OnDisconnectCallbacks);
        AddCallback<IOnPlayerJoinedCallback>(callback, OnPlayerJoinedCallbacks);
        AddCallback<IOnPlayerLeftCallback>(callback, OnPlayerLeftCallbacks);
        AddCallback<IGetNewMessage>(callback, OnGetNewMessage);
    }

    static void AddCallback<T>(ICallback callback, List<T> container) where T : class
    {
        T target = callback as T;
        if (target != null)
        {
            container.Add(target);
        }
    }

    public static void RemoveCallback<T>(T callback) where T : ICallback
    {
        RemoveCallback<IOnConnectingCallbacks>(callback, OnConnectingCallbacks);
        RemoveCallback<IOnConnectedCallbacks>(callback, OnConnectedCallbacks);
        RemoveCallback<IOnConnectingFailedCallbacks>(callback, OnConnectingFailedCallbacks);
        RemoveCallback<IOnConnectingFailedCallbacks>(callback, OnConnectingFailedCallbacks);
        RemoveCallback<IOnErrorCallback>(callback, OnErrorCallbacks);
        RemoveCallback<IReceiveGameStateCallbacks>(callback, OnReceivedGameStateCallbacks);
        RemoveCallback<IOnDisconnectCallbacks>(callback, OnDisconnectCallbacks);
        RemoveCallback<IOnPlayerJoinedCallback>(callback, OnPlayerJoinedCallbacks);
        RemoveCallback<IOnPlayerLeftCallback>(callback, OnPlayerLeftCallbacks);
        RemoveCallback<IGetNewMessage>(callback, OnGetNewMessage);
    }

    static void RemoveCallback<T>(ICallback callback, List<T> container) where T : class
    {
        T target = callback as T;
        if (target != null)
        {
            container.Remove(target);
        }
    }
}