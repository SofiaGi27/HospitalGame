using UnityEngine;

public class NPCMovimiento : MonoBehaviour
{
    public Transform target; // Punto al que caminará el NPC
    public float speed = 2f;
    public float stoppingDistance = 1f;

    private Animator animator;
    private bool hasTalked = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        // Si aún no ha llegado al objetivo
        if (distance > stoppingDistance)
        {
            // Movimiento hacia el objetivo
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.LookAt(target);

            // Activar animación de caminar
            animator.SetFloat("Speed", 1f);
        }
        else
        {
            // Detener animación de caminar
            animator.SetFloat("Speed", 0f);

            // Si aún no ha hablado, activa animación de hablar
            if (!hasTalked)
            {
                animator.SetTrigger("Talk");
                hasTalked = true;
            }
        }
    }
}
