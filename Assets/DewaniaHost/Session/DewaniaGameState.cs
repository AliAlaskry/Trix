using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public static partial class DewaniaSession
{
    public static partial class DewaniaGameData
    {
        public static class DewaniaGameState
        {
            public static int ID { get; set; } = 0;

            private static string Current_State_string;

            public static string LocalGameState
            {
                get { return Current_State_string; }
                set { Current_State_string = value; }
            }

            public static void Update(string state, Action<Error> onFail)
            {
                Debugging.Print("updating game state to " + state);

                JObject data = GetData(state);

                string url = NetworkInstance.Instance.Constants.BaseURL + @"/" + NetworkInstance.Instance.Constants.GamesEndpoint
                    + @"/" + GameId;
                NetworkInstance.Instance.Http.SendRequset(url, data.ToString(Formatting.Indented), HttpStateEnum.UpdateState,
                    HttpMethod.PATCH, true, null, null, onFail);
            }

            private static JObject GetData(string state)
            {
                JObject data = JObject.Parse(state);

                int Id = ID + 1;

                JObject parent = new JObject
                {
                    ["numOfUpdates"] = Id,
                    ["state"] = data
                };

                return parent;
            }
        }
    }
}