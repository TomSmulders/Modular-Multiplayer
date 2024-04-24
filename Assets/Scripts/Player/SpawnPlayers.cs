using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
   //[ServerRpc(RequireOwnership = false)]
   // void LoadDone_ServerRPC()

   // {

   //     loadedPlayers++;

   //     if (loadedPlayers >= NetworkManager.Singleton.ConnectedClients.Count)

   //     {

   //         foreach (var player in NetworkManager.Singleton.ConnectedClients)

   //         {

   //             Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Count)];

   //             spawnPoints.Remove(spawn);

   //             NetworkObject spawnedPlayer = Instantiate(playerPrefab, spawn.position, Quaternion.identity).GetComponent<NetworkObject>();

   //             spawnedPlayer.SpawnWithOwnership(player.Key);

   //         }
   //     }
   // }
}
