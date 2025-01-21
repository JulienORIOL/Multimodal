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
    private Image panelBackground;  // Nouvelle référence pour le background
    private float pulseTimer = 0f;
    private const float PULSE_SPEED = 2f;
    private const float PULSE_MIN_ALPHA = 0.6f;
    private const float PULSE_MAX_ALPHA = 1f;

    [SerializeField]
    private float updateInterval = 1f;
    private float nextUpdateTime;


    private const float PANEL_PULSE_SPEED = 3f; // Vitesse de l'animation
    private const float PANEL_MIN_SCALE = 0.98f; // Échelle minimale
    private const float PANEL_MAX_SCALE = 1.02f; // Échelle maximale
    private Vector3 originalPanelScale; // Pour stocker l'échelle initiale

    void Start()
    {
        mainCamera = Camera.main;

        if (infoPanel != null)
        {
            // Configuration du RectTransform
            RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchoredPosition = new Vector3(0.3f, 0.15f, 0);
                panelRect.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            }
            originalPanelScale = new Vector3(0.004f, 0.004f, 0.004f);

            // Récupération du Panel et configuration du background
            Transform panelTransform = infoPanel.transform.Find("Panel");
            if (panelTransform != null)
            {
                // Configuration du background
                panelBackground = panelTransform.GetComponent<Image>();
                if (panelBackground == null)
                {
                    panelBackground = panelTransform.gameObject.AddComponent<Image>();
                }
                // Définition de la couleur de fond (noir semi-transparent)
                panelBackground.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

                // Ajout d'un effet de contour
                var outline = panelTransform.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = panelTransform.gameObject.AddComponent<Outline>();
                    outline.effectColor = new Color(0.2f, 0.6f, 1f, 0.5f); // Bleu clair semi-transparent
                    outline.effectDistance = new Vector2(2, -2);
                }

                roomDetailsText = panelTransform.Find("RoomDetails")?.GetComponent<TextMeshProUGUI>();
                studentsListText = panelTransform.Find("StudentsListText")?.GetComponent<TextMeshProUGUI>();

                if (roomDetailsText != null)
                {
                    roomDetailsText.alignment = TextAlignmentOptions.Left;
                    roomDetailsText.fontSize = 18;
                    roomDetailsText.enableWordWrapping = true;
                    roomDetailsText.margin = new Vector4(5, 5, 5, 5);
                    // Assurer que le texte est bien visible sur le fond sombre
                    roomDetailsText.color = Color.white;
                }

                if (studentsListText != null)
                {
                    studentsListText.alignment = TextAlignmentOptions.Left;
                    studentsListText.fontSize = 16;
                    studentsListText.enableWordWrapping = true;
                    studentsListText.margin = new Vector4(5, 5, 5, 5);
                    // Assurer que le texte est bien visible sur le fond sombre
                    studentsListText.color = Color.white;
                }
            }

            infoPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            float panelPulse = (Mathf.Sin(Time.time * PANEL_PULSE_SPEED) + 1f) / 2f;
            float currentScale = Mathf.Lerp(PANEL_MIN_SCALE, PANEL_MAX_SCALE, panelPulse);
            infoPanel.transform.localScale = originalPanelScale * currentScale;
            // Billboard effect
            Vector3 lookAtPos = mainCamera.transform.position;
            lookAtPos.y = infoPanel.transform.position.y;
            infoPanel.transform.LookAt(lookAtPos);
            infoPanel.transform.Rotate(0, 180, 0);

            // Mise à jour de l'animation de pulsation
            pulseTimer += Time.deltaTime * PULSE_SPEED;

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

        if (isInfoVisible && SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
    }

    private void UpdateInfoPanel()
    {
        if (roomInfo == null) return;

        if (roomDetailsText != null)
        {
            StringBuilder details = new StringBuilder();
            string currentHour = $"{System.DateTime.Now.Hour}h";

            details.AppendLine($"<size=20><color=#2196F3><b>{roomName}</b></color></size>\n");

            var currentStudents = roomInfo.studentsByHour.ContainsKey(currentHour) ?
                roomInfo.studentsByHour[currentHour].Count : 0;

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
            string currentHour = $"{System.DateTime.Now.Hour}h";

            float pulseAlpha = Mathf.Lerp(PULSE_MIN_ALPHA, PULSE_MAX_ALPHA, (Mathf.Sin(pulseTimer) + 1f) / 2f);
            string pulseHexAlpha = Mathf.RoundToInt(pulseAlpha * 255).ToString("X2");

            schedule.AppendLine("<size=16><b>Today's Schedule:</b></size>");

            if (roomInfo.studentsByHour.Count == 0)
            {
                schedule.AppendLine("\n<size=14><i>No classes scheduled</i></size>");
            }
            else
            {
                foreach (var hourData in roomInfo.studentsByHour.OrderBy(x => x.Key))
                {
                    if (hourData.Key == currentHour)
                    {
                        schedule.AppendLine($"\n<size=14><color=#FF4081{pulseHexAlpha}><b>▶ {hourData.Key}</b></color> ({hourData.Value.Count} students)</size>");
                    }
                    else
                    {
                        schedule.AppendLine($"\n<size=14><color=#FFFFFF>{hourData.Key}</color> ({hourData.Value.Count} students)</size>");
                    }

                    for (int i = 0; i < Math.Min(3, hourData.Value.Count); i++)
                    {
                        var student = hourData.Value[i];
                        if (hourData.Key == currentHour)
                        {
                            schedule.AppendLine($"<size=13><color=#FF4081>• {student.name} ({student.specialization})</color></size>");
                        }
                        else
                        {
                            schedule.AppendLine($"<size=13><color=#E0E0E0>• {student.name} ({student.specialization})</color></size>");
                        }
                    }

                    if (hourData.Value.Count > 3)
                    {
                        schedule.AppendLine($"<size=12><color=#808080>+ {hourData.Value.Count - 3} more...</color></size>");
                    }
                }
            }

            studentsListText.text = schedule.ToString();
        }
    }
}