using AOT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityMainThreadDispatcher;
using static DewaniaSession.DewaniaGameData;
using Message = DewaniaSession.DewaniaGameData.Message;

public enum ClientHostStateEnum : int
{
    Connected = 1, Diconnected = -1
}

public enum CurrentOperationEnum : int
{
    Free = 0, Connecting = 1, Disconnecting = 2, UpdatingGameState = 3, GettingGameState = 3, GettingGameChat = 3, Reconnecting = 4
}

public static class DewaniaHostController
{
#if UNITY_WEBGL && !UNITY_EDITOR
    private delegate void OnConnect_WebGl_Delegate();

    private delegate void OnReconnected_WebGl_Delegate(int attempts);

    private delegate void OnReconnectAttempt_WebGl_Delegate(int attempts);

    private delegate void OnReconnectError_WebGl_Delegate(string error);

    private delegate void OnReconnectFail_WebGl_Delegate();

    private delegate void OnHostError_WebGl_Delegate(string error);

    private delegate void OnDisconnect_WebGl_Delegate(string reason);

    private delegate void OnReceivedGameState_WebGl_Delegate(string response);

    private delegate void OnPlayerConnected_WebGl_Delegate(string response);

    private delegate void OnPlayerDisconnected_WebGl_Delegate(string response);

    private delegate void OnRecievedMessage_WebGl_Delegate(string response);

    [DllImport("__Internal")]
    private static extern void SokcetIOConnect(string accesToken, OnConnect_WebGl_Delegate onConnected, OnHostError_WebGl_Delegate oneConnectionError,
        OnReconnectAttempt_WebGl_Delegate onReconnectAttmept, OnReconnected_WebGl_Delegate onReconnected, OnReconnectError_WebGl_Delegate onReconnectError,
        OnReconnectFail_WebGl_Delegate onReconnectFailed, OnHostError_WebGl_Delegate onError, OnDisconnect_WebGl_Delegate onDisconnected,
        OnReceivedGameState_WebGl_Delegate onReceivedGameState, OnPlayerConnected_WebGl_Delegate onPlayerConnected,
        OnPlayerDisconnected_WebGl_Delegate onPlayerDisconnected,
        OnRecievedMessage_WebGl_Delegate onRecievedMessage);
    [DllImport("__Internal")]
    public static extern bool SokcetIOConnected();
    [DllImport("__Internal")]
    public static extern void SokcetIODisconnect();
#endif

    #region Constants
    const string Access_Token_Header = "access-token";
    const string GameStateEventListenerName = "game:updated";
    const string PlayerConnectedListenerName = "player:online";
    const string PlayerDisconnectedListenerName = "player:offline";
    const string MessagesEventListenerName = "message:sent";
    const int HttpRequestCode = 3;
    #endregion

    #region Variables
    public static ClientHostStateEnum State = ClientHostStateEnum.Diconnected; // to explain current state between client and server 
    public static CurrentOperationEnum CurrentOperation = CurrentOperationEnum.Free; // to explain which operation is performing now
    public static int CurrentErrorType = -1;
    static SocketIOUnity Client;
    #endregion

