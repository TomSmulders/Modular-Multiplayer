using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MoveMentNetworkTransform : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = true)]
    void Syncmovement_serverRpc(Vector3 _movementInput)
    {
        transform.position += _movementInput;
    }

    [ServerRpc(RequireOwnership = true)]
    void Sync_quaternion_serverRpc(Quaternion _quaternionInput)
    {
        transform.rotation = _quaternionInput;
    }


    [ServerRpc(RequireOwnership = true)]
    void Teleport_serverRpc(Vector3 _teleportCordenets)
    {
        transform.position = _teleportCordenets;
    }
}