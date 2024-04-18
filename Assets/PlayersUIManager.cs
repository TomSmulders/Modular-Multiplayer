using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersUIManager : MonoBehaviour
{
    public static PlayersUIManager instance;

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }


}
