using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
public class GearNetworkDiscovery : NetworkDiscovery<DiscoveryBroadcastData, DiscoveryResponseData>
{
    NetworkManager networkManager;
    MainMenu mainMenu;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        mainMenu = GameObject.FindFirstObjectByType<MainMenu>();
    }

    protected override bool ProcessBroadcast(IPEndPoint sender, DiscoveryBroadcastData broadCast, out DiscoveryResponseData response)
    {
        Debug.Log("processing broadcast with name " + mainMenu.saveData.playerName);

        response = new DiscoveryResponseData()
        {
            ServerName = mainMenu.saveData.playerName,
            Port = ((UnityTransport)networkManager.NetworkConfig.NetworkTransport).ConnectionData.Port,
        };
        return true;
    }

    protected override void ResponseReceived(IPEndPoint sender, DiscoveryResponseData response)
    {
        Debug.Log("response from " + sender.Address + "with name " + response.ServerName);

        mainMenu.OnServerFound(response.ServerName, sender.Address.ToString());
    }
}
