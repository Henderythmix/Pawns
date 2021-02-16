using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController : MonoBehaviour
{
    [Header("Controls")]
    public KeyCode North;
    public KeyCode South;
    public KeyCode West;
    public KeyCode East;

    [Header("Status")]
    public float Speed;

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

    void Start() {
        PlayerPosition = gameObject.GetComponent<Transform>();
    }

    void Update() {
        if (BeingControlled) {

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

            PlayerPosition.position += new Vector3(XVelocity * Speed * Time.deltaTime, 0, YVelocity * Speed * Time.deltaTime);
        }
    }

}
