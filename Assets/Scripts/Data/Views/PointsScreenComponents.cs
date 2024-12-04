using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PointsScreenComponents : ScreenComponents
{
    #region Fields
    [Tooltip("[0] down\n[1] right\n[2] up\n[3] left\nor [0] for first team and [1] for second one")]
    [SerializeField] PlayerPointsDisplayItem[] PlayersPoints;
    #endregion

    #region Inherited Fns
    public override void Subscribe()
    {
        base.Subscribe();

        if (!GameState.PlayersAdded())
        {
            Subscribed = false;
            return;
        }

        int order = GameState.LocalPlayer.Order;
        for(int i = 0; i < PlayersPoints.Length; i++)
        {
            PlayersPoints[i].Initialize(GameState.GetPlayer(order).PlayerId);

            order++;
            order %= PlayersPoints.Length;
        }
    }

    public override void UnSubscribe()
    {
        base.UnSubscribe();

        for (int i = 0; i < PlayersPoints.Length; i++)
        {
            PlayersPoints[i].UnInitialize();
        }
    }
    #endregion
}

[Serializable]
public class PlayerPointsDisplayItem
{
    #region Fields
    public string PlayerId;
    public GameObject PointsContainer;
    #endregion

    #region Props
    TrixState GameState => TrixController.GameState;
    TrixPlayer Player => GameState.GetPlayer(PlayerId);
    TrixTeam PlayerTeam => GameState.GetTeam(Player.TeamOrder);
    int Points => Player.Points;
    int TeamPoints => PlayerTeam.TeamPoints;
    #endregion

    #region Fns
    public void Initialize(string playerId)
    {
        PlayerId = playerId;

        PointsUpdated(Points, TeamPoints);
        Subscribe();
    }

    void Subscribe()
    {
        Player.PointsUpdated += (points) =>
        {
            PointsUpdated(points, TeamPoints);
        };

        if (GameState.IsTeams)
        {
            PlayerTeam.PointsUpdated += (points) =>
            {
                PointsUpdated(Points, points);
            };
        }
    }

    void PointsUpdated(int points, int teamPoints)
    {
        PointsContainer.GetComponentInChildren<TMP_Text>().text = points.ToString();

        if (teamPoints != 0) PointsContainer.GetComponentInChildren<TMP_Text>().text += "+" + teamPoints;
    }

    public void UnInitialize()
    {
        PointsUpdated(0, 0);
        Unsubscribe();
    }

    void Unsubscribe()
    {
        Player.PointsUpdated -= (points) =>
        {
            PointsUpdated(points, TeamPoints);
        };

        if (GameState.IsTeams)
        {
            PlayerTeam.PointsUpdated -= (points) =>
            {
                PointsUpdated(Points, points);
            };
        }
    }
    #endregion
}