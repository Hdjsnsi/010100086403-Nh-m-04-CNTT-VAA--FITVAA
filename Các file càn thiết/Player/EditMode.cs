using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditMode : NetworkBehaviour
{
    //[SerializeField] private List<GameObject> selectedObject;
    [SerializeField] private List<SoldierCost> selectedSoldier;
    private GameObject ghostObject;
    
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask ground;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int playerMoney;
    private TextMeshProUGUI moneyText;
    private ClientReady clientReady;
    private Player playerCom;
    private int selectedObjectInt;

    
    

    void Start()
    {
        selectedObjectInt = 0;
        playerCom = GetComponent<Player>();
        clientReady = GetComponent<ClientReady>();
        playerMoney = 1000;
        playerCom.OnEditStateChanged += SpawnGhostObject;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
    }

    


    void Update()
    {
        if (!IsOwner) return;
        PlacementInput();
        GetGameManager();
        MoneyUpdateUI();
        NumberBeenSelected();

    }

    void SpawnGhostObject()
    {
        if (playerCom.isEdit) 
        {
            if (ghostObject != null) Destroy(ghostObject);
            ghostObject = Instantiate(selectedSoldier[selectedObjectInt].selectObject);
            ghostObject = GhostObject(ghostObject);
            ghostObject.SetActive(false); // Đảm bảo nó không hiển thị ngay lập tức
        }else
        {
            Destroy(ghostObject); // Xóa ghostObject khi tắt chế độ chỉnh sửa
        }
    }

    void PlacementInput()
    {
        if (ghostObject == null) return; // Kiểm tra nếu ghostObject chưa được khởi tạo
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
        {
            ghostObject.SetActive(true);
            PredictLocation(hit);
        }
        else
        {
            ghostObject.SetActive(false);
        }
    }

    GameObject GhostObject(GameObject baseObject)
    {
        GameObject ghostObject = baseObject;

        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer ghostRenderer in renderers)
        {
            Collider collider = ghostRenderer.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Material ghostMaterial = ghostRenderer.material;
            ghostMaterial.SetFloat("_Surface", 1); // 1 cho Transparent
            Color ghostColor = ghostMaterial.color;
            ghostColor.a = 0.5f;
            ghostMaterial.color = ghostColor;
        }
        ghostObject.GetComponent<Character>().enabled = false;
        ghostObject.GetComponent<HeathSystem>().enabled = false;
        ghostObject.GetComponent<Weapon>().enabled = false;


        return ghostObject;
    }

    void PredictLocation(RaycastHit hit)
    {
        if (ghostObject != null)
        {
            ghostObject.transform.position = hit.point + hit.normal;
        }
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;  

        if(playerMoney >= selectedSoldier[selectedObjectInt].objectCost)
        {
            SpawnSoldier(hit.point + new Vector3(0, 1, 0));
            playerMoney -= selectedSoldier[selectedObjectInt].objectCost;
        }
    }
    

    void SpawnSoldier(Vector3 position)
    {
        int gameTag ;
        Quaternion rotation;

        if (OwnerClientId == 0) // Host
        {
            gameTag = 1;
            rotation = Quaternion.LookRotation(Vector3.right);
            Debug.Log("Host");
        }
        else // Client
        {
            gameTag = 2;
            rotation = Quaternion.LookRotation(Vector3.left);
            Debug.Log("Client");
        }
        // Instantiate đối tượng trên server
        GameObject soldierObject = Instantiate(selectedSoldier[selectedObjectInt].selectObject, position,rotation);
        SpawnerData spawnerData = new SpawnerData(selectedObjectInt,gameTag,soldierObject.transform.position,soldierObject.transform.rotation);
        clientReady.GetLocalSpawnData(spawnerData,soldierObject);

    }

    
    void MoneyUpdateUI()
    {
        if(moneyText != null && int.TryParse(moneyText.text,out int parseMoney) && parseMoney != playerMoney)
        {
            moneyText.text = playerMoney.ToString();
        }
        
    }
    void GetGameManager()
    {
        if(gameManager != null) return;
        try
        {
            moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }catch{}
    }

    void NumberBeenSelected()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedObjectInt = 0;
            playerCom.OnChangeEdit();
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedObjectInt = 1;
            playerCom.OnChangeEdit();

        }
    }

}

[System.Serializable]
public struct SoldierCost
{
    public GameObject selectObject;
    public int objectCost;
    public SoldierCost(GameObject selectObject_,int objectCost_)
    {
        this.selectObject = selectObject_;
        this.objectCost = objectCost_;
    }
}