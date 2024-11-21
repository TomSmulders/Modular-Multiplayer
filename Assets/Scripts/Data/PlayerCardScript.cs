using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class PlayerCardScript : MonoBehaviour
{
    //Data
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public RawImage readyImage;
    public GameObject ownerImage;
    public bool isReady;
    public bool isOwner;
    public Button SettingsButton;
    [SerializeField] GameObject playerInfo, settings;
    [SerializeField] Texture2D settingsIcon, backicon;
    bool showingSettings = false;
    public RawImage profileImage;

    public void Update_Values()
    {
        readyImage.color = Color.red;
        playerName.text = steamName;
    }

    public void Toggle_Settings()
    {
        showingSettings = !showingSettings;

        settings.SetActive(showingSettings);
        playerInfo.SetActive(!showingSettings);
    }

    public void Ban()
    {

    }

    public void Kick()
    {

    }
}
