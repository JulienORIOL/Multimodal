using UnityEngine;
using System.Collections.Generic;

public class RoomData : MonoBehaviour
{
    public string roomName;
    public List<string> studentsPresent = new List<string>();
    public Dictionary<string, int> studentsByHour = new Dictionary<string, int>();
    public GameObject infoPanel;
    private bool isInfoVisible = false;
    private Camera mainCamera;

    void Start()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        mainCamera = Camera.main;
    }

    void Update()
    {
        // Si le panel est visible, on le fait regarder vers la caméra
        if (infoPanel != null && infoPanel.activeSelf)
        {
            // Faire face à la caméra
            infoPanel.transform.LookAt(infoPanel.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void OnRoomClicked()
    {
        isInfoVisible = !isInfoVisible;
        if (infoPanel != null)
        {
            infoPanel.SetActive(isInfoVisible);
            UpdateInfoPanel();
        }
    }

    private void UpdateInfoPanel()
    {
        TMPro.TextMeshProUGUI infoText = infoPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (infoText != null)
        {
            string info = $"Room: {roomName}\n\n";
            info += "Students per hour:\n";
            foreach (var hourData in studentsByHour)
            {
                info += $"{hourData.Key}: {hourData.Value} students\n";
            }
            info += $"\nTotal students: {studentsPresent.Count}\n";

            infoText.text = info;
        }
    }
}