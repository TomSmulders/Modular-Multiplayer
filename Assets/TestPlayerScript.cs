using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class TestPlayerScript : MonoBehaviour
{
    NetworkObject networkObj;
    void Start()
    {
        transform.position += new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
        networkObj = GetComponent<NetworkObject>();
    }

    void Update()
    {
        if (!networkObj.IsLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Space))
        { 
            float rnd = Random.Range(0.2f, 3);
            transform.localScale = new Vector3(rnd, rnd, rnd);
            Debug.Log("changed scale for " + gameObject.name);
        }
    }
}
