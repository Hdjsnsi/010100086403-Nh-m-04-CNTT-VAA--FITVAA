using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Net;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerUI : NetworkBehaviour
{
    private Button serverBtn;
    private Button hostBtn;
    private Button clientBtn;
    [SerializeField]private TMP_InputField inputField;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GetComponent();
        serverBtn.onClick.AddListener(() => {NetworkManager.Singleton.StartServer();});
        hostBtn.onClick.AddListener(() => {NetworkManager.Singleton.StartHost();LoadMainScene();});
        clientBtn.onClick.AddListener(ClientListener);
    }
    
    void Update()
    {

    }
    private void GetComponent()
    {
        Transform[] allObject = GetComponentsInChildren<Transform>();
        serverBtn = allObject.Where(a => a.transform.gameObject.name == "Server").FirstOrDefault().GetComponent<Button>();
        hostBtn = allObject.Where(a => a.transform.gameObject.name == "Host").FirstOrDefault().GetComponent<Button>();
        clientBtn = allObject.Where(a => a.transform.gameObject.name == "Client").FirstOrDefault().GetComponent<Button>();
    }

    private void LoadMainScene()
    {
        
        NetworkManager.Singleton.SceneManager.LoadScene("MainGame",LoadSceneMode.Single);
        
    }
    
    
    private void ClientListener()
    {
        
        string enter = inputField.text;
            
        if(string.IsNullOrEmpty(enter))
        {
            Debug.Log("Null");
            return;
        }

        if(IPAddress.TryParse(enter,out IPAddress address))
        {
            var network = NetworkManager.Singleton.GetComponent<UnityTransport>();
            network.SetConnectionData(enter,7777);
            NetworkManager.Singleton.StartClient();
        }else
        {
            Debug.Log("Đây ko phải 1 dạng IP");
            inputField.text = "";
        }
            
        
        
        
    }
    
    
}
