using UnityEngine;

public class DualNPCController : MonoBehaviour
{
    public GameObject npc1;
    public GameObject npc2;

    public Transform destination1;
    public Transform destination2;

    public float talkDuration = 3f;
    public float walkSpeed = 2f;
    public float rotationSpeed = 3f;
    public float talkAgainDuration = 3f;

    private Animator anim1;
    private Animator anim2;

    private bool isWalking = false;
    private bool npc1Arrived = false;
    private bool npc2Arrived = false;
    private bool isRotatingToFace = false;
    private float talkTimer = 0f;

    void Start()
    {
        anim1 = npc1.GetComponent<Animator>();
        anim2 = npc2.GetComponent<Animator>();

        // Primera conversación
        anim1.SetTrigger("Talk");
        anim2.SetTrigger("Talk");

        talkTimer = talkDuration;
    }

    void Update()
    {
        if (!isWalking)
        {
            talkTimer -= Time.deltaTime;
            if (talkTimer <= 0f)
            {
                isWalking = true;

                anim1.SetFloat("Speed", walkSpeed);
                anim2.SetFloat("Speed", walkSpeed);
            }
        }
        else if (!npc1Arrived || !npc2Arrived)
        {
            // Mover NPC1 y NPC2 hacia sus destinos
            if (!npc1Arrived)
                MoveToDestination(npc1, destination1, anim1, ref npc1Arrived);

            if (!npc2Arrived)
                MoveToDestination(npc2, destination2, anim2, ref npc2Arrived);
        }
        else if (!isRotatingToFace)
        {
            // Ambos llegaron, detener animaciones de caminar
            anim1.SetFloat("Speed", 0f);
            anim2.SetFloat("Speed", 0f);

            // Empezar rotación para mirarse mutuamente
            isRotatingToFace = true;
        }
        else
        {
            // Rotar NPCs para que se miren entre sí
            bool npc1Done = RotateTowards(npc1, npc2.transform.position);
            bool npc2Done = RotateTowards(npc2, npc1.transform.position);

            if (npc1Done && npc2Done)
            {
                // Ambos terminaron de girar, empezar animación de hablar otra vez
                anim1.SetTrigger("Talk");
                anim2.SetTrigger("Talk");

                enabled = false;
            }
        }
    }

    void MoveToDestination(GameObject npc, Transform destination, Animator anim, ref bool hasArrived)
    {
        npc.transform.position = Vector3.MoveTowards(npc.transform.position, destination.position, walkSpeed * Time.deltaTime);

        Vector3 dir = (destination.position - npc.transform.position).normalized;
        if (dir.magnitude > 0.01f)
            npc.transform.forward = Vector3.Lerp(npc.transform.forward, dir, Time.deltaTime * 5f);

        if (Vector3.Distance(npc.transform.position, destination.position) < 0.1f)
        {
            hasArrived = true;
        }
    }

    bool RotateTowards(GameObject npc, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - npc.transform.position).normalized;
        if (direction == Vector3.zero) return true; 

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        
        float angle = Quaternion.Angle(npc.transform.rotation, targetRotation);
        return angle < 1f;
    }
}
