using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public GameObject readyImage;
    public GameObject ownerImage;
    public bool isReady;
    public bool isOwner;
    public Button kickButton;

    public void UpdateValues()
    {
        readyImage.SetActive(false);
        playerName.text = steamName;
    }
}
