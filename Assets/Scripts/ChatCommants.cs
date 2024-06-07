using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChatCommants : NetworkBehaviour
{


    public void Teleport_Command(ChatCommand command)
    {
        ChatCommandVariable username = command.GetVariableByName("username");
        ChatCommandVariable Coordinates = command.GetVariableByName("Coordinates");
        
        //vann naam naar id

    }

    [ServerRpc(RequireOwnership = true)]
    void Teleport_serverRpc(Vector3 _teleportCoordinates, string name)
    {
        Teleport_ClientRpc(_teleportCoordinates, name);
    }

    [ClientRpc]
    void Teleport_ClientRpc(Vector3 _teleportCoordinates, string name)
    {
        if (name == SteamClient.Name)
        {
            transform.position = _teleportCoordinates;
            Debug.Log(SteamClient.Name + "teleported to" + _teleportCoordinates);
        }
    }

    //[ServerRpc(RequireOwnership = true)]
    //ulong? GetclientIdFromSteamName_serverRpc(Vector3 _teleportCoordinates, string name)
    //{
    //    ulong? id = (GetclientIdFromSteamName_ClientRpc(_teleportCoordinates, name);
    //    if (id.HasValue)
    //    {
    //        return id;
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    //[ClientRpc]
    //ulong? GetclientIdFromSteamName_ClientRpc(Vector3 _teleportCoordinates, string name)
    //{
    //    if (name == SteamClient.Name) 
    //    {
    //        return NetworkManager.LocalClientId;
    //    }
    //    return null;
    //}


    //public void GetClientIdfromName(string name)
    //{
    //    foreach (var client in )
    //    {
    //        if()
    //    }
    //}


}
