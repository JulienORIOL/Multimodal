using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelResizer : MonoBehaviour
{
    private RoomData roomData;
    private Vector2 touchStart0;
    private Vector2 touchStart1;
    private float initialDistance;
    private Vector3 initialScale;
    private bool isResizing = false;
    private bool needsUpdate = false;

    [SerializeField] private float minScale = 0.002f;
    [SerializeField] private float maxScale = 0.008f;
    [SerializeField] private float smoothSpeed = 10f;

    private void Start()
    {
        // Trouver le RoomData parent (sur le GameObject du QR code)
        roomData = GetComponentInParent<RoomData>();
        if (roomData == null)
        {
            Debug.LogError("PanelResizer cannot find RoomData in parent hierarchy!");
        }
    }

    private void Update()
    {
        if (roomData == null || !gameObject.activeSelf)
            return;

        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (!isResizing && (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began))
            {
                StartResizing(touch0, touch1);
            }
            else if (isResizing &&
                    (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
            {
                UpdateResizing(touch0, touch1);
            }
            else if (isResizing &&
                    (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended ||
                     touch0.phase == TouchPhase.Canceled || touch1.phase == TouchPhase.Canceled))
            {
                StopResizing();
            }
        }
        else if (isResizing)
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
        roomData.isResizing = true;

        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    private void UpdateResizing(Touch touch0, Touch touch1)
    {
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);
        float scaleFactor = currentDistance / initialDistance;

        // Calculer la nouvelle échelle
        Vector3 newScale = initialScale * scaleFactor;

        // Appliquer les limites
        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
        newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

        // Appliquer la nouvelle échelle avec un lissage
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            newScale,
            Time.deltaTime * smoothSpeed
        );
    }

    private void StopResizing()
    {
        isResizing = false;
        roomData.isResizing = false;

        // Mettre à jour l'échelle originale dans RoomData
        roomData.UpdateOriginalScale(transform.localScale);
    }
}