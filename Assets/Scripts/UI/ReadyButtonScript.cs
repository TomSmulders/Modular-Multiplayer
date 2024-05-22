using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReadyButtonScript : MonoBehaviour
{
    [SerializeField] Color readyColor;
    [SerializeField] Color unreadyColor;

    Toggle toggle;
    TextMeshProUGUI text;
    bool isReady = false;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        Update_Button_Visuals();
    }

    public void On_Value_Changed()
    {
        isReady = !isReady;
        Update_Button_Visuals();

        if (GameNetworkManager.instance.me != null)
        {
            GameManager.instance.Ready_Player_Up(GameNetworkManager.instance.me, isReady, true);
        }
        else
        {
            isReady = !isReady;
            Update_Button_Visuals();
        }
    }

    public void Unready()
    {
        isReady = false;
        Update_Button_Visuals();
    }

    void Update_Button_Visuals()
    {
        toggle = GetComponent<Toggle>();
        if(TryGetComponent<Toggle>(out toggle))
        {
            ColorBlock block = toggle.colors;
            block.normalColor = isReady ? readyColor : unreadyColor;
            block.selectedColor = isReady ? readyColor : unreadyColor;
            block.highlightedColor = isReady ? readyColor : unreadyColor;
            block.pressedColor = !isReady ? readyColor : unreadyColor;

            toggle.colors = block;


            text.text = isReady ? "Ready" : "Not Ready";
        }
    }
}
