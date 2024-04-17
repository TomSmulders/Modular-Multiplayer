using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] GameObject lobbySearchScreen, inLobbyScreen;
    [SerializeField] Button refreshButton;

    

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }

        refreshButton.onClick.AddListener(() =>
        {
            GameNetworkManager.instance.RequestLobbies();
        });

        GameNetworkManager.instance.RequestLobbies();
    }

    public void ShowLobbySearchScreen()
    {
        lobbySearchScreen.SetActive(true);
        inLobbyScreen.SetActive(false);
    }
    public void ShowInLobbyScreen()
    {
        inLobbyScreen.SetActive(true);
        lobbySearchScreen.SetActive(false);
    }
}
