using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pillar : MonoBehaviour
{
    public int pillarLife = 100;
    public float speed = 10f;

    private Vector3 originalPosition;
    public GameObject pillarObject;
    
    private void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            Debug.Log("Enemy hit pillar");
            pillarLife -= 1;
            
            var randomPosition = new Vector3(originalPosition.x + Random.Range(-0.04f, 0.04f), originalPosition.y, originalPosition.z + Random.Range(-0.04f, 0.04f)); 
            pillarObject.transform.localPosition = randomPosition;
        }

        if (pillarLife > 0) return;
        pillarLife = 100;
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        this.transform.position += -this.transform.forward * (speed * Time.deltaTime);
    }
}
