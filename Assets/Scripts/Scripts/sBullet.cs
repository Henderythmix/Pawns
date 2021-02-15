using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sBullet : MonoBehaviour
{
    public float speed = 5;
    public LayerMask enemy;
    public float damage;
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer.Equals(enemy))
        {
            collision.gameObject.GetComponent<sAiController>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
