using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public SaveData saveData;

    int mainMenuIndex = 0;
    int joinMenuIndex = 1;
    int LobbyMenuIndex = 2;

    public PageSwitcher pageSwitcher;
    public Transform lobbyView;

    public Camera camera;

    public GameObject lobbyLabelPrefab;
    public InputField playerNameInputField;

    public HostButton hostButtonPrefab;
    public Transform hostButtonsParent;
    public GameObject hostListThrubber;

    public GameObject startButton;

    private SessionManager sessionManager;

    private void Start()
    {
        sessionManager = GameObject.FindObjectOfType<SessionManager>();

        playerNameInputField.onEndEdit.AddListener(OnPlayerNameSubmit);
        HostButton.onClicked += OnHostSelected;

        saveData = SaveData.Load();
        UpdatePlayerData();
    }

    private void UpdatePlayerData()
    {
        playerNameInputField.SetTextWithoutNotify(saveData.playerName);
    }

    private void OnPlayerNameSubmit(string name)
    {
        saveData.SetPlayerName(name);
        saveData.Save();
        playerNameInputField.SetTextWithoutNotify(saveData.playerName);
    }

    public void OpenLobbyList(bool asHost)
    {
        pageSwitcher.SwitchToPage(LobbyMenuIndex);
        startButton.SetActive(asHost);
    }

    public void OnHostButton()
    {
        ClearLobbyList();
        OpenLobbyList(true);
        sessionManager.StartHost();
    }

    public void OnLobbyBackToMenu()
    {
        ClearLobbyList();
        sessionManager.EndHost();
        sessionManager.EndBroadcasting();
        pageSwitcher.SwitchToPage(mainMenuIndex);
    }

    public void OnStart()
    {
        sessionManager.StartRace();
    }

    public void ClearLobbyList()
    {
        for (int i = 0; i < lobbyView.childCount; i++)
        {
            Destroy(lobbyView.GetChild(i).gameObject);
        }
    }

    public void RefreshLobbyList()
    {
        ClearLobbyList();

        NetPlayer[] netPlayers = GameObject.FindObjectsByType<NetPlayer>(FindObjectsSortMode.None);

        foreach (NetPlayer netPlayer in netPlayers)
        {
            GameObject gameObject = Instantiate(lobbyLabelPrefab, lobbyView);
            gameObject.GetComponent<Text>().text = netPlayer.playerName.Value.ToString();
        }
    }

    public void ClearHostList()
    {
        for (int i = 0; i < hostButtonsParent.childCount; i++)
        {
            Destroy(hostButtonsParent.GetChild(i).gameObject);
        }
    }

    public void ShowThrubber()
    {
        hostListThrubber.SetActive(true);
    }

    public void HideThrubber()
    {
        hostListThrubber.SetActive(false);
    }

    public void OnJoinButton()
    {
        ClearHostList();
        ShowThrubber();

        sessionManager.StartClientBroadcasting();

        pageSwitcher.SwitchToPage(joinMenuIndex);
    }

    public void OnJoinBackToMenu()
    {
        ClearHostList();
        HideThrubber();
        sessionManager.EndBroadcasting();

        pageSwitcher.SwitchToPage(mainMenuIndex);
    }

    public void OnRefreshHostList()
    {
        ClearHostList();
        sessionManager.EndBroadcasting();
        sessionManager.StartClientBroadcasting();

        ShowThrubber();
    }

    public void OnServerFound(string serverName, string address)
    {
        for (int i = 0; i < hostButtonsParent.childCount; i++)
        {
            HostButton childButton = hostButtonsParent.GetChild(i).GetComponent<HostButton>();

            if (childButton.ip == address)
                return;
        }

        HostButton button = Instantiate(hostButtonPrefab, hostButtonsParent);
        button.Configure(serverName, address);
    }

    public void OnHostSelected(string ip)
    {
        if (sessionManager.StartJoin(ip))
        {
            OpenLobbyList(false);
        }
    }

    public void CloseMenu()
    {
        pageSwitcher.SwitchToPage(-1);

        camera.gameObject.SetActive(false);
    }
}
