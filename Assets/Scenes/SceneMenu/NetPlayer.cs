using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();

    public Vehicle vehicle;

    public override void OnDestroy()
    {
        if (IsHost)
        {
            GameObject.FindFirstObjectByType<SessionManager>().RefreshLobbyListRpc();
        }
    }

    public override void OnNetworkSpawn()
    {

    }

    public override void OnNetworkDespawn()
    {

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
