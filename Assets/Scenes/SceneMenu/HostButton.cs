using System;
using UnityEngine;
using UnityEngine.UI;

public class HostButton : MonoBehaviour
{
    public Text text;
    public string ip;
    public int port;

    public static Action<string, int> onClicked;

    public void Configure(string serverName, string inIp, int inPort)
    {
        ip = inIp;
        port = inPort;

        text.text = serverName;
        GetComponent<Button>().onClick.AddListener(() => onClicked?.Invoke(ip, port));
    }
}
