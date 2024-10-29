using System;
using UnityEngine;
using UnityEngine.UI;

public class HostButton : MonoBehaviour
{
    public Text text;
    public string ip;

    public static Action<string> onClicked;

    public void Configure(string serverName, string inIp)
    {
        ip = inIp;

        text.text = serverName;
        GetComponent<Button>().onClick.AddListener(() => onClicked?.Invoke(ip));
    }
}