    #region Props
    public static bool IsConnected
    {
        get
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (Client == null) return false;
            return Client.Connected;
#else
            return SokcetIOConnected();
#endif
        }
    }
    static NetworkInstance NetworkIns => NetworkInstance.Instance;
    static HttpRequests Http => NetworkIns.Http;
    static DewaniaHostConstants Constants => NetworkIns.Constants;
    static string WebSocketUrl => Constants.Scheme + "://" + Constants.URL + "/" + Constants.WebSocketEndpoint;
    #endregion

    #region Custom
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateNetworkInstance()
    {
        UnityEngine.Object.Instantiate(Resources.Load<GameObject>("NetworkInstance"));
    }
    #endregion

    #region StartHosting
    public static void Setup(string access_token, string localPlayerId, string gameId, bool ingroup)
    {
        State = ClientHostStateEnum.Diconnected;
        DewaniaSession.CreateNewSession(access_token, localPlayerId, gameId, ingroup);
    }
    public static void Initialize()
    {
        CreateNewClient();
        AddListeners();
    }
    #endregion

    #region Main Fns

    /// <summary>
    /// call <c>Start()</c> before calling <c>Connect()</c>
    /// </summary>
    public static void Connect()
    {
        if (string.IsNullOrEmpty(DewaniaSession.AccessToken))
        {
            OnError(new Error(ErrorType.AccessTokenMissing));
            Debugging.Error("Must call start before connecting");
            return;
        }

        if (IsConnected)
        {
            // notification
            Debugging.Print("Already connected");
            return;
        }

        static void ContinueConnecting()
        {
            Debugging.Print("Connecting with client = ", Client);

#if !UNITY_WEBGL || UNITY_EDITOR
            Client.Options.ReconnectionAttempts = int.MaxValue;
#endif
            CurrentOperation = CurrentOperationEnum.Connecting;
            DeleteOldSession();
#if !UNITY_WEBGL || UNITY_EDITOR
            Client.Connect();
#else
            SokcetIOConnect(DewaniaSession.AccessToken, OnConnect_WebGl, OnHostError_WebGl, OnReconnectAttempt_WebGl, OnReconnected_WebGl, 
                OnReconnectError_WebGl, OnReconnectFail_WebGl, OnHostError_WebGl, OnDisconnect_WebGl, OnReceivedGameState_WebGl, 
                OnPlayerConnected_WebGl, OnPlayerDisconnected_WebGl, OnRecievedMessage_WebGl);
#endif
        }

        if (!CanCancelCurrentOperationOrFree())
        {
            Debugging.Print($"can't connect while {CurrentOperation} is processed");
            return;
        }
        else
        {
            foreach (IOnConnectingCallbacks callback in Callbacks.OnConnectingCallbacks)
                callback.OnConnecting();

            CancelCurrentOperation(ContinueConnecting, 0);
        }
    }

    public static void GetGameChat(Action<object> OnSuccess)
    {
        if (!IsConnected)
        {
            OnError(new Error(ErrorType.OfflineMode));
            Debugging.Print("must connect before getting game chat");
            return;
        }

        static void ContinueGettingGameChat(Action<object> onSuccess)
        {
            Debugging.Print("getting game chat");

            CurrentOperation = CurrentOperationEnum.GettingGameChat;

            string url = Constants.BaseURL + @"/" + Constants.ChatsEndpoint + @"/" + GameId + "?fromDate=2023-11-14T00:00Z";
            Http.SendRequset(url, null, HttpStateEnum.GetChat, HttpMethod.GET, true, null, onSuccess, (error) =>
            {
                CurrentOperation = CurrentOperationEnum.Free;
                OnError(error);
            });
        }

        if (!CanCancelCurrentOperationOrFree())
        {
            // notification
            Debugging.Print($"cannot get game chat while {CurrentOperation}");
        }
        else
        {
            CancelCurrentOperation(() => ContinueGettingGameChat((data) =>
            {
                OnGetChatSuccessfully(data);
                OnSuccess?.Invoke(data);
            }), 0);
        }
    }

    private static void OnGetChatSuccessfully(object response)
    {
        Chat.Clear();

        string data = JObject.Parse(response.ToString())["data"].ToString(Formatting.Indented);

        if (!data.IsJsonNullOrEmpty())
        {
            List<Message> messages = JsonConvert.DeserializeObject<List<Message>>(data);

            foreach (Message message in messages) message.LocalMessage.Sent = true;

            Chat.UpdateChat(messages.ToArray());
        }
    }

    public static void GetGameState(Action<object> onSuccess)
    {
        if (!IsConnected)
        {
            OnError(new Error(ErrorType.OfflineMode));
            Debugging.Print("must connect before getting game state");
            return;
        }

        static void ContinueGettingGameState(Action<object> onSuccess)
        {
            Debugging.Print("getting game state");

            CurrentOperation = CurrentOperationEnum.GettingGameState;

            string url = Constants.BaseURL + @"/" + Constants.GamesEndpoint + @"/" + GameId;
            Http.SendRequset(url, null, HttpStateEnum.GetState, HttpMethod.GET, true, null, (data) =>
                {
                    CurrentOperation = CurrentOperationEnum.Free;
                    int id = GetStateIdFromResponse(data);
                    ID = id;
                    onSuccess?.Invoke(data);
                }, (error) =>
                {
                    CurrentOperation = CurrentOperationEnum.Free;
                    OnError(error);
                });
        }

        if (!CanCancelCurrentOperationOrFree())
        {
            // notification
            Debugging.Print($"cannot get game state while {CurrentOperation}");
        }
        else
        {
            CancelCurrentOperation(() => ContinueGettingGameState(onSuccess), 1);
        }
    }

    public static void SendMessage(string message, bool wait)
    {
        if (!IsConnected)
        {
            OnError(new Error(ErrorType.OfflineMode));
            Debugging.Print("must connect before getting game chat");
            return;
        }

        Chat.SendMessage(message, wait, (error) =>
        {
            foreach (IGetNewMessage callback in Callbacks.OnGetNewMessage)
                callback.ClearFailedMessages(Chat.Messages.FindAll(o => !o.LocalMessage.Sent).ToList());

            OnError(error);
        });
    }

    public static void Reconnect()
    {
        if (IsConnected)
        {
            Debugging.Print("already connected");
            return;
        }

        foreach (IOnConnectingCallbacks callback in Callbacks.OnConnectingCallbacks)
            callback.OnConnecting();

#if !UNITY_WEBGL || UNITY_EDITOR
        Client.Options.ReconnectionAttempts = int.MaxValue;
#endif
        CurrentOperation = CurrentOperationEnum.Reconnecting;
#if !UNITY_WEBGL || UNITY_EDITOR
        Client.Connect();
#else
        SokcetIOConnect(DewaniaSession.AccessToken, OnConnect_WebGl, OnHostError_WebGl, OnReconnectAttempt_WebGl, OnReconnected_WebGl,
            OnReconnectError_WebGl, OnReconnectFail_WebGl, OnHostError_WebGl, OnDisconnect_WebGl, OnReceivedGameState_WebGl,
            OnPlayerConnected_WebGl, OnPlayerDisconnected_WebGl, OnRecievedMessage_WebGl);
#endif
    }

    public static void UpdateGameState(string data)
    {
        if (!IsConnected)
        {
            OnError(new Error(ErrorType.OfflineMode));
            Debugging.Print("you should connect before udpating game state");
            return;
        }

        if (!CanCancelCurrentOperationOrFree())
        {
            // notification
            Debugging.Print($"cannot update game state while {CurrentOperation} is processed");
            return;
        }
        else
        {
            CancelCurrentOperation(() =>
            {
                CurrentOperation = CurrentOperationEnum.UpdatingGameState;
                Update(data, OnError);
            }, 2);
        }
    }

    private static void OnError(Error error)
    {
        if (error == null) return;

        foreach (IOnErrorCallback callback in Callbacks.OnErrorCallbacks)
            callback.OnError(error);
    }

    public static void Disconnect()
    {
        DeleteOldSession();
        if (!IsConnected)
        {
            // notification
            Debugging.Print("already disconnected");
            return;
        }

        static void ContinueDisconnecting()
        {
            CurrentOperation = CurrentOperationEnum.Disconnecting;
#if !UNITY_WEBGL || UNITY_EDITOR
            Client.Disconnect();
#else
            SokcetIODisconnect();
#endif
        }

        if (!CanCancelCurrentOperationOrFree())
        {
            // notification
            Debugging.Print($"can't disconnecting while {CurrentOperation} is processed");
            return;
        }
        else
        {
            CancelCurrentOperation(ContinueDisconnecting, 0);
        }
    }

    public static void Dispose()
    {
        if (Client != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            Client.Disconnect();
#else
            SokcetIODisconnect();
#endif
            Client = null;
        }
    }
    #endregion

    #region Socket IO Listeners (WebGl)
