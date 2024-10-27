using System;
using System.Collections;
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

    private List<Ping> pingList = new();

    public void StartJoin()
    {
        netState = NetState.Client;

        string localIp = NetworkUtilities.GetLocalIPv4();
        string subnetMask = NetworkUtilities.GetSubnetMask(localIp);

        List<string> ipAdresses = NetworkUtilities.GetIPRange(localIp, subnetMask);

        pingList.Clear();

        foreach (string ip in ipAdresses)
        {
            pingList.Add(new Ping(ip));
        }

        StartCoroutine(CheckPing());


        //NetworkManager.StartClient();
    }

    public void FoundIp(string ip)
    {
        Debug.Log(ip);
    }

    IEnumerator CheckPing()
    {
        bool flag = false;

        while (!flag)
        {
            flag = true;

            for(int i = pingList.Count - 1; i >= 0; i--)
            {
                Ping ping = pingList[i];

                if(ping.isDone)
                {
                    if(ping.time >= 0)
                    {
                        FoundIp(ping.ip);
                        pingList.RemoveAt(i);
                    }
                }

                else
                {
                    flag = false;
                }
            }

            yield return null;
        }
    }
}
