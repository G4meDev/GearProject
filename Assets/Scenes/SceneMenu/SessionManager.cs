using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NetState
{
    Offline, Host, Client
}

public class SessionManager : NetworkBehaviour
{
    public NetState netState;

    private void OnLevelLoadFinished(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            Debug.Log("Server Loaded");

            SceneManager.Get().PrepareRace();
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong index)
    {
        if(IsServer)
        {
            if(index == 1)
            {
                NetworkManager.SceneManager.LoadScene("Scene_5", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }
    }

    public void Start()
    {
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        
    }



    public void StartHost()
    {
        netState = NetState.Host;

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

    public void StartJoin()
    {
        netState = NetState.Client;

        NetworkManager.StartClient();
    }
}
