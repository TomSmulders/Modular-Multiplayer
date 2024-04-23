using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
            GameNetworkManager.instance.RequestLobbies();
        });
        //Gecomment omdat ik error kreeg (null exeption)
    }

    private void Start()
    {
        GameNetworkManager.instance.RequestLobbies();
    }

    public void ToggleFriends()
    {
        if (!friendScreen.activeSelf)
        {
            FriendsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
            friendScreen.SetActive(true);
            GetFriends.instance.RequestFriends(true);
        }
        else
        {
            FriendsButton.GetComponentInChildren<TextMeshProUGUI>().text = "Friends";
            friendScreen.SetActive(false);
            GetFriends.instance.RequestFriends(false);
        }
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
