using Unity.Netcode;

public enum NetState
{
    Offline, Host, Client
}

public class SessionManager : NetworkBehaviour
{
    public NetState netState;

    public void StartHost()
    {
        netState = NetState.Host;

        NetworkManager.StartHost();
    }

    public void StartJoin()
    {
        netState = NetState.Client;

        NetworkManager.StartClient();
    }
}
