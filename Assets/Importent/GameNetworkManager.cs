using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;
using System.Linq;
using System.Runtime.CompilerServices;

public class GameNetworkManager : NetworkBehaviour
{
    #region variables

    public static GameNetworkManager instance;

    public FacepunchTransport transport;

    public Lobby? currentLobby;
    public LobbyPublicityMode currentLobbyMode;
    public bool partyReady = false;

    public enum LobbyPublicityMode { Public, Private, FriendsOnly, Invisible };

    public List<PlayerData> players = new List<PlayerData>();
    public PlayerData me;

    #endregion

    //Unity functions
    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }
    private void Start()
    {
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;

        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
    } 
    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;

        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;
    }


    //Callbacks
    private void OnLobbyEntered(Lobby _lobby)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Start_Client(_lobby.Owner.Id);
        }

        if (NetworkManager.Singleton.IsHost)
        {
            PlayersUIManager.instance.Is_Host(NetworkManager.Singleton.IsHost);
        }
        else
        {
            PlayersUIManager.instance.Hide_UI();
        }

        Update_Lobby(_lobby.Members);
    }
    private void OnLobbyMemberJoined(Lobby _lobby, Friend _user)
    {
        Update_Lobby(_lobby.Members);
        NetworkTransmittion.instance.Check_If_All_Players_Are_Ready_ClientRPC(); 
    }
    private void OnLobbyMemberLeave(Lobby _lobby, Friend _user)
    {
   
        if (_user.Id.ToString() == _lobby.GetData("LobbyOwner"))
        {
            Disconnect();
        }
        Update_Lobby(_lobby.Members);
        

    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log("Client connected. Client ID: " + clientId);

        PlayersUIManager.instance.Is_Host(NetworkManager.Singleton.IsHost);

        Update_Lobby(currentLobby.Value.Members);

        NetworkTransmittion.instance.Update_Players_That_Are_Ready_ServerRpc(clientId);

        // Add any additional logic you want to execute when a client connects
    }
    private void Singleton_OnClientDisconnectCallback(ulong clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if (clientId == 0)
        {
            Debug.Log("disconnect");
            Disconnect();
        }
    }
    public async void SteamFriends_OnGameLobbyJoinRequested(Lobby _lobby, SteamId _user)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to join lobby");
        }
        else
        {
            currentLobby = _lobby;
            GlobalGameManager.instance.currentLobby = _lobby;
            Update_Lobby(_lobby.Members);
            UIManager.instance.Show_In_Lobby_Screen();
        }
    }


    //Network / hosting Related
    public async void Start_Host(int _maxPlayers, LobbyPublicityMode _lobbyMode)
    {
        NetworkManager.Singleton.StartHost();

        GameManager.instance.myClientID = NetworkManager.Singleton.LocalClientId;

        if (_maxPlayers > GlobalGameSettings.instance.maxPartySize)
        {
            _maxPlayers = GlobalGameSettings.instance.maxPartySize;
        }

        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxPlayers);
        currentLobby.Value.SetData("LobbyOwner", currentLobby.Value.Owner.Id.ToString());

        GlobalGameManager.instance.currentLobby = currentLobby;

        Set_Lobby_PublicityMode(_lobbyMode);
        Setup_Lobby();

        UIManager.instance.Show_In_Lobby_Screen();
    }
    public void Start_Client(SteamId _sId)
    {
        transport.targetSteamId = _sId.Value;

        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartClient();   
    }


    //Lobby Related
    public async void Request_Lobbies()
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
                    GameManager.instance.Create_LobbyCard(_lobby, _lobby.GetData("LobbyName"));
                }
            }
        }
    }
    public async void Join_Public_Lobby(Lobby _lobby)
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();

        foreach (Lobby _updatedLobby in lobbies)
        {
            if (_lobby.Id == _updatedLobby.Id)
            {
                string[] bannedPlayers = _updatedLobby.GetData("BannedPlayers").Split(",");
                if (!bannedPlayers.Contains(SteamClient.SteamId.Value.ToString()))
                {
                    if (_lobby.GetData("CurrentLobbyMode") == LobbyPublicityMode.Public.ToString())
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
                                GlobalGameManager.instance.currentLobby = currentLobby;

                                Update_Lobby(_lobby.Members);
                                UIManager.instance.Show_In_Lobby_Screen();
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("L bozo ur banned");
                }
            }
        }
    }
    public void Setup_Lobby()
    {
        currentLobby.Value.SetJoinable(true);
        currentLobby.Value.SetGameServer(currentLobby.Value.Owner.Id);
        currentLobby.Value.SetData("LobbyName", currentLobby.Value.Owner.Name + "'s lobby");
        currentLobby.Value.SetData("GameName", GlobalGameSettings.instance.gameName);
    }
    public void Set_Lobby_PublicityMode(LobbyPublicityMode _lobbymode)
    {
        switch (_lobbymode)
        {
            case LobbyPublicityMode.Public:
                currentLobby.Value.SetPublic();
                break;
            case LobbyPublicityMode.Private:
                currentLobby.Value.SetPrivate();
                break;
            case LobbyPublicityMode.FriendsOnly:
                currentLobby.Value.SetFriendsOnly();
                break;
            case LobbyPublicityMode.Invisible:
                currentLobby.Value.SetInvisible();
                break;
            default:
                currentLobby.Value.SetPrivate();
                break;
        }
        currentLobby.Value.SetData("CurrentLobbyMode", _lobbymode.ToString());
        currentLobbyMode = _lobbymode;
    }


    //Party related
    public void Update_Lobby(IEnumerable<Friend> _memberIEnumerable)
    {
        GlobalGameManager.instance.currentLobby = currentLobby;

        Update_If_Players_Ready(partyReady);

        Friend[] members = _memberIEnumerable.ToArray();


        foreach (PlayerData player in players)
        {
            if (!members.Contains(player.friend))
            {
                Remove_Player(player);
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
                Add_Player(user);
            }
        }
        foreach (PlayerData player in players)
        {
            if (player.id == SteamClient.SteamId)
            {
                me = player;
            }
        }
    }
    public void Add_Player(Friend? _user)
    {
        PlayerData player = new PlayerData();

        player.id = _user.Value.Id;
        player.friend = _user.Value;
        player.username = _user.Value.Name;
        player.isOwner = currentLobby.Value.IsOwnedBy(player.id);

        GameManager.instance.Create_PlayerCard(player);

        players.Add(player);
    }
    public void Remove_Player(PlayerData _user)
    {
        if (_user.playercard != null)
        {
            Destroy(_user.playercard);
        }
        if (_user.gameobject != null)
        {
            Destroy(_user.gameobject);
        }
        players.Remove(_user);

        if (IsHost)
        {
            NetworkTransmittion.instance.Check_If_All_Players_Are_Ready_ClientRPC();
        }
    }
    public void Update_If_Players_Ready(bool _ready)
    {
        partyReady = _ready;
        if (PlayersUIManager.instance != null)
        {
            PlayersUIManager.instance.Update_Party_Ready_State_Visuals(partyReady);
        }
    }


    //leaving
    public void Disconnect()
    {
        GlobalGameManager.instance.Disconnect_Player(currentLobby);

        //Reset scene 
        foreach (GameObject obj in GameManager.instance.lobbyCards)
        {
            Destroy(obj);
        }
        foreach (GameObject obj in GameManager.instance.playerCards)
        {
            Destroy(obj); 
        }

        PlayersUIManager.instance.ResetUI();

        UIManager.instance.Show_Lobby_Search_Screen();

        UIManager.instance.Hide_Friends();

        players.Clear();
        partyReady = false;

        Debug.Log("THIS IS A TEST IAUWOIJOIJNAI");

        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Kick_Player(ulong _steamId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            
        }
    }
    public void Ban_Player(ulong _steamId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            string bannedPlayers = currentLobby.Value.GetData("BannedPlayers");
            if (bannedPlayers == string.Empty || bannedPlayers == null || bannedPlayers == "")
            {
                bannedPlayers = _steamId.ToString();
            }
            else
            {
                bannedPlayers += "," + _steamId.ToString();
            }
            currentLobby.Value.SetData("BannedPlayers", bannedPlayers);

            Kick_Player(_steamId);
        }
    }


    public void Switch_scene(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}


