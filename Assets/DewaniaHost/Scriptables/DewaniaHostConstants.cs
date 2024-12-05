using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "DewaniaHostConstants", menuName = "DewaniaHost/Constants", order = 0)]
public class DewaniaHostConstants : ScriptableObject
{
    [Header("Host Main Data")]
    public string Scheme;
    public string URL;
    public string BaseURL => Scheme + "://" + URL;
    public string WebSocketEndpoint;
    public List<string> MainHttpHeaders_Keys;
    public List<string> MainHttpHeaders_Values;
    public string GamesEndpoint;
    public string ChatsEndpoint;
    public int HttpTimeout;
    public int HttpRedirectLimit;
    public int RequiredPlayers;

    [Header("Game Display Data")]
    public Avatars Avatars;
    public Frames Frames;
    public Names Names;

    [Header("Test Data")]
    public string access_token_Test;
    public string playerId_Test;
    public string gameId_Test;
    public bool InGroup_Test;

    public int GetRandomAvatarIndex()
    {
        return Random.Range(0, Avatars.Data.Count);
    }

    public Sprite GetAvatar(int index)
    {
        return Avatars.Data[index];
    }

    public int GetRandomFrameIndex()
    {
        return Random.Range(0, Frames.Data.Count);
    }

    public Sprite GetFrame(int index)
    {
        return Frames.Data[index];
    }

    public string GetRandomName()
    {
        return Names.Data[Random.Range(0, Names.Data.Count)];
    }

    public string GetName(int index)
    {
        return Names.Data[index];
    }
}
