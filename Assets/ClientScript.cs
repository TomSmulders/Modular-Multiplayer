using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class ClientScript : MonoBehaviour
{
    public string name;
    public CSteamID id;

    private void Start()
    {
        if (SteamManager.Initialized)
        {
            name = SteamFriends.GetPersonaName();
            id = SteamUser.GetSteamID();
        }
    }
}
