using UnityEngine;

public class SonidoPasos : MonoBehaviour
{
    public AudioSource Pie;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Terrain")
        {
            Pie.Play();
        }
    }
}
