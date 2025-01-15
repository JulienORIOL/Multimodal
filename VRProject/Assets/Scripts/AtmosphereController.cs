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
        
        if (mainLight == null)
        {
            mainLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (mainLight == null)
            {
                return;
            }
        }

        originalColor = mainLight.color;
    }

    void Update()
    {
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        
        if (mouseWheel != 0)
        {
            
            // Gestion des teintes avec la molette + touches
            if (Input.GetKey(KeyCode.R))
            {
                colorModification.x = Mathf.Clamp(colorModification.x + mouseWheel * tintMouseWheelSensitivity, -1f, 1f);
            }
            else if (Input.GetKey(KeyCode.G))
            {
                colorModification.y = Mathf.Clamp(colorModification.y + mouseWheel * tintMouseWheelSensitivity, -1f, 1f);
            }
            else if (Input.GetKey(KeyCode.B))
            {
                colorModification.z = Mathf.Clamp(colorModification.z + mouseWheel * tintMouseWheelSensitivity, -1f, 1f);
            }
            else
            {
                // Luminosité
                float newIntensity = mainLight.intensity + mouseWheel * brightnessMouseWheelSensitivity;
                mainLight.intensity = Mathf.Clamp(newIntensity, minBrightness, maxBrightness);
            }
        }

        // Réinitialisation avec Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            colorModification = Vector3.zero;
            mainLight.color = originalColor;
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
            mainLight.color = newColor;
        }
    }
}