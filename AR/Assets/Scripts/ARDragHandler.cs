// ARDragHandler.cs
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDragHandler : MonoBehaviour
{
    private Vector2 touchStart;
    private Vector3 panelStartPosition;
    private bool isDragging = false;
    private Camera arCamera;

    private void Start()
    {
        arCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsPointerOverPanel(touch.position))
                    {
                        StartDragging(touch);
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        UpdateDragPosition(touch);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }

    private bool IsPointerOverPanel(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == transform;
        }

        return false;
    }

    private void StartDragging(Touch touch)
    {
        isDragging = true;
        touchStart = touch.position;
        panelStartPosition = transform.position;
    }

    private void UpdateDragPosition(Touch touch)
    {
        Vector2 delta = touch.position - touchStart;

        // Convert screen delta to world space delta
        Vector3 screenDelta = new Vector3(delta.x, delta.y, 0);
        Vector3 worldDelta = arCamera.transform.TransformDirection(screenDelta * 0.01f);

        // Update position while maintaining distance from camera
        Vector3 newPosition = panelStartPosition + worldDelta;
        float distanceFromCamera = Vector3.Distance(arCamera.transform.position, newPosition);

        // Ensure panel stays within reasonable distance
        if (distanceFromCamera > 1f && distanceFromCamera < 5f)
        {
            transform.position = newPosition;
        }

        // Make panel face camera
        transform.rotation = Quaternion.LookRotation(transform.position - arCamera.transform.position);
    }
}