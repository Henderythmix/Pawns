using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Data holder")]
    // Used for keeping track of all player characters and seeing if their dead or not.
    public Player[] playerCharactersGlobal;
    [Tooltip("These controls will be added to the character itself.")]
    public KeyCode[] controlsToSwitchCharacters;
    // Keeps track of all the current players merged instead of using the class for dynamic data.
    //public bool[] playerCharactersGlobalMerged;
    //public bool[] playerCharactersGlobalDead;

    public List<sPlayerController> deadPlayers = new List<sPlayerController>();
    public GameObject playerTemplate;
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

        //if (playerCharactersGlobal.Length > 0)
        //{
        //    //Debug.Log(playerCharactersGlobal.Length);
        //    playerCharactersGlobalMerged = new bool[playerCharactersGlobal.Length];
        //    //playerCharactersGlobalDead = new bool[playerCharactersGlobal.Length];
        //}
        else Debug.LogError("No player types are set up in playerCharactersGlobal on the LevelManager script.");

        if (playerCharactersAlive.Count > 0) MergeAllStartingCharacters();
        else Debug.LogError("You didn't add the players into the player characters alive");
    }

    void MergeAllStartingCharacters()
    {
        // Set this since we're calling this in awake of this function
        playerCharactersAlive[0].playerAIScript = playerCharactersAlive[0].gameObject.GetComponent<sAiController>();
        for (int i = 1; i < playerCharactersGlobal.Length; i++)
        {
            if (playerCharactersGlobal[i].playerCharacterType != playerCharactersAlive[0].playerAIScript.aiAgent)
            {
                sPlayerController playerTemplateScript = playerTemplate.GetComponent<sPlayerController>();

                // Create the new instances
                sPlayerController newPlayer;

                // Setup the data of the new instances
                newPlayer = playerTemplateScript;
                newPlayer.playerAIScript = playerTemplate.GetComponent<sAiController>();

                //newPlayer.InitPlayer();
                //newPlayer.playerAIScript.InitAI(playerCharactersGlobal[i].playerCharacterType, true);

                newPlayer.damageOutput = playerCharactersGlobal[i].playerCharacterType.damageOutput;
                newPlayer.playerAIScript.aiType = playerCharactersGlobal[i].playerCharacterType;
                newPlayer.playerAIScript.health = playerCharactersGlobal[i].playerCharacterType.health;
                newPlayer.playerAIScript.maxHealth = playerCharactersGlobal[i].playerCharacterType.health;

                //DEBUGS
                //Debug.Log(newPlayer.playerAIScript + "Player ai script");
                //Debug.Log(newPlayer.damageOutput + " damage output");
                //Debug.Log(newPlayer.playerAIScript.health+ " health");

                // Send off the new data
                playerCharactersAlive[0].MergePlayerCharacters(newPlayer);



            }
        }
        playerCharactersAlive[0].playerAIScript.healthBar.parent = playerCharactersAlive[0].playerAIScript.healthBarHolder;
        playerCharactersAlive[0].playerAIScript.ResetupHealthBar();

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
