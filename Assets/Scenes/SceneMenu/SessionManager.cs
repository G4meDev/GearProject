using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NetState
{
    Offline, Host, Client
}

public class SessionManager : NetworkBehaviour
{
    public NetState netState;
    public MainMenu mainMenu;

    public GearNetworkDiscovery networkDiscovery;

    private void Awake()
    {

    }

    private void OnDestroy()
    {
//         IpButton.onClicked -= StartJoin;
//         NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
//         NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void MainMenuToGameLevelRpc()
    {
        mainMenu.CloseMenu();
    }

    private void OnLevelLoadFinished(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsHost)
        {
            Debug.Log("Server Loaded");

            MainMenuToGameLevelRpc();

            SceneManager.Get().PrepareRace();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RefreshLobbyListRpc()
    {
        mainMenu.RefreshLobbyList();
    }

    [Rpc(SendTo.Server)]
    private void UpdatePlayerNameRpc(ulong index, string name)
    {
        NetworkClient client;
        NetworkManager.ConnectedClients.TryGetValue(index, out client);
        client.PlayerObject.GetComponent<NetPlayer>().playerName.Value = name;

        RefreshLobbyListRpc();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong index)
    {
        if (NetworkManager.LocalClientId == index)
        {
            UpdatePlayerNameRpc(index, mainMenu.saveData.playerName);
        }
    }

    public void StartRace()
    {
        NetworkManager.SceneManager.LoadScene("Scene_5", LoadSceneMode.Additive);
    }


    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public void StartServerBroadcasting()
    {
        Debug.Log("start listening to clients");
        
        networkDiscovery.StartServer();
    }

    public void EndBroadcasting()
    {
        Debug.Log("Stop broadcasting");

        networkDiscovery.StopDiscovery();
    }

    public void StartClientBroadcasting()
    {
        Debug.Log("start broadcasting to servers");
        
        networkDiscovery.StartClient();
        networkDiscovery.ClientBroadcast(new DiscoveryBroadcastData());

    }

    public void Start()
    {
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
    }

    public void StartHost()
    {
        netState = NetState.Host;

        string localIp = NetworkUtilities.GetLocalIPv4();
        NetworkManager.GetComponent<UnityTransport>().SetConnectionData(localIp, 7777);

        if(NetworkManager.StartHost())
        {
            // this only can get bind after server starts
            NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoadFinished;

            StartServerBroadcasting();
        }
    }

    public void EndHost()
    {
        if (IsHost)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoadFinished;
        }

        NetworkManager.Shutdown();
    }

    public bool StartJoin(string ip)
    {
        netState = NetState.Client;

        Debug.Log("join" + ip);

        NetworkManager.GetComponent<UnityTransport>().SetConnectionData(ip, 7777);

        return NetworkManager.StartClient();
    }
}
