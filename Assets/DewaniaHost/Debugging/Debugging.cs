using Newtonsoft.Json;
using UnityEngine;

public static class Debugging 
{
    #region Variables
    public static bool IsDebugMode => NetworkInstance.Instance.IsDebugMode;
    #endregion

    #region Fns
    public static void Print(string message)
    {
        if (!IsDebugMode) return;

        Debug.Log(message);
    }

    public static void Print(string message, params object[] obj)
    {
        if (!IsDebugMode) return;

        string str = "";
        foreach (object obj2 in obj)
        {
            str += "\nobj : ";
            str += JsonConvert.SerializeObject(obj2, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
            str += "\n";
        }
        Debug.Log(message + " : " + str);
    }

    public static void Warn(string message, params object[] obj)
    {
        if (!IsDebugMode) return;

        string str = "";
        foreach (object obj2 in obj)
        {
            str += "\nobj : ";
            str += JsonConvert.SerializeObject(obj2, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
            str += "\n";
        }
        Debug.LogWarning(message + " : " + str);
    }

    public static void Error(string message, params object[] obj)
    {
        if (!IsDebugMode) return;

        string str = "";
        foreach (object obj2 in obj)
        {
            str += "\nobj : ";
            str += JsonConvert.SerializeObject(obj2, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
            str += "\n";
        }
        Debug.LogError(message + " : " + str);
    }
    #endregion
}
