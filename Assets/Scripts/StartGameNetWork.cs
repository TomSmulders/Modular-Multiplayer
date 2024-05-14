using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartGameNetWork : NetworkBehaviour
{
    public static StartGameNetWork instance;

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

        GetComponent<NetworkObject>().Spawn();
    }


    private int loadedPlayers = 0;


    [ServerRpc(RequireOwnership = false)]
    public void LoadDone_ServerRPC()
    {
        Debug.Log("test");
        //LoadDone_ClientRpc();
        loadedPlayers++;
        //if (loadedPlayers >= NetworkManager.Singleton.ConnectedClients.Count)
        //{
        //    foreach (var player in NetworkManager.Singleton.ConnectedClients)
        //    {
                
        //    }
        //}
    }

    [ClientRpc]
    public void LoadDone_ClientRpc()
    {
        Debug.Log("test");
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