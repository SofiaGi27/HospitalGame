using UnityEngine;
using UnityEngine.UIElements;

public class CharacterCardImageLoader : MonoBehaviour
{
    // Actualiza los nombres de las imágenes
    private string[] characterImageNames = { "character-5", "character-7", "character-10", "character-14", "character-11", "character-19" };

    // Referencia al UI Document
    public UIDocument uiDocument;

    // Start is called before the first frame update
    void Start()
    {
        // Obtén la raíz del UXML
        var root = uiDocument.rootVisualElement;

        // Itera sobre las tarjetas de personajes y asigna las imágenes
        for (int i = 0; i < characterImageNames.Length; i++)
        {
            string cardName = "character-" + (i + 1); // nombre de las tarjetas en el UXML

            // Encuentra cada tarjeta por su nombre en el UXML
            var characterCard = root.Q<VisualElement>(cardName);

            // Encuentra el elemento de la imagen dentro de la tarjeta
            var characterImage = characterCard.Q<VisualElement>("character-image-" + (i + 1));

            // Cargar la imagen desde Resources/Images/Characters/ (asegúrate de que las imágenes estén en esa carpeta)
            Texture2D texture = Resources.Load<Texture2D>("Images/Characters/" + characterImageNames[i]);

            // Si la imagen se carga correctamente, asigna la imagen de fondo
            if (texture != null)
            {
                characterImage.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                Debug.LogError("No se encontró la imagen para: " + characterImageNames[i]);
            }
        }
    }
}
