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

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        UpdateButton();
    }

    public void OnValueChanged()
    {
        isReady = !isReady;
        UpdateButton();
    }

    void UpdateButton()
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
