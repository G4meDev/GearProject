using System;
using UnityEngine;
using UnityEngine.UI;

public class IpButton : MonoBehaviour
{
    public Text text;
    public string ip;

    public static Action<string> onClicked;

    public void Configure(string inIp)
    {
        ip = inIp;

        text.text = inIp;
        GetComponent<Button>().onClick.AddListener(() => onClicked?.Invoke(ip));
    }
}
