using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Steamworks.Data;
using TMPro;

public class LobbyData : MonoBehaviour
{   
    //data 
    //public SteamId lobbyid;
    public Lobby lobbyId;
    public string LobbyName;
    public TextMeshProUGUI lobbyNameText;

    

    public void UpdateLobbyData()
    { 
        if (LobbyName == "")
        {
                lobbyNameText.text = "Empty";
        }
        else
        {
            lobbyNameText.text = LobbyName;
        }
    }

    public void JoinLobby()
    {
        GameNetworkManager.instance.Join_Public_Lobby(lobbyId);
    }
}
