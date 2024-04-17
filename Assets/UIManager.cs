using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }

    [SerializeField] GameObject lobbySearchScreen, inLobbyScreen;

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
