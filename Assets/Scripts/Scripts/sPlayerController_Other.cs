using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController_Other : MonoBehaviour
{
    [Header("Controls")]
    public KeyCode North;
    public KeyCode South;
    public KeyCode West;
    public KeyCode East;

    [Header("Status")]
    public float MovementSpeed;
    public float ShootSlowness;

    [Header("Other")]
    public AI playerStats;

    [Tooltip("The point in which the projectiles come out of.")]
    public Transform shootPoint;
    public aiState playerCharState;
    public Transform enemyFocusingOn;
    public bool playerUnit;

    public bool BeingControlled;

    Quaternion orginialRotation;

    Transform PlayerPosition;
    public Transform PlayerModel;
    public Transform Direction;
    public GameObject LaserBolt;
    public Animator PlayerAnimator;

    void Start() {
        PlayerPosition = gameObject.GetComponent<Transform>();
    }

    void Update() {
        if (BeingControlled) {
            // Movement
            int XVelocity = 0;
            int YVelocity = 0;

            if (Input.GetKey(North)) {
                YVelocity = 1;
            } else if (Input.GetKey(South)) {
                YVelocity = -1;                
            } else {
                YVelocity = 0;
            }
            
            if (Input.GetKey(West)) {
                XVelocity = -1;
            } else if (Input.GetKey(East)) {
                XVelocity = 1;
            } else {
                XVelocity = 0;
            }

            PlayerPosition.position += new Vector3(XVelocity * MovementSpeed * Time.deltaTime, 0, YVelocity * MovementSpeed * Time.deltaTime);

            if (XVelocity != 0 || YVelocity != 0) {
                PlayerAnimator.SetBool("Moving", true);
                Direction.localPosition = new Vector3(XVelocity*1.5f, 0, YVelocity*1.5f);
            } else {
                PlayerAnimator.SetBool("Moving", false);
            }

            PlayerModel.LookAt(Direction);

            //Shooting
            if (Input.GetMouseButtonDown(0)) {
                StartCoroutine("Shooting"); // Still Kinda new with Coroutines, so expect them to look a bit like this :P
            }
            if (Input.GetMouseButtonUp(0)) {
                StopCoroutine("Shooting");
            }
        }
    }

    IEnumerator Shooting() {
        while (true) {
        Instantiate(LaserBolt, Direction.position, PlayerModel.rotation);

        yield return new WaitForSeconds(ShootSlowness);
        }
    }
}
