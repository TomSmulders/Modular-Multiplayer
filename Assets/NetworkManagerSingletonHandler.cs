using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking.Transport;

public class NetworkManagerSingletonHandler : MonoBehaviour
{
    public static GameObject networkManagerInstance;
    private void Awake()
    { 
        Debug.Log(networkManagerInstance);
        if(networkManagerInstance == null)
        {
            networkManagerInstance = gameObject;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
