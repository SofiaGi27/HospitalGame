using UnityEngine;
using UnityEngine.AI;

public class SitOnChair : MonoBehaviour
{
    public Transform seatPoint;
    private NavMeshAgent agent;
    private Animator animator;
    private bool hasSat = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.SetDestination(seatPoint.position);
    }

    void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (!hasSat && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            // Detener el movimiento
            agent.isStopped = true;
            transform.rotation = Quaternion.LookRotation(seatPoint.forward); // Girar hacia el asiento
            animator.SetTrigger("SitDown");
            hasSat = true;
        }
    }
}
