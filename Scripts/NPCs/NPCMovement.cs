using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    public Transform destination; // Referencia al punto de destino

    void Start()
    {
        // Inicialización de los componentes
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Iniciar el movimiento hacia el destino
        MoveToPosition(destination.position);
    }

    void Update()
    {
        // Verifica si el NPC ha llegado al destino
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // Buscar otros NPCs cercanos (por ejemplo, el jugador)
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 2f);
            foreach (var col in nearbyColliders)
            {
                if (col.CompareTag("Player")) 
                {
                    // Detener el movimiento y activar la animación de hablar
                    agent.isStopped = true;
                    animator.SetTrigger("Talk");
                }
            }
        }
        else
        {
            // Si el NPC está moviéndose, activa la animación de caminar
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void MoveToPosition(Vector3 targetPosition)
    {
        // Establece el destino del NPC
        agent.SetDestination(targetPosition);
    }
}
