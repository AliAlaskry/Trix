using LessonEra;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class TrixTeam 
{
    #region Constructor
    public TrixTeam(int order, TrixPlayer player_1, TrixPlayer player_2)
    {
        Order = order;

        Player_1 = player_1;
        Player_2 = player_2;

        IsTeam = !player_2.IsNull();
        TeamPoints = 0;
    }
    #endregion

    #region Fields
    [JsonProperty("Order")]
    public readonly int Order;
    [JsonProperty("Player 1")]
    public readonly TrixPlayer Player_1;
    [JsonProperty("Player 2")]
    public readonly TrixPlayer Player_2;
    [JsonProperty("Is Team")]
    public readonly bool IsTeam;

    [JsonProperty("Team Points")]
    private int teamPoints;
    #endregion

    #region Props
    [JsonIgnore]
    public int TeamPoints { get => teamPoints; private set => teamPoints = value; }

    [JsonIgnore]
    public int Points 
    {
        get
        {
            if (IsTeam)
            {
                return Player_1.Points + Player_2.Points + teamPoints;
            }
            
            return Player_1.Points;
        }
        set
        {
            if (IsTeam)
            {
                teamPoints = value;
                PointsUpdated?.Invoke(teamPoints);
            }
            else
                Player_1.Points = value;
        }
    }

    [JsonIgnore]
    public TrixPlayer[] Players
    {
        get
        {
            if (IsTeam)
                return new TrixPlayer[] { Player_1, Player_2 };

            return new TrixPlayer[] { Player_1 };
        }
    }
    #endregion

    #region Actions
    [JsonIgnore]
    public Action<int> PointsUpdated;
    #endregion
}
