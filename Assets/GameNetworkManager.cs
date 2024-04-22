using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;
using System.Linq;

public class GameNetworkManager : NetworkBehaviour
{

    public static GameNetworkManager instance;

    public FacepunchTransport transport;

    public Lobby? currentLobby;
    public LobbyMode currentLobbyMode;

    public enum LobbyMode { Public, Private, FriendsOnly, Invisible };

    public List<PlayerData> players = new List<PlayerData>();
    public PlayerData me;
    

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }

    private void Start()
    {
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
    } 

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
    }

    private void OnLobbyEntered(Lobby _lobby)
    {
        Debug.Log("Joined lobby");
        if (!NetworkManager.Singleton.IsHost)
        {
            StartClient(_lobby.Owner.Id);
        }

        UpdatePlayers(_lobby.Members);
    }

    private void OnLobbyMemberJoined(Lobby _lobby, Friend _user)
    {
        Debug.Log("player joined");
        UpdatePlayers(_lobby.Members);
    }
    private void OnLobbyMemberLeave(Lobby _lobby, Friend _user)
    {
        if(_user.Id.ToString() == _lobby.GetData("LobbyOwner"))
        {
            Disconnected();
        }
        UpdatePlayers(_lobby.Members);
    }

    public void UpdatePlayers(IEnumerable<Friend> _memberIEnumerable)
    {
        Friend[] members = _memberIEnumerable.ToArray();

        Debug.Log("updating players");

        foreach (PlayerData player in players)
        {
            if (!members.Contains(player.friend))
            {
                RemovePlayer(player);
            }
        }
        foreach (Friend user in members)
        {
            bool isInPlayers = false;
            foreach (PlayerData player in players)
            {
                if (player.id == user.Id)
                {
                    isInPlayers = true;
                }
            }
            if (!isInPlayers)
            {
                AddPlayer(user);
            }
        }
        if(me == null)
        {
            foreach (PlayerData player in players)
            {
                if (player.id == SteamClient.SteamId)
                {
                    me = player;
                }
            }
        }
    }

    public void AddPlayer(Friend? _user)
    {
        PlayerData player = new PlayerData();

        player.id = _user.Value.Id;
        player.friend = _user.Value;
        player.username = _user.Value.Name;
        player.isOwner = currentLobby.Value.IsOwnedBy(player.id);

        GameManager.instance.CreatePlayerCard(player);

        players.Add(player);
    }

    public void RemovePlayer(PlayerData player)
    {
        if (player.playercard != null)
        {
            Destroy(player.playercard);
        }
        if (player.gameobject != null)
        {
            Destroy(player.gameobject);
        }
        players.Remove(player);
    }

    public async void StartHost(int _maxPlayers , LobbyMode _lobbyMode)
    {
        NetworkManager.Singleton.StartHost();

        GameManager.instance.myClientID = NetworkManager.Singleton.LocalClientId;

        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxPlayers);
        currentLobby.Value.SetData("LobbyOwner", currentLobby.Value.Owner.Id.ToString());

        SetLobbyMode(_lobbyMode);
        SetupLobby();

        UIManager.instance.ShowInLobbyScreen();
    }

    public void StartClient(SteamId _sId)
    {
        Debug.Log("Trying to start client");

        transport.targetSteamId = _sId.Value;

        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartClient();

        Debug.Log("Client started. Local Client ID: " + NetworkManager.Singleton.LocalClientId);
    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log("Client connected. Client ID: " + clientId);
        // Add any additional logic you want to execute when a client connects
    }

    private void Singleton_OnClientDisconnectCallback(ulong clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if (clientId == 0)
        {
            Debug.Log("disconnect");
            Disconnected();
        }
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
            if (_lobby.GetData("LobbyName").Length > 0 && _lobby.GetData("GameName") == GlobalGameSettings.instance.gameName)
            {
                if (_lobby.MemberCount > 0 && _lobby.Owner.Id != 0)
                {
                    GameManager.instance.CreateLobbyCard(_lobby, _lobby.GetData("LobbyName"));
                }
            }
        }
    }

    public void SetupLobby()
    {
        currentLobby.Value.SetJoinable(true);
        currentLobby.Value.SetGameServer(currentLobby.Value.Owner.Id);
        currentLobby.Value.SetData("LobbyName", currentLobby.Value.Owner.Name + "'s lobby");
        currentLobby.Value.SetData("GameName", GlobalGameSettings.instance.gameName);
    }

    public async void Join_Public_Lobby(Lobby _lobby)
    {
        if(_lobby.Owner.Id != null && _lobby.MemberCount > 0)
        {
            RoomEnter joinedLobby = await _lobby.Join();
            if (joinedLobby != RoomEnter.Success)
            {
                Debug.Log("Failed to join lobby");
            }
            else
            {
                currentLobby = _lobby;
                UpdatePlayers(_lobby.Members);
                UIManager.instance.ShowInLobbyScreen();
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Lobby: " + currentLobby.Value);
            Debug.Log("Lobby Id: " + currentLobby.Value.Id);

            foreach (Friend member in currentLobby.Value.Members)
            {
                Debug.Log("Member: " + member.Name);
            }

            Debug.Log("Owner: " + currentLobby.Value.Owner);
            Debug.Log("OwnerId: " + currentLobby.Value.Owner.Id);

            Debug.Log("ClientId: " + GameManager.instance.myClientID);
        }
    }
}


