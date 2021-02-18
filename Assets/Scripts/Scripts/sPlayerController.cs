using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController : MonoBehaviour
{
    public KeyCode leavePlayerCharacter;
    public KeyCode shortcutToThisCharacter;
    public KeyCode spawnPlayerCharacter;
    public float characterSpeed = 5.0f;
    [Tooltip("Temporary variable to see which controls feel better.")]
    public bool switchControls = false;
    public float damageOutput = 0;


    bool initalized = false;

    List<sAiController> friendliesNearby = new List<sAiController>();
    List<sAiController> friendliesCombined = new List<sAiController>();
    
    float attackCooldownTime;
    public sAiController playerAIScript;
    bool attackOnCooldown = false;

    
    [Header("Other Components")]
    [Tooltip("I setup the prefab so that the rotation happens to the model gameobject and not the main gameobject itself")]
    public Transform PlayerModel;
    [Tooltip("Animations! Can't forget those!")]
    public Animator playerAnimator;

    private void Awake()
    {
        // LevelManager.instance doesn't exist, then we're doing this in start
        if (LevelManager.instance) InitPlayer();

    }

    private void Start()
    {
        // Just so we don't initalize twice 
        if (!initalized) InitPlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<sPlayerController>())
        {
            friendliesNearby.Add(other.gameObject.GetComponent<sAiController>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<sPlayerController>())
        {
            friendliesNearby.Remove(other.gameObject.GetComponent<sAiController>());
        }
    }



    private void OnMouseDown()
    {
        ControlThisCharacter();
    }


    private void Update()
    {
        if (playerAIScript.controlled)
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
                        MergePlayerCharacters(friendliesNearby[i].aiType, friendliesNearby[i].health, friendliesNearby[i].maxHealth);
                        Destroy(friendliesNearby[i].gameObject);
                        
                    }
                }
                else
                {
                    MergePlayerCharacters(friendliesNearby[0].aiType, friendliesNearby[0].health, friendliesNearby[0].maxHealth);
                    Destroy(friendliesNearby[0].gameObject);
                }
                playerAIScript.healthBar.parent = playerAIScript.healthBarHolder;
                playerAIScript.ResetupHealthBar();
            }
        }


    }
    public void InitPlayer()
    {
        if (!playerAIScript)
            playerAIScript = gameObject.GetComponent<sAiController>();
        playerAIScript.InitAI(LevelManager.instance.playerCharactersGlobal[0].playerCharacterType, true);
        attackCooldownTime = playerAIScript.aiType.attackRate;
        initalized = true;
        //LevelManager.instance.playerCharactersAlive.Add(this);
        damageOutput = playerAIScript.aiType.damageOutput;
    }

    public void MergePlayerCharacters(AI _type, float _health, float _maxHealth)
    {
        damageOutput += _type.damageOutput;
        //playerAIScript.health += _health;
        playerAIScript.SetHealth(_health, false);
        playerAIScript.SetNewMaxHealth(_maxHealth, true, false, false);
        //playerAIScript.maxHealth += _maxHealth;
        LevelManager.instance.SetMerged(_type);
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


            Debug.Log(actualSpeed);
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
                transform.position = new Vector3(transform.position.x + (horizontal* actualSpeed * Time.deltaTime), transform.position.y, transform.position.z + (vertical * actualSpeed * Time.deltaTime));
                //Debug.Log(transform.position.x + " after transitioning");
            }
        }
        else if (playerAnimator)
            playerAnimator.SetBool("Moving", true);
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
