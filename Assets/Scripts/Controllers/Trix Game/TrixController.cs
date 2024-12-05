using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;

public class TrixController : Callback, IReceiveGameStateCallbacks, IOnConnectedCallbacks, IOnPlayerLeftCallback, IOnPlayerJoinedCallback
{
    #region Variables
    [SerializeReference] public static TrixState GameState;
    public static bool CombineTrix = false;
    #endregion

    #region Externs
    [DllImport("__Internal")]
    private static extern string GetURLFromPage();
    #endregion

    #region Unity Fns
    private void Start()
    {
        GetWebviewHeaders(out string access_token, out string localPlayerId, out string gameId, out bool ingroup, out CombineTrix);
        DewaniaHostController.Setup(access_token, localPlayerId, gameId, ingroup);
        DewaniaHostController.Initialize();
        DewaniaHostController.Connect();

        GameState = new TrixState(CombineTrix);
        ScreenComponentsController.Instance?.UnsubscribeAll();

        // display connecting panel 
    }
    #endregion

    #region Fns
    private void GetWebviewHeaders(out string access_token, out string localPlayerId, out string gameId, out bool ingroup, out bool combineTrix)
    {
        Debugging.Print($"Get login headers _ isEditor = {Application.isEditor}");
#if UNITY_WEBGL && !UNITY_EDITOR
        // get web parameters 
        string url = GetURLFromPage();
        Debugging.Print("url is = " + url);
        Uri uri = new Uri(url);
        NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
        access_token = queryParameters["access_Token"];
        localPlayerId = queryParameters["Player_id"];
        gameId = queryParameters["Game_id"];
        ingroup = bool.Parse(queryParameters["InGroup"]);
        combineTrix = bool.Parse(queryParameters["Combine"]);
        // get combine trix value
#else
        DewaniaHostConstants constants = NetworkInstance.Instance.Constants;
        access_token = constants.access_token_Test;
        localPlayerId = constants.playerId_Test;
        gameId = constants.gameId_Test;
        ingroup = constants.InGroup_Test;
        combineTrix = false;
#endif
    }

    void AddPlayerData(TrixState state)
    {
        int teamOrder;

        TrixPlayer p1 = null, p2 = null;

        int index = 0;
        int count = DewaniaSession.DewaniaGameData.Players.Count;
        for (; index < count; index++)
        {
            DewaniaPlayer player_1 = DewaniaSession.DewaniaGameData.Players[index];

            if (CombineTrix)
            {
                teamOrder = index / 2;

                p1 = new TrixPlayer(player_1.ID, teamOrder, player_1.Name, player_1.Avatar, player_1.Frame, 0, count, player_1.IsBot, false, false);

                DewaniaPlayer player_2 = DewaniaSession.DewaniaGameData.Players[index];
                p2 = new TrixPlayer(player_2.ID, teamOrder, player_2.Name, player_2.Avatar, player_2.Frame, 0, count, player_2.IsBot, false, false);
            }
            else
            {
                teamOrder = index;

                p1 = new TrixPlayer(player_1.ID, teamOrder, player_1.Name, player_1.Avatar, player_1.Frame, 0, count, player_1.IsBot, false, false);
            }

            GameState.Teams[teamOrder] = new TrixTeam(teamOrder, p1, p2);
        }
    }
    #endregion

    #region Listeners
    public void OnConnectedToHost()
    {
        // hide connecting panel
        //      with fade out animation
        // display waiting for other players panel with:
        //      rotating loading icon
        //      no open animation
    }

