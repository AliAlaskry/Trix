using UnityEngine;

interface INetworkInstance
{
    static NetworkInstance singltons { get; }
    DewaniaHostConstants Constants { get; }
    HttpRequests Http { get; }
}

public class NetworkInstance : MonoBehaviour, INetworkInstance
{
    #region Singlton
    static NetworkInstance instance;
    public static NetworkInstance Instance { get => instance; }
    #endregion

    #region Variables
    [SerializeField] private bool isDebugMode = true;
    #endregion

    #region Props
    public bool IsDebugMode => isDebugMode;
    #endregion

    #region Unity Fns
    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);

        instance = this;

        DontDestroyOnLoad(gameObject);

        http = new GameObject("httpRequest").AddComponent<HttpRequests>();
        http.gameObject.transform.parent = transform;
        hideFlags = HideFlags.HideInHierarchy;
    }

    private void OnDestroy()
    {
        DewaniaHostController.Dispose();
    }
    #endregion

    #region Props

    [SerializeField] DewaniaHostConstants constants;
    public DewaniaHostConstants Constants => constants;

    HttpRequests http;
    public HttpRequests Http => http;
    #endregion
}
