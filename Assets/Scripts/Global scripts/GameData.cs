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
public class Player
{
    public bool dead = false;
    // Checking when this character is currently combined with another. Also useful for if a level is suppose to start with all 4 characters out or all merged together.
    //public bool merged = false;
    public float costToRevive = 500;
    public AI playerCharacterType;
}