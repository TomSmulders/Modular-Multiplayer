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

    //Ready state RPC's
    [ServerRpc(RequireOwnership = false)]
    public void Player_Changed_Ready_State_ServerRPC(ulong _id, bool _ready)
    {
        Change_Player_Ready_State_ClientRPC(_id, _ready);
        Check_If_All_Players_Are_Ready_ClientRPC();
    }

    [ClientRpc]
    public void Change_Player_Ready_State_ClientRPC(ulong _id, bool _ready ,ulong _clientid = 999999999999)
    {
        if(_clientid == 999999999999 || _clientid == NetworkManager.LocalClientId)
        {
            foreach (PlayerData player in GameNetworkManager.instance.players)
            {
                if (player.id == _id)
                {
                    GameManager.instance.Ready_Player_Up(player, _ready, false);
                }
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

    [ServerRpc(RequireOwnership = false)]
    public void Update_Players_That_Are_Ready_ServerRpc(ulong _clientId)
    {
        foreach (var player in GameNetworkManager.instance.players)
        {
            if (player.isReady)
            {
                Change_Player_Ready_State_ClientRPC(player.id,true,_clientId);
            }
        }
    }


    //Kick player RPC
    [ServerRpc(RequireOwnership = false)]
    public void I_Want_To_Kick_A_Player_ServerRPC(ulong _id)
    {
        Kick_Player_ClientRPC(_id);
    }

    [ClientRpc]
    public void Kick_Player_ClientRPC(ulong _id)
    {
        if(_id == GameNetworkManager.instance.me.id)
        {
            GameNetworkManager.instance.Disconnect();
        }
    }


}