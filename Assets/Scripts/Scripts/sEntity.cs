using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sEntity : MonoBehaviour
{
    [Header("Player Status")]
    public float Health;
    public float MaxHealth;
    public float Defence;
    public float Strength;
    
    public bool isAlive;
    public bool CanMove;

    public GameObject Healthbar;

    void Update() {
        if (Health <= 0) {
            isAlive = false;
        } else {
            isAlive = true;
        }

        Healthbar.GetComponent<Transform>().localScale = new Vector3(2f*((float)Health/(float)MaxHealth), 0.3f, 0.3f);
    }

    public void Drop(GameObject obj, int amount) {
        for (int i=1; i <= amount; i++) {
            Instantiate(obj, gameObject.GetComponent<Transform>());
        }
    }

    public void Die() {
        Destroy(gameObject);
    }
}
