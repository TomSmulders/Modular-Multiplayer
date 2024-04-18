using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject lobbyCardPrefab, lobbyCardParent;
    public List<GameObject> lobbyCards = new List<GameObject>();

    public ulong myClientID;

    private void Awake()
    {
        if(instance != null) { Destroy(this); } else { instance = this; }
    }

    public void CreateLobbyCard(Lobby _lobbyId, string _lobbyName)
    {
        GameObject card = Instantiate(lobbyCardPrefab, transform.parent = lobbyCardParent.transform);

        LobbyData data = card.GetComponent<LobbyData>();
        data.LobbyName = _lobbyName;
        data.lobbyId = _lobbyId;
        data.UpdateLobbyData();

        lobbyCards.Add(card);
    }
}
