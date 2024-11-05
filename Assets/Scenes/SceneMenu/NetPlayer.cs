using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetPlayer : NetworkBehaviour
{
    public SpectatorView spectatorViewPrefab;
    public SpectatorView spectatorView;

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();
    public Vehicle vehicle;

    SessionManager sessionManager;

    public void GoSpectator()
    {

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void OnEndedRaceRpc(int position, Vector3 endPos, Quaternion endRot)
    {
        if (IsOwner)
        {
            Debug.Log("i finished at " + position);

            GoSpectator();
        }


    }

    public override void OnDestroy()
    {
        if (IsHost)
        {
            sessionManager.RefreshLobbyListRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        sessionManager = GameObject.FindFirstObjectByType<SessionManager>();

        playerName.OnValueChanged += OnPlayerNameChanged;
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnPlayerNameChanged;
    }

    public void OnPlayerNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        sessionManager.RefreshLobbyListRpc();
    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
