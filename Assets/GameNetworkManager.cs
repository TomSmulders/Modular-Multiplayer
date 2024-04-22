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
    public bool partyReady = false;

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
        if (!NetworkManager.Singleton.IsHost)
        {
            StartClient(_lobby.Owner.Id);
        }

        PlayersUIManager.instance.IsHost(NetworkManager.Singleton.IsHost);

        UpdatePlayers(_lobby.Members);
    }

    private void OnLobbyMemberJoined(Lobby _lobby, Friend _user)
    {
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
        UpdatePartyReady(partyReady);

        Friend[] members = _memberIEnumerable.ToArray();


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

        if(_maxPlayers > GlobalGameSettings.instance.maxPartySize)
        {
            _maxPlayers = GlobalGameSettings.instance.maxPartySize;
        }

        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxPlayers);
        currentLobby.Value.SetData("LobbyOwner", currentLobby.Value.Owner.Id.ToString());

        SetLobbyMode(_lobbyMode);
        SetupLobby();

        UIManager.instance.ShowInLobbyScreen();
    }

    public void StartClient(SteamId _sId)
    {
        transport.targetSteamId = _sId.Value;

        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartClient();
    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            foreach (PlayerData player in players)
            {
                NetworkTransmittion.instance.PlayerChangedReadyStateServerRPC(player.id, player.isReady);
            }
        }
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

        players.Clear();
        
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
                if (_lobby.MemberCount > 0)
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
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();

        foreach (Lobby _updatedLobby in lobbies)
        {
            if (_lobby.Id == _updatedLobby.Id)
            {
                if(_lobby.GetData("CurrentLobbyMode") == LobbyMode.Public.ToString())
                {
                    if (_lobby.MemberCount > 0)
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
                currentLobby.Value.SetPrivate();
                break;
        }
        currentLobby.Value.SetData("CurrentLobbyMode", _lobbymode.ToString());
        currentLobbyMode = _lobbymode;
    }

    public void UpdatePartyReady(bool _ready)
    {
        partyReady = _ready;
        if(PlayersUIManager.instance != null)
        {
            PlayersUIManager.instance.ChangePartyReady(partyReady);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log(currentLobby.Value.GetData("CurrentLobbyMode"));
        }
    }
}


