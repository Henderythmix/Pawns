using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sCamera : MonoBehaviour
{
    public static sCamera instance;
    
    public float cameraMoveSpeed = 5f;
    public float cameraFollowSpeed = 10f;
    public Transform playerCharacterSelected;
    public float heightToFollowAt = 10f;
    public float offset = 5f;
    public KeyCode[] switchPlayerCharacters;

    [Header("Boundaries")]
    [Space(20)]
    public float xBoundaryMin = 10.5f;
    public float xBoundaryMax = 10.5f;
    public float zBoundaryMin = 8.5f;
    public float zBoundaryMax = 8.5f;

    float originalHeight;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        originalHeight = transform.position.y;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (!playerCharacterSelected)
        {
            Move();
            if (transform.position.y != originalHeight)
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, originalHeight, transform.position.z), cameraFollowSpeed * Time.deltaTime);
        }
        else
            FollowObject();

        if (switchPlayerCharacters.Length > 0)
        {
            if (switchPlayerCharacters.Length > 1)
            {
                for (int i = 0; i < switchPlayerCharacters.Length; i++)
                {
                   if (Input.GetKeyDown(switchPlayerCharacters[i]) && LevelManager.instance.playerCharactersAlive[i] != null)
                   {
                        LevelManager.instance.playerCharactersAlive[i].ControlThisCharacter();
                    }
                }
            }
            else
            {
                LevelManager.instance.playerCharactersAlive[0].ControlThisCharacter();
            }
        } 
    }

    void FollowObject()
    {
        //Debug.Log("Following object");
        Vector3 newPos = new Vector3(playerCharacterSelected.position.x, heightToFollowAt, playerCharacterSelected.position.z - offset);
        transform.position = Vector3.Lerp(transform.position, newPos, cameraFollowSpeed * Time.deltaTime);
    }

    void Move()
    {
        //Debug.Log("Moving around");
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        if (vertical != 0 || horizontal != 0)
        {
            float actualSpeed = cameraMoveSpeed;
            if (vertical != 0 && horizontal != 0)
            {
                actualSpeed /= 1.5f;
            }
            transform.position = new Vector3(Mathf.Clamp(transform.position.x + (actualSpeed * horizontal), xBoundaryMin, xBoundaryMax), 
                                            transform.position.y, 
                                            Mathf.Clamp(transform.position.z + (actualSpeed * vertical), zBoundaryMin, zBoundaryMax));
        }
    }
}
