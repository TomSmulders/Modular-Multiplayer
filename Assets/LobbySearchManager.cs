using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Unity.Netcode;


public class LobbySearchManager : NetworkBehaviour
{
    public void GetLobbyListNoPassword()
    {
        SteamMatchmaking.AddRequestLobbyListStringFilter("Password", "", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.RequestLobbyList();


    }
}
