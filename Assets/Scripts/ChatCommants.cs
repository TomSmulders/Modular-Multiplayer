using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChatCommants : NetworkBehaviour
{
    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public void Teleport_Command(ChatCommand _command)
    {
        
        ChatCommandVariable username = _command.GetVariableByName("username");
        ChatCommandVariable coordinates = _command.GetVariableByName("Coordinates");

        Vector3 coordinate = StringToVector3(coordinates.ToString());

        if (username == null)
        {
            Teleport(coordinate);
        }
        else
        {
            Teleport_serverRpc(coordinate, username.ToString());
        }
    }

    void Teleport(Vector3 _teleportCoordinates)
    {
            transform.position = _teleportCoordinates;
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
