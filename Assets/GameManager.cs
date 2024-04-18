using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using System.Runtime.CompilerServices;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject lobbyCardPrefab, lobbyCardParent, playerCardPrefab, playerCardParent;
    public List<GameObject> lobbyCards = new List<GameObject>();
    public List<GameObject> playerCards = new List<GameObject>();

    public Dictionary<ulong, GameObject> playerInfo = new Dictionary<ulong, GameObject>();


    public ulong myClientID;

    private void Awake()
    {
        if(instance != null) { Destroy(this); } else { instance = this; }
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

    public void CreatePlayerCard(PlayerData player)
    {
        GameObject card = Instantiate(playerCardPrefab);
        card.transform.SetParent(playerCardParent.transform);

        PlayerInfo info = card.GetComponent<PlayerInfo>();

        info.steamName = player.username;
        info.steamId = player.id;
        info.isOwner = player.isOwner;
        info.ownerImage.SetActive(info.isOwner);

        info.UpdateValues();

        player.playercard = card;
    }

    public void Disconnected()
    {
        playerInfo.Clear();
        foreach (GameObject card in playerCards)
        {
            Destroy(card);
        }
    }
}
