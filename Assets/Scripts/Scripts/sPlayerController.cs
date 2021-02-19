using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController : MonoBehaviour
{
    public KeyCode leavePlayerCharacter;
    //public KeyCode shortcutToThisCharacter;
    public KeyCode spawnCharacterHere;
    //public KeyCode spawnPlayerCharacter;
    public float characterSpeed = 5.0f;
    [Tooltip("Temporary variable to see which controls feel better.")]
    public bool switchControls = false;
    public float damageOutput = 0;
    public GameObject playerPrefab;

    // For the awake and start functions
    bool initalized = false;

    List<sPlayerController> friendliesNearby = new List<sPlayerController>();
    List<sPlayerController> friendliesCombined = new List<sPlayerController>();
    
    float attackCooldownTime;
    public sAiController playerAIScript;
    bool attackOnCooldown = false;

    
    [Header("Other Components")]
    [Tooltip("I setup the prefab so that the rotation happens to the model gameobject and not the main gameobject itself")]
    public Transform PlayerModel;
    [Tooltip("Animations! Can't forget those!")]
    public Animator playerAnimator;
    public bool freezeInput = false;

    private void Awake()
    {
        // LevelManager.instance doesn't exist, then we're doing this in start
        if (LevelManager.instance) InitPlayer();

    }

    private void Start()
    {
        // Just so we don't initalize twice 
        if (!initalized) InitPlayer();
        if (!playerAnimator) gameObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<sPlayerController>())
        {
            friendliesNearby.Add(other.gameObject.GetComponent<sPlayerController>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<sPlayerController>())
        {
            friendliesNearby.Remove(other.gameObject.GetComponent<sPlayerController>());
        }
    }



    private void OnMouseDown()
    {
        ControlThisCharacter();
    }


    private void Update()
    {
        if (playerAIScript.controlled && !freezeInput && Time.timeScale != 0)
        {
            Rotation();
            Move();
            
            if (Input.GetKey(leavePlayerCharacter) || sCamera.instance.playerCharacterSelected != transform)
            {
                if (sCamera.instance.playerCharacterSelected == transform)
                    sCamera.instance.playerCharacterSelected = null;
                playerAIScript.controlled = false;
            }

            if (Input.GetMouseButton(0) && !attackOnCooldown)
            {
                playerAIScript.Shoot();
                StartCoroutine("PlayerAttackCooldown", attackCooldownTime);
            }

            if (friendliesNearby.Count > 0)
            {
                if (friendliesNearby.Count > 1)
                {
                    for (int i = 0; i < friendliesNearby.Count; i++)
                    {
                        MergePlayerCharacters(friendliesNearby[i]);
                        Destroy(friendliesNearby[i].gameObject);
                    }
                }
                else
                {
                    MergePlayerCharacters(friendliesNearby[0]);
                    Destroy(friendliesNearby[0].gameObject);
                }
                playerAIScript.healthBar.parent = playerAIScript.healthBarHolder;
                playerAIScript.ResetupHealthBar();
            }

            if (Input.GetKeyDown(spawnCharacterHere) && friendliesCombined.Count > 0)
            {
                Debug.Log("Placing down playable character");
                SpawnCharacterHere();
            }
        }


    }

    public void SpawnCharacterHere()
    {
        // Spawn the character
        sAiController tmp = Instantiate(playerPrefab).GetComponent<sAiController>();
        tmp.health = friendliesCombined[0].playerAIScript.health;
        tmp.maxHealth = friendliesCombined[0].playerAIScript.maxHealth;
        tmp.GetComponent<sPlayerController>().damageOutput = friendliesCombined[0].damageOutput;
        tmp.transform.position = transform.position;
        tmp.transform.rotation = transform.rotation;

        // Remove everything off this player controller that was from that character
        playerAIScript.SetHealth(-friendliesCombined[0].playerAIScript.health, false);
        playerAIScript.SetNewMaxHealth(-friendliesCombined[0].playerAIScript.maxHealth, true, false, true);
        damageOutput -= friendliesCombined[0].damageOutput;

        // Finished spawning the character
        friendliesCombined.Remove(friendliesCombined[0]);
    }

    //IEnumerator PlayCellSplittingAnimation()
    //{

    //}

    //IEnumerator PlayMergeAnimation()
    //{

    //}

    //IEnumerator PlayUnmergeAnimation()
    //{
    //    freezeInput = true;
    //    while ()
    //    {

    //    }
    //}

    public void InitPlayer()
    {
        // Checking if this is on an instance or just being called
        if (!playerAIScript)
            playerAIScript = gameObject.GetComponent<sAiController>();

        playerAIScript.InitAI(LevelManager.instance.playerCharactersGlobal[0].playerCharacterType, true);
        attackCooldownTime = playerAIScript.aiType.attackRate;
        initalized = true;
        playerAIScript.stayPut = true;
        //LevelManager.instance.playerCharactersAlive.Add(this);
        damageOutput = playerAIScript.aiType.damageOutput;
    }

    public void MergePlayerCharacters(sPlayerController thePlayerCharacter)
    {
        damageOutput += thePlayerCharacter.damageOutput;
        //playerAIScript.health += _health;

        playerAIScript.SetHealth(thePlayerCharacter.playerAIScript.health, false);
        playerAIScript.SetNewMaxHealth(thePlayerCharacter.playerAIScript.maxHealth, true, false, false);
        //playerAIScript.maxHealth += _maxHealth;
        LevelManager.instance.SetMerged(thePlayerCharacter.playerAIScript.aiType);
        friendliesCombined.Add(thePlayerCharacter);
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
            if (switchControls)
            {
                // If we're using both the forward/backward and left/right keys to keep a consistent speed
                if (vertical != 0 && horizontal != 0)
                {
                    actualSpeed *= 0.5f;
                }
                if (playerAnimator)
                    playerAnimator.SetBool("Moving", true);
                transform.position += (transform.forward * vertical) * actualSpeed * Time.deltaTime;
                transform.position += (transform.right * horizontal) * actualSpeed * Time.deltaTime;
            }
            else
            {
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
            }
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
