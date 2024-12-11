using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class NetworkLobbyUI : MonoBehaviour
{
    private NetworkLobby networkLobby;
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private TMP_InputField enterPlayerNameIF;
    [SerializeField] private Button enterPlayerNameBtn;

    [Header("Lobby list")]
    [SerializeField] private GameObject lobbyPanelPrefab;
    [SerializeField] private GameObject mainLobby;
    [SerializeField] private GameObject mainLobbyBG;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Button createLobbyUIBtn;
    [SerializeField] private Button getListLobby;
    [SerializeField] private Button mainMenuUIBtn;
    [SerializeField] private Button createRoomBTN;
    [SerializeField] private Button enterJoinCodeBtn;





    [Header("Create Room")]
    [SerializeField] private GameObject createRoomUI;
    [SerializeField] private TMP_InputField roomNameIF;
    [SerializeField] private TMP_InputField maxPlayerIF;
    [SerializeField] private Toggle isPrivateToggle;





    [Header("Room Panel")]
    [SerializeField] private GameObject roomPanelUI;
    [SerializeField] private GameObject playerInfoPrefab;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Button leaveRoomBtn;
    [SerializeField] private Button startGameBtn;


    [Header("Join Code Panel")]
    [SerializeField] private GameObject joinRoomByCode;
    [SerializeField] private TMP_InputField enterCodeIF;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button exitBtnJoinCode;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void ButtonAdd()
    {
        createRoomBTN.onClick.AddListener(CreateRoomBTN);
        getListLobby.onClick.AddListener(VisualizeLobbyList);
        createLobbyUIBtn.onClick.AddListener(CreateLobbyUI);
        mainMenuUIBtn.onClick.AddListener(MainLobbyUI);
        leaveRoomBtn.onClick.AddListener(ExitRoom);
        exitBtnJoinCode.onClick.AddListener(ExitJoinCodePanel);
        joinBtn.onClick.AddListener(EnterRoomByCode);
        enterJoinCodeBtn.onClick.AddListener(EnterJoinCodePanel);
        startGameBtn.onClick.AddListener(StartGame);
        enterPlayerNameBtn.onClick.AddListener(EnterName);
    }

    void Start()
    {
        networkLobby = GetComponent<NetworkLobby>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ButtonAdd();
    }

    // Update is called once per frame  
    void Update()
    {
        CheckInputMaxPlayer();
    }
    void CheckInputMaxPlayer()
    {
        int maxPlayers;
        if (int.TryParse(maxPlayerIF.text, out maxPlayers))
        {
            if (maxPlayers > 20)
            {
                maxPlayerIF.text = "20";
            }
        }
    }
    private async void CreateRoomBTN()
    {
        await networkLobby.CreateLobby(roomNameIF.text,int.Parse(maxPlayerIF.text),isPrivateToggle.isOn);
        roomNameIF.text = "";
        maxPlayerIF.text = "";
        EnterRoom();
        
        
    }
    

    private async void VisualizeLobbyList()
    {
        for(int i = 0; i < lobbyContainer.childCount; i++)
        {
            Destroy(lobbyContainer.transform.GetChild(i).gameObject);
        }
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log(queryResponse.Results.Count);
            foreach(Lobby lobby in queryResponse.Results)
            {
                GameObject newLobby = Instantiate(lobbyPanelPrefab,lobbyContainer);
                var detailText = newLobby.GetComponentsInChildren<TextMeshProUGUI>();
                detailText[0].text = lobby.Name;
                detailText[1].text = (lobby.MaxPlayers - lobby.AvailableSlots).ToString() + "/" + lobby.MaxPlayers.ToString();
                newLobby.GetComponent<Button>().onClick.AddListener(async () => {await networkLobby.JoinLobbyOnList(lobby.Id);EnterRoom();});
            }
        }catch(LobbyServiceException e){Debug.LogError(e);}   
    }

    public void VisualizePlayerInfo()
    {
        if(networkLobby.joinLobby == null) return;
        for(int i = 0; i < playerContainer.childCount; i++)
        {
            Destroy(playerContainer.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < networkLobby.joinLobby.Players.Count; i++)
        {
            Unity.Services.Lobbies.Models.Player player = networkLobby.joinLobby.Players[i];
            GameObject newPlayer = Instantiate(playerInfoPrefab,playerContainer);
            newPlayer.GetComponentInChildren<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
            if(i != 0)
            {
                newPlayer.transform.Find("IsHost").gameObject.SetActive(false);

            }
        }
        // foreach(Unity.Services.Lobbies.Models.Player player in networkLobby.joinLobby.Players)
        // {
        //     GameObject newPlayer = Instantiate(playerInfoPrefab,playerContainer);
        //     newPlayer.GetComponentInChildren<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
            
        // }
    }

    private void EnterRoom()
    {
        roomNameText.text = networkLobby.joinLobby.Name;
        roomCodeText.text = networkLobby.joinLobby.LobbyCode;
        roomPanelUI.SetActive(true); 
        createRoomUI.SetActive(false);
        mainLobby.SetActive(false);
        mainMenuUIBtn.transform.gameObject.SetActive(false);
        VisualizePlayerInfo();
    }

    private void CreateLobbyUI()
    {
        createRoomUI.SetActive(true);
        roomPanelUI.SetActive(false);
        mainLobby.SetActive(false);
        mainMenuUIBtn.transform.gameObject.SetActive(true);
    }

    private void MainLobbyUI()
    {
        createRoomUI.SetActive(false);
        roomPanelUI.SetActive(false);
        mainLobby.SetActive(true);
        mainMenuUIBtn.transform.gameObject.SetActive(false);
    }

    private async void ExitRoom()
    {
        await networkLobby.LeaveRoom();
        MainLobbyUI();
    }

    private void ExitJoinCodePanel()
    {
        joinRoomByCode.SetActive(false);
    }
    private void EnterJoinCodePanel()
    {
        joinRoomByCode.SetActive(true);
    }

    private async void EnterRoomByCode()
    {
        await networkLobby.JoinLobbyByCode(enterCodeIF.text);
        enterCodeIF.text = "";
        ExitJoinCodePanel();
        EnterRoom();
    }
    private async void StartGame()
    {
        await networkLobby.StartGame();
    }

    private async void EnterName()
    {
        if(enterPlayerNameIF.text.IsNullOrEmpty()) return;
        await networkLobby.AuthenticationId(enterPlayerNameIF.text);
        enterPlayerNameIF.text="";
        mainMenu.SetActive(false);
        mainLobby.SetActive(true);
        mainLobbyBG.SetActive(true);
    }
    

}
