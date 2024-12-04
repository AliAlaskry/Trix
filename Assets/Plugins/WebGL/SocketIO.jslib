var LibrarySocketIO = {
    $SocketIOCallbacks: {
        socket:null,

        OnConnected: null,
        OnConnectError: null,
        OnReconnectAttempt: null,
        OnReconnected: null,
        OnReconnectError: null,
        OnReconnectFailed: null,
        OnError: null,
        OnDisconnected: null,
        OnReceivedGameState: null,
        OnPlayerConnected: null,
        OnPlayerDisconnected: null,
        OnRecievedMessage: null
    },

    SokcetIOConnect: function(AccessToken, onConnected, onConnectError, onReconnectAttempt, onReconnected, onReconnectError, 
        onReconnectFailed, onError, onDisconnected, onReceivedGameState, onPlayerConnected, onPlayerDisconnected, onRecievedMessage) {
        if (SocketIOCallbacks.socket != null) if (SocketIOCallbacks.socket.connected) return;
        
        AccessToken = UTF8ToString(AccessToken);
        
        SocketIOCallbacks.socket = new io("https://api.aldewanyh.com/users", {
            extraHeaders: {
                "access-token": AccessToken
            },
            randomizationFactor: 0,
            reconnection: true,
            reconnectionAttempts: 1,
            reconnectionDelay: 1000,
            timeout: 5000,
            autoConnect: false
        });

        console.log("connecting " + AccessToken, SocketIOCallbacks.socket.io);

        SocketIOCallbacks.OnConnected = onConnected;
        SocketIOCallbacks.OnConnectError = onConnectError;
        SocketIOCallbacks.OnReconnectAttempt = onReconnectAttempt;
        SocketIOCallbacks.OnReconnected = onReconnected;
        SocketIOCallbacks.OnReconnectError = onReconnectError;
        SocketIOCallbacks.OnReconnectFailed = onReconnectFailed;
        SocketIOCallbacks.OnError = onError;
        SocketIOCallbacks.OnDisconnected = onDisconnected;
        SocketIOCallbacks.OnReceivedGameState = onReceivedGameState;
        SocketIOCallbacks.OnPlayerConnected = onPlayerConnected;
        SocketIOCallbacks.OnPlayerDisconnected = onPlayerDisconnected;
        SocketIOCallbacks.OnRecievedMessage = onRecievedMessage;

        SocketIOCallbacks.socket.on("error", (error) => {
            console.log("error ", error);
          
            var temp = error.message;
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);

            Module['dynCall_vi'](SocketIOCallbacks.OnError, [buffer]);
        });

        SocketIOCallbacks.socket.on("reconnect", (attempt) => {
            console.log("reconnected ", attempt);
            Module['dynCall_vi'](SocketIOCallbacks.OnReconnected, [attempt]);
        });

        SocketIOCallbacks.socket.on("reconnect_attempt", (attempt) => {
            console.log("reconnecting attempt ", attempt);
            Module['dynCall_vi'](SocketIOCallbacks.OnReconnectAttempt, [attempt]);
        });

        SocketIOCallbacks.socket.on("reconnect_error", (error) => {
            console.log("reconnect error ", error);
            
            var temp = error.message;
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);

            Module['dynCall_vi'](SocketIOCallbacks.OnReconnectError, [buffer]);
        });
        SocketIOCallbacks.socket.on("reconnect_failed", () => {
            console.log("reconnect failed");
            Module['dynCall_v'](SocketIOCallbacks.OnReconnectFailed, []);
        });

        SocketIOCallbacks.socket.on("disconnect", (reason) => {
            if (SocketIOCallbacks.socket.active) {
                console.log("dis temp ", reason);
            } else {
                console.log("disconnected ", reason);
               
                var temp = JSON.stringify(reason);
                var bufferSize = lengthBytesUTF8(temp) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(temp, buffer, bufferSize);

                Module['dynCall_vi'](SocketIOCallbacks.OnDisconnected, [buffer]);
            }
        });

        var GameStateEventListenerName = "game:updated";
        var PlayerConnectedListenerName = "player:online";
        var PlayerDisconnectedListenerName = "player:offline";
        var MessagesEventListenerName = "message:sent";

        SocketIOCallbacks.socket.on(GameStateEventListenerName, (data) => {
            console.log("state data ", data);
            
            var temp = JSON.stringify(data);
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);
            
            Module['dynCall_vi'](SocketIOCallbacks.OnReceivedGameState, [buffer]);
        });

        SocketIOCallbacks.socket.on(PlayerConnectedListenerName, (data) => {
            console.log("player connected data ", data);
            
            var temp = JSON.stringify(data);
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);

            Module['dynCall_vi'](SocketIOCallbacks.OnPlayerConnected, [buffer]);
        });

        SocketIOCallbacks.socket.on(PlayerDisconnectedListenerName, (data) => {
            console.log("player disconnected data ", data);
            
            var temp = JSON.stringify(data);
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);

            Module['dynCall_vi'](SocketIOCallbacks.OnPlayerDisconnected, [buffer]);
        });

        SocketIOCallbacks.socket.on(MessagesEventListenerName, (data) => {
            console.log("message data ", data);

            var temp = JSON.stringify(data);
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);

            Module['dynCall_vi'](SocketIOCallbacks.OnRecievedMessage, [buffer]);
        });

        SocketIOCallbacks.socket.on("connect_error", (error) => {
           
            var temp = error.message;
            var bufferSize = lengthBytesUTF8(temp) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(temp, buffer, bufferSize);
           
            if (SocketIOCallbacks.socket.active) {
                SocketIOCallbacks.socket.disconnect();
            }

            Module['dynCall_vi'](SocketIOCallbacks.OnConnectError, [buffer]);
        });

        SocketIOCallbacks.socket.on("connect", () => {
            console.log("connected ", SocketIOCallbacks.socket);
            Module['dynCall_v'](SocketIOCallbacks.OnConnected, []);
        });

        SocketIOCallbacks.socket.connect();
    },

    SokcetIOConnected: function() {
        console.log("check connected ", SocketIOCallbacks.socket);
        if (SocketIOCallbacks.socket == null) return false;
        return SocketIOCallbacks.socket.connected;
    },

    SokcetIODisconnect: function() {
        SocketIOCallbacks.socket.disconnect();
    }
};

autoAddDeps(LibrarySocketIO, '$SocketIOCallbacks');
mergeInto(LibraryManager.library, LibrarySocketIO);
