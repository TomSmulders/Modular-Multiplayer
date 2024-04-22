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

    public GameObject HostUI;
    public GameObject ClientUI;

    public void IsHost(bool _state)
    {
        HostUI.SetActive(_state);
        ClientUI.SetActive(!_state);
    }
}
