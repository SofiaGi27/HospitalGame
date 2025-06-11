using UnityEngine;

public class PruebaSceneManager : MonoBehaviour 
{
    // Prefabs de personajes disponibles
    public GameObject[] characterPrefabs;

    // Posición inicial donde se va a instanciar el personaje
    public Vector3 spawnPosition = new Vector3(31.24976f, 1.268694f, 6.08461f);

    // Offset para la cámara (opcional, no usado directamente en este script)
    public Vector3 cameraOffset = new Vector3(0, 5, -10);

    void Start()
    {
        // Instancia al personaje seleccionado al comenzar la escena
        SpawnCharacterWithPreciseGroundPlacement();
    }

    void SpawnCharacterWithPreciseGroundPlacement()
    {
        // Se obtiene el índice del personaje guardado con PlayerPrefs
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        // Se valida que el índice esté dentro del rango
        if (selectedIndex >= 0 && selectedIndex < characterPrefabs.Length)
        {
            // Instancia el personaje en la posición exacta deseada
            GameObject character = Instantiate(characterPrefabs[selectedIndex], spawnPosition, Quaternion.identity);
            character.name = "PersonajePrueba";
            character.tag = "Player";

            // Añade CharacterController si no existe
            CharacterController controller = character.GetComponent<CharacterController>();
            if (controller == null)
                controller = character.AddComponent<CharacterController>();

            // Establecer valores de altura y radio de inmediato
            controller.height = 1.3f; // Altura deseada
            controller.radius = 0.5325089f; // Radio deseado

            // Raycast para ajustar la altura del personaje al suelo
            RaycastHit hit;
            float raycastDistance = 10f;
            Vector3 rayOrigin = spawnPosition + Vector3.up * raycastDistance;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance))
            {
                
                Vector3 adjustedPosition = new Vector3(
                    spawnPosition.x, 
                    hit.point.y, 
                    spawnPosition.z
                );

                // Centro del CharacterController
                controller.center = new Vector3(0, controller.height / 2f, 0);

                character.transform.position = adjustedPosition;

            }
            else
            {
               // Debug.LogWarning("[PruebaSceneManager] No se detectó el suelo con el Raycast.");
            }

            // Añadir scripts si faltan
            if (character.GetComponent<CharacterMovementV2>() == null)
                character.AddComponent<CharacterMovementV2>();

            if (character.GetComponent<AutoColliderFitter>() == null)
                character.AddComponent<AutoColliderFitter>();

            if (character.GetComponent<NPCTrigger>() == null)
                character.AddComponent<NPCTrigger>();

            // Asignar el personaje a la cámara
            SimpleCameraFollow cameraFollow = Camera.main.GetComponent<SimpleCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.target = character.transform;
            }
        }
        else
        {
           // Debug.LogWarning("Índice de personaje fuera de rango.");
        }
    }
} 