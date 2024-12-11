using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WebSocketSharp;

public class PlayerUI : NetworkBehaviour
{
    [Header("PlayerUI")]
    private Transform playerUI;
    private Button settingBtn;

    [Header("SettingUI")]
    private Transform settingUI;
    private Button resumeBtn;
    private Button backToMainMenu;

    [Header("EndingUI")]
    private Transform endingUI;
    private Button backToMainMenuEnding;
    private TextMeshProUGUI whoWin;

    [Header("Guide")]
    private Transform playerModeUI;
    private Transform editModeUI;
    private Transform playerReadyUI;
    private TextMeshProUGUI isPlayerReady;
    [Header("Player Name")]
    public TextMeshProUGUI playerName;
    private List<TextMeshProUGUI> allPlayerName;
    string setName;
    NetworkVariable<NetString> getName = new NetworkVariable<NetString>(new NetString(""),
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    private Player playerCom;
    GameManager gameManager;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!IsOwner) return;
        playerName = GetComponentInChildren<TextMeshProUGUI>();
        playerCom = GetComponent<Player>();
        playerCom.OnEditStateChanged += ModeSwitch;
        //getName.OnValueChanged += HandleChangeName;
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsOwner) return;
        playerName = GetComponentInChildren<TextMeshProUGUI>();
        playerCom = GetComponent<Player>();
        playerCom.OnEditStateChanged += ModeSwitch;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        //SetPlayerName();
        GetTransform();
        if(Input.GetKeyDown(KeyCode.Escape)) SettingUIShow();
        IsGameEnd();
        PlayerReady();
        SeeAllPlayerName();
    }
    
    void HandleChangeName(NetString oldValue, NetString newValue)
    {
        Debug.Log("Old name:" + oldValue.name);
        Debug.Log("New name:" + newValue.name);
        ApplyPlayerInforServerRpc();
    }
    void SetPlayerName()
    {
        //if(getName.Value.name.IsNullOrEmpty())
        if(!string.IsNullOrEmpty(getName.Value)) return;
        try
        {
            NetworkPlayerInformation networkPlayer = GameObject.Find("PlayerInformation").GetComponent<NetworkPlayerInformation>();
            getName.Value = new NetString(networkPlayer.playerName);
            setName = networkPlayer.playerName;
        }catch{}
    }

    

    [ServerRpc(RequireOwnership = false)]
    void ApplyPlayerInforServerRpc()
    {
        Debug.Log("Change");
        // Truyền NetworkObjectId để áp dụng tên người chơi trên client
        NetworkObject networkObject = GetComponent<NetworkObject>();
        ApplyPlayerNameClientRpc(networkObject.NetworkObjectId);
    }

    // ClientRpc để cập nhật tên người chơi trên các client khác
    [ClientRpc]
    void ApplyPlayerNameClientRpc(ulong playerObjId)
    {
        

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjId, out var obj))
        {
            PlayerUI playerUI = obj.GetComponent<PlayerUI>();
            if(playerUI == null)
            {
                Debug.LogError("PlayerUI is null");
            }

            if(playerUI.playerName == null)
            {
                Debug.LogError("PlayerName is null");
            }
            if (playerUI != null && playerUI.playerName != null)
            {
                playerUI.playerName.text = getName.Value;
                Debug.Log("Player name updated: " + getName.Value);
            }
        }
        
    }
    void SeeAllPlayerName()
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("PlayerName");
        foreach(var allName in all)
        {
            allName.transform.rotation = Quaternion.LookRotation(transform.forward);
        }
        
    }

    void SettingUIShow()
    {
        if(!IsOwner) return;
        if(playerCom.isEdit == true) playerCom.SetIsEdit();
        playerUI.gameObject.SetActive(false);                   
        settingUI.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerCom.isSettingUI = true;
    }

    void IsGameEnd()
    {
        if(gameManager == null) return;
        if(gameManager.isGameEnd)
        {
            playerUI.gameObject.SetActive(false);
            settingUI.gameObject.SetActive(false);
            endingUI.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if(gameManager.win == 1)
            {
                if(OwnerClientId == 0)
                {
                    whoWin.text = "You Win";
                }else
                {
                    whoWin.text = "You Lose";
                }
            }
            if(gameManager.win == 2)
            {
                if(OwnerClientId == 0)
                {
                    whoWin.text = "You Lose";
                }else
                {   
                    whoWin.text = "You Win";
                }
            }
        }
    }
    void PlayerUIShow()
    {
        if(!IsOwner) return;
        playerUI.gameObject.SetActive(true);
        settingUI.gameObject.SetActive(false);
        playerCom.isSettingUI = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }
    void BackToMainMenu()
    {
        if(!IsOwner) return;
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("LoginScene");
    }   

    void ModeSwitch()
    {
        if(playerCom.isEdit)
        {
            editModeUI.gameObject.SetActive(true);
            playerModeUI.gameObject.SetActive(false);
        }else
        {
            editModeUI.gameObject.SetActive(false);
            playerModeUI.gameObject.SetActive(true);
        }
    }

    void PlayerReady()
    {
        if(!Input.GetKeyDown(KeyCode.R)) return;
        isPlayerReady.text = "You are ready";
    }

    void GetTransform()
    {
        if(playerUI != null) return;
        try
        {
            playerUI = GameObject.Find("PlayerUI").GetComponent<Transform>();
            settingUI = GameObject.Find("SettingUI").GetComponent<Transform>();
            settingBtn = GameObject.Find("SettingBtn").GetComponent<Button>();
            resumeBtn = GameObject.Find("ResumeBtn").GetComponent<Button>();
            backToMainMenu = GameObject.Find("ReturnMainMenu").GetComponent<Button>();
            endingUI = GameObject.Find("EndingUI").transform;
            backToMainMenuEnding = GameObject.Find("ReturnMainMenuEnding").GetComponent<Button>();
            whoWin = GameObject.Find("WhoWin").GetComponent<TextMeshProUGUI>();
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            playerModeUI = GameObject.Find("PlayerModeUI").GetComponent<Transform>();
            editModeUI = GameObject.Find("EditModeUI").GetComponent<Transform>();
            playerReadyUI = GameObject.Find("PlayerReadyUI").GetComponent<Transform>();
            isPlayerReady = playerReadyUI.Find("IsPlayerReady").GetComponent<TextMeshProUGUI>();

            backToMainMenu.onClick.AddListener(BackToMainMenu);
            backToMainMenuEnding.onClick.AddListener(BackToMainMenu);
            resumeBtn.onClick.AddListener(PlayerUIShow);
            settingBtn.onClick.AddListener(SettingUIShow);
            
            endingUI.gameObject.SetActive(false);

            ModeSwitch();
            PlayerUIShow();

        }catch{}
    }
    
}

public struct NetString : INetworkSerializable, System.IEquatable<NetString>
{
    public string name;

    public NetString(string value)
    {
        name = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
    }

    public override bool Equals(object obj)
    {
        if (obj is NetString other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(NetString other)
    {
        return name == other.name;
    }

    public override int GetHashCode()
    {
        return name?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return name;
    }

    public static implicit operator string(NetString netString)
    {
        return netString.name;
    }

    public static implicit operator NetString(string value)
    {
        return new NetString(value);
    }
}
