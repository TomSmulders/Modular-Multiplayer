using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using UnityEngine.UI;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] Button serverButton;
    [SerializeField] Button clientButton;
    [SerializeField] Button hostButton;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        hostButton.onClick.AddListener(() =>
        {
            GameNetworkManager.instance.Start_Host(6 , GameNetworkManager.LobbyPublicityMode.Public);
        });
    }
}
