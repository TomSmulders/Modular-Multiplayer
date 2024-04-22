using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using static GameNetworkManager;

public class PlayersUIManager : MonoBehaviour
{
    public static PlayersUIManager instance;

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }

    public GameObject HostUI;
    public GameObject ClientUI;
    [SerializeField] Color publicColor;
    [SerializeField] Color privateColor;
    [SerializeField] Color FriendsOnlyColor;
    [SerializeField] Color InvisibleColor;

    [SerializeField] Button lobbyModeButton;
    [SerializeField] TextMeshProUGUI lobbyModeText;

    [SerializeField] Button startGameButton;
    [SerializeField] Color startGameColor;
    [SerializeField] Color startGameInactiveColor;


    public void IsHost(bool _state)
    {
        HostUI.SetActive(_state);
        ClientUI.SetActive(!_state);
        SetLobbyMode(GameNetworkManager.instance.currentLobbyMode);
    }

    public void NextLobbyMode()
    {
        GameNetworkManager.LobbyMode mode = GameNetworkManager.instance.currentLobbyMode;
        Debug.Log("1: "+mode);

        switch (mode)
        {
            case LobbyMode.Public:
                mode = LobbyMode.Private;
                break;
            case LobbyMode.Private:
                mode = LobbyMode.FriendsOnly;
                break;
            case LobbyMode.FriendsOnly:
                mode = LobbyMode.Public;
                break;
            case LobbyMode.Invisible:
                mode = LobbyMode.Private;
                break;
            default:
                mode = LobbyMode.Private;
                break;
        }
        Debug.Log("2: "+mode);
        GameNetworkManager.instance.currentLobbyMode = mode;
        SetLobbyMode(mode);
    }

    void SetLobbyMode(GameNetworkManager.LobbyMode _lobbyMode)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            switch (_lobbyMode)
            {
                case LobbyMode.Public:
                    ChangeLobbyMode("Public", publicColor);
                    break;
                case LobbyMode.Private:
                    ChangeLobbyMode("Private", privateColor);
                    break;
                case LobbyMode.FriendsOnly:
                    ChangeLobbyMode("Friends", FriendsOnlyColor);
                    break;
                case LobbyMode.Invisible:
                    ChangeLobbyMode("Invisible", InvisibleColor);
                    break;
                default:
                    ChangeLobbyMode("Public", publicColor);
                    break;
            }
            GameNetworkManager.instance.SetLobbyMode(_lobbyMode);
        }
    }

    void ChangeLobbyMode(string _text, Color _color)
    {
        lobbyModeText.text = _text;

        ColorBlock block = lobbyModeButton.colors;
        block.normalColor = _color;
        block.selectedColor = _color;
        block.highlightedColor = _color;
        block.pressedColor = _color;

        lobbyModeButton.colors = block;
    }

    public void ExitParty()
    {
        GameNetworkManager.instance.Disconnected();
    }

    public void ExitPartyHOST()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //Add a check to actually leave

            GameNetworkManager.instance.Disconnected();
        }
    }

    public void ChangePartyReady(bool _ready)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Color _color;
            if (_ready)
            {
                startGameButton.interactable = true;
                _color = startGameColor;
            }
            else
            {
                startGameButton.interactable = false;
                _color = startGameInactiveColor;
            }

            ColorBlock block = startGameButton.colors;

            block.normalColor = _color;
            block.selectedColor = _color;
            block.highlightedColor = _color;
            block.pressedColor = _color;

            startGameButton.colors = block;
        }
    }

    public void StartGame()
    {

    }
}
