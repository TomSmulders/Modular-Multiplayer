using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking.Transport;

public class NetworkTransmittion : NetworkBehaviour
{
    public static NetworkTransmittion instance;

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

    [ServerRpc(RequireOwnership = false)]
    public void ChangePlayerReadyUpStateServerRPC(ulong _id, bool _ready)
    {
        foreach (PlayerData player in GameNetworkManager.instance.players)
        {
            if(player.id == _id)
            {
                GameManager.instance.ReadyUp(player, _ready, false);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DebugTextClientRPC(string text)
    {
        Debug.Log($"Received text from server: {text}");
    }

    public void SendDebugText(string text)
    {
        if (IsClient || IsHost)
        {
            DebugTextClientRPC(text);
        }
    }
}