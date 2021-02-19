using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum aiState
{
    FINDING,
    COMBAT,
    MERGING,
}


[CreateAssetMenu(fileName = "New AI Type", menuName = "Scriptable Objects/AI Types")]
public class AI : ScriptableObject
{
    //[TextAreaAttribute(5,5)]
    //public string name;   
    [Space(15)]
    [Tooltip("How much health does this AI start with?")]
    public float health = 100;
    [Tooltip("What's the damage output of this AI?")]
    public float damageOutput = 25;
    [Tooltip("Can this ai see hidden enemies if the enemy is hidden?")]
    public bool canSeeHiddenEnemies = false;
    [Tooltip("How fast this ai can move. This will only be for enemies")]
    public float movementSpeed = 5;
    [Tooltip("Can this ai be seen by enemies")]
    public bool isHidden = false;
    [HideInInspector] public aiState currentState;
    [Tooltip("How much time to wait in between attacks")]
    public float attackRate = 0.25f;
    public float rewardForEliminating = 0;
    public string enemy = "None";
    //public LayerMask enemy;
}

[CreateAssetMenu(fileName = "New Player Type", menuName = "Scriptable Objects/Player Types")]
public class Player : ScriptableObject
{
    //public bool dead = false;
    // Checking when this character is currently combined with another. Also useful for if a level is suppose to start with all 4 characters out or all merged together.
    //public bool merged = false;
    public float costToRevive = 500;
    public AI playerCharacterType;
}