using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TestingWindow : EditorWindow
{
    // Add a menu item to open the window
    [MenuItem("Trix/Test Game (Console Logs)")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make a new one.
        GetWindow<TestingWindow>("Test Game (Console Logs)");
    }

    private int index = 0;
    private readonly int[] options = { 1, 2, 4, 8, 16 };

    private bool toggleLastValue;
    private bool IsTeams;

    private void OnGUI()
    {
        GUILayout.Space(20);
        // Add a label
        GUILayout.Label("All functionalities of trix game \ncan be done with one mouse click on button \nwithout any visualization", EditorStyles.boldLabel);
        GUILayout.Space(20);

        toggleLastValue = EditorGUILayout.Toggle(new GUIContent { text = "Teams Or Individual" }, toggleLastValue);
        IsTeams = toggleLastValue;

        index = EditorGUILayout.Popup("Select Tolba", index, new string[] { "Lotoch", "OldKoba", "Girls", "Denary", "Trix" });
        tolba = options[index >=0 && index < options.Length ? index : 0];

        // Buttons
        if (GUILayout.Button("Join Game", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            JoinGame();
        }

        if (GUILayout.Button("Players Joined Game", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            PlayersJoinedGame();
        }

        if (GUILayout.Button("Select Tolba", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            SelectTolba();
        }

        if (GUILayout.Button("All Players Ready To Play", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            AllPlayersReadyToPlay();
        }

        if (GUILayout.Button("Drop Card", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            DropCard();
        }

        if (GUILayout.Button("Debug", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            Print();
        }
    }


    #region Test
    [SerializeField] int tolba;
    TrixState GameState
    {
        get
        {
            return TrixController.GameState;
        }
        set
        {
            TrixController.GameState = value;
            ScreenComponentsController.Instance?.SubscribeAll();
        }
    }
    void JoinGame()
    {
        DewaniaSession.SetLocalPlayerId("1");

        ScreenComponentsController.Instance?.UnsubscribeAll();

        GameState = new TrixState(IsTeams);
        GameState.Teams = new TrixTeam[4]
        {
            new TrixTeam(0, new TrixPlayer("1", 0, "", null, null, 0, 0, false, false, false), null),
            new TrixTeam(1, new TrixPlayer("2", 1, "", null, null, 0, 1, false, false, false), null),
            new TrixTeam(2, new TrixPlayer("3", 2, "", null, null, 0, 2, false, false, false), null),
            new TrixTeam(3, new TrixPlayer("4", 3, "", null, null, 0, 3, false, false, false), null),
        };

        ScreenComponentsController.Instance?.SubscribeAll();
        GameState.ReceiveState(GameState);
    }

    void PlayersJoinedGame()
    {
        foreach (var player in GameState.Players)
        {
            player.JoinedGame = true;
        }
    
        GameState.ReceiveState(GameState);
    }

    void SelectTolba()
    {
        GameState.OnSelectNextTolba(tolba);
    }

    void AllPlayersReadyToPlay()
    {
        if (GameState.Stage == GameStageEnum.DoublingCards)
        {
            foreach (var player in GameState.Players)
            {
                if (player.HasOldKoba(out Card koba))
                {
                    GameState.ClickCard(player, koba);
                }

                if (player.HasGirls(out List<Card> girls))
                {
                    girls.ForEach(girl => GameState.ClickCard(player, girl));
                }
            }
        }

        foreach (var player in GameState.Players)
            GameState.ClickReadyToPlay(player);
    }

    void DropCard()
    {
        TrixPlayer player = GameState.CurrentTurnPlayer;
        List<Card> canBeDropped = player.CanBeDroppedCards;
        if (canBeDropped.Count > 0)
        {
            Debug.Log($"can be dropped cards for \nplayer: {player.PlayerId}\n are: {JsonConvert.SerializeObject(canBeDropped)}");
            GameState.ClickCard(player, canBeDropped[Random.Range(0, canBeDropped.Count)], true);
        }
        else
        {
            Debug.LogWarning("no can dropped cards for " + player.PlayerId);
            if (TrixController.GameState.Tolba == TolbaEnum.TRS)
            {
                int order = TrixController.GameState.CurrentTurnPlayer.Order + 1;
                order %= TrixController.GameState.Players.Length;
                //todo: remove comment => TrixController.GameState.Turn = TrixController.GameState.GetPlayer(order).PlayerId;

                TrixController.GameState.SendUpdate();
            }
        }
    }

    void Print()
    {
        Debug.Log("state is\n" + GameState.ToString());
    }
    #endregion

}
