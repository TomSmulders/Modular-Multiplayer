using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    #region variables

    public GameObject HostUI;
    public GameObject ClientUI;

    [SerializeField] ReadyButtonScript HostReadyButton;
    [SerializeField] ReadyButtonScript ClientReadyButton;


    [SerializeField] Color publicColor;
    [SerializeField] Color privateColor;
    [SerializeField] Color FriendsOnlyColor;
    [SerializeField] Color InvisibleColor;

    [SerializeField] Button lobbyModeButton;
    [SerializeField] TextMeshProUGUI lobbyModeText;

    [SerializeField] Button startGameButton;
    [SerializeField] Color startGameColor;
    [SerializeField] Color startGameInactiveColor;

    #endregion

    public void Is_Host(bool _state)
    {
        HostUI.SetActive(_state);
        ClientUI.SetActive(!_state);
        Set_Lobby_PublicityMode(GameNetworkManager.instance.currentLobbyMode);
    }

    public void ResetUI()
    {
        Update_Party_Ready_State_Visuals(true);
        Set_Lobby_PublicityMode(LobbyPublicityMode.Public);

        HostReadyButton.Unready();
        ClientReadyButton.Unready();
    }

    public void Next_Lobby_PublicityMode()
    {
        GameNetworkManager.LobbyPublicityMode mode = GameNetworkManager.instance.currentLobbyMode;
        Debug.Log("1: "+mode);

        switch (mode)
        {
            case LobbyPublicityMode.Public:
                mode = LobbyPublicityMode.Private;
                break;
            case LobbyPublicityMode.Private:
                mode = LobbyPublicityMode.FriendsOnly;
                break;
            case LobbyPublicityMode.FriendsOnly:
                mode = LobbyPublicityMode.Public;
                break;
            case LobbyPublicityMode.Invisible:
                mode = LobbyPublicityMode.Private;
                break;
            default:
                mode = LobbyPublicityMode.Private;
                break;
        }
        Debug.Log("2: "+mode);
        GameNetworkManager.instance.currentLobbyMode = mode;
        Set_Lobby_PublicityMode(mode);
    }

    public void Set_Lobby_PublicityMode(GameNetworkManager.LobbyPublicityMode _lobbyMode)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            switch (_lobbyMode)
            {
                case LobbyPublicityMode.Public:
                    Change_Lobby_PublicityMode_Visuals("Public", publicColor);
                    break;
                case LobbyPublicityMode.Private:
                    Change_Lobby_PublicityMode_Visuals("Private", privateColor);
                    break;
                case LobbyPublicityMode.FriendsOnly:
                    Change_Lobby_PublicityMode_Visuals("Friends", FriendsOnlyColor);
                    break;
                case LobbyPublicityMode.Invisible:
                    Change_Lobby_PublicityMode_Visuals("Invisible", InvisibleColor);
                    break;
                default:
                    Change_Lobby_PublicityMode_Visuals("Public", publicColor);
                    break;
            }
            GameNetworkManager.instance.Set_Lobby_PublicityMode(_lobbyMode);
        }
    }

    void Change_Lobby_PublicityMode_Visuals(string _text, Color _color)
    {
        lobbyModeText.text = _text;

        ColorBlock block = lobbyModeButton.colors;
        block.normalColor = _color;
        block.selectedColor = _color;
        block.highlightedColor = _color;
        block.pressedColor = _color;

        lobbyModeButton.colors = block;
    }

    public void Exit_Party()
    {
        GameNetworkManager.instance.Disconnect();
    }

    public void Exit_Party_HOST()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //Add a check to actually leave

            GameNetworkManager.instance.Disconnect();
        }
    }

    public void Update_Party_Ready_State_Visuals(bool _ready)
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
}
