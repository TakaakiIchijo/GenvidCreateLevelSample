using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public int life = 100;

    private GameObject parent;

    private void Awake()
    {
        parent = gameObject.transform.parent.gameObject;
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Pillar"))
        {
            life -= 1;

            if (life < 0)
            {
                Destroy(parent);
            }
        }
    }
}
