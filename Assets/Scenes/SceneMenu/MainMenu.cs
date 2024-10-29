using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public SaveData saveData;

    int mainMenuIndex = 0;
    int joinMenuIndex = 1;
    int hostMenuIndex = 2;

    public PageSwitcher pageSwitcher;
    public Transform lobbyView;

    public Camera camera;

    public GameObject lobbyLabelPrefab;
    public InputField playerNameInputField;

    public IpButton hostButtonPrefab;
    public Transform hostButtonsParent;

    private SessionManager sessionManager;

    private void Start()
    {
        sessionManager = GameObject.FindObjectOfType<SessionManager>();

        playerNameInputField.onEndEdit.AddListener(OnPlayerNameSubmit);

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

    public void OnHostButton()
    {
        ClearLobbyList();
        pageSwitcher.SwitchToPage(hostMenuIndex);
        sessionManager.StartHost();
    }

    public void OnHostBackToMenu()
    {
        ClearLobbyList();
        sessionManager.EndHost();
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

    public void OnJoinButton()
    {
        ClearHostList();
        GetComponent<IPScanner>().Scan();
        pageSwitcher.SwitchToPage(joinMenuIndex);
    }

    public void CloseMenu()
    {
        pageSwitcher.SwitchToPage(-1);

        camera.gameObject.SetActive(false);
    }
}
