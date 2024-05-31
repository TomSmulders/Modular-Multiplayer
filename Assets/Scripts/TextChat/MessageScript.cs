using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageScript : MonoBehaviour
{
    public string sender;
    public string content;
    public Color textColor;

    TextMeshProUGUI tmpro;

    public void Init(string _s, string _t, Color _c)
    {
        this.sender = _s;
        this.content = _t;
        this.textColor = _c;

        tmpro = GetComponentInChildren<TextMeshProUGUI>(); 

        float preferredHeight = tmpro.GetPreferredValues().y;
        Debug.Log(preferredHeight);
        RectTransform rectTransform = GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, preferredHeight);

        tmpro.color = textColor;
        tmpro.text = sender + ": " + content;
    }
}
