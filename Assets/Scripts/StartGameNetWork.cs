using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartGameNetWork : NetworkBehaviour
{
    public static StartGameNetWork instance;
    [SerializeField]
    GameObject playerPrefab;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        LoadDone_ServerRPC();
    }


    private int loadedPlayers = 0;
    [ServerRpc(RequireOwnership = false)] 
    public void LoadDone_ServerRPC()
    {
        loadedPlayers++;
        if (loadedPlayers >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            foreach (var player in NetworkManager.Singleton.ConnectedClients)
            {
                LoadDone_ServerRpc(player.Key);

                NetworkObject spawnedPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<NetworkObject>();
                spawnedPlayer.SpawnWithOwnership(player.Key);
            }
        }
    }

    [ServerRpc]
    public void LoadDone_ServerRpc(ulong _clientId)
    {
        Debug.Log("SpawnPlayer");
    }



    [ServerRpc(RequireOwnership = false)]
    public void IWantToDebugTextServerRPC(string text)
    {
        DebugTextClientRPC(text);
    }

    [ClientRpc]
    public void DebugTextClientRPC(string text)
    {
        Debug.Log($"Received text from server: {text}");
    }

    public void startGameTest()
    {
       

        IWantToDebugTextServerRPC("start game");
    }
}