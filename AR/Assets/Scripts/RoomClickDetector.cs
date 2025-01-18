using UnityEngine;

public class RoomClickDetector : MonoBehaviour
{
    private RoomData roomData;

    void Start()
    {
        roomData = GetComponent<RoomData>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
}