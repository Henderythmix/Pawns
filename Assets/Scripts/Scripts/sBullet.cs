using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sBullet : MonoBehaviour
{
    public float speed = 25;
    public string enemy;
    public float damage;
    private void Awake()
    {
        StartCoroutine("RemoveAfterTime");
    }

    // In the particular event where the bullet bugged out and did not hit a wall, clear it out anyways
    IEnumerator RemoveAfterTime()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(enemy))
        {
            //Debug.Log("Sending take damage");
            collision.gameObject.GetComponent<sAiController>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
