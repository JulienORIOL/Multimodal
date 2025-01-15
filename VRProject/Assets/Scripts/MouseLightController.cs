using UnityEngine;

public class MouseLightController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCharacter;
    public Light spotLight;
    public float heightAbovePlayer = 2f;

    [Header("Light Settings")]
    public float lightRange = 10f;
    public float lightIntensity = 2f;
    public float smoothSpeed = 5f;

    [Header("Mouse Settings")]
    public LayerMask aimLayerMask = -1; // Par défaut, tous les layers
    public float maxAimDistance = 100f;

    private Vector3 targetLookPosition;
    private Vector3 smoothedLookPosition;
    private Camera mainCamera;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (!ValidateComponents()) return;

        if (spotLight == null)
        {
            spotLight = gameObject.AddComponent<Light>();
            ConfigureSpotlight();
        }

        mainCamera = Camera.main;
    }

    private bool ValidateComponents()
    {
        if (playerCharacter == null)
        {
            Debug.LogError("Player Character non assigné!");
            return false;
        }
        return true;
    }

    private void ConfigureSpotlight()
    {
        spotLight.type = LightType.Spot;
        spotLight.intensity = lightIntensity;
        spotLight.range = lightRange;
        spotLight.spotAngle = 30f;
    }

    private void Update()
    {
        UpdateMouseAiming();
        UpdateLightTransform();
    }

    private void UpdateMouseAiming()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxAimDistance, aimLayerMask))
        {
            targetLookPosition = hit.point;
        }
        else
        {
            // Si le rayon ne touche rien, on vise un point dans le vide à maxAimDistance
            targetLookPosition = ray.origin + ray.direction * maxAimDistance;
        }

        // Lissage du mouvement
        smoothedLookPosition = Vector3.Lerp(smoothedLookPosition, targetLookPosition, Time.deltaTime * smoothSpeed);
    }

    private void UpdateLightTransform()
    {
        // Position fixe au-dessus du joueur
        Vector3 lightPosition = playerCharacter.position + Vector3.up * heightAbovePlayer;
        spotLight.transform.position = lightPosition;

        // Orientation vers le point ciblé
        if (smoothedLookPosition != Vector3.zero)
        {
            Vector3 lookDirection = (smoothedLookPosition - lightPosition).normalized;
            spotLight.transform.rotation = Quaternion.Lerp(
                spotLight.transform.rotation,
                Quaternion.LookRotation(lookDirection),
                Time.deltaTime * smoothSpeed
            );
        }
    }
}