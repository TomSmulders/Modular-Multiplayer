using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] bool onStart = false;

    [SerializeField] string sceneName;

    void Start()
    {
        if (onStart)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
        
    public void Switch_Scene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void Refresh_Scene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
