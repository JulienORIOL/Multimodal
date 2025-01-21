using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Outline))]
public class LogPanelController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // Composants UI
    private ScrollRect scrollRect;
    private RectTransform titleBarRect;
    private Camera arCamera;
    private Canvas parentCanvas;

    // Drag handling
    private Vector2 touchStart;
    private Vector3 panelStartPosition;
    private bool isDragging = false;
    private bool isTouchInTitleBar = false;

    // Resize handling
    private Vector2 touchStart0;
    private Vector2 touchStart1;
    private float initialDistance;
    private Vector3 initialScale;
    private bool isResizing = false;

    // Configuration
    [SerializeField] private float minScale = 0.002f;
    [SerializeField] private float maxScale = 0.008f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float dragSpeed = 0.01f;
    [SerializeField] private float titleBarHeight = 40f; // Hauteur de la zone de drag en pixels
    [SerializeField] private Color normalOutlineColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    [SerializeField] private Color activeOutlineColor = new Color(0f, 0.7f, 1f, 1f);

    // Components
    private Outline panelOutline;

    private void Start()
    {
        SetupComponents();
        CreateTitleBar();
    }

    private void SetupComponents()
    {
        arCamera = Camera.main;
        panelOutline = GetComponent<Outline>();
        scrollRect = GetComponentInChildren<ScrollRect>();
        parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
        {
            parentCanvas.worldCamera = arCamera;
        }

        if (panelOutline != null)
        {
            panelOutline.effectColor = normalOutlineColor;
        }
    }

    private void CreateTitleBar()
    {
        // Créer une zone de titre pour le drag
        GameObject titleBarObj = new GameObject("TitleBar", typeof(RectTransform));
        titleBarObj.transform.SetParent(transform, false);
        titleBarRect = titleBarObj.GetComponent<RectTransform>();

        // Configurer le RectTransform du titre
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = Vector2.one;
        titleBarRect.pivot = new Vector2(0.5f, 1);
        titleBarRect.sizeDelta = new Vector2(0, titleBarHeight);

        // Ajouter une image pour la visualisation (optionnel)
        Image titleImage = titleBarObj.AddComponent<Image>();
        titleImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Handle two-finger resize
        if (Input.touchCount == 2)
        {
            HandleResizing();
            if (scrollRect != null) scrollRect.vertical = false;
        }
        else if (scrollRect != null && !isDragging)
        {
            scrollRect.vertical = true;
        }

        // Make panel face camera
        if (arCamera != null)
        {
            Vector3 lookAtPos = arCamera.transform.position;
            lookAtPos.y = transform.position.y;
            transform.LookAt(lookAtPos);
            transform.Rotate(0, 180, 0);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.touchCount == 1 && !isResizing)
        {
            // Vérifier si le touch est dans la barre de titre
            isTouchInTitleBar = RectTransformUtility.RectangleContainsScreenPoint(
                titleBarRect,
                eventData.position,
                eventData.pressEventCamera
            );

            if (isTouchInTitleBar)
            {
                StartDragging(eventData);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && isTouchInTitleBar)
        {
            UpdateDragPosition(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            StopDragging();
        }
        isTouchInTitleBar = false;
    }

    private void StartDragging(PointerEventData eventData)
    {
        isDragging = true;
        touchStart = eventData.position;
        panelStartPosition = transform.position;
        SetOutlineActive(true);

        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 delta = eventData.position - touchStart;
        Vector3 screenDelta = new Vector3(delta.x, delta.y, 0);
        Vector3 worldDelta = arCamera.transform.TransformDirection(screenDelta * dragSpeed);

        Vector3 newPosition = panelStartPosition + worldDelta;
        float distanceFromCamera = Vector3.Distance(arCamera.transform.position, newPosition);

        if (distanceFromCamera > 0.5f && distanceFromCamera < 5f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                newPosition,
                Time.deltaTime * smoothSpeed
            );
        }

        panelStartPosition = transform.position;
        touchStart = eventData.position;
    }

    private void HandleResizing()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (!isResizing && (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began))
        {
            StartResizing(touch0, touch1);
        }
        else if (isResizing && (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
        {
            UpdateResizing(touch0, touch1);
        }
        else if (isResizing && (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended ||
                 touch0.phase == TouchPhase.Canceled || touch1.phase == TouchPhase.Canceled))
        {
            StopResizing();
        }
    }

    private void StartResizing(Touch touch0, Touch touch1)
    {
        touchStart0 = touch0.position;
        touchStart1 = touch1.position;
        initialDistance = Vector2.Distance(touch0.position, touch1.position);
        initialScale = transform.localScale;
        isResizing = true;
        SetOutlineActive(true);

        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    private void UpdateResizing(Touch touch0, Touch touch1)
    {
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);
        float scaleFactor = currentDistance / initialDistance;

        Vector3 newScale = initialScale * scaleFactor;
        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
        newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            newScale,
            Time.deltaTime * smoothSpeed
        );
    }

    private void StopResizing()
    {
        isResizing = false;
        SetOutlineActive(false);
        if (scrollRect != null)
        {
            scrollRect.vertical = true;
        }
    }

    private void StopDragging()
    {
        isDragging = false;
        SetOutlineActive(false);
    }

    private void SetOutlineActive(bool active)
    {
        if (panelOutline != null)
        {
            panelOutline.effectColor = active ? activeOutlineColor : normalOutlineColor;
        }
    }
}