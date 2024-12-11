using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkInput : NetworkBehaviour
{
    private TestMap testMap;
    [SerializeField] private Button generateButton;
    private bool isHideGenerateButton = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        testMap = GetComponent<TestMap>();
        generateButton.onClick.AddListener(OnGenerateButtonClickedServerRpc);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

    }
    void Update()
    {
        ClientBehaviour();
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SceneManager.LoadScene("LoginScene");  // Chuyển về scene login
        }
    }

    // Update is called once per frame
    [ServerRpc]
    void OnGenerateButtonClickedServerRpc()
    {
        testMap.MapGenerator();
    }


    [ClientRpc]
    protected void RequestSeedToClientRpc(int setSeed)
    {
        
        TestMap clientTestMap = GetComponent<TestMap>();
        if(IsHost || clientTestMap.seed == setSeed) return;
        clientTestMap.seed = setSeed;
        clientTestMap.setSeed = true;
        clientTestMap.MapGenerator();
    }

    void ClientBehaviour()
    {
        UIClientRpc();
        RequestSeedToClientRpc(testMap.seed);
    }
    [ClientRpc]
    void UIClientRpc()
    {
        if(IsHost || isHideGenerateButton) return;
        isHideGenerateButton = true;
        generateButton.gameObject.SetActive(false);
    }
    
}
