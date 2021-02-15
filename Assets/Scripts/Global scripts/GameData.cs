using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum aiState
{
    FINDING,
    COMBAT,
    MERGING,
}

[System.Serializable]
public class AI
{
    public float health = 100;
    public float damageOutput = 25;
    public bool canSeeHiddenEnemies = false;
    public float movementSpeed = 5;
    public aiState currentState;
}