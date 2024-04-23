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

    //Ready state rpc's
    [ServerRpc(RequireOwnership = false)]
    public void Player_Changed_Ready_State_ServerRPC(ulong _id, bool _ready)
    {
        Change_Player_Ready_State_ClientRPC(_id, _ready);
        Check_If_All_Players_Are_Ready_ClientRPC();
    }

    [ClientRpc]
    public void Change_Player_Ready_State_ClientRPC(ulong _id, bool _ready)
    {
        foreach (PlayerData player in GameNetworkManager.instance.players)
        {
            if (player.id == _id)
            {
                GameManager.instance.Ready_Player_Up(player, _ready, false);
            }
        }
    }

    [ClientRpc]
    public void Check_If_All_Players_Are_Ready_ClientRPC()
    {
        bool ready = true;
        foreach (PlayerData player in GameNetworkManager.instance.players)
        {
            if (!player.isReady)
            {
                ready = false;
            }
        }
        GameNetworkManager.instance.Update_If_Players_Ready(ready);
    }
}