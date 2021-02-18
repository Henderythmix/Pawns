using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //public List<Transform> playableCharacters = new List<Transform>();
    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
        if (Time.timeScale == 0)
            Time.timeScale = 1;
    }
}
