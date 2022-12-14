using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public void SetAgentDestination(Transform newGoal)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.destination = newGoal.position;
    }
}
