using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularPlayer_FollowObject : MonoBehaviour
{
    public Transform ObjectToFollow;
    private void FixedUpdate()
    {
        transform.position = ObjectToFollow.position;
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), -transform.up - new Vector3(0, 10, 0), Color.red, 10);
    }
}