#if UNITY_WEBGL && !UNITY_EDITOR
    [MonoPInvokeCallback(typeof(OnConnect_WebGl_Delegate))]
    private static void OnConnect_WebGl()
    {
        OnConnect(null, null);
    }

    [MonoPInvokeCallback(typeof(OnReconnected_WebGl_Delegate))]
    private static void OnReconnected_WebGl(int attempts)
    {
        OnReconnected(null, attempts);
    }

    [MonoPInvokeCallback(typeof(OnReconnectAttempt_WebGl_Delegate))]
    private static void OnReconnectAttempt_WebGl(int attempts)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"reconnect attempt with {attempts}");

            foreach (IOnConnectingCallbacks callback in Callbacks.OnConnectingCallbacks)
                callback.OnConnecting();
        });
    }

    [MonoPInvokeCallback(typeof(OnReconnectError_WebGl_Delegate))]
    private static void OnReconnectError_WebGl(string error)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print("reconnect error " + error);
        });
    }

    [MonoPInvokeCallback(typeof(OnReconnectFail_WebGl_Delegate))]
    private static void OnReconnectFail_WebGl()
    {
        OnReconnectFail(null, "");
    }

    [MonoPInvokeCallback(typeof(OnHostError_WebGl_Delegate))]
    private static void OnHostError_WebGl(string error)
    {
        OnHostError(null, error);
    }

    [MonoPInvokeCallback(typeof(OnDisconnect_WebGl_Delegate))]
    private static void OnDisconnect_WebGl(string reason)
    {
        OnDisconnect(null, reason);
    }

    [MonoPInvokeCallback(typeof(OnReceivedGameState_WebGl_Delegate))]
    private static void OnReceivedGameState_WebGl(string response)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"recieved game state: \n" + response.ToString());

            static void ContinueParsingData(string response)
            {
                ParseReceivedGameData(response);

                JObject data = GetDataFromResponse(response);

                int id = GetStateIdFromData(data);

                if (ID == id)
                {
                    Debugging.Print($"the same current state {id}");
                    return;
                }

                ID = id;

                JObject Jstate = GetStateFromData(data);

                string state = Jstate == null ? "" : Jstate.ToString(Formatting.Indented);

                LocalGameState = state;

                foreach (IReceiveGameStateCallbacks callback in Callbacks.OnReceivedGameStateCallbacks)
                    callback.OnReceivedGameState(state);
            }

            CancelCurrentOperation(() => ContinueParsingData(response), 1);
        });
    }

    [MonoPInvokeCallback(typeof(OnPlayerConnected_WebGl_Delegate))]
    private static void OnPlayerConnected_WebGl(string response)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print("some player connected " + response);
            JObject data = JObject.Parse(response);
            string id = (string)data["player"];
            Debug.Log("player join = " + id);
            DewaniaPlayer player = GetPlayer(id);
            if (player != null)
            {
                player.UpdateConnectionState(true);

                foreach (IOnPlayerJoinedCallback callback in Callbacks.OnPlayerJoinedCallbacks)
                    callback.OnPlayerJoined(player);
            }
        });
    }

    [MonoPInvokeCallback(typeof(OnPlayerDisconnected_WebGl_Delegate))]
    private static void OnPlayerDisconnected_WebGl(string response)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print("some player disconnected " + response);
            JObject data = JObject.Parse(response);
            string id = (string)data["player"];
            Debugging.Print("player left = " + id);
            DewaniaPlayer player = GetPlayer(id);
            if (player != null)
            {
                player.UpdateConnectionState(false);

                foreach (IOnPlayerLeftCallback callback in Callbacks.OnPlayerLeftCallbacks)
                    callback.OnPlayerLeft(player);
            }
        });
    }

    [MonoPInvokeCallback(typeof(OnRecievedMessage_WebGl_Delegate))]
    private static void OnRecievedMessage_WebGl(string response)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print("message received " + response);
            Message message = JsonConvert.DeserializeObject<Message>(response);
            Debugging.Print("message = ", message);
            message.LocalMessage.Sent = true;
            Chat.UpdateChat(message);
        });
    }
