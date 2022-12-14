using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Transform goal;
    
    private Vector3 thisPosition;
    
    void Start()
    {
        thisPosition = transform.position;
        
        StartCoroutine(nameof(Spawn));
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            var randomPosition = new Vector3(thisPosition.x +Random.Range(-10, 10), 0, thisPosition.z + Random.Range(-10, 10));
            
            var enemyGameObject = Instantiate(enemyPrefab, randomPosition, Quaternion.identity);

            enemyGameObject.GetComponent<Enemy>().SetAgentDestination(goal);

            yield return new WaitForSeconds(3);
        }
    }
}
