using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform parentTransform;
    public int columns = 4;
    public float spacing = 2.5f;
    public float rowHeight = 1.2f;

    void Start()
    {
        int totalCharacters = characterPrefabs.Length;
        int rows = Mathf.CeilToInt((float)totalCharacters / columns);
        Vector3 offset = new Vector3((columns - 1) * spacing / 2f, 0, -(rows - 1) * spacing / 2f);

        Vector3 totalPosition = Vector3.zero;

        for (int i = 0; i < totalCharacters; i++)
        {
            int row = i / columns;
            int column = i % columns;

            Vector3 localPosition = new Vector3(column * spacing, row * rowHeight, -row * spacing);
            Vector3 position = localPosition - offset;

            GameObject character = Instantiate(characterPrefabs[i], position, Quaternion.identity, parentTransform);

            Debug.Log($"Personaje {i} instanciado en: {position}");

            CapsuleCollider collider = character.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.center = new Vector3(0, 1, 0);
                collider.radius = 0.5f;
                collider.height = 2f;
                collider.transform.rotation = character.transform.rotation;
            }

            // Medidas del tamaño del personaje instanciado
            Renderer renderer = character.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Vector3 size = renderer.bounds.size;
                Debug.Log($"Tamaño del personaje {i}: {size}");
            }
            else
            {
                Debug.LogWarning($"Personaje {i} no tiene Renderer.");
            }

            CharacterClickHandler clickHandler = character.AddComponent<CharacterClickHandler>();
            clickHandler.characterIndex = i;

            totalPosition += position;
        }

        Vector3 averagePosition = totalPosition / totalCharacters;
        Debug.Log($"Centro promedio de la grilla: {averagePosition}");

        float cameraHeight = 4f;
        float cameraDistance = 8f;
        Vector3 cameraPosition = averagePosition + new Vector3(0, cameraHeight, cameraDistance);

        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.LookAt(averagePosition);
        Camera.main.transform.RotateAround(averagePosition, Vector3.right, 10f);

        Debug.Log($"Cámara posicionada en: {Camera.main.transform.position}, mirando a: {averagePosition}");
    }
}
