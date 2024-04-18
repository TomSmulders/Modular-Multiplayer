using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject lobbyCardPrefab, lobbyCardParent, playerCardPrefab, playerCardParent;
    public List<GameObject> lobbyCards = new List<GameObject>();

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

    public void AddPlayerToDictionary(ulong _clientId, string _steamName, ulong _steamId)
    {
        if (!playerInfo.ContainsKey(_clientId))
        {
            PlayerInfo _pi = Instantiate(playerCardPrefab).GetComponent<PlayerInfo>();
            _pi.gameObject.transform.SetParent(playerCardParent.transform);
            _pi.steamId = _steamId;
            _pi.steamName = _steamName;
            playerInfo.Add(_clientId, _pi.gameObject);
        }
    }

    public void RemovePlayerDromDictionary(ulong _steamId)
    {
        GameObject _value = null;
        ulong _key = 100;
        foreach (KeyValuePair<ulong, GameObject> _player in playerInfo)
        {
            if (_player.Value.GetComponent<PlayerInfo>().steamId == _steamId)
            {
                _value = _player.Value;
                _key = _player.Key;
            }
        }
        if (_key != 100)
        {
            playerInfo.Remove(_key);
        }
        if (_value != null)
        {
            Destroy(_value);
        }
    }

    public void UpdateClients()
    {
        foreach (KeyValuePair<ulong, GameObject> _player in playerInfo)
        {
            ulong _steamId = _player.Value.GetComponent<PlayerInfo>().steamId;
            string _steamName = _player.Value.GetComponent<PlayerInfo>().steamName;
            ulong _clientId = _player.Key;

            NetworkTransmittion.instance.UpdateClientsPlayerInfoClientRPC(_steamId, _steamName, _clientId);
        }
    }
}