#endif
    #endregion

    #region Socket IO Listeners (Editor)
    private static void AddListeners()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debugging.Print("listeners added");
        Client.OnConnected += OnConnect;
        Client.OnReconnectAttempt += OnReconnectAttempt;
        Client.OnReconnected += OnReconnected;
        Client.OnReconnectError += OnReconnectError;
        Client.OnReconnectFailed += OnReconnectFail;
        Client.OnError += OnHostError;
        Client.OnDisconnected += OnDisconnect;
        Client.OnPing += (sender, obj) => { Debugging.Print($"ping {sender}", obj); };
        Client.OnPong += (sender, obj) => { Debugging.Print($"pong {sender}", obj); };
        Client.On(GameStateEventListenerName, OnReceivedGameState);
        Client.On(PlayerConnectedListenerName, OnPlayerConnected);
        Client.On(PlayerDisconnectedListenerName, OnPlayerDisconnected);
        Client.On(MessagesEventListenerName, OnRecievedMessage);
#endif
    }

    private static void OnConnect(object sender, EventArgs args)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"player connected");
            CurrentOperation = CurrentOperationEnum.Free;
            State = ClientHostStateEnum.Connected;

            foreach (IOnConnectedCallbacks callback in Callbacks.OnConnectedCallbacks)
                callback.OnConnectedToHost();

            GetGameChat((data) =>
            {
                GetGameState(OnGettingGameStateSuccess);
            });
        });
    }

    public static void OnGettingGameStateSuccess(object data)
    {
        Debugging.Print("got game state", data);

        ParseReceivedGameData(data);

        JObject obj = GetStateFromResponse(data);

        string state = obj == null ? "" : obj.ToString(Formatting.Indented);

        LocalGameState = state;

        foreach (IReceiveGameStateCallbacks Callback in Callbacks.OnReceivedGameStateCallbacks)
        {
            Callback.OnReceivedGameState(state);
        }
    }

    private static void OnReconnected(object sender, int attempts)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"player reconnected " + attempts);
            CurrentOperation = CurrentOperationEnum.Free;
            State = ClientHostStateEnum.Connected;

            foreach (IOnConnectedCallbacks callback in Callbacks.OnConnectedCallbacks)
                callback.OnConnectedToHost();

            GetGameState(OnGettingGameStateSuccess);
        });
    }

    private static void OnReconnectAttempt(object sender, int attempts)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"reconnect attempt with {sender} and {attempts}");
            Client.Options.ReconnectionAttempts = attempts;

            foreach (IOnConnectingCallbacks callback in Callbacks.OnConnectingCallbacks)
                callback.OnConnecting();
        });
    }

    private static void OnReconnectError(object sender, Exception ex)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print("reconnect error " + ex.Message);
        });
    }

    private static void OnReconnectFail(object sender, object message)
    {
        Dispatcher.Enqueue(() =>
        {
            foreach (IOnConnectingFailedCallbacks callback in Callbacks.OnConnectingFailedCallbacks)
                callback.OnConnectFail();

            Debugging.Print("reconnect fail ", message);
            OnError(new Error(ErrorType.NetwrokConnection));
        });
    }

    private static void OnHostError(object sender, string error)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"error occured from cause of {error}");

            ParseHostError(error);
        });
    }

    private static void ParseHostError(string message)
    {
        if (message == "jwt malformed")
        {
            OnError(new Error(ErrorType.AccessTokenMissing));
        }
        else if (message == "jwt expired")
        {
            OnError(new Error(ErrorType.AccessTokenExpired));
        }
        else if (message == "invalid token")
        {
            OnError(new Error(ErrorType.AccessTokenInvalid));
        }
        else
        {
            OnError(new Error(ErrorType.Unknonw));
        }
    }

    private static void OnDisconnect(object sender, string reason)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"player disconnected with");

            CancelCurrentOperation(null, 0);

            State = ClientHostStateEnum.Diconnected;
            foreach (IOnDisconnectCallbacks callback in Callbacks.OnDisconnectCallbacks)
                callback.OnDisconnectFromHost();

            OnError(new Error(ErrorType.NetwrokConnection));
        });
    }

    private static void OnReceivedGameState(SocketIOResponse response)
    {
        Dispatcher.Enqueue(() =>
        {
            Debugging.Print($"recieved game state: \n" + response.ToString());

            static void ContinueParsingData(SocketIOResponse response)
            {
                ParseReceivedGameData(response);

                JObject data = GetDataFromResponse(response);

                int id = GetStateIdFromData(data);

                if (ID == id)
                {
                    Debugging.Print($"the same current state {id}");
                    return;
                }

                ID = id;

                JObject Jstate = GetStateFromData(data);

                string state = Jstate == null ? "" : Jstate.ToString(Formatting.Indented);

                LocalGameState = state;

                foreach (IReceiveGameStateCallbacks callback in Callbacks.OnReceivedGameStateCallbacks)
                    callback.OnReceivedGameState(state);
            }

            CancelCurrentOperation(() => ContinueParsingData(response), 1);
        });
    }

    private static void OnPlayerConnected(SocketIOResponse response)
    {
        Dispatcher.Enqueue(() =>
        {
            JObject data = JObject.Parse(response.GetValue<object>().ToString());
            string id = (string)data["player"];
            Debug.Log("player join = " + id);
            DewaniaPlayer player = GetPlayer(id);
            if (player != null)
            {
                player.UpdateConnectionState(true);

                foreach (IOnPlayerJoinedCallback callback in Callbacks.OnPlayerJoinedCallbacks)
                    callback.OnPlayerJoined(player);
            }
        });
    }

    private static void OnPlayerDisconnected(SocketIOResponse response)
    {
        Dispatcher.Enqueue(() =>
        {
            JObject data = JObject.Parse(response.GetValue<object>().ToString());
            string id = (string)data["player"];
            Debugging.Print("player left = " + id);
            DewaniaPlayer player = GetPlayer(id);
            if (player != null)
            {
                player.UpdateConnectionState(false);

                foreach (IOnPlayerLeftCallback callback in Callbacks.OnPlayerLeftCallbacks)
                    callback.OnPlayerLeft(player);
            }
        });
    }

    private static void OnRecievedMessage(SocketIOResponse response)
    {
        Dispatcher.Enqueue(() =>
        {
            Message message = JsonConvert.DeserializeObject<Message>(response.GetValue<object>().ToString());
            Debugging.Print("message = ", message);
            message.LocalMessage.Sent = true;
            Chat.UpdateChat(message);
        });
    }
    #endregion

    #region Helper Fns
    private static void DeleteOldSession()
    {
        DewaniaSession.DewaniaGameData.Reset();
    }

    private static void CreateNewClient()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debugging.Print($"creating new client to " + WebSocketUrl);

        SocketIOOptions options = new SocketIOOptions
        {
            ConnectionTimeout = TimeSpan.FromSeconds(5),
            ExtraHeaders = new Dictionary<string, string>
            {
                { Access_Token_Header, DewaniaSession.AccessToken }
            },
            RandomizationFactor = 0,
            Reconnection = true,
            ReconnectionAttempts = int.MaxValue,
            ReconnectionDelay = 1000,
            ReconnectionDelayMax = 5000,
            Transport = TransportProtocol.WebSocket,
        };

        Client = new SocketIOUnity(WebSocketUrl, options, SocketIOUnity.UnityThreadScope.Update);
        Client.HttpClient.Timeout = TimeSpan.FromSeconds(5);

        Debugging.Print($"created new client", Client);
