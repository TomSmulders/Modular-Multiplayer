using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public ulong myClientID;

    private void Awake()
    {
        if(instance != null) { Destroy(this); } else { instance = this; }
    }
}
