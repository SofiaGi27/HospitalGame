using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Vector3 currentTarget;
    private Animator animator;

    void Start()
    {
        currentTarget = pointB.position;
        animator = GetComponent<Animator>();

        // Activar animación de caminar
        if (animator != null)
        {
            animator.Play("walk"); 
        }
    }

    void Update()
    {
        // Mover hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);

        // Mirar hacia el objetivo
        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        // Al llegar al destino, cambiar el objetivo
        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentTarget = (currentTarget == pointA.position) ? pointB.position : pointA.position;
        }
    }
}
