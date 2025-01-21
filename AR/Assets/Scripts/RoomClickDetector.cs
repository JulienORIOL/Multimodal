// RoomClickDetector.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomClickDetector : MonoBehaviour
{
    private RoomData roomData;
    private float doubleTapTime = 0.2f;
    private float lastTapTime;

    void Start()
    {
        roomData = GetComponent<RoomData>();
    }

    void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Check for double tap
                float timeSinceLastTap = Time.time - lastTapTime;
                lastTapTime = Time.time;

                if (timeSinceLastTap <= doubleTapTime)
                {
                    // Double tap detected
                    HandleInteraction();
                }
                else
                {
                    // Single tap - check if hit
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

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                roomData.OnRoomClicked();
            }
        }
    }
}
