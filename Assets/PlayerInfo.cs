using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public RawImage readyImage;
    public GameObject ownerImage;
    public bool isReady;
    public bool isOwner;
    public Button kickButton;
    public RawImage profileImage;

    public void UpdateValues()
    {
        readyImage.color = Color.red;
        playerName.text = steamName;
    }

}
