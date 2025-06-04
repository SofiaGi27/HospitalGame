using UnityEngine;

public class IdleAtEndPathFollower : MonoBehaviour
{
    public Transform[] waypoints; // Lista de puntos
    public float speed = 2f;
    public float reachThreshold = 0.1f;

    private int currentIndex = 0;
    private bool isIdle = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isIdle || currentIndex >= waypoints.Length)
            return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        // Movimiento hacia el punto
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);

        // Animación de caminar (blend tree)
        animator.SetFloat("Speed", speed);

        // Si llegó al punto
        if (Vector3.Distance(transform.position, target.position) < reachThreshold)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Length)
            {
                // Detener movimiento y pasar a Idle
                animator.SetFloat("Speed", 0f);

                
                 animator.SetTrigger("Idle");

                isIdle = true;
            }
        }
    }
}
