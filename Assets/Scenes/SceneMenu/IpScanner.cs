using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPScanner : MonoBehaviour
{
    private List<Ping> pings = new List<Ping>();
    private float timer;

    private MainMenu mainMenu;

    private void Start()
    {
        mainMenu = GetComponent<MainMenu>();
    }

    public void Scan()
    {
        StopAllCoroutines();

        timer = 0;

        pings.Clear();

        string localIp = NetworkUtilities.GetLocalIPv4();
        string subnetMask = NetworkUtilities.GetSubnetMask(localIp);

        List<string> ipAddresses = NetworkUtilities.GetIPRange(localIp, subnetMask);

        Debug.Log("Scanning Network");

        foreach (string ipAddress in ipAddresses)
            pings.Add(new Ping(ipAddress));

        StartCoroutine(CheckPingsCoroutine());
    }

    IEnumerator CheckPingsCoroutine()
    {
        List<string> foundIps = new();

        bool allDone = false;

        while (!allDone)
        {
            allDone = true;

            for (int i = pings.Count - 1; i >= 0; i--)
            {
                Ping ping = pings[i];

                if (ping.isDone)
                {
                    if (ping.time >= 0)
                    {
                        IPFound(ping.ip);
                        pings.RemoveAt(i);
                    }
                }
                else
                    allDone = false;
            }

            timer += Time.deltaTime;

            if (timer >= 20)
            {
                allDone = true;
                Debug.Log("Timeout");
            }

            yield return null;
        }

        mainMenu.HideThrubber();
        Debug.Log("Scan Completed");
    }

    private void IPFound(string ip)
    {
        Debug.Log(ip);

        for (int i = 0; i < mainMenu.hostButtonsParent.childCount; i++)
        {
            HostButton childButton = mainMenu.hostButtonsParent.GetChild(i).GetComponent<HostButton>();

            if (childButton.ip == ip)
                return;
        }

        HostButton button = Instantiate(mainMenu.hostButtonPrefab, mainMenu.hostButtonsParent);
        button.Configure(ip);
    }
}
