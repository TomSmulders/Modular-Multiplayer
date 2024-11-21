using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveDuplicates : MonoBehaviour
{
    public static List<string> names = new List<string>();

    private void Awake() 
    {
        if (names.Contains(gameObject.name))
        {
            Destroy(gameObject);
        }
        else
        {
            names.Add(gameObject.name);
        }
    }
}
