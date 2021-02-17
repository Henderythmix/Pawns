using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController : MonoBehaviour
{
    public KeyCode leavePlayerCharacter;
    public float characterSpeed = 5.0f;
    
    float attackCooldownTime;
    sAiController playerCharacterScript;
    bool attackOnCooldown = false;

    private void Awake()
    {
        playerCharacterScript = gameObject.GetComponent<sAiController>();
    }

    private void OnMouseDown()
    {
        playerCharacterScript.controlled = true;
        sCamera.instance.playerCharacterSelected = transform;
        attackCooldownTime = playerCharacterScript.aiType.attackRate;
        // Stops the player character from randomly shooting after just trying to click on the character.
        StartCoroutine("PlayerAttackCooldown", 0.05f);
    }

    private void Update()
    {
        if (playerCharacterScript.controlled)
        {
            Rotation();
            Move();
            
            if (Input.GetKey(leavePlayerCharacter))
            {
                sCamera.instance.playerCharacterSelected = null;
                playerCharacterScript.controlled = false;
            }

            if (Input.GetMouseButton(0) && !attackOnCooldown)
            {
                playerCharacterScript.Shoot();
                StartCoroutine("PlayerAttackCooldown", attackCooldownTime);
            }
        }


    }

    IEnumerator PlayerAttackCooldown(float time)
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
            if (vertical != 0 && horizontal != 0)
            {
                actualSpeed *= 0.5f;
            }

            Debug.Log(actualSpeed);
            transform.position += (transform.forward * vertical) * actualSpeed * Time.deltaTime;
            transform.position += (transform.right * horizontal) * actualSpeed * Time.deltaTime;
        }
    }

    void Rotation()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);

            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }
}
