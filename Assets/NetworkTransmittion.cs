using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
    public void AddMeToDictionaryServerRPC(ulong _steamId, string _steamName, ulong _clientId)
    {
        GameManager.instance.AddPlayerToDictionary(_clientId, _steamName, _steamId);
        GameManager.instance.UpdateClients();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMeFromDictionaryServerRPC(ulong _steamId)
    {
        RemovePlayerFomDictionaryClientRPC(_steamId);
    }

    [ClientRpc]
    private void RemovePlayerFomDictionaryClientRPC(ulong _steamId)
    {
        GameManager.instance.RemovePlayerDromDictionary(_steamId);
    }

    [ClientRpc]

    public void UpdateClientsPlayerInfoClientRPC(ulong _steamId, string _steamName, ulong _clientId)
    {
        GameManager.instance.AddPlayerToDictionary(_clientId, _steamName, _steamId);
    }

}
