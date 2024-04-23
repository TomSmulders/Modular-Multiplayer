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
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public Button InviteFriend;
    public RawImage profileImage;

    void Awake()
    {
        InviteFriend.onClick.AddListener(() =>
        {
            if (GameNetworkManager.instance.currentLobby.HasValue)
            {
                SendInviteFriend(GameNetworkManager.instance.currentLobby.Value);
            }
        });
    }

    public void UpdateFriendData()
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

    public void SendInviteFriend(Lobby _lobby)
    {
        Debug.Log("invited friend : " + _lobby.InviteFriend(this.steamId));
    }
}
