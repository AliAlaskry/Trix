using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkInstance))]
public class DisplaySessionEditor : Editor
{
    GUIStyle headerStyle;

    private void OnEnable()
    {
        headerStyle = new GUIStyle();
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 13;
        headerStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DisplayDewaniaHostController();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        DisplaySession();
    }

    void DisplayDewaniaHostController()
    {
        EditorGUILayout.HelpBox(" Current Host (ReadOnly Data).", MessageType.Info);

        EditorGUILayout.LabelField("DewaniaHost", headerStyle);
        EditorGUILayout.LabelField("State : " + DewaniaHostController.State);
        EditorGUILayout.LabelField("Current Operation : " + DewaniaHostController.CurrentOperation);
        EditorGUILayout.LabelField("Error Type Index : " + DewaniaHostController.CurrentErrorType);
    }

    void DisplaySession()
    {
        EditorGUILayout.HelpBox(" Current Session (ReadOnly Data).\n Don't press reset while playing.", MessageType.Info);

        EditorGUILayout.LabelField("Session", headerStyle);
        EditorGUILayout.LabelField("Received Access Token : " + DewaniaSession.AccessToken);
        EditorGUILayout.LabelField("Received Player Id : " + DewaniaSession.LocalPlayerId);
        EditorGUILayout.LabelField("Received Game Id : " + DewaniaSession.DewaniaGameData.GameId);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("State ID : ", DewaniaSession.DewaniaGameData.ID.ToString());

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Players", headerStyle);
        if (DewaniaSession.DewaniaGameData.Players != null)
        {
            for (int i = 0, count = DewaniaSession.DewaniaGameData.Players.Count; i < count; i++)
            {
                EditorGUILayout.LabelField("Player " + i, headerStyle);
                DewaniaPlayer current = DewaniaSession.DewaniaGameData.Players[i];
                if (current != null)
                {
                    DisplayPlayer(current);
                    EditorGUILayout.Space();
                }
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Game State");
        EditorGUILayout.TextArea(DewaniaSession.DewaniaGameData.LocalGameState == null ? "" : DewaniaSession.DewaniaGameData.LocalGameState.ToString());

        EditorGUILayout.Space();

        if (NetworkInstance.Instance && NetworkInstance.Instance.IsDebugMode)
        {
            if (GUILayout.Button("Reset"))
            {
                DewaniaSession.DewaniaGameData.Reset();
            }
        }
    }

    void DisplayPlayer(DewaniaPlayer player)
    {
        EditorGUILayout.LabelField("ID : " + player.ID);
        EditorGUILayout.LabelField("Is Local : " + player.IsLocal.ToString());
        EditorGUILayout.LabelField("Is online : " + player.IsOnline().ToString());
        EditorGUILayout.LabelField("Is Bot : " + player.IsBot.ToString());
        EditorGUILayout.LabelField("Name : " + player.Name);
        EditorGUILayout.LabelField("Picture : " + player.Pic);
        EditorGUILayout.LabelField("Level : " + player.Level);
        EditorGUILayout.LabelField("Points : " + player.Points);
    }

    [MenuItem("DewaniaHost/Setup", priority = 1, validate = false)]
    static void Setup()
    {
        string SetupObjectName = "DewaniaHostConstants";
        UnityEngine.Object setupObj = Resources.Load(SetupObjectName);
        if (setupObj != null)
        {
            Selection.activeObject = null;
            Selection.activeObject = setupObj;
        }
        else
        {
            Debugging.Warn("Setup File Not Found!!");
        }
    }
}
