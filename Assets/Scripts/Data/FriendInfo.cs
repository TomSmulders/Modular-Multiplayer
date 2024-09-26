using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Mono.Cecil;
using Steamworks.Data;

public class FriendInfo : MonoBehaviour
{
    //Data
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public Button InviteFriend;
    public RawImage profileImage;
    public GameObject isPlayingThisGameImage;

    void Awake()
    {
        InviteFriend.onClick.AddListener(() =>
        {
            if (GameNetworkManager.instance.currentLobby.HasValue)
            {
                Send_Party_Invite_To_Friend(GameNetworkManager.instance.currentLobby.Value);
            }
        });
    }
    public void Update_FriendData()
    {
        if (steamName == "")
        {
            playerName.text = "Empty";
        }
        else
        {
            playerName.text = steamName;
        }
    }
    public void Send_Party_Invite_To_Friend(Lobby _lobby)
    {
        Debug.Log("invited friend : " + _lobby.InviteFriend(this.steamId));
    }
}
