using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking.Transport;

public class NetworkTransmittion : NetworkBehaviour
{
    public static NetworkTransmittion instance;
    private NetworkObject _networkObject;

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
        _networkObject = GetComponent<NetworkObject>();
    }

    [Rpc(SendTo.Everyone)]
    public void ChangePlayerReadyUpStateRPC(ulong _id, bool _ready)
    {
        foreach (PlayerData player in GameNetworkManager.instance.players)
        {
            if(player.id == _id)
            {
                GameManager.instance.ReadyUp(player, _ready, false);
            }
        }
    }
}