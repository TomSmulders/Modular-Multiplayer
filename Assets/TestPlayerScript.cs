using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;

public class TestPlayerScript : NetworkBehaviour
{

    [SerializeField] float speed = 50;

    void Start()
    {
        transform.position += new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
    }

    void Update()
    {
/*        if (!IsOwner) { return; }

        Vector3 movementInput = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical"));
        movementInput *= speed * Time.deltaTime;

        if(Mathf.Abs(movementInput.x) + Mathf.Abs(movementInput.z) > 0)
        {
            MovePlayer_ServerRpc(movementInput);
        }*/
    }

    [ServerRpc(RequireOwnership = true)]
    void MovePlayer_ServerRpc(Vector3 _movementInput)
    {
        transform.position += _movementInput;

        Debug.Log("Moved " + gameObject.name + " to :  "+ transform.position + " by : " + SteamClient.Name);
    }
}
