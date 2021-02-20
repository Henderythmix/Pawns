using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Type", menuName = "Scriptable Objects/Player Types")]
public class Player : ScriptableObject
{
    //public bool dead = false;
    // Checking when this character is currently combined with another. Also useful for if a level is suppose to start with all 4 characters out or all merged together.
    //public bool merged = false;
    public float costToRevive = 500;
    public AI playerCharacterType;
}