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

    private void NetworkManager_OnClientConnectedCallback(ulong index)
    {
        if(IsServer)
        {
            mainMenu.RefreshLobbyList();

            if(index == 1)
            {
                //StartRace();
            }
        }
    }

    public void StartRace()
    {
        NetworkManager.SceneManager.LoadScene("Scene_5", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void Start()
    {
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

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
