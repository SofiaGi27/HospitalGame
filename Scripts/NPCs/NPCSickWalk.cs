using UnityEngine;
using UnityEngine.AI;

public class NPCSickWalk : MonoBehaviour
{
    public Transform destinationPoint;

    private NavMeshAgent agent;
    private Vector3 originalPosition;
    private bool goingToDestination = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalPosition = transform.position;

        if (destinationPoint != null)
        {
            agent.SetDestination(destinationPoint.position);
        }
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (goingToDestination)
            {
                agent.SetDestination(originalPosition);
                goingToDestination = false;
            }
            else
            {
                agent.SetDestination(destinationPoint.position);
                goingToDestination = true;
            }
        }
    }
}
