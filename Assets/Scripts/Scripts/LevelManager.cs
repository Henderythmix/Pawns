using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Data holder")]
    // Used for keeping track of all player characters and seeing if their dead or not.
    public List<PlayerData> playerCharactersGlobal;
    public Player[] playerCharacters;
    [Tooltip("These controls will be added to the character itself.")]
    public KeyCode[] controlsToSwitchCharacters;
    // Keeps track of all the current players merged instead of using the class for dynamic data.
    //public bool[] playerCharactersGlobalMerged;
    //public bool[] playerCharactersGlobalDead;
    public GameObject playerPrefab;
    // Used for keeping track of alive player characters
    public List<sPlayerController> playerCharactersSpawned;
    public List<sAiController> currentCache = new List<sAiController>();
    //Used for keeping track of the players money
    public float playersMoney;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        //if (playerCharactersGlobal.Length > 0)
        //{
        //    //Debug.Log(playerCharactersGlobal.Length);
        //    playerCharactersGlobalMerged = new bool[playerCharactersGlobal.Length];
        //    //playerCharactersGlobalDead = new bool[playerCharactersGlobal.Length];
        //}
        else Debug.LogError("No player types are set up in playerCharactersGlobal on the LevelManager script.");
        playerCharactersGlobal = new List<PlayerData>();
        if (playerCharactersSpawned.Count > 0) MergeAllStartingCharacters();
        else Debug.LogError("You didn't add the players into the player characters alive");
    }


    // TODO: Fix this so that we keep track of data
    void MergeAllStartingCharacters()
    {
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            PlayerData newPlayer = new PlayerData();

            newPlayer.damageOutput = playerCharacters[i].playerCharacterType.damageOutput;
            newPlayer.health = playerCharacters[i].playerCharacterType.health;
            newPlayer.maxHealth = playerCharacters[i].playerCharacterType.health;
            newPlayer.playerType = playerCharacters[i];
            playerCharactersGlobal.Add(newPlayer);
    
        }
        playerCharactersSpawned[0].thisPlayerData = playerCharactersGlobal[0];
        for (int i = 1; i < playerCharactersGlobal.Count; i++)
        {
            playerCharactersSpawned[0].MergePlayerCharacters(playerCharactersGlobal[i], null);
        }
        playerCharactersSpawned[0].playerAIScript.healthBar.parent = playerCharactersSpawned[0].playerAIScript.healthBarHolder;
        playerCharactersSpawned[0].playerAIScript.ResetupHealthBar();

    }

    //public void SetMerged(AI _typeMatcher)
    //{
    //    for (int i = 0; i < playerCharactersGlobal.Length; i++)
    //    {
    //        if (playerCharactersGlobal[i].playerCharacterType == _typeMatcher)
    //        {
    //            //playerCharactersGlobalMerged[i] = true;
    //        }
    //    }
    //}

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
