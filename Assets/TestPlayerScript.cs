using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;

public class TestPlayerScript : NetworkBehaviour
{

    [SerializeField] float speed = 50;
    [SerializeField] GameObject Camholder;
    Camera camera;
    public Vector2 rotation;
    public Vector2 sensitivity;
    private float currentX = 0.0f;
    private float currentY = 0.0f;


    void Start()
    {
        transform.position += new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
        if (IsOwner) { Camholder.SetActive(true); }
        camera = Camholder.GetComponent<Camera>();
    }

    void Update()
    {
        if (!IsOwner) { return; }

        Vector3 movementInput = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical"));
        movementInput *= speed * Time.deltaTime;

        if(Mathf.Abs(movementInput.x) + Mathf.Abs(movementInput.z) > 0)
        {
            //MovePlayer_ServerRpc(movementInput);
            transform.position += movementInput;

            //Debug.Log("Moved " + gameObject.name + " to :  " + transform.position + " by : " + SteamClient.Name);
        }

        currentX += Input.GetAxis("Mouse X") * sensitivity.x * Time.deltaTime;
        currentY += Input.GetAxis("Mouse Y") * sensitivity.y * Time.deltaTime;

        Debug.Log(currentX);
        Debug.Log(currentY);

        transform.eulerAngles += new Vector3(0, currentY * Time.deltaTime * sensitivity.y, 0);
        camera.transform.localRotation = Quaternion.Euler(currentX, 0, 0);
    }

    [ServerRpc(RequireOwnership = true)]
    void MovePlayer_ServerRpc(Vector3 _movementInput)
    {
        transform.position += _movementInput;

        Debug.Log("Moved " + gameObject.name + " to :  "+ transform.position + " by : " + SteamClient.Name);
    }
}
