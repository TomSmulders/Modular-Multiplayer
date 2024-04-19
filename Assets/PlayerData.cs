using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
using Unity.Netcode;
using Unity.Networking.Transport;


public class PlayerData 
{
    public string username;
    public ulong id;
    public Friend friend;
    public GameObject playercard;
    public GameObject gameobject;
    public bool isOwner;
    public bool isReady;
    public Texture2D profilePicture;
}
