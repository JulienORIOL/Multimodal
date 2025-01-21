using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;


public class RoomData : MonoBehaviour
{
    public string roomName;
    public RoomInfo roomInfo;
    public List<string> studentsPresent = new List<string>();
    public Dictionary<string, int> studentsByHour = new Dictionary<string, int>();
    public GameObject infoPanel;

    private bool isInfoVisible = false;
    private Camera mainCamera;
    private Animator panelAnimator;
    private TextMeshProUGUI roomDetailsText;
    private TextMeshProUGUI studentsListText;
    private RawImage evacuationMapImage;

    private enum InfoTab { Overview, Schedule, Statistics }
    private InfoTab currentTab = InfoTab.Overview;

    [SerializeField]
    private float updateInterval = 1f;
    private float nextUpdateTime;

    void Start()
    {
        mainCamera = Camera.main;

        if (infoPanel != null)
        {
            // Positionnement initial du panel
            RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchoredPosition = new Vector3(0.3f, 0.15f, 0);
                panelRect.localScale = new Vector3(0.004f, 0.004f, 0.004f); // Légèrement plus gros
            }

            // Configuration du panel et des textes
            Transform panelTransform = infoPanel.transform.Find("Panel");
            if (panelTransform != null)
            {
                roomDetailsText = panelTransform.Find("RoomDetails")?.GetComponent<TextMeshProUGUI>();
                studentsListText = panelTransform.Find("StudentsListText")?.GetComponent<TextMeshProUGUI>();

                // Configuration des textes
                if (roomDetailsText != null)
                {
                    roomDetailsText.alignment = TextAlignmentOptions.Left;
                    roomDetailsText.fontSize = 18; // Un peu plus gros
                    roomDetailsText.enableWordWrapping = true;
                    roomDetailsText.margin = new Vector4(5, 5, 5, 5);
                }

                if (studentsListText != null)
                {
                    studentsListText.alignment = TextAlignmentOptions.Left;
                    studentsListText.fontSize = 16; // Un peu plus gros
                    studentsListText.enableWordWrapping = true;
                    studentsListText.margin = new Vector4(5, 5, 5, 5);
                }
            }

            // Configuration initiale
            infoPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            // Billboard effect - make panel face camera
            Vector3 lookAtPos = mainCamera.transform.position;
            lookAtPos.y = infoPanel.transform.position.y;
            infoPanel.transform.LookAt(lookAtPos);
            infoPanel.transform.Rotate(0, 180, 0);

            // Mettre à jour les informations si nécessaire
            if (Time.time >= nextUpdateTime)
            {
                UpdateInfoPanel();
                nextUpdateTime = Time.time + updateInterval;
            }
        }
    }

    public void OnRoomClicked()
    {
        Debug.Log($"Room {roomName} clicked!");
        isInfoVisible = !isInfoVisible;

        if (infoPanel != null)
        {
            infoPanel.SetActive(isInfoVisible);

            if (isInfoVisible)
            {
                Debug.Log("Updating info panel...");
                UpdateInfoPanel();
                if (panelAnimator != null)
                {
                    panelAnimator.SetTrigger("Show");
                }
            }
            else if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("Hide");
            }
        }

        // Provide haptic feedback on mobile devices
        if (isInfoVisible && SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    // Dans la méthode UpdateInfoPanel de RoomData.cs
    private void UpdateInfoPanel()
    {
        if (roomInfo == null) return;

        if (roomDetailsText != null)
        {
            StringBuilder details = new StringBuilder();
            string currentHour = $"{System.DateTime.Now.Hour}h";

            // En-tête avec nom de la salle (plus gros et centré)
            details.AppendLine($"<size=20><color=#2196F3><b>{roomName}</b></color></size>\n");

            // Nombre de places actuels
            var currentStudents = roomInfo.studentsByHour.ContainsKey(currentHour) ?
                roomInfo.studentsByHour[currentHour].Count : 0;

            // État de la salle avec nombre de places
            if (currentStudents > 0)
            {
                details.AppendLine($"<size=14><color=#4CAF50>● In Use</color></size>");
                details.AppendLine($"<size=14>Seats: {currentStudents}/{roomInfo.capacity}</size>");
            }
            else
            {
                details.AppendLine($"<size=14><color=#FFA000>● Empty</color></size>");
                details.AppendLine($"<size=14>All seats available ({roomInfo.capacity})</size>");
            }

            roomDetailsText.text = details.ToString();
        }

        if (studentsListText != null)
        {
            StringBuilder schedule = new StringBuilder();

            // Planning de la journée
            schedule.AppendLine("<size=16><b>Today's Schedule:</b></size>");

            if (roomInfo.studentsByHour.Count == 0)
            {
                schedule.AppendLine("\n<size=14><i>No classes scheduled</i></size>");
            }
            else
            {
                foreach (var hourData in roomInfo.studentsByHour.OrderBy(x => x.Key))
                {
                    schedule.AppendLine($"\n<size=14><color=#666666>{hourData.Key}</color> ({hourData.Value.Count} students)</size>");

                    // Afficher jusqu'à 3 étudiants par créneau
                    for (int i = 0; i < Math.Min(3, hourData.Value.Count); i++)
                    {
                        var student = hourData.Value[i];
                        schedule.AppendLine($"<size=13>• {student.name} ({student.specialization})</size>");
                    }

                    if (hourData.Value.Count > 3)
                    {
                        schedule.AppendLine($"<size=12><color=#666666>+ {hourData.Value.Count - 3} more...</color></size>");
                    }
                }
            }

            studentsListText.text = schedule.ToString();
        }
    }



    private int GetCurrentOccupancy()
    {
        int currentHour = System.DateTime.Now.Hour;
        string hourKey = $"{currentHour}h";

        if (studentsByHour.TryGetValue(hourKey, out int count))
        {
            return count;
        }
        return 0;
    }
}