#endif
    }

    private static void ParseReceivedGameData(SocketIOResponse Response)
    {
        JObject data = GetDataFromResponse(Response);
        ParseReceivedGameData(data);
    }

    private static void ParseReceivedGameData(object Response)
    {
        JObject data = GetDataFromResponse(Response);
        ParseReceivedGameData(data);
    }

    private static void ParseReceivedGameData(JObject data)
    {
        if (data == null) return;

        List<DewaniaPlayer> players = GetPlayers(data["players"]);
        if (players != null)
            ParseReceivedPlayers(players);
    }

    private static JObject GetDataFromResponse(SocketIOResponse Response)
    {
        JObject data = JObject.Parse(Response.GetValue<object>().ToString());
        return JObject.Parse((data["data"] == null ? data : data["data"]).ToString(Formatting.Indented));
    }

    private static JObject GetDataFromResponse(object Response)
    {
        JObject data = JObject.Parse((string)Response);
        return JObject.Parse((data["data"] == null ? data : data["data"]).ToString(Formatting.Indented));
    }

    public static JObject GetStateFromResponse(SocketIOResponse response)
    {
        JObject data = GetDataFromResponse(response);
        return GetStateFromData(data);
    }

    public static JObject GetStateFromResponse(object response)
    {
        JObject data = GetDataFromResponse(response);
        return GetStateFromData(data);
    }

    public static JObject GetStateFromData(JObject data)
    {
        if (data == null) return null;

        JToken token;
        if (data.TryGetValue("state", out token))
        {
            return JObject.Parse(token.ToString(Formatting.Indented));
        }
        return null;
    }

    private static int GetStateIdFromResponse(SocketIOResponse state)
    {
        JObject data = GetDataFromResponse(state);
        return GetStateIdFromData(data);
    }

    private static int GetStateIdFromResponse(object state)
    {
        JObject data = GetDataFromResponse(state);
        return GetStateIdFromData(data);
    }

    private static int GetStateIdFromData(JObject data)
    {
        JToken token;
        if (data.TryGetValue("numOfUpdates", out token))
        {
            return ((int)token);
        }
        return -1;
    }

    public static bool IsJsonNullOrEmpty(this string jsonString)
    {
        // Check if the JSON string is null or empty
        if (string.IsNullOrEmpty(jsonString))
            return true;

        try
        {
            // Attempt to parse the JSON string
            var jsonObject = JObject.Parse(jsonString);

            // Check if the parsed object is an empty JSON object
            return !jsonObject.HasValues;
        }
        catch (Exception)
        {
            // If parsing fails, the JSON string is invalid
            return true;
        }
    }

    private static List<DewaniaPlayer> GetPlayers(JToken json)
    {
        List<DewaniaPlayer> players = new List<DewaniaPlayer>();
        foreach (var player in json)
        {
            JToken hostPlayer = player["player"];
            players.Add(new DewaniaPlayer(false, (string)hostPlayer["id"], (string)hostPlayer["name"], (string)hostPlayer["picture"], "", (int)hostPlayer["level"],
                (int)hostPlayer["points"], ((string)player["connectionState"]) == "Online" || ((string)hostPlayer["id"]) == DewaniaSession.LocalPlayerId));
        }
        return players;
    }

    private static void ParseReceivedPlayers(List<DewaniaPlayer> players)
    {
        // check new left players
        if (Players != null)
        {
            foreach (DewaniaPlayer player in Players)
            {
                if (player.IsBot) continue;

                DewaniaPlayer oldPlayer = players.Find(o => o.ID == player.ID);
                if (oldPlayer == null)
                {
                    // old player left
                    foreach (IOnPlayerLeftCallback callback in Callbacks.OnPlayerLeftCallbacks)
                        callback.OnPlayerLeft(player);
                }
            }
        }

        // check new joined players
        foreach (DewaniaPlayer recievedPlayer in players)
        {
            DewaniaPlayer player = GetPlayer(recievedPlayer.ID);
            if (player == null)
            {
                // new player joined
                if (TryGetBot(out DewaniaPlayer bot))
                {
                    player = bot = new DewaniaPlayer(true, recievedPlayer.ID, recievedPlayer.Name, recievedPlayer.Pic, "",
                        recievedPlayer.Level, recievedPlayer.Points, recievedPlayer.IsOnline());
                }
                else
                {
                    player = new DewaniaPlayer(false, recievedPlayer.ID,
                        recievedPlayer.Name, recievedPlayer.Pic, "", 
                        recievedPlayer.Level, recievedPlayer.Points, recievedPlayer.IsOnline());

                    Players.Add(player);
                }

                foreach (IOnPlayerJoinedCallback callback in Callbacks.OnPlayerJoinedCallbacks)
                    callback.OnPlayerJoined(player);
            }
        }

        AddRemainsPlayersAsBots();
    }

    public static bool CanCancelCurrentOperationOrFree()
    {
        if (CurrentOperation == CurrentOperationEnum.Free) return true;

        if (CurrentOperation == CurrentOperationEnum.Connecting || CurrentOperation == CurrentOperationEnum.Disconnecting ||
            CurrentOperation == CurrentOperationEnum.Reconnecting)
            return false;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onCancel"></param>
    /// <param name="operations">
    /// 0 for all.<br/>
    /// 1 for state operations only.<br/>
    /// 2 for update state opratoins.<br/>
    /// 3 for get state operations.<br/>
    /// 4 for get chat operations.<br/>
    /// 5 for send message operations.
    /// </param>
    public static void CancelCurrentOperation(Action onCancel, int operations)
    {
        if (((int)CurrentOperation) == HttpRequestCode)
        {
            switch (operations)
            {
                case 0:
                    Http.CancelAnyReqeust(() =>
                    {
                        CurrentOperation = CurrentOperationEnum.Free;
                        onCancel?.Invoke();
                    });
                    break;

                case 1:
                    Http.CancelStateRequests(() =>
                    {
                        CurrentOperation = CurrentOperationEnum.Free;
                        onCancel?.Invoke();
                    });
                    break;

                case 2:
                    Http.CancelStateRequests(() =>
                    {
                        CurrentOperation = CurrentOperationEnum.Free;
                        onCancel?.Invoke();
                    });
                    break;

                case 3:
                    Http.CancelStateRequests(() =>
                    {
                        CurrentOperation = CurrentOperationEnum.Free;
                        onCancel?.Invoke();
                    });
                    break;

                case 4:
                    Http.CancelStateRequests(() =>
                    {
                        CurrentOperation = CurrentOperationEnum.Free;
                        onCancel?.Invoke();
                    });
                    break;

                case 5:
                    Http.CancelStateRequests(() =>
                    {
                        CurrentOperation = CurrentOperationEnum.Free;
                        onCancel?.Invoke();
                    });
                    break;
            }
        }
        else
        {
            CurrentOperation = CurrentOperationEnum.Free;
            onCancel?.Invoke();
        }
    }
    #endregion
}
