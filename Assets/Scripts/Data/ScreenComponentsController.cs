using System.Collections.Generic;
using UnityEngine;

public class ScreenComponentsController : ScreenComponents
{
    #region Singlton
    public static ScreenComponentsController Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    #region Fields
    public GameObject CardPrefab;

    [SerializeField] GameObject WaitingForPlayersPanel;
    [SerializeField] GameObject LoadingDataPanel;

    [SerializeField] TolbaMenuScreenComponents TolbaMenuScript;

    public CardSpritesData CardSpritesData;

    [Tooltip("[0] down\n[1] right\n[2] up\n[3] left")]
    public List<PlayerScreenComponents> PlayersOnScreen;

    public TableSlotsScreenComponents TableSlots;

    public TrixTolba_TableSlotsScreenComponents TrixTolbaTableSlots;

    [SerializeField] private ScreenComponents[] screenComponents;

    public float DragCardSpeed;

    public Color ActiveColor;
    public Color DeactiveColor;
    #endregion

    #region Unity Fns
    private void Start()
    {
        screenComponents = FindObjectsOfType<ScreenComponents>(true);
    }
    #endregion

    #region Calls
    public void SubscribeAll()
    {
        if (Subscribed) return;
        Subscribed = true;

        foreach (var item in screenComponents)
        {
            if (!item.Subscribed)
            {
                Subscribed = false;
                item.Subscribe();
            }
        }
    }

    public void UnsubscribeAll()
    {
        Subscribed = false;

        foreach (var item in screenComponents)
        {
            if (item.Subscribed)
                item.UnSubscribe();
        }
    }
    #endregion

    #region Listeners
    void OnTrixStateStageChanged(GameStageEnum stage)
    {
        bool ActiveTolbaMenu = stage == GameStageEnum.SelectTolba && TrixController.GameState.OwnerOfLoyalId == TrixController.GameState.LocalPlayer.PlayerId;
        TolbaMenuScript.gameObject.SetActive(ActiveTolbaMenu);

        if (ActiveTolbaMenu)
            TolbaMenuScript.Initalize(TrixController.GameState.TolbaNO);
    }

    void OnWaitingForPlayers()
    {
        WaitingForPlayersPanel.SetActive(true);
    }

    void OnPlayersJoinedGame()
    {
        WaitingForPlayersPanel.SetActive(false);
    }

    void OnLoadingData()
    {
        LoadingDataPanel.SetActive(!WaitingForPlayersPanel.activeSelf);
    }

    void OnLoadedData()
    {
        LoadingDataPanel.SetActive(false);
    }

    void OnTolbaSelected(TolbaEnum tolba)
    {
        TableSlots.gameObject.SetActive(tolba != TolbaEnum.TRS);
        TrixTolbaTableSlots.gameObject.SetActive(tolba == TolbaEnum.TRS);
    }
    #endregion

    #region Fns
    public override void Subscribe()
    {
        TrixController.GameState.StageUpdated += OnTrixStateStageChanged;
        TrixController.GameState.WaitingForPlayers += OnWaitingForPlayers;
        TrixController.GameState.PlayersJoinedGame += OnPlayersJoinedGame;
        TrixController.GameState.ProccessingAndLoadingData += OnLoadingData;
        TrixController.GameState.LoadedData += OnLoadedData;
        TrixController.GameState.TolbaSelected += OnTolbaSelected;
    }

    public override void UnSubscribe()
    {
        TrixController.GameState.StageUpdated -= OnTrixStateStageChanged;
        TrixController.GameState.WaitingForPlayers -= OnWaitingForPlayers;
        TrixController.GameState.PlayersJoinedGame -= OnPlayersJoinedGame;
        TrixController.GameState.ProccessingAndLoadingData -= OnLoadingData;
        TrixController.GameState.LoadedData -= OnLoadedData;
        TrixController.GameState.TolbaSelected -= OnTolbaSelected;
    }
    #endregion
}
