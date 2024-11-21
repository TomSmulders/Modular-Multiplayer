using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    #region variables

    public static GameManager instance;

    [SerializeField] GameObject lobbyCardPrefab, lobbyCardParent, playerCardPrefab, playerCardParent;
    public List<GameObject> lobbyCards = new List<GameObject>();
    public List<GameObject> playerCards = new List<GameObject>();

    public ulong myClientID;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        
    }

    
    public void Ready_Player_Up(PlayerData _user,bool _ready,bool callRPC)
    {
        _user.isReady = _ready;
        RawImage img = _user.playercard.GetComponent<PlayerCardScript>().readyImage;
        img.color = _ready ? UnityEngine.Color.green : UnityEngine.Color.red;

        if (callRPC)
        {
            NetworkTransmittion.instance.Player_Changed_Ready_State_ServerRPC(_user.id,_ready);
        }
    }
    public void Create_LobbyCard(Lobby _lobbyId, string _lobbyName) 
    {
        GameObject card = Instantiate(lobbyCardPrefab);
        card.transform.SetParent(lobbyCardParent.transform);

        LobbyData data = card.GetComponent<LobbyData>();
        data.LobbyName = _lobbyName;
        data.lobbyId = _lobbyId;
        data.Update_Lobby_Data();

        lobbyCards.Add(card);
    }
    public async void Create_PlayerCard(PlayerData player)
    {
        GameObject card = Instantiate(playerCardPrefab);
        card.transform.SetParent(playerCardParent.transform);

        PlayerCardScript info = card.GetComponent<PlayerCardScript>();

        info.steamName = player.username;
        info.steamId = player.id;
        info.isOwner = player.isOwner;
        info.ownerImage.SetActive(info.isOwner); 

        info.SettingsButton.gameObject.SetActive(false);
        if (NetworkManager.Singleton.IsHost)
        {
            if (info.steamId != SteamClient.SteamId)
            {
                info.SettingsButton.gameObject.SetActive(true);
            }
        }

        info.Update_Values();

        player.playercard = card;

        player.profilePicture = await Get_User_Profile_Picture(info.steamId);
        info.profileImage.texture = player.profilePicture;

        playerCards.Add(card);
    }
    public async Task<Texture2D> Get_User_Profile_Picture(ulong _SteamId)
    {
        Debug.Log(_SteamId);
        if(_SteamId != null)
        {
            Steamworks.Data.Image? profilepic = await SteamFriends.GetLargeAvatarAsync(_SteamId);

            if (profilepic.HasValue)
            {
                var avatar = new Texture2D((int)profilepic.Value.Width, (int)profilepic.Value.Height, TextureFormat.ARGB32, false);
                avatar.filterMode = FilterMode.Trilinear;

                for (int x = 0; x < profilepic.Value.Width; x++)
                {
                    for (int y = 0; y < profilepic.Value.Height; y++)
                    {
                        var p = profilepic.Value.GetPixel(x, y);
                        avatar.SetPixel(x, (int)profilepic.Value.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
                    }
                }

                avatar.Apply();
                return avatar;
            }
        }
        return new Texture2D(1, 1, TextureFormat.ARGB32, false);
    }
}
