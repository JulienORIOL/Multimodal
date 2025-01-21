using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;
using UnityEngine.EventSystems;

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
    private Image panelBackground;
    private Outline panelOutline;

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
        InitializePanelComponents();
        
        // Charger les informations de la salle depuis DataManager
        roomInfo = DataManager.Instance.GetRoomInfo(roomName);
    }

    private void InitializePanelComponents()
    {
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
                panelOutline = panelTransform.GetComponent<Outline>();
                if (panelOutline == null)
                {
                    panelOutline = panelTransform.gameObject.AddComponent<Outline>();
                    panelOutline.effectColor = new Color(0.2f, 0.6f, 1f, 0.5f); // Bleu clair semi-transparent
                    panelOutline.effectDistance = new Vector2(2, -2);
                }

                roomDetailsText = panelTransform.Find("RoomDetails")?.GetComponent<TextMeshProUGUI>();
                studentsListText = panelTransform.Find("StudentsListText")?.GetComponent<TextMeshProUGUI>();

                ConfigureTextComponent(roomDetailsText, 18);
                ConfigureTextComponent(studentsListText, 16);
            }

            infoPanel.SetActive(false);
        }
    }

    private void ConfigureTextComponent(TextMeshProUGUI textComponent, int fontSize)
    {
        if (textComponent != null)
        {
            textComponent.alignment = TextAlignmentOptions.Left;
            textComponent.fontSize = fontSize;
            textComponent.enableWordWrapping = true;
            textComponent.margin = new Vector4(5, 5, 5, 5);
            textComponent.color = Color.white;
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

    public void UpdateStudentsList(List<StudentData> filteredStudents, string timeFilter = "", string specFilter = "", string transportFilter = "")
    {
        // Filtrer les étudiants pour cette salle en fonction des filtres
        var roomStudents = filteredStudents.Where(s => 
            s.room == roomName && 
            (string.IsNullOrEmpty(timeFilter) || s.schedule == timeFilter) &&
            (string.IsNullOrEmpty(specFilter) || s.specialization == specFilter) &&
            (string.IsNullOrEmpty(transportFilter) || s.transport == transportFilter)
        ).ToList();

        // Mettre à jour les statistiques par heure
        studentsByHour.Clear();
        studentsPresent.Clear();

        foreach (var student in roomStudents)
        {
            if (!studentsByHour.ContainsKey(student.schedule))
                studentsByHour[student.schedule] = 0;
            
            studentsByHour[student.schedule]++;
            studentsPresent.Add(student.name);
        }

        // Mettre à jour le panneau si visible
        if (isInfoVisible)
        {
            UpdateInfoPanel();
        }
    }

    public Dictionary<string, int> GetStudentsBySpecialization()
    {
        if (roomInfo == null) return new Dictionary<string, int>();
        return roomInfo.specializationStats;
    }

    public Dictionary<string, int> GetStudentsByTransport()
    {
        if (roomInfo == null) return new Dictionary<string, int>();
        return roomInfo.transportStats;
    }

    private void UpdateInfoPanel()
    {
        if (roomDetailsText != null)
        {
            StringBuilder details = new StringBuilder();
            string currentHour = $"{DateTime.Now.Hour}h";

            details.AppendLine($"<size=20><color=#2196F3><b>{roomName}</b></color></size>\n");

            // Calculer l'occupation actuelle en fonction des étudiants
            var currentTimeStudents = studentsPresent.Where(s => 
                roomInfo.studentsByHour.ContainsKey(currentHour) && 
                roomInfo.studentsByHour[currentHour].Any(student => student.name == s)
            ).ToList();

            int currentOccupancy = currentTimeStudents.Count;
            int roomCapacity = roomInfo.capacity;

            Debug.Log($"[{roomName}] Current occupancy: {currentOccupancy}/{roomCapacity} at {currentHour}");

            if (currentOccupancy > 0)
            {
                details.AppendLine($"<size=14><color=#4CAF50>● In Use</color></size>");
                details.AppendLine($"<size=14>Seats: {currentOccupancy}/{roomCapacity}</size>");
            }
            else
            {
                details.AppendLine($"<size=14><color=#FFA000>● Empty</color></size>");
                details.AppendLine($"<size=14>All seats available ({roomCapacity})</size>");
            }

            roomDetailsText.text = details.ToString();
        }

        if (studentsListText != null)
        {
            StringBuilder schedule = new StringBuilder();
            string currentHour = $"{DateTime.Now.Hour}h";

            schedule.AppendLine("<size=16><b>Today's Schedule:</b></size>");

            var hourGroups = studentsByHour
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            Debug.Log($"[{roomName}] Found {hourGroups.Count} time slots with students");

            if (hourGroups.Count == 0)
            {
                schedule.AppendLine("\n<size=14><i>No classes scheduled</i></size>");
            }
            else
            {
                foreach (var hourGroup in hourGroups)
                {
                    if (hourGroup.Key == currentHour)
                    {
                        schedule.AppendLine($"\n<size=14><color=#FF4081>▶ {hourGroup.Key}</color> ({hourGroup.Value} students)</size>");
                        
                        // Afficher les noms des étudiants pour l'heure actuelle
                        var studentsInCurrentHour = roomInfo.studentsByHour[currentHour];
                        foreach (var student in studentsInCurrentHour.Take(3))
                        {
                            schedule.AppendLine($"<size=13><color=#FF4081>• {student.name} ({student.specialization})</color></size>");
                        }

                        if (studentsInCurrentHour.Count > 3)
                        {
                            schedule.AppendLine($"<size=12><color=#808080>+ {studentsInCurrentHour.Count - 3} more...</color></size>");
                        }
                    }
                    else
                    {
                        schedule.AppendLine($"\n<size=14>{hourGroup.Key} ({hourGroup.Value} students)</size>");
                    }
                }

                // Statistiques de transport
                if (studentsPresent.Any())
                {
                    schedule.AppendLine("\n<size=14><b>Transport:</b></size>");
                    var transportStats = roomInfo.transportStats
                        .OrderByDescending(x => x.Value);

                    foreach (var transport in transportStats)
                    {
                        schedule.AppendLine($"<size=13>{transport.Key}: {transport.Value}</size>");
                    }
                }
            }

            studentsListText.text = schedule.ToString();
        }
    }
}