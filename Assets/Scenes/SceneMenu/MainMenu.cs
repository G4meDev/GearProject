using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    int mainMenuIndex = 0;
    int joinMenuIndex = 1;
    int hostMenuIndex = 2;

    public PageSwitcher pageSwitcher;
    public Transform lobbyView;

    public Camera camera;

    public GameObject lobbyLabelPrefab; 

    private SessionManager sessionManager;

    private void Start()
    {
        sessionManager = GameObject.FindObjectOfType<SessionManager>();  
    }

    public void OnHostButton()
    {
        ClearLobbyList();
        pageSwitcher.SwitchToPage(hostMenuIndex);
        sessionManager.StartHost();
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

        for(int i = 0; i < sessionManager.NetworkManager.ConnectedClients.Count; i++)
        {
            GameObject gameObject = Instantiate(lobbyLabelPrefab, lobbyView);
            gameObject.GetComponent<Text>().text = "player " + i;

            Debug.Log(i);
        }
    }

    public void CloseMenu()
    {
        pageSwitcher.SwitchToPage(-1);

        camera.gameObject.SetActive(false);
    }
}
