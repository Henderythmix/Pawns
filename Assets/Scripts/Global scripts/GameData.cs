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
public class PlayerData
{
    public float health = 0;
    public float maxHealth = 0;
    public float damageOutput = 0;
    public Player playerType;
    public bool dead = false;
}