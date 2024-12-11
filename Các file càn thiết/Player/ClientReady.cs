using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClientReady : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerIsReady;
    [SerializeField] private NetworkList<SpawnerData> spawnerDatas = new NetworkList<SpawnerData>(new List<SpawnerData>(),
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    [SerializeField] private List<GameObject> selectedObject;
    [SerializeField]private List<GameObject> localSpawnObject;
    bool isReady;
    bool allPlayerReady = false;
    GameManager gameManager;
    void Start()
    {
        isReady = false;
        playerIsReady = new Dictionary<ulong, bool>();   
    }

    // Update is called once per frame
    void Update()
    {
        GetGameManager();
        if(Input.GetKeyDown(KeyCode.R))
        {
            PlayerIsReadyServerRpc();
        }
    }

    public override void  OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spawnerDatas.OnListChanged += (NetworkListEvent<SpawnerData> changeEvent) =>
        {
            
        };
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerIsReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if(isReady) return;
        playerIsReady[serverRpcParams.Receive.SenderClientId] = true;
        allPlayerReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!playerIsReady.ContainsKey(clientId) || !playerIsReady[clientId])
            {
                allPlayerReady = false;
                break;
            }
        }
        if(allPlayerReady == true)
        {
            DestroyLocalObjectClientRpc();
            if(!IsServer) return;
            StartGameAndShowServerRpc();
            isReady = true;
            gameManager.isGameStart = true;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void StartGameAndShowServerRpc()
    {
        foreach(var spawner in spawnerDatas)
        {
            GameObject gameObject = Instantiate(selectedObject[spawner.objectSelection],spawner.position,spawner.rotation);
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Spawn();
            SetTagClientRpc(networkObject.NetworkObjectId,spawner.gameTag);
        }
    }
    [ClientRpc]
    void DestroyLocalObjectClientRpc()
    {
        foreach(GameObject localObject in localSpawnObject)
        {
            Destroy(localObject);
        }
        localSpawnObject.Clear();
    }
    public void GetLocalSpawnData(SpawnerData data,GameObject objectData)
    {
        spawnerDatas.Add(data);
        localSpawnObject.Add(objectData);
    }

    [ClientRpc]
    public void SetTagClientRpc(ulong networkObjectId, int gameTagInt)
    {
        string gameTag = "";
        string enemyTag = "";
        if(gameTagInt == 1)
        { 
            gameTag = "RedTeam";
            enemyTag = "BlueTeam";
        }else if(gameTagInt == 2)
        {
            gameTag = "BlueTeam";
            enemyTag = "RedTeam";
        }
        
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var obj))
        {
            var healthSystem = obj.GetComponent<HeathSystem>();
            if (healthSystem != null)
            {
                healthSystem.targetTag.gameObject.tag = gameTag;
            }

            var character = obj.GetComponent<Character>();
            if (character != null)
            {
                character.enemyTag = enemyTag;
            }
            
            var marker = obj.transform.Find("Marker").GetComponent<MeshRenderer>().material;
            if(marker != null)
            {
                if(gameTagInt == 1)
                {
                    marker.color = Color.red;
                }else
                {
                    marker.color = Color.blue;
                }
            }
        } 
    }
    
    
    void GetGameManager()
    {
        if(gameManager != null) return;
        try
        {
            
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }catch{}
    }
}
