using UnityEngine;

public class SmoothPathFollower : MonoBehaviour
{
    public Transform[] waypoints; // Lista de puntos
    public float speed = 2f;
    public float reachThreshold = 0.1f;

    private int currentIndex = 0;
    private bool isSitting = false;
    private Animator animator;

    void Start()
    {
        // Obtener el Animator automáticamente del GameObject
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isSitting || currentIndex >= waypoints.Length)
            return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        // Movimiento hacia el punto
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f);

        // Animación de caminar
        animator.SetFloat("Speed", speed);

        // Si llegó al punto
        if (Vector3.Distance(transform.position, target.position) < reachThreshold)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Length)
            {
                // Gira el personaje manualmente (ejemplo: 180° en Y)
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);

                // Detiene la animación de caminar
                animator.SetFloat("Speed", 0f);

                // Ejecuta la animación de sentarse
                animator.SetTrigger("Sit");

                isSitting = true;
            }
        }
    }
}
