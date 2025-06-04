using UnityEngine;
using UnityEngine.AI;

public class NurseMovement : MonoBehaviour
{
    public Transform targetPoint;
    private Vector3 startPosition;
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        startPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        MoveToTarget(targetPoint.position);
    }

    void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // Rotación suave hacia la dirección del movimiento
        Vector3 direction = agent.velocity.normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending && speed < 0.1f)
        {
            animator.SetFloat("Speed", 0f);
            MoveToTarget(startPosition);
        }
    }

    void MoveToTarget(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
