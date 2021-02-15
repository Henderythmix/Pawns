using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerController : MonoBehaviour
{
    public AI playerStats;

    [Tooltip("The point in which the projectiles come out of.")]
    public Transform shootPoint;
    public aiState playerCharState;
    public Transform enemyFocusingOn;
    public bool playerUnit;

    Quaternion orginialRotation;


}
