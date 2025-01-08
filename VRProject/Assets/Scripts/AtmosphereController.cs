using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    [Header("Lighting References")]
    public Light mainLight;

    [Header("Tint Settings")]
    public float tintChangeSpeed = 0.1f;
    public float tintMouseWheelSensitivity = 0.5f;
    public float minTintIntensity = -1.0f;
    public float maxTintIntensity = 1.0f;

    [Header("Brightness Settings")]
    public float brightnessChangeSpeed = 0.1f;
    public float brightnessMouseWheelSensitivity = 0.1f;
    public float minBrightness = 0.0f;
    public float maxBrightness = 2.0f;

    private Vector3 colorModification = Vector3.zero;
    private Color originalColor;

    void Start()
    {
        Debug.Log("=== AtmosphereController démarré ===");
        
        if (mainLight == null)
        {
            mainLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (mainLight == null)
            {
                Debug.LogError("Aucune lumière principale n'a été trouvée !");
                return;
            }
        }

        originalColor = mainLight.color;
        Debug.Log($"Couleur initiale de la lumière: {originalColor}");
    }

    void Update()
    {
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        
        if (mouseWheel != 0)
        {
            Debug.Log($"Molette: {mouseWheel}");
            
            // Gestion des teintes avec la molette + touches
            if (Input.GetKey(KeyCode.R))
            {
                colorModification.x = Mathf.Clamp(colorModification.x + mouseWheel * tintMouseWheelSensitivity, -1f, 1f);
                Debug.Log($"Rouge modifié: {colorModification.x}");
            }
            else if (Input.GetKey(KeyCode.G))
            {
                colorModification.y = Mathf.Clamp(colorModification.y + mouseWheel * tintMouseWheelSensitivity, -1f, 1f);
                Debug.Log($"Vert modifié: {colorModification.y}");
            }
            else if (Input.GetKey(KeyCode.B))
            {
                colorModification.z = Mathf.Clamp(colorModification.z + mouseWheel * tintMouseWheelSensitivity, -1f, 1f);
                Debug.Log($"Bleu modifié: {colorModification.z}");
            }
            else
            {
                // Luminosité
                float newIntensity = mainLight.intensity + mouseWheel * brightnessMouseWheelSensitivity;
                mainLight.intensity = Mathf.Clamp(newIntensity, minBrightness, maxBrightness);
                Debug.Log($"Luminosité: {mainLight.intensity}");
            }
        }

        // Réinitialisation avec Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            colorModification = Vector3.zero;
            mainLight.color = originalColor;
            Debug.Log("Couleurs réinitialisées");
            return;
        }

        // Application de la couleur
        UpdateLightColor();
    }

    void UpdateLightColor()
    {
        // Calculer la nouvelle couleur en ajoutant la modification à la couleur de base
        Color newColor = new Color(
            Mathf.Clamp01(1f + colorModification.x),  // Base blanche (1,1,1) + modification
            Mathf.Clamp01(1f + colorModification.y),
            Mathf.Clamp01(1f + colorModification.z)
        );

        if (mainLight.color != newColor)
        {
            Debug.Log($"Nouvelle couleur appliquée - R:{newColor.r} G:{newColor.g} B:{newColor.b}");
            mainLight.color = newColor;
        }
    }
}