using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController_Other : MonoBehaviour
{

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
            float YVelocity = Input.GetAxis("Vertical");
            float XVelocity = Input.GetAxis("Horizontal");

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
