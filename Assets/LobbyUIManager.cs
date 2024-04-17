using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance;

    [SerializeField] GameObject LobbySearchScreen;
    [SerializeField] GameObject InLobbyScreen;

    private void Awake()
    {
        if(instance != null) { Destroy(this); } else { instance = this; }
    }

    public void OpenLobbySearchScreen()
    {
        LobbySearchScreen.SetActive(true);
        InLobbyScreen.SetActive(false);
    }

    public void OpenInLobbyScreen()
    {
        InLobbyScreen.SetActive(true);
        LobbySearchScreen.SetActive(false);
    }

}
