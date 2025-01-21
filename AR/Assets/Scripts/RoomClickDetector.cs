using UnityEngine;
using UnityEngine.EventSystems;

public class RoomClickDetector : MonoBehaviour
{
    private RoomData roomData;
    private float doubleTapTime = 0.2f;
    private float lastTapTime;

    // Références pour les effets visuels
    private Material originalMaterial;
    private MeshRenderer meshRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    [SerializeField]
    private Color selectedColor = new Color(0.3f, 0.6f, 1f); // Bleu
    [SerializeField]
    private float clickScaleAmount = 0.75f;
    [SerializeField]
    private float clickAnimationDuration = 0.3f;

    private bool isAnimating = false;
    private float animationTime = 0f;

    void Start()
    {
        roomData = GetComponent<RoomData>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            // Créer une instance du matériau pour ne pas affecter d'autres objets
            originalMaterial = new Material(meshRenderer.material);
            meshRenderer.material = originalMaterial;
            originalColor = originalMaterial.color;
        }

        originalScale = transform.localScale;
    }

    void Update()
    {
        // Gestion de l'animation de clic
        if (isAnimating)
        {
            animationTime += Time.deltaTime;
            float progress = animationTime / clickAnimationDuration;

            if (progress <= 1f)
            {
                // Animation de l'échelle
                float scaleMultiplier = roomData.isInfoVisible
                    ? Mathf.Lerp(1f, clickScaleAmount, progress)
                    : Mathf.Lerp(clickScaleAmount, 1f, progress);
                transform.localScale = originalScale * scaleMultiplier;
            }
            else
            {
                // Fin de l'animation
                isAnimating = false;
                transform.localScale = originalScale;
            }
        }

        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                float timeSinceLastTap = Time.time - lastTapTime;
                lastTapTime = Time.time;

                if (timeSinceLastTap <= doubleTapTime)
                {
                    HandleInteraction();
                }
                else
                {
                    HandleInteraction();
                }
            }
        }
        // Handle mouse input for testing in editor
        else if (Input.GetMouseButtonDown(0))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // Ignore UI interactions
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            // Jouer le son
            AudioManager.Instance.PlayClickSound();

            // Déclencher l'animation de clic
            StartClickAnimation();

            // Changer la couleur en fonction de l'état
            UpdateVisualState(!roomData.isInfoVisible);

            // Appeler la logique existante
            roomData.OnRoomClicked();
        }
    }

    private void StartClickAnimation()
    {
        isAnimating = true;
        animationTime = 0f;
    }

    private void UpdateVisualState(bool isSelected)
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            originalMaterial.color = isSelected ? selectedColor : originalColor;
        }
    }

    private void OnDestroy()
    {
        // Nettoyer le matériau instancié
        if (originalMaterial != null)
        {
            Destroy(originalMaterial);
        }
    }
}