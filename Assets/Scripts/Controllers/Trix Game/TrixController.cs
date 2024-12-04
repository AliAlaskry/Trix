using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TrixController : MonoBehaviour, IReceiveGameStateCallbacks, IOnConnectedCallbacks, IOnPlayerLeftCallback, IOnPlayerJoinedCallback
{
    #region Variables
    [SerializeReference] public static TrixState GameState;
    public static bool CombineTrix = false;
    #endregion

    #region Unity Fns
    private void Start()
    {
        GetWebviewHeaders(out string access_token, out string localPlayerId, out string gameId, out bool ingroup, out CombineTrix);
        //DewaniaHostController.Setup(access_token, localPlayerId, gameId, ingroup);
        //DewaniaHostController.Initialize();
        //DewaniaHostController.Connect();

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
        }
        else 
        {
            temp = JsonConvert.DeserializeObject<TrixState>(data);
        }

        if (!GameState.PlayersAdded())
        {
            AddPlayerData(temp);
        }

        GameState.RecieveState(temp);

        if(isNewGame)
            GameState.SendUpdate();

        ScreenComponentsController.Instance?.SubscribeAll();
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
