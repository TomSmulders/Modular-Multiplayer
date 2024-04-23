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
    public static GameManager instance;

    [SerializeField] GameObject lobbyCardPrefab, lobbyCardParent, playerCardPrefab, playerCardParent;
    public List<GameObject> lobbyCards = new List<GameObject>();
    public List<GameObject> playerCards = new List<GameObject>();

    public ulong myClientID;

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

    public void ReadyUp(PlayerData _user,bool _ready,bool callRPC)
    {
        _user.isReady = _ready;
        RawImage img = _user.playercard.GetComponent<PlayerCardInfo>().readyImage;
        img.color = _ready ? UnityEngine.Color.green : UnityEngine.Color.red;

        foreach (var item in GetComponent<NetworkObject>().NetworkManager.ConnectedClientsIds)
        {
            Debug.Log("Client: " + item);
        }

        //NetworkTransmittion.instance.SendDebugText("test");

        if (callRPC)
        {
            NetworkTransmittion.instance.PlayerChangedReadyStateServerRPC(_user.id,_ready);
        }
    }

    public void CreateLobbyCard(Lobby _lobbyId, string _lobbyName) 
    {
        GameObject card = Instantiate(lobbyCardPrefab);
        card.transform.SetParent(lobbyCardParent.transform);

        LobbyData data = card.GetComponent<LobbyData>();
        data.LobbyName = _lobbyName;
        data.lobbyId = _lobbyId;
        data.UpdateLobbyData();

        lobbyCards.Add(card);
    }

    public async void CreatePlayerCard(PlayerData player)
    {
        GameObject card = Instantiate(playerCardPrefab);
        card.transform.SetParent(playerCardParent.transform);

        PlayerCardInfo info = card.GetComponent<PlayerCardInfo>();

        info.steamName = player.username;
        info.steamId = player.id;
        info.isOwner = player.isOwner;
        info.ownerImage.SetActive(info.isOwner);

        info.kickButton.gameObject.SetActive(false);
        if (NetworkManager.Singleton.IsHost)
        {
            if (info.steamId != SteamClient.SteamId)
            {
                info.kickButton.gameObject.SetActive(true);
            }
        }

        info.UpdateValues();

        player.playercard = card;

        player.profilePicture = await GetProfilePicture(info.steamId);
        info.profileImage.texture = player.profilePicture;
    }

    public async Task<Texture2D> GetProfilePicture(ulong _SteamId)
    {
        Steamworks.Data.Image? profilepic = await SteamFriends.GetLargeAvatarAsync(_SteamId);
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
