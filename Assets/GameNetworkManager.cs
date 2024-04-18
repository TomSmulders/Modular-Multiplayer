using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;

public class GameNetworkManager : NetworkBehaviour
{

    public static GameNetworkManager instance;

    private FacepunchTransport transport = null;

    public Lobby? currentLobby;
    public LobbyMode currentLobbyMode;

    public enum LobbyMode { Public, Private, FriendsOnly, Invisible };


    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }

    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeave;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnMemberLeave;
    }

    private void OnLobbyEntered(Lobby _lobby)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }
        StartClient(currentLobby.Value.Owner.Id);
    }

    private void OnMemberJoined(Lobby _lobby, Friend _user)
    {
        Debug.Log(_user.Name + " Joined");
    }
    private void OnMemberLeave(Lobby _lobby, Friend _user)
    {
        Debug.Log(_user.Name + " left");
    }

    public async void StartHost(int _maxPlayers , LobbyMode _lobbyMode)
    {
        NetworkManager.Singleton.StartHost();

        GameManager.instance.myClientID = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxPlayers);

        SetLobbyMode(_lobbyMode);
        SetupLobby();

        UIManager.instance.ShowInLobbyScreen();
    }

    public void StartClient(SteamId _sId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        transport.targetSteamId = _sId;
        GameManager.instance.myClientID = NetworkManager.Singleton.LocalClientId;
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client has started");
        }
    }

    private void Singleton_OnClientDisconnectCallback(ulong _clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if (_clientId == 0)
        {
            Disconnected();
        }
    }

    private void Singleton_OnClientConnectedCallback(ulong _clientId)
    {
        GameManager.instance.myClientID = _clientId;
    }

    public void Disconnected()
    {
        currentLobby?.Leave();
        if (NetworkManager.Singleton == null)
        {
            return;
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        }

        NetworkManager.Singleton.Shutdown(true);
        GameManager.instance.Disconnected();
        Debug.Log("Disconnected");
    }


    public async void RequestLobbies()
    {
        foreach (GameObject card in GameManager.instance.lobbyCards)
        {
            Destroy(card);
        }

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();

        foreach (Lobby _lobby in lobbies)
        {
            if (_lobby.GetData("LobbyName").Length > 0)
            {
                GameManager.instance.CreateLobbyCard(_lobby, _lobby.GetData("LobbyName"));
            }
        }
    }

    public void SetupLobby()
    {
        currentLobby.Value.SetJoinable(true);
        currentLobby.Value.SetGameServer(currentLobby.Value.Owner.Id);
        currentLobby.Value.SetData("LobbyName", currentLobby.Value.Owner.Name + "'s lobby");
    }

    public async void Join_Public_Lobby(Lobby _lobby)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to join lobby");
        }
        else
        {
            currentLobby = _lobby;
            Debug.Log("Joined lobby");
        }

        UIManager.instance.ShowInLobbyScreen();
    }

    private void Update()
    {
        if(currentLobby != null)
        {
            foreach (Friend? member in currentLobby.Value.Members)
            {
                Debug.Log(member.Value.Name);
            }
        }
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
