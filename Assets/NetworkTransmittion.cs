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

    public void SyncPlayerData(List<PlayerData> _server_Players)
    {
        foreach (PlayerData server_player in _server_Players)
        {
            foreach (PlayerData player in GameNetworkManager.instance.players)
            {
                if(server_player.id == player.id)
                {
                    player.isReady = server_player.isReady;
                    if (player.isReady)
                    {
                        Debug.Log(player.username + " is now ready");
                    }
                    else
                    {
                        Debug.Log(player.username + " is no longer ready");
                    }
                }
            }
        }
    }

    [ServerRpc]
    public void UpdateListRPC(List<PlayerData> playerDataList)
    {
        foreach (var client in _networkObject.NetworkManager.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var targetNetworkBehaviour = client.PlayerObject.GetComponent<NetworkTransmittion>();
                if (targetNetworkBehaviour != null)
                {
                    targetNetworkBehaviour.ReceiveUpdatedList(playerDataList);
                }
            }
        }
    }

    [ClientRpc]
    public void ReceiveUpdatedList(List<PlayerData> playerDataList)
    {
        SyncPlayerData(playerDataList);
    }
}
