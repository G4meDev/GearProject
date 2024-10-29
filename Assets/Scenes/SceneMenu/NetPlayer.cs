using Unity.Collections;
using Unity.Netcode;

public class NetPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();

    public Vehicle vehicle;

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