    public void OnReceivedGameState(string data)
    {
        bool isNewGame = data.IsJsonNullOrEmpty();

        TrixState temp;
        if (isNewGame)
        {
            temp = new TrixState(CombineTrix);

            if (!temp.PlayersAdded())
            {
                AddPlayerData(temp);
            }

            if (CombineTrix)
            {
                int teamIndex = 0, playerIndex = 0;
                TrixPlayer player_1 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[playerIndex].ID, teamIndex, DewaniaSession.DewaniaGameData.Players[playerIndex].Name,
                        DewaniaSession.DewaniaGameData.Players[playerIndex].Avatar, DewaniaSession.DewaniaGameData.Players[playerIndex].Frame, 0, playerIndex,
                        DewaniaSession.DewaniaGameData.Players[playerIndex].IsBot, DewaniaSession.DewaniaGameData.Players[playerIndex].IsOnline(), false);

                playerIndex = 1;
                TrixPlayer player_2 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[playerIndex].ID, teamIndex, DewaniaSession.DewaniaGameData.Players[playerIndex].Name,
                         DewaniaSession.DewaniaGameData.Players[playerIndex].Avatar, DewaniaSession.DewaniaGameData.Players[playerIndex].Frame, 0, playerIndex,
                         DewaniaSession.DewaniaGameData.Players[playerIndex].IsBot, DewaniaSession.DewaniaGameData.Players[playerIndex].IsOnline(), false);

                teamIndex = 1;

                playerIndex = 2;
                TrixPlayer player_3 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[playerIndex].ID, teamIndex, DewaniaSession.DewaniaGameData.Players[playerIndex].Name,
                         DewaniaSession.DewaniaGameData.Players[playerIndex].Avatar, DewaniaSession.DewaniaGameData.Players[playerIndex].Frame, 0, playerIndex,
                         DewaniaSession.DewaniaGameData.Players[playerIndex].IsBot, DewaniaSession.DewaniaGameData.Players[playerIndex].IsOnline(), false);

                playerIndex = 3;
                TrixPlayer player_4 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[playerIndex].ID, teamIndex, DewaniaSession.DewaniaGameData.Players[playerIndex].Name,
                         DewaniaSession.DewaniaGameData.Players[playerIndex].Avatar, DewaniaSession.DewaniaGameData.Players[playerIndex].Frame, 0, playerIndex,
                         DewaniaSession.DewaniaGameData.Players[playerIndex].IsBot, DewaniaSession.DewaniaGameData.Players[playerIndex].IsOnline(), false);

                temp.Teams = new TrixTeam[2]
                {
                    new TrixTeam(0, player_1, player_2),
                    new TrixTeam(1, player_3, player_4),
                };
            }
            else
            {
                int index = 0;
                TrixPlayer player_1 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[index].ID, index, DewaniaSession.DewaniaGameData.Players[index].Name,
                        DewaniaSession.DewaniaGameData.Players[index].Avatar, DewaniaSession.DewaniaGameData.Players[index].Frame, 0, index,
                        DewaniaSession.DewaniaGameData.Players[index].IsBot, DewaniaSession.DewaniaGameData.Players[index].IsOnline(), false);

                index = 1;
                TrixPlayer player_2 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[index].ID, index, DewaniaSession.DewaniaGameData.Players[index].Name,
                         DewaniaSession.DewaniaGameData.Players[index].Avatar, DewaniaSession.DewaniaGameData.Players[index].Frame, 0, index,
                         DewaniaSession.DewaniaGameData.Players[index].IsBot, DewaniaSession.DewaniaGameData.Players[index].IsOnline(), false);

                index = 2;
                TrixPlayer player_3 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[index].ID, index, DewaniaSession.DewaniaGameData.Players[index].Name,
                         DewaniaSession.DewaniaGameData.Players[index].Avatar, DewaniaSession.DewaniaGameData.Players[index].Frame, 0, index,
                         DewaniaSession.DewaniaGameData.Players[index].IsBot, DewaniaSession.DewaniaGameData.Players[index].IsOnline(), false);

                index = 3;
                TrixPlayer player_4 = new TrixPlayer(DewaniaSession.DewaniaGameData.Players[index].ID, index, DewaniaSession.DewaniaGameData.Players[index].Name,
                         DewaniaSession.DewaniaGameData.Players[index].Avatar, DewaniaSession.DewaniaGameData.Players[index].Frame, 0, index,
                         DewaniaSession.DewaniaGameData.Players[index].IsBot, DewaniaSession.DewaniaGameData.Players[index].IsOnline(), false);

                temp.Teams = new TrixTeam[4]
                {
                    new TrixTeam(0, player_1, null),
                    new TrixTeam(1, player_2, null),
                    new TrixTeam(2, player_3, null),
                    new TrixTeam(3, player_4, null),
                };
            }

            GameState.ReceiveState(temp);

            GameState.SendUpdate();
        }
        else
        {
            temp = JsonConvert.DeserializeObject<TrixState>(data);

            GameState.ReceiveState(temp);
        }
    }

    public void OnPlayerJoined(DewaniaPlayer player)
    {
        TrixPlayer p = GameState.GetPlayer(player.ID);
        if (!p.IsNull())
            p.BotPlay = false;
    }

    public void OnPlayerLeft(DewaniaPlayer player)
    {
        TrixPlayer p = GameState.GetPlayer(player.ID);
        if (!p.IsNull())
            p.BotPlay = true;
    }
    #endregion
}
