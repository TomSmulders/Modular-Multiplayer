using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;

public class TestPlayerScript : NetworkBehaviour
{
    void Start()
    {
        transform.position += new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsOwner)
        {
            float rnd = Random.Range(0.2f, 3);
            transform.localScale = new Vector3(rnd, rnd, rnd);
            Debug.Log("changed scale for " + gameObject.name + " " + SteamClient.Name);
        }
    }
}
