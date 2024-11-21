using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameSettings : MonoBehaviour
{
    //Global game settings
    public static GlobalGameSettings instance;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this); }
        DontDestroyOnLoad(gameObject);
    }

    public string gameName;
    public int maxPartySize = 5;
}
