using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sPlayerController : MonoBehaviour
{
    public KeyCode leavePlayerCharacter;
    public KeyCode spawnCharacterHere;
    public KeyCode mergeCharactersButton;
    public LayerMask playerMask;
    //public KeyCode spawnPlayerCharacter;
    public float characterSpeed = 5.0f;
    //[Tooltip("Temporary variable to see which controls feel better.")]
    //public bool switchControls = false;
    public GameObject playerPrefab;

    public float tmpSpeed = 5;
    [Tooltip("How much time does it take for the player to be able to spawn or merge again")]
    public float waitToSpawn = 0.5f;

    // For the awake and start functions
    bool initalized = false;

    float attackCooldownTime;
    bool attackOnCooldown = false;
    bool cooldown = false;
    // Use for when the player can't control the character, aka merging and spawning

    
    [Header("Other Components")]
    [Tooltip("I setup the prefab so that the rotation happens to the model gameobject and not the main gameobject itself")]
    public Transform PlayerModel;
    [Tooltip("Animations! Can't forget those!")]
    public Animator playerAnimator;

    [Header("Public for debugging only, dont touch")]
    [Space(20)]
    public sAiController playerAIScript;
    public float damageOutput = 0;
    public PlayerData thisPlayerData;
    public List<sPlayerController> friendliesNearby;
    public List<PlayerData> friendliesCombined;
    public bool freezeInput = false;
    public KeyCode shortcutToThisCharacter;
    public bool invulnerable = false;

    //private int charactersHealthBeingUsed = 1;

    private void Awake()
    {
        // LevelManager.instance doesn't exist, then we're doing this in start
        //if (LevelManager.instance) InitPlayer();
        friendliesCombined = new List<PlayerData>();
    }

    private void Start()
    {
        // Just so we don't initalize twice 
        if (!initalized) InitPlayer();
        //if (!playerAnimator) gameObject.GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        ControlThisCharacter();
    }


    IEnumerator DisplayTimer(float time)
    {
        cooldown = true;
        cHudManager.instance.timerText.gameObject.SetActive(true);
        while (time > 0)
        {
            time -= Time.deltaTime;
            float timeDisplay = Mathf.Round(time * 10f) / 10f;
            cHudManager.instance.timerText.text = timeDisplay.ToString();
            yield return new WaitForEndOfFrame();
        }
        cHudManager.instance.timerText.gameObject.SetActive(false);
        cooldown = false;
    }

    private void Update()
    {

        if (!playerAIScript.controlled && playerAnimator.GetBool("Moving")) playerAnimator.SetBool("Moving", false);
        if (playerAIScript.controlled)
        {
            if (Input.GetKey(leavePlayerCharacter) || sCamera.instance.playerCharacterSelected != transform)
            {
                if (sCamera.instance.playerCharacterSelected == transform)
                    sCamera.instance.playerCharacterSelected = null;
                playerAIScript.controlled = false;
            }

            if (!freezeInput && Time.timeScale != 0)
            {
                Rotation();
                Move();

                //if (Input.GetKey(leavePlayerCharacter) || sCamera.instance.playerCharacterSelected != transform)
                //{
                //    if (sCamera.instance.playerCharacterSelected == transform)
                //        sCamera.instance.playerCharacterSelected = null;
                //    playerAIScript.controlled = false;
                //}

                if (Input.GetMouseButton(0) && !attackOnCooldown)
                {
                    playerAIScript.Shoot();
                    StartCoroutine("PlayerAttackCooldown", attackCooldownTime);
                }

                Collider[] playerCharacters = Physics.OverlapSphere(transform.position, 10, playerMask);
                friendliesNearby = new List<sPlayerController>();
                foreach (Collider player in playerCharacters)
                {
                    // If it's a player (known by the tag name)
                    if (player.gameObject.CompareTag(gameObject.tag) && player.gameObject != gameObject)
                    {
                        friendliesNearby.Add(player.GetComponent<sPlayerController>());
                    }
                }

                if (friendliesNearby.Count > 0 && Input.GetKeyDown(mergeCharactersButton) && !cooldown)
                {
                    Debug.Log("Merging characters");
                    StartCoroutine(PlayMergingAnimation(friendliesNearby));
                    StartCoroutine(DisplayTimer(waitToSpawn));
                }


                if (Input.GetKeyDown(spawnCharacterHere) && friendliesCombined.Count > 0 && !cooldown)
                {
                    Debug.Log("Placing down playable character");
                    //SpawnCharacterHere();
                    StartCoroutine(StartSpawningAnim());
                    StartCoroutine(DisplayTimer(waitToSpawn));
                }
            }
            
        }

        if (Input.GetKeyDown(shortcutToThisCharacter))
            ControlThisCharacter();
    }

    public void PlayerTakeDamage(float damage)
    {

        // Don't take damage if invulnerable
        if (invulnerable) return;

        if (friendliesCombined.Count > 0)
        {
            friendliesCombined[0].health -= damage;
            if (friendliesCombined[0].health <= 0)
            {
                int idx = 0;
                // Search for the index that the friendlies combined is using
                for (int i = 0; i < LevelManager.instance.playerCharactersGlobal.Count; i++)
                {
                    if (LevelManager.instance.playerCharactersGlobal[i] == friendliesCombined[0])
                        idx = i;
                }

                LevelManager.instance.playerCharactersGlobal[idx].dead = true;
                friendliesCombined.Remove(friendliesCombined[0]);
                //LevelManager.instance.deadPlayers.Add(friendliesCombined[0]);
            }
        }

        playerAIScript.AiTakeDamage(damage);
    }


    public sPlayerController SpawnCharacterHere()
    {
        // Spawn the character
        sPlayerController spawnedPlayer = Instantiate(playerPrefab).GetComponent<sPlayerController>();
        sAiController tmp = spawnedPlayer.transform.GetChild(2).GetComponent<sAiController>();
        tmp.health = friendliesCombined[0].health;
        Rigidbody rb = spawnedPlayer.GetComponent<Rigidbody>();
        Rigidbody rb2 = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb2.constraints = RigidbodyConstraints.FreezeAll;
        tmp.maxHealth = friendliesCombined[0].maxHealth;
        spawnedPlayer.damageOutput = friendliesCombined[0].damageOutput;
        tmp.transform.position = transform.position;
        tmp.transform.rotation = transform.rotation;

        // Remove everything off this player controller that was from that character
        playerAIScript.SetHealth(-friendliesCombined[0].health, false);
        playerAIScript.SetNewMaxHealth(-friendliesCombined[0].maxHealth, true, false, true);
        damageOutput -= friendliesCombined[0].damageOutput;

        // Finished spawning the character
        LevelManager.instance.playerCharactersSpawned.Add(spawnedPlayer);
        friendliesCombined.Remove(friendliesCombined[0]);

        return spawnedPlayer;
    }

    IEnumerator PlayMergingAnimation(List<sPlayerController> playerCharacters)
    {
        freezeInput = true;
        //Setup animations 
        playerAnimator.SetTrigger("Spawning");
        playerAnimator.SetBool("Leaving", false);
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            //Destroy(playerCharacters[i].GetComponent<Rigidbody>());
            Destroy(playerCharacters[i].GetComponent<BoxCollider>());
            //playerCharacters[i].GetComponent<CapsuleCollider>().isTrigger = true;
            playerCharacters[i].playerAnimator.SetTrigger("Spawning");
            playerCharacters[i].playerAnimator.SetBool("Leaving", false);
        }

        //int remainingToEnter = playerCharacters.Count;

        List<sPlayerController> mergedCharacters;
        // Move the characters towards the selected player character
        while (playerCharacters.Count > 0)
        {
            mergedCharacters = new List<sPlayerController>();
            //Debug.Log(playerCharacters.Count);
            for (int i = 0; i < playerCharacters.Count; i++)
            {
                playerCharacters[i].transform.position = Vector3.MoveTowards(playerCharacters[i].transform.position, transform.position, tmpSpeed * Time.deltaTime);
                if (Vector3.Distance(playerCharacters[i].transform.position, transform.position) < 0.1f)
                {
                    mergedCharacters.Add(playerCharacters[i]);
                }
            }
            if (mergedCharacters.Count > 0)
            {
                for (int i = 0; i < mergedCharacters.Count; i++)
                {
                    sPlayerController playerCharacter = mergedCharacters[i];
                    playerCharacters.Remove(playerCharacter);
                    MergePlayerCharacters(playerCharacter.thisPlayerData, playerCharacter);
                    //remainingToEnter--;
                    Destroy(playerCharacter.gameObject);
                }
            }
            yield return new WaitForEndOfFrame();
        }
        //yield return new WaitForSeconds(0.05f);
        // Fades back in.
        playerAnimator.SetTrigger("Starting Spawn");
        playerAnimator.SetBool("Leaving", true);
        freezeInput = false;
    }

    IEnumerator PlaySpawningAnimation(sPlayerController spawnedPlayer)
    {
        spawnedPlayer.playerAnimator.SetTrigger("Starting Spawn");
        spawnedPlayer.freezeInput = true;
        playerAnimator.SetTrigger("Starting Spawn");
        Vector3 spawnPos = spawnedPlayer.transform.position + spawnedPlayer.transform.right + new Vector3(2, 0, 2);
        while (Vector3.Distance(spawnPos, spawnedPlayer.transform.position) > 0.25f)
        {
            spawnedPlayer.transform.position = Vector3.MoveTowards(spawnedPlayer.transform.position, spawnPos, 5 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        spawnedPlayer.freezeInput = false;
        spawnedPlayer.invulnerable = false;
        Rigidbody rb = spawnedPlayer.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

        Rigidbody rb2 = gameObject.GetComponent<Rigidbody>();
        rb2.constraints = RigidbodyConstraints.None;
        rb2.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        freezeInput = false;
        invulnerable = false;

        spawnedPlayer.InitPlayer();
    }
    
    IEnumerator StartSpawningAnim()
    {
        freezeInput = true;
        invulnerable = true;
        playerAnimator.SetTrigger("Spawning");
        yield return new WaitForSeconds(0.05f);
        sPlayerController spawnedPlayer = SpawnCharacterHere();
        StartCoroutine(PlaySpawningAnimation(spawnedPlayer));
    }

    public void InitPlayer()
    {

        playerAIScript.InitAI(thisPlayerData.playerType.playerCharacterType, true);
        attackCooldownTime = playerAIScript.aiType.attackRate;
        initalized = true;
        playerAIScript.player = true;
;
    }

    public void MergePlayerCharacters(PlayerData thePlayerData, sPlayerController thePlayer)
    {
        if (thePlayer)
        {
            if (thePlayer.friendliesCombined.Count > 0)
            {
                for (int i = 0; i < thePlayer.friendliesCombined.Count; i++)
                {
                    friendliesCombined.Add(thePlayer.friendliesCombined[i]);
                }
            }
        }

        damageOutput += thePlayerData.damageOutput;
        //playerAIScript.health += _health;

        playerAIScript.SetHealth(thePlayerData.health, false);
        playerAIScript.SetNewMaxHealth(thePlayerData.maxHealth, true, false, false);
        friendliesCombined.Add(thePlayerData);
        //StartCoroutine(DisplayTimer(waitToSpawn));
    }

    public void ControlThisCharacter()
    {
        playerAIScript.controlled = true;
        sCamera.instance.playerCharacterSelected = transform;
        // Stops the player character from randomly shooting after just trying to click on the character.
        StartCoroutine("PlayerAttackCooldown", 0.05f);
    }


    public IEnumerator PlayerAttackCooldown(float time)
    {
        attackOnCooldown = true;
        yield return new WaitForSeconds(time);
        attackOnCooldown = false;
    }

    void Move()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        if (vertical != 0 || horizontal != 0)
        {
            float actualSpeed = characterSpeed;


            //Debug.Log(actualSpeed);
            //if (switchControls)
            //{
            //    // If we're using both the forward/backward and left/right keys to keep a consistent speed
            //    if (vertical != 0 && horizontal != 0)
            //    {
            //        actualSpeed *= 0.5f;
            //    }
            //    if (playerAnimator)
            //        playerAnimator.SetBool("Moving", true);
            //    transform.position += (transform.forward * vertical) * actualSpeed * Time.deltaTime;
            //    transform.position += (transform.right * horizontal) * actualSpeed * Time.deltaTime;
            //}
            //else
            //{
            if (vertical != 0 && horizontal != 0)
            {
                actualSpeed *= 0.75f;
            }
            //Debug.Log(transform.position.x + " before transitioning");
            //Debug.Log(horizontal);

            if (playerAnimator)
                playerAnimator.SetBool("Moving", true);
            transform.position = new Vector3(transform.position.x + (horizontal* actualSpeed * Time.deltaTime), transform.position.y, transform.position.z + (vertical * actualSpeed * Time.deltaTime));
                //Debug.Log(transform.position.x + " after transitioning");
            //}
        }
        else if (playerAnimator)
            playerAnimator.SetBool("Moving", false);
    }

    void Rotation()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);

            if (PlayerModel)
                PlayerModel.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
            else
                transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }
}
