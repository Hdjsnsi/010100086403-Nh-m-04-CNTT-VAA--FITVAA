using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


public class RelayServices : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TMP_InputField joinCodeInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private  void Start()
    {
        // await UnityServices.InitializeAsync();
        // AuthenticationService.Instance.SignedIn += () => 
        // {
        //     Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        // };
        // await AuthenticationService.Instance.SignInAnonymouslyAsync();
        // hostBtn.onClick.AddListener(() => {CreateRelay();});
        // clientBtn.onClick.AddListener(() => {JoinCode(joinCodeInput.text);});
    }

    
    public async Task<string> CreateRelay(int maxPlayer)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
    
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame",LoadSceneMode.Single);
            return joinCode;
        }catch(RelayServiceException e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public async Task JoinCode(string joinCode)
    {
        try
        {
            Debug.Log(joinCode);
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData
            (
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
        }catch(RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    
}