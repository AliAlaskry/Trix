using System;
using System.Collections.Generic;

public static partial class DewaniaSession
{
    public static partial class DewaniaGameData
    {
        private static string gameId;

        public static string GameId
        {
            get { return gameId; }
        }

        private static List<DewaniaPlayer> players;

        public static List<DewaniaPlayer> Players
        {
            get
            {
                return players;
            }
        }

        public static DewaniaPlayer LocalPlayer
        {
            get
            {
                if (Players == null)
                    return null;

                return players.Find(o => o.ID == localPlayerId);
            }
        }

        public static DewaniaPlayer GetPlayer(string id)
        {
            if (players == null) return null;
            foreach (DewaniaPlayer player in players)
            {
                if (player.ID == id) return player;
            }
            return null;
        }

        public static void SetGameData(string Id)
        {
            gameId = Id;
        }

        public static void AddRemainsPlayersAsBots()
        {
            for (int i = players.Count; i < NetworkInstance.Instance.Constants.RequiredPlayers; i++)
            {
                string id = Guid.NewGuid().ToString();
                Debugging.Print($"add bot = {id}");
                Players.Add(new DewaniaPlayer(true, id, NetworkInstance.Instance.Constants.GetRandomName(), 
                    NetworkInstance.Instance.Constants.GetRandomAvatarIndex().ToString(), "", 0, 0, false));
            }
        }

        public static bool TryGetBot(out DewaniaPlayer player)
        {
            player = null;

            if (players == null)
            {
                return false;
            }

            foreach (DewaniaPlayer p in players)
            {
                if (p.IsBot)
                {
                    player = p;
                    return true;
                }
            }

            return false;
        }

        public static void Reset()
        {
            players = new List<DewaniaPlayer>();
            ID = 0;
        }
    }
}