using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cCacheUI : MonoBehaviour
{
    public Transform healthBar;
    public Transform healthBarHolder;

    private sAiController thisAI;

    private void Start()
    {
        thisAI = gameObject.GetComponent<sAiController>();
        SetNewHealthBarSize();
    }

    void SetNewHealthBarSize()
    {
        Debug.Log(thisAI.health);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<sBullet>())
        {
            SetNewHealthBarSize();
        }
    }
}
