using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] GameObject lobbySearchScreen, inLobbyScreen , friendScreen;
    [SerializeField] Button refreshButton;
    [SerializeField] Button FriendsButton;

    

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }

        refreshButton.onClick.AddListener(() =>
        {
            GameNetworkManager.instance.Request_Lobbies();
        });
    }

    private void Start()
    {
        GameNetworkManager.instance.Request_Lobbies();
    }

    public void Toggle_Friends()
    {
        if (!friendScreen.activeSelf)
        {
            FriendsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
            friendScreen.SetActive(true);
            GetFriends.instance.Request_Friends(true);
        }
        else
        {
            FriendsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Friends";
            friendScreen.SetActive(false);
            GetFriends.instance.Request_Friends(false);
        }
    }


    public void Show_Lobby_Search_Screen()
    {
        lobbySearchScreen.SetActive(true);
        inLobbyScreen.SetActive(false);
    }
    public void Show_In_Lobby_Screen()
    {
        inLobbyScreen.SetActive(true);
        lobbySearchScreen.SetActive(false);
    }

    public void Start_Game()
    {
        SceneManager.LoadScene("PlayingField");
    }
}
