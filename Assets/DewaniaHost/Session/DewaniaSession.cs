public static partial class DewaniaSession
{
    public static void CreateNewSession(string access_token, string playerId, string gmId, bool ingroup)
    {
        Debugging.Print($"create new session : {access_token}, {playerId}, {gmId}, {ingroup}");
        accessToken = access_token;
        localPlayerId = playerId;
        DewaniaGameData.SetGameData(gmId);
        inGroup = ingroup;
    }

    private static string accessToken;

    public static string AccessToken
    {
        get { return accessToken; }
    }

    private static string localPlayerId;

    public static string LocalPlayerId
    {
        get { return localPlayerId; }
    }

    private static bool inGroup;

    public static bool InGroup
    {
        get { return inGroup; }
    }

#if UNITY_EDITOR
    public static void SetLocalPlayerId(string id)
    {
        localPlayerId = id;
    }
#endif
}