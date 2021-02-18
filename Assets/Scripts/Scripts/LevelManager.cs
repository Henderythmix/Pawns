using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Data holder")]
    // Used for keeping track of all player characters and seeing if their dead or not.
    public Player[] playerCharactersGlobal;
    // Used for keeping track of alive player characters
    public List<sPlayerController> playerCharactersAlive;
    public List<sAiController> currentCache = new List<sAiController>();
    //Used for keeping track of the players money
    public float playersMoney;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (playerCharactersAlive.Count == 0) Debug.LogError("You didn't add the players into the player characters alive");
        else MergeAllStartingCharacters();
    }

    void MergeAllStartingCharacters()
    {
        for (int i = 0; i < playerCharactersGlobal.Length; i++)
        {
            if (playerCharactersGlobal[i].playerCharacterType != playerCharactersAlive[0].playerAIScript)
            {
                playerCharactersGlobal[i].merged = true;
            }
        }
    }

    //private void Update()
    //{
    //    if (playerCharactersAlive.Count == 0)
    //    {
    //        LoseGame();
    //    }
    //}

    public void LoseGame()
    {
        Time.timeScale = 0;
        cHudManager.instance.gameOverCanvas.SetActive(true);
    }
}
