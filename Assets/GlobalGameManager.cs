using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.SceneManagement;

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager instance;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this); }
        DontDestroyOnLoad(gameObject);

        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    public Lobby? currentLobby;

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {

    }

    private void Singleton_OnClientDisconnectCallback(ulong clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if (clientId == 0)
        {
            Debug.Log("disconnect");
            Disconnect_Player(currentLobby);
        }
    }

    private void OnApplicationQuit()
    {
        if (currentLobby.HasValue)
        {
            Disconnect_Player(currentLobby.Value);
        }
    }

    public void Disconnect_Player(Lobby? _LobbyToLeave)
    {
        _LobbyToLeave?.Leave();
        if (NetworkManager.Singleton == null) 
        {
            return;
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        }


        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("Disconnected");

        currentLobby = _LobbyToLeave;
    }
}
