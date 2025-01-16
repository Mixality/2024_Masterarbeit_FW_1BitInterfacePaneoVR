using System.Threading.Tasks;
using Network;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SimpleRelay : MonoBehaviour
{ 
    [SerializeField] public TMP_Text joinCodeText;
    [SerializeField] public TMP_InputField joinInput;
    private UnityTransport transport;
    private const int MaxPlayers = 5;
    
    private NetworkController networkController;
    
    private async void Awake() {
        transport = FindObjectOfType<UnityTransport>();
        if (!networkController)
        {
            Debug.Log("No UnityTransport found in scene!");
        }
        networkController = FindObjectOfType<NetworkController>();
        if (!networkController)
        {
            Debug.Log("No NetworkController found in scene!");
        }
        await Authenticate();
    }
    
    private static async Task Authenticate() {
        /*await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();*/
    }

    public async void StartSingleplayerSession()
    {
        //await Authenticate();
#if !(UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP)
        await CreateSingleplayerGame();
#endif
        networkController.OnHostedLocally();
    }
    
    public async void CreateMultiplayerGame() {

        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        //joinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        Debug.Log("INVITE CODE: " + await RelayService.Instance.GetJoinCodeAsync(a.AllocationId));
        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
        
        NetworkManager.Singleton.StartHost();
    }

    private async Task<bool> CreateSingleplayerGame()
    {
        transport.SetConnectionData("127.0.0.1", 7777);
        NetworkManager.Singleton.StartHost();
        return true;
    }
    
    public async void JoinGame() {

        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinInput.text);
        
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
        
        NetworkManager.Singleton.StartClient();
    }
}