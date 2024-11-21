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
    public Lobby lobbyId;
    public string LobbyName;
    public TextMeshProUGUI lobbyNameText;

    public void Update_Lobby_Data()
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
    public void Join_Lobby()
    {
        GameNetworkManager.instance.Join_Public_Lobby(lobbyId);
    }
}
