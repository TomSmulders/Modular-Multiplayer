using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;

public class GameNetworkManager : NetworkBehaviour
{

    public static GameNetworkManager instance;

    public Lobby? currentLobby;
    public LobbyMode currentLobbyMode;

    public enum LobbyMode { Public, Private, FriendsOnly, Invisible };


    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }

    public async void StartHost(int _maxPlayers , LobbyMode _lobbyMode)
    {
        NetworkManager.Singleton.StartHost();
        GameManager.instance.myClientID = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxPlayers);

        SetLobbyMode(_lobbyMode);

        UIManager.instance.ShowInLobbyScreen();
    }

    public void SetLobbyMode(LobbyMode _lobbymode)
    {
        switch (_lobbymode)
        {
            case LobbyMode.Public:
                currentLobby.Value.SetPublic();
                break;
            case LobbyMode.Private:
                currentLobby.Value.SetPrivate();
                break;
            case LobbyMode.FriendsOnly:
                currentLobby.Value.SetFriendsOnly();
                break;
            case LobbyMode.Invisible:
                currentLobby.Value.SetInvisible();
                break;
            default:
                currentLobby.Value.SetPublic();
                break;
        }
    }


}
