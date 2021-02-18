using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sBullet : MonoBehaviour
{
    public float speed = 25;
    public string enemy;
    public float damage;
    public float life;

    void Start() {
        StartCoroutine("TookTooLong");
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

    IEnumerator TookTooLong() {
        yield return new WaitForSeconds(life);
        Destroy(gameObject);
    }
}
