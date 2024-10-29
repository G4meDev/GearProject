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

    public GameObject playerPrefab;

    private void Awake()
    {
        IpButton.onClicked += StartJoin;
    }

    private void OnDestroy()
    {
        IpButton.onClicked -= StartJoin;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void MainMenuToGameLevelRpc()
    {
        mainMenu.CloseMenu();
    }

    private void OnLevelLoadFinished(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            Debug.Log("Server Loaded");

            SceneManager.Get().PrepareRace();

            MainMenuToGameLevelRpc();
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

        NetworkManager.StartHost();
        NetworkManager.SceneManager.OnLoadEventCompleted += OnLevelLoadFinished;
    }

    public void EndHost()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLevelLoadFinished;
        }
    }

    public void StartJoin(string ip)
    {
        netState = NetState.Client;



        Debug.Log("join" + ip);

        NetworkManager.GetComponent<UnityTransport>().SetConnectionData(ip, 7777);

        NetworkManager.StartClient();
    }
}
